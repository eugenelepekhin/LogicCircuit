using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LogicCircuit.DataPersistent {

	/// <summary>
	/// Internal class not for public consumption.
	/// Table that allows transactional modifications and querying data at specific version.
	/// </summary>
	/// <typeparam name="TRecord">Type of record of the data</typeparam>
	internal sealed partial class SnapTable<TRecord> : ISnapTable where TRecord:struct {

		/// <summary>
		/// One record of the table. Stores latest version of the data as well as index of previous version in the log.
		/// </summary>
		private struct Row {
			/// <summary>
			/// Current data.
			/// </summary>
			public TRecord Data;

			/// <summary>
			/// index of previous version of the data and IsDeleted bit.
			/// </summary>
			private volatile int index;

			/// <summary>
			/// Index of previous version of the data in the log.
			/// </summary>
			public int LogIndex {
				get {
					// cache index here for correctness of conditional operator
					int i = this.index;
					return (0 <= i) ? i : ~i;
				}
				set {
					Debug.Assert(0 < value, "LogIndex must be positive number");
					this.index = (0 <= this.index) ? value : ~value;
				}
			}

			/// <summary>
			/// True if the record is deleted
			/// </summary>
			public bool IsDeleted {
				get { return this.index < 0; }
				set {
					// no threading problems here as only one thread can change index
					if((this.index < 0) != value) {
						this.index = ~this.index;
					}
				}
			}

			/// <summary>
			/// Row is invalid when it was deleted in the same transaction it was created in which case ~index == -1
			/// </summary>
			public bool IsValid { get { return this.index != -1; } }

			/// <summary>
			/// Combination of log index and IsDeleted flag. Used for passing to the log. to avoid separate setting of index is IsDeleted flag
			/// </summary>
			public int RawLogIndex { get { return this.index; } }
		}

		/// <summary>
		/// Record of previous value of the row
		/// </summary>
		private struct Log {
			/// <summary>
			/// Previous data
			/// </summary>
			public TRecord Data;

			/// <summary>
			/// id of the row in the table
			/// </summary>
			public RowId RowId;

			/// <summary>
			/// index of previous version of the data and IsDeleted bit.
			/// </summary>
			private int index;

			/// <summary>
			/// Combination of log index and IsDeleted flag. Used for getting from Row to avoid separate setting of index is IsDeleted flag
			/// </summary>
			public int RawLogIndex {
				set { this.index = value; }
				#if UnitTestInternal
					get { return this.index; }
				#endif
			}

			/// <summary>
			/// Index of previous version of the data in the log.
			/// this logic should be similar to Row but no threading issues as this is immutable
			/// </summary>
			public int LogIndex {
				get {
					int i = this.index;
					return (0 <= i) ? i : ~i;
				}
			}

			/// <summary>
			/// True if the record is deleted
			/// </summary>
			public bool IsDeleted {
				get { return this.index < 0; }
			}
		}

		/// <summary>
		/// Snap of the state of the table at the end of transaction
		/// </summary>
		private struct Snap {
			/// <summary>
			/// Transaction number
			/// </summary>
			public int Version;

			/// <summary>
			/// Size of the table
			/// </summary>
			public int TableSize;

			/// <summary>
			/// Size of the log
			/// </summary>
			public int LogSize;
		}

		// the SnapTable consist of three lists:
		// 1. table - contains one entry for any row ever been inserted in the table
		private readonly ValueList<Row> table = new ValueList<Row>();
		// 2. log - contains one entry for any modification ever been made for row that was previously committed
		private readonly ValueList<Log> log = new ValueList<Log>();
		// 3. snap - contains one entry for any transaction ever modified the table
		private readonly ValueList<Snap> snap = new ValueList<Snap>();

		// NOTE! As a general rule of thumb. In all modification methods lets change table and/or log first and snap second.
		// When reading start walking table/log starting from current snap value. This will guarantee correctness of reading.

		/// <summary>
		/// Gets the store this table belongs to
		/// </summary>
		public SnapStore SnapStore { get; private set; }

		/// <summary>
		/// Name of the table. Should be unique in the store
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Fields of the table.
		/// </summary>
		public IField<TRecord>[] Fields { get; private set; }

		/// <summary>
		/// List of indexes each field should modify
		/// </summary>
		public List<IIndex<TRecord>>[] Indexes { get; private set; }

		/// <summary>
		/// Gets or sets Primary Key of the table
		/// </summary>
		public IIndex<TRecord> PrimaryKey { get; set; }

		/// <summary>
		/// List of foreign key for each column.
		/// </summary>
		public IForeignKey[] ForeignKeys { get; private set; }

		/// <summary>
		/// List of Foreign keys of children tables
		/// </summary>
		public List<IForeignKey> Children { get; private set; }

		/// <summary>
		/// True if the table was created as a back end of TableSnapshot.
		/// </summary>
		public bool IsUserTable { get; private set; }

		/// <summary>
		/// Constructor of SnapTable.
		/// </summary>
		/// <param name="store"></param>
		/// <param name="name"></param>
		/// <param name="initialSize"></param>
		/// <param name="fields"></param>
		/// <param name="isUserTable"></param>
		public SnapTable(SnapStore store, string name, int initialSize, IField<TRecord>[] fields, bool isUserTable) {
			if(!(0 <= initialSize && initialSize < int.MaxValue - 1)) {
				throw new ArgumentOutOfRangeException("initialSize");
			}
			this.SnapStore = store;
			this.Name = name;

			//this will reserve a special value to allow updating the nodes
			Log record = new Log();
			this.log.Add(ref record);

			if(0 < initialSize) {
				Row row = new Row();
				for(int i = 0; i < initialSize; i++) {
					this.table.Add(ref row);
				}
			}

			//set initial sizes of the lists
			Snap point = new Snap() {
				//Version = 0,
				TableSize = initialSize,
				LogSize = 1
			};
			this.snap.Add(ref point);

			IField<TRecord>[] list = (IField<TRecord>[])fields.Clone();
			for(int i = 0; i < list.Length; i++) {
				Debug.Assert(list[i].Order == 0 || list[i].Order == i);
				list[i].Order = i;
			}
			this.Fields = list;

			this.IsUserTable = isUserTable;
			if(this.IsUserTable) {
				this.Indexes = new List<IIndex<TRecord>>[list.Length];
				this.ForeignKeys = new IForeignKey[list.Length];
				this.Children = new List<IForeignKey>();
			}

			this.SnapStore.Add(this);
		}

		/// <summary>
		/// Create TableSnapshot base on the TRecord.
		/// </summary>
		/// <param name="storeSnapshot"></param>
		/// <returns></returns>
		public ITableSnapshot CreateTableSnapshot(StoreSnapshot storeSnapshot) {
			Debug.Assert(this.IsUserTable);
			return new TableSnapshot<TRecord>(storeSnapshot, this);
		}

		/// <summary>
		/// Inserts new row in the table
		/// </summary>
		/// <param name="data">data to be inserted</param>
		/// <returns>index of the new row</returns>
		public RowId Insert(ref TRecord data) {
			this.ValidateModification();
			if(int.MaxValue - 1 <= this.table.Count) {
				throw new InvalidOperationException(Properties.Resources.ErrorTableTooBig(this.Name));
			}
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(this.snap.Count - 1);
			bool firstChange = (snapAddress.Page[snapAddress.Index].Version < this.SnapStore.Version);
			Debug.Assert(firstChange || snapAddress.Page[snapAddress.Index].Version == this.SnapStore.Version, "Impossible state: this should be the current transaction");
			Debug.Assert(firstChange || snapAddress.Page[snapAddress.Index].TableSize == this.table.Count, "Impossible state: wrong table size");
			Debug.Assert(firstChange || snapAddress.Page[snapAddress.Index].LogSize == this.log.Count, "Impossible state: wrong log size");
			this.table.PrepareAdd();
			if(firstChange) {
				this.snap.PrepareAdd();
			}
			RowId rowId;
			RuntimeHelpers.PrepareConstrainedRegions();
			try {} finally {
				rowId = new RowId(this.table.FixedAllocate());
				ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
				rowAddress.Page[rowAddress.Index].Data = data;
				if(firstChange) {
					// this is the first change in this transaction
					Snap point = new Snap() {
						Version = this.SnapStore.Version,
						TableSize = this.table.Count,
						LogSize = this.log.Count
					};
					this.snap.FixedAdd(ref point);
				} else {
					// this transaction already altered this table
					snapAddress.Page[snapAddress.Index].TableSize = this.table.Count;
				}
			}
			return rowId;
		}

		/// <summary>
		/// Deletes row.
		/// </summary>
		/// <param name="rowId"></param>
		public void Delete(RowId rowId) {
			Debug.Assert(0 <= rowId.Value && rowId.Value < this.table.Count, "broken rowId");
			this.ValidateModification();
			ValueList<Row>.Address address = this.table.ItemAddress(rowId.Value);
			SnapTable<TRecord>.ValidateModification(ref address);
			this.PushToLog(ref address.Page[address.Index], rowId);
			// if the row was inserted in this transaction, then after deletion it will be invalid. so just ignore it in all future operations
			address.Page[address.Index].IsDeleted = true;
		}

		/// <summary>
		/// Undelete previously deleted row.
		/// </summary>
		/// <param name="rowId"></param>
		public void UnDelete(RowId rowId) {
			Debug.Assert(0 <= rowId.Value && rowId.Value < this.table.Count, "broken rowId");
			this.ValidateModification();
			ValueList<Row>.Address address = this.table.ItemAddress(rowId.Value);
			if(!address.Page[address.Index].IsDeleted) {
				throw new InvalidOperationException(Properties.Resources.ErrorUndeleteRow);
			}
			this.PushToLog(ref address.Page[address.Index], rowId);
			address.Page[address.Index].IsDeleted = false;
		}

		/// <summary>
		/// Checks if row was deleted in the provided version
		/// </summary>
		/// <param name="rowId"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool IsDeleted(RowId rowId, int version, bool includeNotCreated) {
			if(version != 0) {
				this.ValidateVersion(version);
			}
			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			if(!(0 <= rowId.Value && rowId.Value < snapAddress.Page[snapAddress.Index].TableSize)) {
				if(includeNotCreated && 0 <= rowId.Value && rowId.Value < this.table.Count) {
					return true;
				}
				throw new ArgumentOutOfRangeException("rowId");
			}
			// cache log size here. the original can only grow in time
			int logSize = snapAddress.Page[snapAddress.Index].LogSize;
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
				// so the latest version of data was requested.
				bool isDeleted = rowAddress.Page[rowAddress.Index].IsDeleted;
				LockFreeSync.ReadBarrier();
				// check if the row is still of the latest version
				if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
					// if it is still latest return.
					return isDeleted;
				}
			}
			// older version of the row requested. The data is in the log. Log is immutable so easy to read.
			ValueList<Log>.Address logAddress = this.log.ItemAddress(rowAddress.Page[rowAddress.Index].LogIndex);
			while(logSize <= logAddress.Page[logAddress.Index].LogIndex) {
				Debug.Assert(0 < logAddress.Page[logAddress.Index].LogIndex, "Log entry at 0 does not contain any real data and used as a stub");
				logAddress = this.log.ItemAddress(logAddress.Page[logAddress.Index].LogIndex);
			}
			return logAddress.Page[logAddress.Index].IsDeleted;
		}

		/// <summary>
		/// Warning! Do not expose this functionality to external user. This is for internal use only.
		/// Returns true if provided row is deleted in the latest version. This version can be not committed yet.
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public bool IsLatestDeleted(RowId rowId) {
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			return rowAddress.Page[rowAddress.Index].IsDeleted;
		}

		/// <summary>
		/// Gets number of rows in the table including ones marked as deleted.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public int Count(int version) {
			if(version != 0) {
				this.ValidateVersion(version);
			}
			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			return snapAddress.Page[snapAddress.Index].TableSize;
		}

		/// <summary>
		/// Warning! Do not expose this functionality to external user. This is for internal use only.
		/// Returns number of records in the table in the latest version. This version can be not committed yet.
		/// </summary>
		/// <returns></returns>
		public int LatestCount() {
			return this.table.Count;
		}

		/// <summary>
		/// Updates field of the row
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="rowId"></param>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool SetField<TField>(RowId rowId, IField<TRecord, TField> field, TField value) {
			Debug.Assert(0 <= rowId.Value && rowId.Value < this.table.Count, "broken rowId");
			this.ValidateModification();
			// It is only possible to set value via basic fields defined on table.
			this.ValidateField(field);
			ValueList<Row>.Address address = this.table.ItemAddress(rowId.Value);
			SnapTable<TRecord>.ValidateModification(ref address);
			if(field.Compare(field.GetValue(ref address.Page[address.Index].Data), value) != 0) {
				this.PushToLog(ref address.Page[address.Index], rowId);
				field.SetValue(ref address.Page[address.Index].Data, value);
				Debug.Assert(field.Compare(field.GetValue(ref address.Page[address.Index].Data), value) == 0, "Assignment or comparison failed");
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the value of the field in the specified version
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="rowId"></param>
		/// <param name="field"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public TField GetField<TField>(RowId rowId, IField<TRecord, TField> field, int version) {
			if(version != 0) {
				this.ValidateVersion(version);
			}
			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			if(!(0 <= rowId.Value && rowId.Value < snapAddress.Page[snapAddress.Index].TableSize)) {
				throw new ArgumentOutOfRangeException("rowId");
			}
			// cache log size here. the original can only grow in time
			int logSize = snapAddress.Page[snapAddress.Index].LogSize;
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			TField value;
			if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
				// so the latest version of data was requested.
				value = field.GetValue(ref rowAddress.Page[rowAddress.Index].Data);
				bool isDeleted = rowAddress.Page[rowAddress.Index].IsDeleted;
				LockFreeSync.ReadBarrier();
				// check if the row is still of the latest version
				if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
					// if it is still latest return.
					if(isDeleted) {
						throw new ArgumentOutOfRangeException("rowId");
					}
					return value;
				}
			}
			// older version of the row requested. The data is in the log. Log is immutable so easy to read.
			ValueList<Log>.Address logAddress = this.log.ItemAddress(rowAddress.Page[rowAddress.Index].LogIndex);
			while(logSize <= logAddress.Page[logAddress.Index].LogIndex) {
				Debug.Assert(0 < logAddress.Page[logAddress.Index].LogIndex, "Log entry at 0 does not contain any real data and used as a stub");
				logAddress = this.log.ItemAddress(logAddress.Page[logAddress.Index].LogIndex);
			}
			if(logAddress.Page[logAddress.Index].IsDeleted) {
				throw new ArgumentOutOfRangeException("rowId");
			}
			return field.GetValue(ref logAddress.Page[logAddress.Index].Data);
		}

		/// <summary>
		/// Warning! Do not expose this functionality to external user. This is for internal use only.
		/// Returns value of the field of the record in the table in the latest version even if the record is deleted. This version can be not committed yet.
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="rowId"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField GetLatestField<TField>(RowId rowId, IField<TRecord, TField> field) {
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			return field.GetValue(ref rowAddress.Page[rowAddress.Index].Data);
		}

		/// <summary>
		/// Gets record in the specified version
		/// </summary>
		/// <param name="rowId"></param>
		/// <param name="version"></param>
		/// <param name="data"></param>
		public void GetData(RowId rowId, int version, out TRecord data) {
			if(version != 0) {
				this.ValidateVersion(version);
			}
			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			if(!(0 <= rowId.Value && rowId.Value < snapAddress.Page[snapAddress.Index].TableSize)) {
				throw new ArgumentOutOfRangeException("rowId");
			}
			// cache log size here. the original can only grow in time
			int logSize = snapAddress.Page[snapAddress.Index].LogSize;
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
				// so the latest version of data was requested.
				data = rowAddress.Page[rowAddress.Index].Data;
				bool isDeleted = rowAddress.Page[rowAddress.Index].IsDeleted;
				LockFreeSync.ReadBarrier();
				// check if the row is still of the latest version
				if(rowAddress.Page[rowAddress.Index].LogIndex < logSize) {
					// if it is still latest return.
					if(isDeleted) {
						data = default(TRecord);
						throw new ArgumentOutOfRangeException("rowId");
					}
					return;
				}
			}
			// older version of the row requested. The data is in the log. Log is immutable so easy to read.
			ValueList<Log>.Address logAddress = this.log.ItemAddress(rowAddress.Page[rowAddress.Index].LogIndex);
			while(logSize <= logAddress.Page[logAddress.Index].LogIndex) {
				Debug.Assert(0 < logAddress.Page[logAddress.Index].LogIndex, "Log entry at 0 does not contain any real data and used as a stub");
				logAddress = this.log.ItemAddress(logAddress.Page[logAddress.Index].LogIndex);
			}
			if(logAddress.Page[logAddress.Index].IsDeleted) {
				data = default(TRecord);
				throw new ArgumentOutOfRangeException("rowId");
			}
			data = logAddress.Page[logAddress.Index].Data;
		}

		/// <summary>
		/// Warning! Do not expose this functionality to external user. This is for internal use only.
		/// Gets the entire record in the table in the latest version even if the record is deleted. This version can be not committed yet.
		/// </summary>
		/// <param name="rowId"></param>
		/// <param name="data"></param>
		/// <returns>True if the record is deleted and False otherwise.</returns>
		public bool GetLatestData(RowId rowId, out TRecord data) {
			ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
			data = rowAddress.Page[rowAddress.Index].Data;
			return rowAddress.Page[rowAddress.Index].IsDeleted;
		}

		/// <summary>
		/// Sets entire structure.
		/// </summary>
		/// <param name="rowId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool SetData(RowId rowId, ref TRecord data) {
			Debug.Assert(0 <= rowId.Value && rowId.Value < this.table.Count, "broken rowId");
			this.ValidateModification();
			ValueList<Row>.Address address = this.table.ItemAddress(rowId.Value);
			SnapTable<TRecord>.ValidateModification(ref address);
			if(TableSnapshot<TRecord>.Compare(this.Fields, ref address.Page[address.Index].Data, ref data) != 0) {
				this.PushToLog(ref address.Page[address.Index], rowId);
				address.Page[address.Index].Data = data;
				Debug.Assert(TableSnapshot<TRecord>.Compare(this.Fields, ref address.Page[address.Index].Data, ref data) == 0, "Assignment or comparison failed");
				return true;
			}
			return false;
		}

		/// <summary>
		/// Rolls back current transaction
		/// </summary>
		public void Rollback() {
			this.ValidateModification();
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(this.snap.Count - 1);
			if(snapAddress.Page[snapAddress.Index].Version == this.SnapStore.Version) {
				Debug.Assert(1 < this.snap.Count, "Only real transactions can be rolled back");
				// this table was modified in this transaction
				// it is impossible to completely rollback current changes, so just get old data back and delete new rows and undelete deleted ones.
				int tableEnd = snapAddress.Page[snapAddress.Index].TableSize;
				int logEnd = snapAddress.Page[snapAddress.Index].LogSize;
				if(tableEnd != this.table.Count || logEnd != this.log.Count) {
					// if rolling back due to exception then ensure snap showing right data.
					snapAddress.Page[snapAddress.Index].TableSize = tableEnd = this.table.Count;
					snapAddress.Page[snapAddress.Index].LogSize = logEnd = this.log.Count;
				}
				snapAddress = this.snap.ItemAddress(this.snap.Count - 2);
				int tableStart = snapAddress.Page[snapAddress.Index].TableSize;
				int logStart = snapAddress.Page[snapAddress.Index].LogSize;
				for(int i = tableStart; i < tableEnd; i++) {
					ValueList<Row>.Address rowAddress = this.table.ItemAddress(i);
					rowAddress.Page[rowAddress.Index].IsDeleted = true;
					// Row must be freshly inserted but can be deleted in the same transaction, so check here that it is invalid now.
					Debug.Assert(!rowAddress.Page[rowAddress.Index].IsValid, "Invalid state: row should be freshly inserted one");
				}
				for(int i = logStart; i < logEnd; i++) {
					ValueList<Log>.Address logAddress = this.log.ItemAddress(i);
					RowId rowId = logAddress.Page[logAddress.Index].RowId;
					Debug.Assert(rowId.Value < tableStart, "Invalid rowId: changes are only possible to rows that already in the table, not just inserted ones");
					ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
					rowAddress.Page[rowAddress.Index].Data = logAddress.Page[logAddress.Index].Data;
					rowAddress.Page[rowAddress.Index].IsDeleted = logAddress.Page[logAddress.Index].IsDeleted;
				}
			}
		}

		/// <summary>
		/// Reverts changes made in the provided transaction. Intended to be used in undo/redo.
		/// Warning! Assumes that higher stack functionality is responsible for providing correct version number,
		/// so there no gaps between versions in undo/redo operations
		/// </summary>
		/// <param name="version">Transaction to be reverted</param>
		public void Revert(int version) {
			this.ValidateModification();
			this.ValidateVersion(version);

			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			if(version == snapAddress.Page[snapAddress.Index].Version) {
				int tableEnd = snapAddress.Page[snapAddress.Index].TableSize;
				int logEnd = snapAddress.Page[snapAddress.Index].LogSize;
				snapAddress = this.snap.ItemAddress(--pointIndex);
				int tableStart = snapAddress.Page[snapAddress.Index].TableSize;
				int logStart = snapAddress.Page[snapAddress.Index].LogSize;
				for(int i = tableStart; i < tableEnd; i++) {
					ValueList<Row>.Address rowAddress = this.table.ItemAddress(i);
					if(rowAddress.Page[rowAddress.Index].IsValid) {
						this.PushToLog(ref rowAddress.Page[rowAddress.Index], new RowId(i));
						rowAddress.Page[rowAddress.Index].IsDeleted = true;
						Debug.Assert(rowAddress.Page[rowAddress.Index].IsValid, "Invalid state: row with a history of changes can't be invalid");
					}
				}
				for(int i = logStart; i < logEnd; i++) {
					ValueList<Log>.Address logAddress = this.log.ItemAddress(i);
					RowId rowId = logAddress.Page[logAddress.Index].RowId;
					Debug.Assert(rowId.Value < tableStart, "Invalid rowId: changes are only possible to rows that already in the table, not just inserted ones");
					ValueList<Row>.Address rowAddress = this.table.ItemAddress(rowId.Value);
					this.PushToLog(ref rowAddress.Page[rowAddress.Index], rowId);
					rowAddress.Page[rowAddress.Index].Data = logAddress.Page[logAddress.Index].Data;
					rowAddress.Page[rowAddress.Index].IsDeleted = logAddress.Page[logAddress.Index].IsDeleted;
				}
			}
		}

		/// <summary>
		/// Checks if the table was modified at the specified transaction
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool WasChangedIn(int version) {
			if(version == 0) {
				return false;
			}
			this.ValidateVersion(version);

			int pointIndex = this.snap.Count - 1;
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(pointIndex);
			while(version < snapAddress.Page[snapAddress.Index].Version) {
				snapAddress = this.snap.ItemAddress(--pointIndex);
			}
			return version == snapAddress.Page[snapAddress.Index].Version;
		}

		/// <summary>
		/// Returns enumerator of changes made in the specified transaction
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public IEnumerator<SnapTableChange<TRecord>> GetChanges(int version) {
			this.SnapStore.ValidateChangeEnumeration(version);
			return this.GetChanges(version, false);
		}

		/// <summary>
		/// Returns enumerator of changes made and rolled back in the specified transaction
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public IEnumerator<SnapTableChange<TRecord>> GetRolledBackChanges(int version) {
			this.ValidateVersion(version); // ensure version != 0
			this.SnapStore.ValidateRollbackEnumeration(version);
			return this.GetChanges(version, true);
		}

		private IEnumerator<SnapTableChange<TRecord>> GetChanges(int version, bool forRollback) {
			if(version != 0) {
				this.ValidateVersion(version);
			}
			return new ChangeEnumerator(this, version, forRollback);
		}

		/// <summary>
		/// Checks if modifications are allowed i.e. transaction is open
		/// </summary>
		private void ValidateModification() {
			this.SnapStore.ValidateModification();
		}

		/// <summary>
		/// Checks if row at the provided address was not deleted
		/// </summary>
		/// <param name="address"></param>
		private static void ValidateModification(ref ValueList<Row>.Address address) {
			if(address.Page[address.Index].IsDeleted) {
				throw new ArgumentOutOfRangeException("address");
			}
		}

		private void ValidateVersion(int version) {
			if(!(0 < version && version <= this.SnapStore.Version)) {
				throw new ArgumentOutOfRangeException("version");
			}
		}

		internal void ValidateField(IField<TRecord> field) {
			if(this.Fields[field.Order] != field) {
				throw new ArgumentOutOfRangeException("field");
			}
		}

		/// <summary>
		/// Pushes row to log. Row can be logged only once in transaction, so only initial state (as it was at the beginning of transaction) will be saved
		/// </summary>
		/// <param name="row">Reference to the data to be logged</param>
		/// <param name="rowId">Id of this data. This should be the Id of row provided in the first param.</param>
		private void PushToLog(ref Row row, RowId rowId) {
			// ref to row should be in table at rowId. It is impossible to check this in C#, so just check if bits are equal
			Debug.Assert(
				0 == TableSnapshot<TRecord>.Compare(
					this.Fields, ref row.Data, ref this.table.ItemAddress(rowId.Value).Page[this.table.ItemAddress(rowId.Value).Index].Data
				),
				"ref to row should be in table at rowId"
			);
			ValueList<Snap>.Address snapAddress = this.snap.ItemAddress(this.snap.Count - 1);
			if(snapAddress.Page[snapAddress.Index].Version < this.SnapStore.Version) {
				// first change in this transaction so the row is older
				Debug.Assert(rowId.Value < snapAddress.Page[snapAddress.Index].TableSize, "Row should be already in the table");
				this.snap.PrepareAdd();
				this.log.PrepareAdd();
				Snap point = new Snap() {
					Version = this.SnapStore.Version,
					TableSize = this.table.Count,
					LogSize = this.log.Count + 1
				};
				RuntimeHelpers.PrepareConstrainedRegions();
				try {} finally {
					int index = this.log.FixedAllocate();
					ValueList<Log>.Address logAddress = this.log.ItemAddress(index);
					logAddress.Page[logAddress.Index].Data = row.Data;
					logAddress.Page[logAddress.Index].RowId = rowId;
					logAddress.Page[logAddress.Index].RawLogIndex = row.RawLogIndex;
					row.LogIndex = index;
					this.snap.FixedAdd(ref point);
				}
			} else {
				Debug.Assert(snapAddress.Page[snapAddress.Index].Version == this.SnapStore.Version, "Impossible state: this should be the current transaction");
				Debug.Assert(snapAddress.Page[snapAddress.Index].TableSize == this.table.Count, "Impossible state: wrong table size");
				Debug.Assert(snapAddress.Page[snapAddress.Index].LogSize == this.log.Count, "Impossible state: wrong log size");
				// some changes were made in this transaction. Check if the row was already modified in this transaction.
				// get size of log in the previous transaction
				ValueList<Snap>.Address oldSnapAddress = this.snap.ItemAddress(this.snap.Count - 2);
				if(rowId.Value < oldSnapAddress.Page[oldSnapAddress.Index].TableSize && row.LogIndex < oldSnapAddress.Page[oldSnapAddress.Index].LogSize) {
					// this is the first time the row is updated in this transaction
					Debug.Assert(snapAddress.Page[snapAddress.Index].LogSize == this.log.Count, "Invalid state: wrong log size");
					this.log.PrepareAdd();
					RuntimeHelpers.PrepareConstrainedRegions();
					try {} finally {
						int index = this.log.FixedAllocate();
						ValueList<Log>.Address logAddress = this.log.ItemAddress(index);
						logAddress.Page[logAddress.Index].Data = row.Data;
						logAddress.Page[logAddress.Index].RowId = rowId;
						logAddress.Page[logAddress.Index].RawLogIndex = row.RawLogIndex;
						row.LogIndex = index;
						snapAddress.Page[snapAddress.Index].LogSize = this.log.Count;
					}
				}
			}
		}

		/// <summary>
		/// Enumerator of changes made in specified transaction
		/// </summary>
		private class ChangeEnumerator : IEnumerator<SnapTableChange<TRecord>>, ISnapTableChange<TRecord> {

			private readonly SnapTable<TRecord> table;
			private readonly Snap newVersion;
			private readonly Snap oldVersion;
			private readonly bool forRollback;
			private int changeIndex;

			public ChangeEnumerator(SnapTable<TRecord> table, int version, bool forRollback) {
				this.table = table;
				this.forRollback = forRollback;
				int pointIndex = this.table.snap.Count - 1;
				ValueList<Snap>.Address snapAddress = this.table.snap.ItemAddress(pointIndex);
				while(version < snapAddress.Page[snapAddress.Index].Version) {
					snapAddress = this.table.snap.ItemAddress(--pointIndex);
				}
				if(version != snapAddress.Page[snapAddress.Index].Version) {
					throw new ArgumentOutOfRangeException("version");
				}
				this.newVersion = snapAddress.Page[snapAddress.Index];
				snapAddress = this.table.snap.ItemAddress(pointIndex - 1);
				this.oldVersion = snapAddress.Page[snapAddress.Index];
				this.changeIndex = -1;
			}

			public SnapTableChange<TRecord> Current {
				get {
					if(0 <= this.changeIndex && this.changeIndex < this.ChangeCount()) {
						return new SnapTableChange<TRecord>(this, this.changeIndex);
					} else {
						throw new InvalidOperationException(Properties.Resources.ErrorEnumeratorPosition);
					}
				}
			}

			object IEnumerator.Current { get { return this.Current; } }

			public bool MoveNext() {
				this.changeIndex = Math.Min(this.changeIndex + 1, this.ChangeCount());
				while(0 <= this.changeIndex && this.changeIndex < this.ChangeCount()) {
					if(!this.forRollback && this.changeIndex < this.InsertCount()) {
						ValueList<Row>.Address rowAddress = this.table.table.ItemAddress(this.changeIndex + this.oldVersion.TableSize);
						if(!rowAddress.Page[rowAddress.Index].IsValid) {
							this.changeIndex++;
							continue;
						}
					}
					return true;
				}
				return false;
			}

			public void Dispose() {
			}

			public void Reset() {
				throw new NotSupportedException();
			}

			// Let assume changes are enumerated in the following way: insertions first and all other changes next

			/// <summary>
			/// Returns id of modified row
			/// </summary>
			/// <param name="change"></param>
			/// <returns></returns>
			public RowId RowId(int change) {
				if(change < this.InsertCount()) {
					return new RowId(change + this.oldVersion.TableSize);
				} else {
					ValueList<Log>.Address logAddress = this.table.log.ItemAddress(this.oldVersion.LogSize + change - this.InsertCount());
					return logAddress.Page[logAddress.Index].RowId;
				}
			}

			/// <summary>
			/// Returns action made to the row during the transaction
			/// </summary>
			/// <param name="change"></param>
			/// <returns></returns>
			public SnapTableAction Action(int change) {
				if(change < this.InsertCount()) {
					Debug.Assert(this.forRollback || !this.table.IsDeleted(this.RowId(change), this.newVersion.Version, false),
						"This row is invalid and should not be enumerated as a change"
					);
					return SnapTableAction.Insert;
				} else {
					RowId rowId = this.RowId(change);
					bool oldDeleted = this.table.IsDeleted(rowId, this.oldVersion.Version, false);
					bool newDeleted = this.table.IsDeleted(rowId, this.newVersion.Version, false);
					if(oldDeleted != newDeleted) {
						return oldDeleted ? SnapTableAction.Insert : SnapTableAction.Delete;
					} else {
						Debug.Assert(!oldDeleted || this.forRollback, "Row must not be deleted in order to be modified");
						return SnapTableAction.Update;
					}
				}
			}

			public void GetNewData(int change, out TRecord data) {
				this.table.GetData(this.NewRow(change), this.newVersion.Version, out data);
			}

			public void GetOldData(int change, out TRecord data) {
				this.table.GetData(this.OldRow(change), this.oldVersion.Version, out data);
			}

			public TField GetNewField<TField>(int change, IField<TRecord, TField> field) {
				return this.table.GetField<TField>(this.NewRow(change), field, this.newVersion.Version);
			}

			public TField GetOldField<TField>(int change, IField<TRecord, TField> field) {
				return this.table.GetField<TField>(this.OldRow(change), field, this.oldVersion.Version);
			}

			private int InsertCount() {
				return this.newVersion.TableSize - this.oldVersion.TableSize;
			}

			private int ChangeCount() {
				return this.InsertCount() + this.newVersion.LogSize - this.oldVersion.LogSize;
			}

			private RowId NewRow(int change) {
				RowId rowId = this.RowId(change);
				if(this.table.IsDeleted(rowId, this.newVersion.Version, false)) {
					throw new InvalidOperationException(Properties.Resources.ErrorWrongNewData);
				}
				return rowId;
			}

			private RowId OldRow(int change) {
				if(this.Action(change) == SnapTableAction.Insert) {
					throw new InvalidOperationException(Properties.Resources.ErrorWrongOldRow);
				}
				return this.RowId(change);
			}
		}

		// Debugging visualize
		#if DEBUG
			public string DebuggingVisualization {
				get {
					System.Text.StringBuilder text = new System.Text.StringBuilder();
					text.AppendLine("Table:");
					text.AppendLine("_Data___________________________________________________________________________|_LogIndex_|_Deleted_|_Valid_|_RawLogIndex_");
					for(int i = 0; i < this.table.Count; i++) {
						ValueList<Row>.Address address = this.table.ItemAddress(i);
						string data = address.Page[address.Index].Data.ToString().Trim().Replace("\r\n", " ");
						text.AppendFormat(CultureInfo.InvariantCulture,
							" {0,78} | {1,8:D} |   {2}     |  {3}    | {4:X}",
							data.Substring(0, Math.Min(data.Length, 78)),
							address.Page[address.Index].LogIndex,
							address.Page[address.Index].IsDeleted ? "X" : " ",
							address.Page[address.Index].IsValid ? " " : "X",
							address.Page[address.Index].RawLogIndex
						);
						text.AppendLine();
					}
					text.AppendLine();
					text.AppendLine("Log:");
					text.AppendLine("_Data___________________________________________________________________________|_RowId____|_LogIndex_|_Deleted_");
					for(int i = 0; i < this.log.Count; i++) {
						ValueList<Log>.Address address = this.log.ItemAddress(i);
						string data = address.Page[address.Index].Data.ToString().Trim().Replace("\r\n", " ");
						text.AppendFormat(CultureInfo.InvariantCulture,
							" {0,78} | {1,8:D} | {2,8:D} |   {3}",
							data.Substring(0, Math.Min(data.Length, 78)),
							address.Page[address.Index].RowId.Value,
							address.Page[address.Index].LogIndex,
							address.Page[address.Index].IsDeleted ? "X" : " "
						);
						text.AppendLine();
					}
					text.AppendLine();
					text.AppendLine("Snap:");
					text.AppendLine("_Version_|_TableSize_|_LogSize_");
					for(int i = 0; i < this.snap.Count; i++) {
						ValueList<Snap>.Address address = this.snap.ItemAddress(i);
						text.AppendFormat(CultureInfo.InvariantCulture,
							" {0,7:D} | {1,9:D} | {2,7:D}",
							address.Page[address.Index].Version,
							address.Page[address.Index].TableSize,
							address.Page[address.Index].LogSize
						);
						text.AppendLine();
					}
					return text.ToString();
				}
			}
		#endif
	}
}
