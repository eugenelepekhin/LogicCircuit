using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit.DataPersistent {

	internal interface IIndex<TRecord> where TRecord:struct {
		void Insert(RowId rowId);
		void Delete(RowId rowId);
		bool IsUnique { get; }
		int  Timestamp { get; set; }
		bool IsEmpty(int version);
	}

	internal interface IUniqueIndex<TField> {
		RowId FindUnique(TField value, int version);
		TField Value(RowId rowId, int version);
	}

	internal interface IPrimaryKeyHolder {
		IUniqueIndex<TField> PrimaryKey<TField>();
		List<IForeignKey> Children { get; }
	}

	partial class TableSnapshot<TRecord> where TRecord:struct {

		internal interface IFieldIndex<TField> : IIndex<TRecord> {
			IEnumerable<RowId> Find(TField value, int version);
			bool Exists(TField value, int version);
		}

		internal interface IFieldHolder {
			IField<TRecord> Field { get; }
		}

		internal class UniqueIndex<TField> : IFieldIndex<TField>, IFieldHolder, IUniqueIndex<TField> {
			private readonly SnapTable<TRecord> table;
			private readonly Unique<TField> unique;
			private readonly IField<TRecord, TField> field;

			public UniqueIndex(SnapTable<TRecord> table, string name, IField<TRecord, TField> field) {
				this.table = table;
				this.field = field;
				this.unique = new Unique<TField>(table.SnapStore, name, field);
			}

			public RowId FindUnique(TField value, int version) {
				return this.unique.Find(value, version);
			}

			public IEnumerable<RowId> Find(TField value, int version) {
				RowId rowId = this.FindUnique(value, version);
				if(!rowId.IsEmpty) {
					yield return rowId;
				}
			}

			public bool Exists(TField value, int version) {
				RowId rowId = this.FindUnique(value, version);
				return !rowId.IsEmpty;
			}

			public TField Value(RowId rowId, int version) {
				return this.table.GetField<TField>(rowId, this.field, version);
			}

			public void Insert(RowId rowId) {
				this.unique.Insert(this.table.GetLatestField<TField>(rowId, this.field), rowId);
			}

			public void Delete(RowId rowId) {
				this.unique.Remove(this.table.GetLatestField<TField>(rowId, this.field));
			}

			public bool IsUnique { get { return true; } }
			public int Timestamp { get; set; }
			public IField<TRecord> Field { get { return this.field; } }

			public bool IsEmpty(int version) {
				return this.unique.Count(version) == 0;
			}
		}

		/// <summary>
		/// Unique pseudo index used as primary key in the tables where primary key was defined with MakeAutoUnique().
		/// </summary>
		internal class UniquePseudoIndex : IFieldIndex<RowId>, IFieldHolder, IUniqueIndex<RowId> {
			private readonly SnapTable<TRecord> table;

			public UniquePseudoIndex(SnapTable<TRecord> table) {
				this.table = table;
			}

			public RowId FindUnique(RowId value, int version) {
				if(0 <= value.Value && !this.table.IsDeleted(value, version, true)) {
					return value;
				} else {
					return RowId.Empty;
				}
			}

			public IEnumerable<RowId> Find(RowId value, int version) {
				RowId rowId = this.FindUnique(value, version);
				if(!rowId.IsEmpty) {
					yield return rowId;
				}
			}

			public bool Exists(RowId value, int version) {
				return 0 <= value.Value && !this.table.IsDeleted(value, version, true);
			}

			public RowId Value(RowId rowId, int version) {
				if(this.table.IsDeleted(rowId, version, false)) {
					throw new ArgumentOutOfRangeException("rowId");
				}
				return rowId;
			}

			public void Insert(RowId rowId) {
			}

			public void Delete(RowId rowId) {
			}

			public bool IsUnique { get { return true; } }
			public int Timestamp { get; set; }
			public IField<TRecord> Field { get { return SnapTable<TRecord>.RowIdPseudoField.Field; } }

			public bool IsEmpty(int version) {
				return TableSnapshot<TRecord>.IsEmpty(this.table, version);
			}
		}

		internal class RangeIndex<TField> : IFieldIndex<TField>, IFieldHolder {
			private readonly SnapTable<TRecord> table;
			private readonly BTree<TField> tree;
			private readonly IField<TRecord, TField>  field;

			public RangeIndex(SnapTable<TRecord> table, string name, IField<TRecord, TField> field) {
				this.table = table;
				this.field = field;
				this.tree = new BTree<TField>(table.SnapStore, name, field);
			}

			public IEnumerable<RowId> Find(TField value, int version) {
				return this.tree.Select(value, version);
			}

			public IEnumerable<RowId> Find(TField min, TField max, int version) {
				return this.tree.Select(min, max, version);
			}

			public bool Exists(TField value, int version) {
				return this.tree.Exists(value, version);
			}

			public TField MinimumValue(int version) {
				return this.tree.MinimumValue(version);
			}

			public TField MaximumValue(int version) {
				return this.tree.MaximumValue(version);
			}

			public void Insert(RowId rowId) {
				this.tree.Insert(this.table.GetLatestField<TField>(rowId, this.field), rowId);
			}

			public void Delete(RowId rowId) {
				this.tree.Remove(this.table.GetLatestField<TField>(rowId, this.field), rowId);
			}

			public bool IsUnique { get { return false; } }
			public int Timestamp { get; set; }
			public IField<TRecord> Field { get { return this.field; } }

			public bool IsEmpty(int version) {
				return this.tree.IsEmpty(version);
			}
		}
	}
}
