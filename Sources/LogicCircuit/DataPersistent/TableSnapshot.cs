using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Represents snapshot of the table.
	/// If StoreSnapshot this table is belongs to is owning transaction then modification of this table is allowed.
	/// </summary>
	/// <typeparam name="TRecord">Defines shape of the table</typeparam>
	public sealed partial class TableSnapshot<TRecord> : ITableSnapshot, IPrimaryKeyHolder, IChildTable where TRecord:struct {

		/// <summary>
		/// Store this table belongs to
		/// </summary>
		public StoreSnapshot StoreSnapshot { get; private set; }

		/// <summary>
		/// Actual data of this table.
		/// </summary>
		private readonly SnapTable<TRecord> table;

		/// <summary>
		/// Gets name of this table
		/// </summary>
		public string Name { get { return this.table.Name; } }

		/// <summary>
		/// Gets fields of the table
		/// </summary>
		public IEnumerable<IField<TRecord>> Fields { get { return new ReadOnlyCollection<IField<TRecord>>(this.table.Fields); } }

		/// <summary>
		/// Gets enumerator of row id
		/// </summary>
		public IEnumerable<RowId> Rows { get { return this; } }

		/// <summary>
		/// Constructs the table
		/// </summary>
		/// <param name="store"></param>
		/// <param name="name"></param>
		/// <param name="fields"></param>
		public TableSnapshot(StoreSnapshot store, string name, params IField<TRecord>[] fields) {
			if(store == null) {
				throw new ArgumentNullException("store");
			}
			if(fields == null) {
				throw new ArgumentNullException("fields");
			}
			this.StoreSnapshot = store;
			this.table = new SnapTable<TRecord>(store.SnapStore, name, 0, fields, true);
			this.StoreSnapshot.Add(this, this.table);
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="storeSnapshot"></param>
		/// <param name="table"></param>
		internal TableSnapshot(StoreSnapshot storeSnapshot, SnapTable<TRecord> table) {
			this.StoreSnapshot = storeSnapshot;
			this.table = table;
			this.StoreSnapshot.Add(this, this.table);
		}

		/// <summary>
		/// Creates unique constraint for the provided field, optionally make it primary key
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="name"></param>
		/// <param name="field"></param>
		/// <param name="primaryKey"></param>
		public void MakeUnique<TField>(string name, IField<TRecord, TField> field, bool primaryKey) {
			if(primaryKey && this.table.PrimaryKey != null) {
				throw new InvalidOperationException(Properties.Resources.ErrorPrimaryKeyRedefinition(this.Name));
			}
			this.table.ValidateField(field);
			IIndex<TRecord> index = new UniqueIndex<TField>(this.table, name, field);
			List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
			if(list == null) {
				list = new List<IIndex<TRecord>>();
				this.table.Indexes[field.Order] = list;
			}
			list.Insert(0, index);
			if(primaryKey) {
				this.table.PrimaryKey = index;
			}
		}

		/// <summary>
		/// Creates primary key for pseudo column of <seealso cref="RowId"/>. This eliminates the necessity of having other surrogate primary key.
		/// </summary>
		public void MakeAutoUnique() {
			if(this.table.PrimaryKey != null) {
				throw new InvalidOperationException(Properties.Resources.ErrorPrimaryKeyRedefinition(this.Name));
			}
			this.StoreSnapshot.SnapStore.CheckNotFrozen();
			IIndex<TRecord> index = new UniquePseudoIndex(this.table);
			this.table.PrimaryKey = index;
		}

		/// <summary>
		/// Creates unique constraint for the provided field
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="name"></param>
		/// <param name="field"></param>
		public void MakeUnique<TField>(string name, IField<TRecord, TField> field) {
			this.MakeUnique<TField>(name, field, false);
		}

		/// <summary>
		/// Creates index
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="name"></param>
		/// <param name="field"></param>
		public void CreateIndex<TField>(string name, IField<TRecord, TField> field) {
			this.table.ValidateField(field);
			List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
			if(list == null) {
				list = new List<IIndex<TRecord>>();
				this.table.Indexes[field.Order] = list;
			}
			list.Add(new RangeIndex<TField>(this.table, name, field));
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TField1"></typeparam>
		/// <typeparam name="TField2"></typeparam>
		/// <param name="name"></param>
		/// <param name="field1"></param>
		/// <param name="field2"></param>
		/// <param name="unique"></param>
		private void CreateIndex<TField1, TField2>(string name, IField<TRecord, TField1> field1, IField<TRecord, TField2> field2, bool unique) {
			this.table.ValidateField(field1);
			this.table.ValidateField(field2);
			if(field1 == field2) {
				throw new ArgumentException(Properties.Resources.ErrorCompositeFields);
			}
			CompositeField<TField1, TField2> field = new CompositeField<TField1, TField2>(field1, field2);
			IFieldIndex<Composite<TField1, TField2>> index;
			if(unique) {
				index = new UniqueIndex<Composite<TField1, TField2>>(this.table, name, field);
			} else {
				index = new RangeIndex<Composite<TField1, TField2>>(this.table, name, field);
			}
			List<IIndex<TRecord>> list1 = this.table.Indexes[field1.Order];
			if(list1 == null) {
				list1 = new List<IIndex<TRecord>>();
				this.table.Indexes[field1.Order] = list1;
			}
			List<IIndex<TRecord>> list2 = this.table.Indexes[field2.Order];
			if(list2 == null) {
				list2 = new List<IIndex<TRecord>>();
				this.table.Indexes[field2.Order] = list2;
			}
			list1.Add(index);
			list2.Add(index);
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TField1"></typeparam>
		/// <typeparam name="TField2"></typeparam>
		/// <param name="name"></param>
		/// <param name="field1"></param>
		/// <param name="field2"></param>
		public void MakeUnique<TField1, TField2>(string name, IField<TRecord, TField1> field1, IField<TRecord, TField2> field2) {
			this.CreateIndex<TField1, TField2>(name, field1, field2, true);
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TField1"></typeparam>
		/// <typeparam name="TField2"></typeparam>
		/// <param name="name"></param>
		/// <param name="field1"></param>
		/// <param name="field2"></param>
		public void CreateIndex<TField1, TField2>(string name, IField<TRecord, TField1> field1, IField<TRecord, TField2> field2) {
			this.CreateIndex<TField1, TField2>(name, field1, field2, false);
		}

		IUniqueIndex<TField> IPrimaryKeyHolder.PrimaryKey<TField>() {
			return this.table.PrimaryKey as IUniqueIndex<TField>;
		}

		List<IForeignKey> IPrimaryKeyHolder.Children { get { return this.table.Children; } }

		/// <summary>
		/// Creates foreign key
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="name"></param>
		/// <param name="parentTable"></param>
		/// <param name="foreignColumn"></param>
		/// <param name="action"></param>
		/// <param name="allowsDefault"></param>
		public void CreateForeignKey<TField>(string name, ITableSnapshot parentTable, IField<TRecord, TField> foreignColumn, ForeignKeyAction action, bool allowsDefault) {
			this.StoreSnapshot.SnapStore.CheckNotFrozen();
			if(string.IsNullOrEmpty(name)) {
				throw new ArgumentNullException("name");
			}
			if(parentTable == null) {
				throw new ArgumentNullException("parentTable");
			}
			IPrimaryKeyHolder parent = (IPrimaryKeyHolder)parentTable;
			if(parent.PrimaryKey<TField>() == null) {
				throw new ArgumentException(Properties.Resources.ErrorPrimaryKeyMissing(this.Name), "parentTable");
			}
			this.table.ValidateField(foreignColumn);
			if(!Enum.IsDefined(typeof(ForeignKeyAction), action)) {
				throw new ArgumentOutOfRangeException("action");
			}
			if(this.table.ForeignKeys[foreignColumn.Order] != null) {
				throw new ArgumentException(Properties.Resources.ErrorForeignKeyExists(this.Name, foreignColumn.Name));
			}
			foreach(IForeignKey fk in this.table.ForeignKeys) {
				if(fk != null && fk.Name == name) {
					throw new ArgumentException(Properties.Resources.ErrorForeignKeyNameExists(this.Name, name), "name");
				}
			}

			ForeignKey<TField> foreignKey = new ForeignKey<TField>(name, parent.PrimaryKey<TField>(), this, foreignColumn, action, allowsDefault);
			this.table.ForeignKeys[foreignColumn.Order] = foreignKey;
			parent.Children.Add(foreignKey);
		}

		/// <summary>
		/// Create foreign keys with disabled defaults
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="name"></param>
		/// <param name="parentTable"></param>
		/// <param name="foreignColumn"></param>
		/// <param name="action"></param>
		public void CreateForeignKey<TField>(string name, ITableSnapshot parentTable, IField<TRecord, TField> foreignColumn, ForeignKeyAction action) {
			this.CreateForeignKey<TField>(name, parentTable, foreignColumn, action, false);
		}

		/// <summary>
		/// Inserts new row in the table
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public RowId Insert(ref TRecord data) {
			this.ValidateModification();
			RowId rowId = this.table.Insert(ref data);
			int timestamp = TableSnapshot<TRecord>.Timestamp();
			foreach(List<IIndex<TRecord>> list in this.table.Indexes) {
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						if(index.Timestamp != timestamp) {
							index.Insert(rowId);
							index.Timestamp = timestamp;
						}
					}
				}
			}
			return rowId;
		}

		/// <summary>
		/// Deletes row from table
		/// </summary>
		/// <param name="rowId"></param>
		public void Delete(RowId rowId) {
			this.ValidateModification();
			if(this.table.PrimaryKey != null) {
				HashSet<ChildRow> deleteList = new HashSet<ChildRow>();
				deleteList.Add(new ChildRow(this, rowId));
				foreach(IForeignKey fk in this.table.Children) {
					fk.OnParentDelete(rowId, deleteList);
				}
				foreach(ChildRow childRow in deleteList) {
					childRow.Table.DeleteRow(childRow.RowId);
				}
			} else {
				this.DeleteRow(rowId);
			}
		}

		private void DeleteRow(RowId rowId) {
			int timestamp = TableSnapshot<TRecord>.Timestamp();
			foreach(List<IIndex<TRecord>> list in this.table.Indexes) {
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						if(index.Timestamp != timestamp) {
							index.Delete(rowId);
							index.Timestamp = timestamp;
						}
					}
				}
			}
			this.table.Delete(rowId);
		}
		void IChildTable.DeleteRow(RowId rowId) {
			this.DeleteRow(rowId);
		}

		/// <summary>
		/// Checks if the row was deleted
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public bool IsDeleted(RowId rowId) {
			return this.table.IsDeleted(rowId, this.StoreSnapshot.Version, false);
		}

		/// <summary>
		/// Checks if provided rowId is id of row that is exists in current snapshot.
		/// It will be impossible to use IsDeleted if rowId comes from future snapshot and this snapshot is probing.
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		public bool IsValid(RowId rowId) {
			return !this.table.IsDeleted(rowId, this.StoreSnapshot.Version, true);
		}

		/// <summary>
		/// Updates field of the row
		/// </summary>
		/// <typeparam name="TField">Type of the field</typeparam>
		/// <param name="rowId">row id</param>
		/// <param name="field">Field to update</param>
		/// <param name="value">New value to assign</param>
		/// <returns></returns>
		public bool SetField<TField>(RowId rowId, IField<TRecord, TField> field, TField value) {
			this.ValidateModification();
			this.table.ValidateField(field);
			if(field.Compare(this.table.GetLatestField<TField>(rowId, field), value) != 0) {
				List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						// no need to check for timestamp as only one field updated
						index.Delete(rowId);
					}
				}
				bool updated = this.table.SetField<TField>(rowId, field, value);
				Debug.Assert(updated);
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						// no need to check for timestamp as only one field updated
						index.Insert(rowId);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets value of the filed
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="rowId"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField GetField<TField>(RowId rowId, IField<TRecord, TField> field) {
			return this.table.GetField<TField>(rowId, field, this.StoreSnapshot.Version);
		}

		/// <summary>
		/// Gets the entire record
		/// </summary>
		/// <param name="rowId"></param>
		/// <param name="data"></param>
		public void GetData(RowId rowId, out TRecord data) {
			this.table.GetData(rowId, this.StoreSnapshot.Version, out data);
		}

		public IEnumerator<RowId> GetEnumerator() {
			return new RowEnumerator(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		/// <summary>
		/// Selects rows by the provided criteria
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public IEnumerable<RowId> Select<TField>(IField<TRecord, TField> field, TField value) {
			IIndex<TRecord> index = this.FindIndex(field, false);
			if(index != null) {
				return ((IFieldIndex<TField>)index).Find(value, this.StoreSnapshot.Version);
			} else {
				return this.SelectDirect(field, value);
			}
		}

		/// <summary>
		/// Selects rows where value of provided field in range from min to max inclusively
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public IEnumerable<RowId> Select<TField>(IField<TRecord, TField> field, TField min, TField max) {
			this.table.ValidateField(field);
			List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
			if(list != null) {
				foreach(IIndex<TRecord> index in list) {
					if(!index.IsUnique) {
						RangeIndex<TField> rangeIndex = index as RangeIndex<TField>;
						if(rangeIndex != null && rangeIndex.Field == field) {
							return rangeIndex.Find(min, max, this.StoreSnapshot.Version);
						}
					}
				}
			}
			return this.SelectDirect(field, min, max);
		}

		/// <summary>
		/// Selects rows by the provided criteria
		/// </summary>
		/// <typeparam name="TField1"></typeparam>
		/// <typeparam name="TField2"></typeparam>
		/// <param name="field1"></param>
		/// <param name="field2"></param>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns></returns>
		public IEnumerable<RowId> Select<TField1, TField2>(IField<TRecord, TField1> field1, IField<TRecord, TField2> field2, TField1 value1, TField2 value2) {
			IFieldIndex<Composite<TField1, TField2>> index = (IFieldIndex<Composite<TField1, TField2>>)this.FindIndex(field1, field2, false);
			if(index != null) {
				return index.Find(new Composite<TField1, TField2>() { t1 = value1, t2 = value2 }, this.StoreSnapshot.Version);
			} else {
				return this.SelectDirect(new CompositeField<TField1, TField2>(field1, field2), new Composite<TField1,TField2>() { t1 = value1, t2 = value2 });
			}
		}

		/// <summary>
		/// Searches a unique value. the field should be unique
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public RowId Find<TField>(IField<TRecord, TField> field, TField value) {
			IUniqueIndex<TField> index = (IUniqueIndex<TField>)this.FindIndex(field, true);
			if(index != null) {
				return index.FindUnique(value, this.StoreSnapshot.Version);
			}
			throw new InvalidOperationException(Properties.Resources.ErrorNoIndex(this.Name, field.Name));
		}

		/// <summary>
		/// Searches for a unique value. The field1 and field2 should be unique
		/// </summary>
		/// <typeparam name="TField1"></typeparam>
		/// <typeparam name="TField2"></typeparam>
		/// <param name="field1"></param>
		/// <param name="field2"></param>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns></returns>
		public RowId Find<TField1, TField2>(IField<TRecord, TField1> field1, IField<TRecord, TField2> field2, TField1 value1, TField2 value2) {
			IFieldIndex<Composite<TField1, TField2>> index = (IFieldIndex<Composite<TField1, TField2>>)this.FindIndex(field1, field2, true);
			if(index != null) {
				return ((IUniqueIndex<Composite<TField1, TField2>>)index).FindUnique(new Composite<TField1,TField2>() { t1 = value1, t2 = value2 }, this.StoreSnapshot.Version);
			}
			throw new InvalidOperationException(Properties.Resources.ErrorNoIndex2(this.Name, field1.Name, field2.Name));
		}

		public bool Exists<TField>(IField<TRecord, TField> field, TField value) {
			IIndex<TRecord> index = this.FindIndex(field, false);
			if(index != null) {
				return ((IFieldIndex<TField>)index).Exists(value, this.StoreSnapshot.Version);
			} else {
				return this.SelectDirect(field, value).Any();
			}
		}

		public bool Exists<TField1, TField2>(IField<TRecord, TField1> field1, IField<TRecord, TField2> field2, TField1 value1, TField2 value2) {
			IIndex<TRecord> index = this.FindIndex(field1, field2, false);
			if(index != null) {
				return ((IFieldIndex<Composite<TField1, TField2>>)index).Exists(new Composite<TField1, TField2>() { t1 = value1, t2 = value2 }, this.StoreSnapshot.Version);
			} else {
				return this.SelectDirect(new CompositeField<TField1, TField2>(field1, field2), new Composite<TField1, TField2>() { t1 = value1, t2 = value2 }).Any();
			}
		}

		/// <summary>
		/// Checks if table is empty i.e. no records exist.
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty() {
			foreach(List<IIndex<TRecord>> list in this.table.Indexes) {
				if(list != null && 0 < list.Count) {
					foreach(IIndex<TRecord> index in list) {
						return index.IsEmpty(this.StoreSnapshot.Version);
					}
				}
			}
			if(this.table.PrimaryKey != null) {
				// No real index were found. The only hope is pseudo unique index of primary key.
				// Right now it is not faster then just table scan, but anyway use it as in the future it may change.
				return this.table.PrimaryKey.IsEmpty(this.StoreSnapshot.Version);
			}
			// There is no indexes on the table. Do a full scan.
			return TableSnapshot<TRecord>.IsEmpty(this.table, this.StoreSnapshot.Version);
		}

		/// <summary>
		/// Gets minimum value of the provided field.
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField Minimum<TField>(IField<TRecord, TField> field) {
			this.table.ValidateField(field);
			if(!this.IsEmpty()) {
				List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						if(((IFieldHolder)index).Field == field) {
							RangeIndex<TField> rangeIndex = index as RangeIndex<TField>;
							if(rangeIndex != null) {
								return rangeIndex.MinimumValue(this.StoreSnapshot.Version);
							}
						}
					}
				}
				TField min = field.DefaultValue;
				bool first = true;
				foreach(RowId rowId in this) {
					if(first) {
						min = this.GetField(rowId, field);
						first = false;
					} else {
						TField value = this.GetField(rowId, field);
						if(field.Compare(value, min) < 0) {
							min = value;
						}
					}
				}
				return min;
			}
			return field.DefaultValue;
		}

		/// <summary>
		/// Gets maximum value of the provided field.
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField Maximum<TField>(IField<TRecord, TField> field) {
			this.table.ValidateField(field);
			if(!this.IsEmpty()) {
				List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
				if(list != null) {
					foreach(IIndex<TRecord> index in list) {
						if(((IFieldHolder)index).Field == field) {
							RangeIndex<TField> rangeIndex = index as RangeIndex<TField>;
							if(rangeIndex != null) {
								return rangeIndex.MaximumValue(this.StoreSnapshot.Version);
							}
						}
					}
				}
				TField max = field.DefaultValue;
				bool first = true;
				foreach(RowId rowId in this) {
					if(first) {
						max = this.GetField(rowId, field);
						first = false;
					} else {
						TField value = this.GetField(rowId, field);
						if(field.Compare(max, value) < 0) {
							max = value;
						}
					}
				}
				return max;
			}
			return field.DefaultValue;
		}

		/// <summary>
		/// Checks if the data of the table was changed between fromVersion and toVersion inclusively
		/// </summary>
		/// <param name="fromVersion"></param>
		/// <param name="toVersion"></param>
		/// <returns></returns>
		public bool WasChanged(int fromVersion, int toVersion) {
			int completed = this.StoreSnapshot.SnapStore.CompletedVersion;
			if(!(0 <= fromVersion && fromVersion <= completed)) {
				throw new ArgumentOutOfRangeException("fromVersion");
			}
			if(!(fromVersion <= toVersion && toVersion <= completed)) {
				throw new ArgumentOutOfRangeException("toVersion");
			}
			for(int i = fromVersion; i <= toVersion; i++) {
				if(this.table.WasChangedIn(i)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if the table was affected by transaction even if the transaction was rolled back.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool WasAffected(int version) {
			return this.table.WasChangedIn(version);
		}

		/// <summary>
		/// Gets changes made in the provided version
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public IEnumerator<TableChange<TRecord>> GetChanges(int version) {
			return this.GetChanges(version, version);
		}

		/// <summary>
		/// Gets changes made in the interval of versions inclusively
		/// </summary>
		/// <param name="fromVersion"></param>
		/// <param name="toVersion"></param>
		/// <returns></returns>
		public IEnumerator<TableChange<TRecord>> GetChanges(int fromVersion, int toVersion) {
			if(toVersion < fromVersion) {
				throw new ArgumentOutOfRangeException("toVersion");
			}
			List<int> version = new List<int>();
			for(int i = fromVersion; i <= toVersion; i++) {
				if(this.table.WasChangedIn(i)) {
					version.Add(i);
				}
			}
			if(0 < version.Count) {
				return new ChangeEnumerator(this, version);
			}
			return null;
		}

		/// <summary>
		/// Gets changes that happened when store version changed from one version to another
		/// </summary>
		/// <param name="fromVersion"></param>
		/// <param name="toVersion"></param>
		/// <returns></returns>
		public IEnumerator<TableChange<TRecord>> GetVersionChangeChanges(int fromVersion, int toVersion) {
			if(fromVersion != toVersion) {
				bool reverse = toVersion < fromVersion;
				int min = reverse ? toVersion : fromVersion;
				int max = reverse ? fromVersion : toVersion;

				IEnumerator<TableChange<TRecord>> enumerator = this.GetChanges(min + 1, max);
				if(enumerator != null && reverse) {
					enumerator = new ReverseChangeEnumerator((ChangeEnumerator)enumerator);
				}
				return enumerator;
			}
			return null;
		}

		/// <summary>
		/// Enumerate rows touched by rolled back transaction
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public IEnumerator<RowId> GetRolledBackChanges(int version) {
			return new RolledBackChangesEnumerator(this, version);
		}

		/// <summary>
		/// Not for public consumption
		/// </summary>
		/// <param name="fields"></param>
		/// <param name="data1"></param>
		/// <param name="data2"></param>
		/// <returns></returns>
		internal static int Compare(IEnumerable<IField<TRecord>> fields, ref TRecord data1, ref TRecord data2) {
			foreach(IField<TRecord> field in fields) {
				int result = field.Compare(ref data1, ref data2);
				if(result != 0) {
					return result;
				}
			}
			return 0;
		}

		private	IEnumerable<RowId> SelectDirect<TField>(IField<TRecord, TField> field, TField value) {
			int version = this.StoreSnapshot.Version;
			foreach(RowId rowId in this) {
				if(field.Compare(this.table.GetField<TField>(rowId, field, version), value) == 0) {
					yield return rowId;
				}
			}
		}

		private static bool IsEmpty(SnapTable<TRecord> table, int version) {
			int count = table.Count(version);
			for(int i = 0; i < count; i++) {
				if(!table.IsDeleted(new RowId(i), version, false)) {
					return false;
				}
			}
			return true;
		}

		private IEnumerable<RowId> SelectDirect<TField>(IField<TRecord, TField> field, TField min, TField max) {
			Debug.Assert(field.Compare(min, max) <= 0);
			int version = this.StoreSnapshot.Version;
			foreach(RowId rowId in this) {
				TField value = this.table.GetField<TField>(rowId, field, version);
				if(field.Compare(value, min) >= 0 && field.Compare(value, max) <= 0) {
					yield return rowId;
				}
			}
		}

		private void ValidateModification() {
			if(this.table.SnapStore.Editor != this.StoreSnapshot) {
				throw new InvalidOperationException(Properties.Resources.ErrorEditOutsideTransaction);
			}
		}

		private static int currentTimestamp = 0;
		private static int Timestamp() {
			return ++TableSnapshot<TRecord>.currentTimestamp;
		}

		private IIndex<TRecord> FindIndex(IField<TRecord> field, bool uniqueOnly) {
			this.table.ValidateField(field);
			List<IIndex<TRecord>> list = this.table.Indexes[field.Order];
			if(list != null) {
				foreach(IIndex<TRecord> index in list) {
					if(!uniqueOnly || index.IsUnique) {
						if(((IFieldHolder)index).Field == field) {
							return index;
						}
					}
				}
			}
			return null;
		}

		private IIndex<TRecord> FindIndex(IField<TRecord> field1, IField<TRecord> field2, bool uniqueOnly) {
			this.table.ValidateField(field1);
			this.table.ValidateField(field2);
			List<IIndex<TRecord>> list = this.table.Indexes[field1.Order];
			if(list != null) {
				foreach(IIndex<TRecord> index in list) {
					if(!uniqueOnly || index.IsUnique) {
						ICompositeField field = ((IFieldHolder)index).Field as ICompositeField;
						if(field != null && field.ConsistOf(field1, field2)) {
							return index;
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Enumerates rows of the table
		/// </summary>
		private class RowEnumerator : IEnumerator<RowId> {

			protected readonly TableSnapshot<TRecord> table;
			private readonly int version;
			private readonly int count;
			private int index;

			public RowEnumerator(TableSnapshot<TRecord> table) {
				this.table = table;
				this.version = this.table.StoreSnapshot.Version;
				this.count = this.table.table.Count(this.version);
				this.index = -1;
			}

			public virtual bool MoveNext() {
				if(this.version == this.table.StoreSnapshot.Version) {
					this.index = Math.Min(this.index + 1, this.count);
					while(this.index < this.count) {
						if(!this.table.table.IsDeleted(new RowId(this.index), this.version, false)) {
							return true;
						}
						this.index++;
					}
					return false;
				}
				throw new InvalidOperationException(Properties.Resources.ErrorEnumeratorPosition);
			}

			public RowId Current {
				get {
					if(this.version == this.table.StoreSnapshot.Version && 0 <= this.index && this.index < this.count) {
						return new RowId(this.index);
					} else {
						throw new InvalidOperationException(Properties.Resources.ErrorEnumeratorVersion);
					}
				}
			}

			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Dispose() {
			}

			public void Reset() {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Enumerates changes made over multiple transactions
		/// </summary>
		private class ChangeEnumerator : IEnumerator<TableChange<TRecord>>, ITableChange<TRecord> {
			private readonly SnapTable<TRecord> table;
			private readonly int newVersion;
			private readonly int oldVersion;
			private readonly Dictionary<RowId, SnapTableAction> action = new Dictionary<RowId, SnapTableAction>();
			// Enumerator is a struct so I can't make it read-only
			private Dictionary<RowId, SnapTableAction>.Enumerator enumerator;

			public ChangeEnumerator(TableSnapshot<TRecord> table, List<int> version) {
				// assuming version is sorted
				this.table = table.table;
				this.newVersion = version[version.Count - 1];
				this.oldVersion = version[0];
				this.MergeChanges(version);
				this.enumerator = this.action.GetEnumerator();
			}

			private void MergeChanges(List<int> version) {
				for(int i = 0; i < version.Count; i++) {
					IEnumerator<SnapTableChange<TRecord>> e = this.table.GetChanges(version[i]);
					while(e.MoveNext()) {
						SnapTableAction old;
						switch(e.Current.Action) {
						case SnapTableAction.Insert:
							if(this.action.ContainsKey(e.Current.RowId)) {
								Debug.Assert(this.action[e.Current.RowId] == SnapTableAction.Delete);
								this.action.Remove(e.Current.RowId);
							} else {
								this.action.Add(e.Current.RowId, SnapTableAction.Insert);
							}
							break;
						case SnapTableAction.Delete:
							if(this.action.TryGetValue(e.Current.RowId, out old) && old == SnapTableAction.Insert) {
								this.action.Remove(e.Current.RowId);
							} else {
								Debug.Assert(!this.action.TryGetValue(e.Current.RowId, out old) || old == SnapTableAction.Update);
								this.action[e.Current.RowId] = SnapTableAction.Delete;
							}
							break;
						case SnapTableAction.Update:
							if(!this.action.ContainsKey(e.Current.RowId)) {
								this.action.Add(e.Current.RowId, SnapTableAction.Update);
							}
							Debug.Assert(this.action[e.Current.RowId] != SnapTableAction.Delete);
							break;
						default:
							Debug.Fail("Unknown action");
							break;
						}
					}
				}
			}

			public TableChange<TRecord> Current {
				get { return new TableChange<TRecord>(this, this.enumerator.Current.Key); }
			}

			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Dispose() {
				this.enumerator.Dispose();
			}

			public void Reset() {
				throw new NotSupportedException();
			}

			public bool MoveNext() {
				return this.enumerator.MoveNext();
			}

			public SnapTableAction Action(RowId rowId) {
				return this.action[rowId];
			}

			public void GetNewData(RowId rowId, out TRecord data) {
				this.table.GetData(rowId, this.newVersion, out data);
			}

			public void GetOldData(RowId rowId, out TRecord data) {
				this.table.GetData(rowId, this.oldVersion - 1, out data);
			}

			public TField GetNewField<TField>(RowId rowId, IField<TRecord, TField> field) {
				return this.table.GetField<TField>(rowId, field, this.newVersion);
			}

			public TField GetOldField<TField>(RowId rowId, IField<TRecord, TField> field) {
				return this.table.GetField<TField>(rowId, field, this.oldVersion - 1);
			}
		}

		/// <summary>
		/// Enumerate changes made over multiple transaction when version changed back from later to older.
		/// </summary>
		private class ReverseChangeEnumerator : IEnumerator<TableChange<TRecord>>, ITableChange<TRecord> {
			private ChangeEnumerator enumerator;

			public ReverseChangeEnumerator(ChangeEnumerator enumerator) {
				this.enumerator = enumerator;
			}

			public bool MoveNext() {
				return this.enumerator.MoveNext();
			}

			public TableChange<TRecord> Current {
				get { return new TableChange<TRecord>(this, this.enumerator.Current.RowId); }
			}

			object System.Collections.IEnumerator.Current {
				get { return this.Current; }
			}

			public void Dispose() {
				this.enumerator.Dispose();
			}

			public void Reset() {
				throw new NotSupportedException();
			}

			public SnapTableAction Action(RowId rowId) {
				SnapTableAction action = this.enumerator.Action(rowId);
				switch(action) {
				case SnapTableAction.Insert: return SnapTableAction.Delete;
				case SnapTableAction.Delete: return SnapTableAction.Insert;
				case SnapTableAction.Update: return SnapTableAction.Update;
				default:
					Debug.Fail("Unknown action");
					throw new InvalidOperationException();
				}
			}

			public void GetNewData(RowId rowId, out TRecord data) {
				this.enumerator.GetOldData(rowId, out data);
			}

			public void GetOldData(RowId rowId, out TRecord data) {
				this.enumerator.GetNewData(rowId, out data);
			}

			public TField GetNewField<TField>(RowId rowId, IField<TRecord, TField> field) {
				return this.enumerator.GetOldField(rowId, field);
			}

			public TField GetOldField<TField>(RowId rowId, IField<TRecord, TField> field) {
				return this.enumerator.GetNewField(rowId, field);
			}
		}

		private class RolledBackChangesEnumerator : IEnumerator<RowId> {

			private IEnumerator<SnapTableChange<TRecord>> enumerator;

			public RolledBackChangesEnumerator(TableSnapshot<TRecord> table, int version) {
				this.enumerator = table.table.GetRolledBackChanges(version);
			}

			public bool MoveNext() {
				return this.enumerator.MoveNext();
			}

			public RowId Current { get { return this.enumerator.Current.RowId; } }

			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Dispose() {
				this.enumerator.Dispose();
			}

			public void Reset() {
				throw new NotSupportedException();
			}
		}
	}
}
