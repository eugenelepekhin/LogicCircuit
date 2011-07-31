using System;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Base class for fields of type RowId to be used as foreign keys to parents tables where pseudo primary key was created with MakeAutoUnique().
	/// </summary>
	/// <typeparam name="TRecord"></typeparam>
	public abstract class RowIdField<TRecord> : IField<TRecord, RowId> where TRecord: struct {
		/// <summary>
		/// Construct the field with provided name
		/// </summary>
		/// <param name="name"></param>
		protected RowIdField(string name) {
			this.Name = name;
		}

		/// <summary>
		/// Gets default value of the field. This default value does not correspond to any real record in the parent table and so can be treated as NULL.
		/// </summary>
		public RowId DefaultValue { get { return RowId.Empty; } }

		public abstract RowId GetValue(ref TRecord record);
		public abstract void SetValue(ref TRecord record, RowId value);

		public string Name { get; private set; }

		public int Order { get; set; }

		/// <summary>
		/// Provides a base definition of the comparison.
		/// </summary>
		/// <param name="data1"></param>
		/// <param name="data2"></param>
		/// <returns></returns>
		public virtual int Compare(ref TRecord data1, ref TRecord data2) {
			return this.GetValue(ref data1).CompareTo(this.GetValue(ref data2));
		}

		public int Compare(RowId x, RowId y) {
			return x.CompareTo(y);
		}
	}
}
