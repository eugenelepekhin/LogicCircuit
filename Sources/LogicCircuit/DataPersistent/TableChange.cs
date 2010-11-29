using System;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Describes change of data
	/// </summary>
	/// <typeparam name="TRecord"></typeparam>
	public struct TableChange<TRecord> where TRecord:struct {
		private ITableChange<TRecord> changeData;
		private RowId rowId;

		internal TableChange(ITableChange<TRecord> changeData, RowId rowId) {
			this.changeData = changeData;
			this.rowId = rowId;
		}

		/// <summary>
		/// Gets row id of the change
		/// </summary>
		public RowId RowId {
			get { return this.rowId; }
		}
		
		/// <summary>
		/// Gets change action
		/// </summary>
		public SnapTableAction Action {
			get { return this.changeData.Action(this.rowId); }
		}

		/// <summary>
		/// Gets new version of the data
		/// </summary>
		/// <param name="data"></param>
		public void GetNewData(out TRecord data) {
			if(this.Action == SnapTableAction.Delete) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongNewData);
			}
			this.changeData.GetNewData(this.rowId, out data);
		}

		/// <summary>
		/// Gets old version of the data
		/// </summary>
		/// <param name="data"></param>
		public void GetOldData(out TRecord data) {
			if(this.Action == SnapTableAction.Insert) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongOldRow);
			}
			this.changeData.GetOldData(this.rowId, out data);
		}

		/// <summary>
		/// Gets new version of the field
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField GetNewField<TField>(IField<TRecord, TField> field) {
			if(this.Action == SnapTableAction.Delete) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongNewData);
			}
			return this.changeData.GetNewField<TField>(this.rowId, field);
		}

		/// <summary>
		/// Gets old version of the field
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="field"></param>
		/// <returns></returns>
		public TField GetOldField<TField>(IField<TRecord, TField> field) {
			if(this.Action == SnapTableAction.Insert) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongOldRow);
			}
			return this.changeData.GetOldField<TField>(this.rowId, field);
		}

		public override bool Equals(object obj) {
			if(obj is TableChange<TRecord>) {
				TableChange<TRecord> other = (TableChange<TRecord>)obj;
				return this.changeData == other.changeData && this.rowId == other.rowId;
			}
			return false;
		}

		public override int GetHashCode() {
			return this.changeData.GetHashCode() ^ this.rowId.GetHashCode();
		}

		public static bool operator ==(TableChange<TRecord> left, TableChange<TRecord> right) {
			return left.changeData == right.changeData && left.rowId == right.rowId;
		}

		public static bool operator !=(TableChange<TRecord> left, TableChange<TRecord> right) {
			return left.changeData != right.changeData || left.rowId != right.rowId;
		}
	}
}
