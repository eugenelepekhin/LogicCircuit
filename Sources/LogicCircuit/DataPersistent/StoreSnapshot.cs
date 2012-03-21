using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Snapshot of relational database.
	/// The store can be set to edit mode by opening transaction after this tables that belongs to the store will accept modifications.
	/// </summary>
	public partial class StoreSnapshot {

		/// <summary>
		/// Occurs after version of this store was changed
		/// </summary>
		public event EventHandler<VersionChangeEventArgs> VersionChanged;

		/// <summary>
		/// Occurs when latest available version has changed. This can happed when other store have modified and committed data
		/// </summary>
		public event EventHandler LatestVersionChanged;

		/// <summary>
		/// Occurred when transaction owned by this store rolled back.
		/// </summary>
		public event EventHandler<RolledBackEventArgs> RolledBack;

		/// <summary>
		/// Gets the actual store that holds the data
		/// </summary>
		internal SnapStore SnapStore { get; private set; }

		private int snapshotVersion;

		/// <summary>
		/// Gets or sets version of the snapshot.
		/// </summary>
		public int Version {
			get { return this.snapshotVersion; }
			set {
				if(this.SnapStore.Editor == this) {
					throw new InvalidOperationException(Properties.Resources.ErrorInvalidUpgrade);
				}
				this.SnapStore.CheckFrozen();
				if(value < 0 || this.SnapStore.CompletedVersion < value) {
					throw new ArgumentOutOfRangeException("value");
				}
				if(this.snapshotVersion != value) {
					this.Upgrade(value);
				}
			}
		}

		/// <summary>
		/// Gets latest available version this store can be upgraded to
		/// </summary>
		public int LatestAvailableVersion { get { return this.SnapStore.CompletedVersion; } }

		/// <summary>
		/// True if PrepareCommit is successful
		/// </summary>
		public bool CommitPrepared { get; internal set; }

		private Dictionary<ISnapTable, ITableSnapshot> table = new Dictionary<ISnapTable, ITableSnapshot>();
		private List<IPrimaryKeyHolder> primaryKeyHolder = new List<IPrimaryKeyHolder>();

		private StoreSnapshot(SnapStore storeData) {
			this.SnapStore = storeData;
			this.snapshotVersion = this.SnapStore.CompletedVersion;
			this.SnapStore.Committed += new EventHandler(this.StoreDataCommitted);
		}

		/// <summary>
		/// Builds new store with unique actual store that holds the data.
		/// This is the way to build new StoreSnapshot. Other snapshots of this store can be constructed with other constructor
		/// </summary>
		public StoreSnapshot() : this(new SnapStore()) {
		}

		/// <summary>
		/// Builds new snapshot looking the same data as a provided one
		/// </summary>
		/// <param name="store"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public StoreSnapshot(StoreSnapshot store) : this(store.SnapStore.CheckFrozen()) {
			foreach(ISnapTable snapTable in this.SnapStore.Tables) {
				if(snapTable.IsUserTable) {
					this.primaryKeyHolder.Add((IPrimaryKeyHolder)snapTable.CreateTableSnapshot(this));
				}
			}
		}

		/// <summary>
		/// Be nice and call Close when you done with this snapshot
		/// </summary>
		public void Close() {
			this.SnapStore.Committed -= new EventHandler(this.StoreDataCommitted);
			this.SnapStore = null;
			this.snapshotVersion = -1;
		}

		/// <summary>
		/// Gets table by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ITableSnapshot Table(string name) {
			ISnapTable snapTable = this.SnapStore.Table(name);
			if(snapTable != null && snapTable.IsUserTable) {
				return this.table[snapTable];
			}
			return null;
		}

		/// <summary>
		/// Gets snapshot of list of table
		/// </summary>
		public IEnumerable<ITableSnapshot> Tables {
			get { return this.table.Values; }
		}

		internal void Add(ITableSnapshot tableSnapshot, ISnapTable snapTable) {
			Debug.Assert(tableSnapshot.StoreSnapshot == this);
			Debug.Assert(snapTable.SnapStore == this.SnapStore);
			this.table.Add(snapTable, tableSnapshot);
			this.primaryKeyHolder.Add((IPrimaryKeyHolder)tableSnapshot);
		}

		/// <summary>
		/// Freezes shape of the store.
		/// </summary>
		public void FreezeShape() {
			this.SnapStore.FreezeShape();
		}

		/// <summary>
		/// Gets true if store is frozen for adding new tables and indexes.
		/// Transaction can be started only on frozen store.
		/// </summary>
		public bool IsFrozen { get { return this.SnapStore.IsFrozen; } }

		/// <summary>
		/// Upgrades this snapshot to latest available version
		/// </summary>
		public void Upgrade() {
			this.Version = this.LatestAvailableVersion;
		}

		/// <summary>
		/// Enumerates tables affected by transactions in range
		/// </summary>
		/// <param name="fromVersion"></param>
		/// <param name="toVersion"></param>
		/// <returns></returns>
		public IEnumerator<string> AffectedTables(int fromVersion, int toVersion) {
			int completed = this.SnapStore.CompletedVersion;
			if(!(0 <= fromVersion && fromVersion <= completed)) {
				throw new ArgumentOutOfRangeException("fromVersion");
			}
			if(!(fromVersion <= toVersion && toVersion <= completed)) {
				throw new ArgumentOutOfRangeException("toVersion");
			}
			return new ChangeEnumerator(this, fromVersion, toVersion);
		}

		/// <summary>
		/// Gets true if the store is currently owning a transaction
		/// </summary>
		public bool IsEditor { get { return this.SnapStore.Editor == this; } }

		/// <summary>
		/// Tries to starts transaction.
		/// The transaction will be started if no other store already started transaction.
		/// </summary>
		/// <returns>true if transaction has started</returns>
		public bool StartTransaction() {
			try {
				if(this.SnapStore.StartTransaction(this)) {
					Debug.Assert(this.Version < this.SnapStore.Version, "Invalid state");
					int oldVersion = this.Version;
					int newVersion = this.SnapStore.Version;
					if(oldVersion < newVersion - 1) {
						this.Upgrade(newVersion - 1);
					}
					this.snapshotVersion = this.SnapStore.Version;
					return true;
				}
				return false;
			} catch {
				if(this.SnapStore.Editor == this) {
					this.SnapStore.Rollback();
				}
				throw;
			}
		}

		/// <summary>
		/// Prepares commit. All foreign keys will be checked and exception will thrown on any violation.
		/// Commit will call this function if it wasn't call before.
		/// </summary>
		public void PrepareCommit() {
			this.ValidateModification();
			foreach(IPrimaryKeyHolder pk in this.primaryKeyHolder) {
				foreach(IForeignKey fk in pk.Children) {
					fk.Validate();
				}
			}
			this.CommitPrepared = true;
		}

		private int Commit(bool withOmit) {
			this.ValidateModification();
			if(!this.CommitPrepared) {
				this.PrepareCommit();
				Debug.Assert(this.CommitPrepared);
			}
			int commitedVersion = this.SnapStore.Commit(withOmit);
			//TODO: compensate for exception get thrown during commit?
			this.NotifyVersion(commitedVersion - 1, commitedVersion);
			return this.Version;
		}

		/// <summary>
		/// Commits transaction
		/// </summary>
		/// <returns>Committed version</returns>
		public int Commit() {
			return this.Commit(withOmit: false);
		}

		/// <summary>
		/// Commits transaction but erases it from undo history
		/// </summary>
		/// <returns></returns>
		public int Omit() {
			return this.Commit(withOmit: true);
		}

		/// <summary>
		/// Rolls current transaction back
		/// </summary>
		public void Rollback() {
			this.ValidateModification();
			int version = this.Version;
			this.SnapStore.Rollback();
			//TODO: compensate for exception get thrown during Rollback?
			EventHandler<RolledBackEventArgs> handler = this.RolledBack;
			if(handler != null) {
				handler(this, new RolledBackEventArgs(version));
			}
			this.NotifyVersion(version - 1, version);
		}

		/// <summary>
		/// Returns true if undo can be applied to the data
		/// </summary>
		public bool CanUndo { get { return this.SnapStore.CanUndo; } }

		/// <summary>
		/// Returns true if previous undo can be redone
		/// </summary>
		public bool CanRedo { get { return this.SnapStore.CanRedo; } }

		/// <summary>
		/// Undoes one transaction
		/// </summary>
		/// <returns></returns>
		public bool Undo() {
			int version = this.SnapStore.Undo(this);
			if(version != this.Version) {
				this.Upgrade(version);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Redoes previously undone transaction
		/// </summary>
		/// <returns></returns>
		public bool Redo() {
			int version = this.SnapStore.Redo(this);
			if(version != this.Version) {
				this.Upgrade(version);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clean up undo/redo history
		/// </summary>
		public void ResetUndoRedo() {
			this.ValidateModification();
			this.SnapStore.ResetUndoRedo();
		}

		/// <summary>
		/// Listening data store for commits
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StoreDataCommitted(object sender, EventArgs e) {
			EventHandler handler = this.LatestVersionChanged;
			if(handler != null) {
				handler(this, e);
			}
		}

		/// <summary>
		/// Notifies version changes
		/// </summary>
		/// <param name="oldVersion"></param>
		/// <param name="newVersion"></param>
		private void NotifyVersion(int oldVersion, int newVersion) {
			EventHandler<VersionChangeEventArgs> handler = this.VersionChanged;
			if(handler != null) {
				handler(this, new VersionChangeEventArgs(oldVersion, newVersion));
			}
		}

		/// <summary>
		/// Upgrades store to provided version
		/// </summary>
		/// <param name="newVersion"></param>
		private void Upgrade(int newVersion) {
			this.SnapStore.CheckFrozen();
			Debug.Assert(0 <= newVersion && newVersion <= this.SnapStore.CompletedVersion, "Incorrect version");
			int oldVersion = this.Version;
			this.snapshotVersion = newVersion;
			this.NotifyVersion(oldVersion, newVersion);
		}

		private void ValidateModification() {
			if(this.SnapStore.Editor != this) {
				throw new InvalidOperationException(Properties.Resources.ErrorEditOutsideTransaction);
			}
		}

		private class ChangeEnumerator : IEnumerator<string> {
			private IEnumerator<ITableSnapshot> enumerator;
			private int oldVersion;
			private int newVersion;

			public ChangeEnumerator(StoreSnapshot store, int oldVersion, int newVersion) {
				Debug.Assert(0 < oldVersion && oldVersion <= newVersion && newVersion <= store.SnapStore.CompletedVersion);
				this.enumerator = store.Tables.GetEnumerator();
				this.oldVersion = oldVersion;
				this.newVersion = newVersion;
			}

			public bool MoveNext() {
				while(this.enumerator.MoveNext()) {
					if(enumerator.Current.WasChanged(this.oldVersion, this.newVersion)) {
						return true;
					}
				}
				return false;
			}

			public string Current { get { return this.enumerator.Current.Name; } }

			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Dispose() {
			}

			public void Reset() {
				throw new NotSupportedException();
			}
		}
	}
}
