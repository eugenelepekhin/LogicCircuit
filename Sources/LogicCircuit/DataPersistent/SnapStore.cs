using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Internal class not for public consumption.
	/// Store is a set of SnapTable tables. It supports one StoreSnapshot editor, that can commit and rollback transactions, and undo/redo previously committed one.
	/// </summary>
	internal sealed partial class SnapStore {

		/// <summary>
		/// Occurred when transaction is committed
		/// </summary>
		public event EventHandler Committed;

		/// <summary>
		/// Type of transaction
		/// </summary>
		private enum TransactionType : byte {
			Omit,
			Edit,
			Undo,
			Redo
		}

		private readonly Dictionary<string, ISnapTable> table = new Dictionary<string, ISnapTable>();
		private readonly ValueList<TransactionType> version = new ValueList<TransactionType>();
		private int minUndo = 0;
		private StoreSnapshot editor = null;
		private Thread editorThread = null;

		/// <summary>
		/// true when SnapStore was "frozen" for metadata modifications
		/// </summary>
		public bool IsFrozen { get; private set; }

		/// <summary>
		/// Gets last committed version
		/// </summary>
		public int CommittedVersion { get; private set; }

		/// <summary>
		/// Gets current latest version of the store.
		/// </summary>
		public int Version { get { return this.version.Count; } }

		/// <summary>
		/// Gets collection of all tables
		/// </summary>
		public IEnumerable<ISnapTable> Tables { get { return this.table.Values; } }

		/// <summary>
		/// If transaction is started then contains the Owner of the transaction, otherwise null.
		/// </summary>
		public StoreSnapshot Editor { get { return this.editor; } }

		/// <summary>
		/// Returns true if undo operation is available.
		/// </summary>
		public bool CanUndo { get { return this.editor == null && 0 < this.RevertVersion(TransactionType.Undo); } }

		/// <summary>
		/// Returns true if redo operation is available
		/// </summary>
		public bool CanRedo { get { return this.editor == null && 0 < this.RevertVersion(TransactionType.Redo); } }

		/// <summary>
		/// Searches for table by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ISnapTable Table(string name) {
			ISnapTable snapTable;
			if(this.table.TryGetValue(name, out snapTable)) {
				return snapTable;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Adds the table to the store.
		/// Table name should be unique in the store.
		/// </summary>
		/// <param name="snapTable"></param>
		/// <returns></returns>
		public void Add(ISnapTable snapTable) {
			if(snapTable.SnapStore != this) {
				throw new ArgumentException(Properties.Resources.ErrorSnapTable);
			}
			this.CheckNotFrozen();
			this.table.Add(snapTable.Name, snapTable);
		}

		/// <summary>
		/// Freezes shape of the store and allows transactions to be opened.
		/// </summary>
		public void FreezeShape() {
			this.CheckNotFrozen();
			if(this.table.Count < 1) {
				throw new InvalidOperationException(Properties.Resources.ErrorStoreIsEmpty);
			}
			this.IsFrozen = true;
		}

		/// <summary>
		/// Checks if store is frozen and throw if not. returns the store otherwise.
		/// </summary>
		/// <returns></returns>
		public SnapStore CheckFrozen() {
			if(!this.IsFrozen) {
				throw new InvalidOperationException(Properties.Resources.ErrorStoreNotFrozen);
			}
			return this;
		}

		/// <summary>
		/// Checks if store is not frozen and table modification is allowed
		/// </summary>
		public void CheckNotFrozen() {
			if(this.IsFrozen) {
				throw new InvalidOperationException(Properties.Resources.ErrorStoreIsFrozen);
			}
		}

		/// <summary>
		/// Starts new transaction. If successful then returns:
		/// - number of the started transaction if it is Edit transaction
		/// - number of transaction to revert if it is Undo/Redo transaction
		/// If transaction was not started then returns 0.
		/// </summary>
		/// <param name="newEditor">StoreSnapshot that owns the transaction</param>
		/// <param name="transactionType">Edit, Undo or Redo transaction</param>
		/// <returns></returns>
		private int StartTransaction(StoreSnapshot newEditor, TransactionType transactionType) {
			if(newEditor == null) {
				throw new ArgumentNullException("newEditor");
			}
			Debug.Assert(transactionType == TransactionType.Edit || transactionType == TransactionType.Undo || transactionType == TransactionType.Redo, "Wrong transaction type");
			this.CheckFrozen();

			int transaction = 0;
			// any value of oldEditor other then null means this thread does not own the transaction
			// null means this thread is exclusive owner of transaction
			StoreSnapshot oldEditor = newEditor;
			bool success = false;

			RuntimeHelpers.PrepareConstrainedRegions();
			try {
				RuntimeHelpers.PrepareConstrainedRegions();
				try {} finally {
					oldEditor = Interlocked.CompareExchange<StoreSnapshot>(ref this.editor, newEditor, null);
				}
				if(oldEditor == null) {
					Debug.Assert(this.editor == newEditor, "Expecting to be current editor");
					Debug.Assert(this.editorThread == null, "Editor thread should be null here");
					if(this.version.Count == int.MaxValue) {
						throw new InvalidOperationException(Properties.Resources.ErrorTooManyTransactions);
					}
					this.editorThread = Thread.CurrentThread;
					transaction = (transactionType == TransactionType.Edit) ? this.version.Count + 1 : this.RevertVersion(transactionType);
					if(0 < transaction) {
						this.version.PrepareAdd();
						success = true;
					}
				}
			} finally {
				if(success) {
					this.version.FixedAdd(ref transactionType);
				} else if(oldEditor == null) {
					Debug.Assert(this.editor == newEditor, "newEditor expected to be current editor");
					this.editorThread = null; // this should be the first assignment
					this.editor = null;
				}
			}
			return transaction;
		}

		/// <summary>
		/// Starts new transaction
		/// </summary>
		/// <param name="storeSnapshot">Owner of the transaction</param>
		/// <returns>true if the transaction started, false if other transaction is in progress</returns>
		public bool StartTransaction(StoreSnapshot storeSnapshot) {
			return 0 < this.StartTransaction(storeSnapshot, TransactionType.Edit);
		}

		/// <summary>
		///Commits current transaction. Optionally if <paramref name="withOmit"/> set to true erases transaction from undo history
		/// </summary>
		/// <param name="withOmit"></param>
		/// <returns></returns>
		public int Commit(bool withOmit) {
			this.ValidateModification();
			int v = this.Version;
			RuntimeHelpers.PrepareConstrainedRegions();
			try {} finally {
				if(withOmit) {
					ValueList<TransactionType>.Address address = this.version.ItemAddress(this.version.Count - 1);
					Debug.Assert(address.Page[address.Index] == TransactionType.Edit, "Only edit transactions can be omitted");
					address.Page[address.Index] = TransactionType.Omit;
				}
				this.CommittedVersion = v;
				this.editorThread = null;
				this.editor = null;
				LockFreeSync.WriteBarrier();
			}
			EventHandler handler = this.Committed;
			if(handler != null) {
				handler(this, EventArgs.Empty);
			}
			return v;
		}

		/// <summary>
		/// Reverts current transaction
		/// </summary>
		public void Rollback() {
			this.ValidateModification();
			RuntimeHelpers.PrepareConstrainedRegions();
			try { } finally {
				foreach(ISnapTable snapTable in this.Tables) {
					snapTable.Rollback();
				}
				ValueList<TransactionType>.Address address = this.version.ItemAddress(this.version.Count - 1);
				address.Page[address.Index] = TransactionType.Omit;
				this.editorThread = null;
				this.editor = null;
				LockFreeSync.WriteBarrier();
			}
		}

		/// <summary>
		/// Starts new transaction, performs undo and commits.
		/// If transaction was started successfully return new version of the store, otherwise return version of editor.
		/// </summary>
		/// <param name="storeSnapshot"></param>
		/// <returns></returns>
		public int Undo(StoreSnapshot storeSnapshot) {
			return this.Revert(storeSnapshot, TransactionType.Undo);
		}

		/// <summary>
		/// Starts new transaction, performs redo and commits.
		/// If transaction was started successfully return new version of the store, otherwise return version of editor.
		/// </summary>
		/// <param name="storeSnapshot"></param>
		/// <returns></returns>
		public int Redo(StoreSnapshot storeSnapshot) {
			return this.Revert(storeSnapshot, TransactionType.Redo);
		}

		/// <summary>
		/// Resets undo/redo history
		/// </summary>
		public void ResetUndoRedo() {
			this.ValidateModification();
			this.minUndo = this.Version;
		}

		/// <summary>
		/// Evaluates version that will be Undone or Redone depending on value of parameter.
		/// </summary>
		/// <param name="transactionType">Undo or Redo only</param>
		/// <returns>If version for reverting is found returns version number, otherwise returns 0.</returns>
		private int RevertVersion(TransactionType transactionType) {
			Debug.Assert(transactionType == TransactionType.Undo || transactionType == TransactionType.Redo);
			int max = this.CommittedVersion - 1;
			int min = this.minUndo;
			int level = 0;
			int threshold = (transactionType == TransactionType.Undo) ? 1 : -1;
			for(int i = max; min <= i; i--) {
				ValueList<TransactionType>.Address address = this.version.ItemAddress(i);
				Debug.Assert(Enum.IsDefined(typeof(TransactionType), address.Page[address.Index]), "Unexpected value");
				switch(address.Page[address.Index]) {
				case TransactionType.Edit:
					if(transactionType == TransactionType.Redo) {
						Debug.Assert(level == 0, "Number of Redo transactions exceeded number of correspondent Undo transactions.");
						return 0;
					}
					goto case TransactionType.Redo;
				case TransactionType.Redo:
					level++;
					break;
				case TransactionType.Undo:
					level--;
					break;
				}
				if(level == threshold) {
					return i + 1;
				}
			}
			return 0;
		}

		/// <summary>
		/// Performs undo or redo
		/// </summary>
		/// <param name="storeSnapshot"></param>
		/// <param name="isUndo"></param>
		/// <returns></returns>
		private int Revert(StoreSnapshot storeSnapshot, TransactionType transactionType) {
			Debug.Assert(transactionType == TransactionType.Undo || transactionType == TransactionType.Redo);
			int originalVersion = storeSnapshot.Version;
			int transactionToRevert = 0;
			try {
				transactionToRevert = this.StartTransaction(storeSnapshot, transactionType);
				if(0 < transactionToRevert) {
					foreach(ISnapTable snapTable in this.Tables) {
						snapTable.Revert(transactionToRevert);
					}
					return this.Commit(withOmit: false);
				}
			} catch {
				if(0 < transactionToRevert && storeSnapshot.IsEditor) {
					this.Rollback();
				}
				throw;
			}
			return originalVersion;
		}

		/// <summary>
		/// Checks if modifications can be made
		/// </summary>
		public void ValidateModification() {
			if(this.editor == null) {
				throw new InvalidOperationException(Properties.Resources.ErrorEditOutsideTransaction);
			}
			if(this.editorThread != Thread.CurrentThread) {
				throw new InvalidOperationException(Properties.Resources.ErrorEditOnWrongThread);
			}
			this.editor.CommitPrepared = false;
		}

		/// <summary>
		/// Checks that uncommitted changes are not enumerated from thread other then editorThread
		/// </summary>
		/// <param name="enumeratedVersion"></param>
		public void ValidateChangeEnumeration(int enumeratedVersion) {
			if(!(enumeratedVersion <= this.CommittedVersion || this.editor != null && this.editorThread == Thread.CurrentThread)) {
				throw new InvalidOperationException(Properties.Resources.ErrorEditOnWrongThread);
			}
		}

		/// <summary>
		/// Checks that provided version was rolled back. There is no way to distinguish between omit and rollback.
		/// </summary>
		/// <param name="enumeratedVersion"></param>
		public void ValidateRollbackEnumeration(int enumeratedVersion) {
			Debug.Assert(0 < enumeratedVersion, "Only real transaction can be rolled back");
			ValueList<TransactionType>.Address address = this.version.ItemAddress(enumeratedVersion - 1);
			if(address.Page[address.Index] != TransactionType.Omit) {
				throw new InvalidOperationException(Properties.Resources.ErrorRollbackEnumeration);
			}
		}
	}
}
