using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit.DataPersistent {

	public enum ForeignKeyAction {
		Restrict,
		Cascade,
		SetDefault
	}

	internal struct ChildRow : IEquatable<ChildRow> {
		private readonly IChildTable table;
		private readonly RowId rowId;

		public ChildRow(IChildTable table, RowId rowId) {
			this.table = table;
			this.rowId = rowId;
		}

		public IChildTable Table { get { return this.table; } }
		public RowId RowId { get { return this.rowId; } }

		public bool Equals(ChildRow other) {
			return this.table == other.table && this.rowId == other.rowId;
		}

		public override bool Equals(object obj) {
			if(obj is ChildRow) {
				return this.Equals((ChildRow)obj);
			}
			return false;
		}

		public override int GetHashCode() {
			return this.table.GetHashCode() ^ this.rowId.Value;
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "<Table={0}, {1}>", ((ITableSnapshot)this.table).Name, this.rowId.ToString());
			}
		#endif
	}

	internal interface IChildTable {
		void DeleteRow(RowId rowId);
	}

	internal interface IForeignKey {
		string Name { get; }
		void OnParentDelete(RowId parentId, HashSet<ChildRow> deleteList);
		void Validate();
	}

	partial class TableSnapshot<TRecord> {
		private class ForeignKey<TField> : IForeignKey {
			public string Name { get; private set; }
			private readonly IUniqueIndex<TField> primaryKey;
			private readonly TableSnapshot<TRecord> childTable;
			private readonly IField<TRecord, TField> childColumn;
			private readonly ForeignKeyAction action;
			private readonly bool allowsDefault;

			public ForeignKey(
				string name, IUniqueIndex<TField> primaryKey, TableSnapshot<TRecord> childTable, IField<TRecord, TField> childColumn, ForeignKeyAction action, bool allowsDefault
			) {
				this.Name = name;
				this.primaryKey = primaryKey;
				this.childTable = childTable;
				this.childColumn = childColumn;
				this.action = action;
				this.allowsDefault = allowsDefault;
			}

			public void Validate() {
				int version = this.childTable.StoreSnapshot.Version;
				if(this.childTable.table.WasChangedIn(version)) {
					IEnumerator<SnapTableChange<TRecord>> enumerator = this.childTable.table.GetChanges(version);
					while(enumerator.MoveNext()) {
						if(enumerator.Current.Action != SnapTableAction.Delete) {
							TField value = enumerator.Current.GetNewField<TField>(this.childColumn);
							if(!this.allowsDefault || this.childColumn.Compare(value, this.childColumn.DefaultValue) != 0) {
								RowId parentId = this.primaryKey.FindUnique(value, version);
								if(parentId == RowId.Empty) {
									throw new ForeignKeyViolationException(this.Name);
								}
							}
						}
					}
				}
			}

			public void OnParentDelete(RowId parentId, HashSet<ChildRow> deleteList) {
				TField parentValue = this.primaryKey.Value(parentId, this.childTable.table.SnapStore.Version);
				foreach(RowId childId in this.childTable.Select<TField>(this.childColumn, parentValue)) {
					switch(this.action) {
					case ForeignKeyAction.Restrict:
						throw new ForeignKeyViolationException(this.Name);
					case ForeignKeyAction.Cascade:
						if(deleteList.Add(new ChildRow(this.childTable, childId)) && this.childTable.table.PrimaryKey != null) {
							foreach(IForeignKey fk in this.childTable.table.Children) {
								fk.OnParentDelete(childId, deleteList);
							}
						}
						break;
					case ForeignKeyAction.SetDefault:
						this.childTable.SetField<TField>(childId, this.childColumn, this.childColumn.DefaultValue);
						break;
					}
				}
			}
		}
	}
}
