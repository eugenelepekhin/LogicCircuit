using System;

namespace LogicCircuit.DataPersistent {
	internal struct SnapTableChange<TRecord> where TRecord:struct  {
		private ISnapTableChange<TRecord> changeData;
		private int changeIndex;

		public SnapTableChange(ISnapTableChange<TRecord> changeData, int changeIndex) {
			this.changeData = changeData;
			this.changeIndex = changeIndex;
		}

		public RowId RowId {
			get { return this.changeData.RowId(this.changeIndex); }
		}

		public SnapTableAction Action {
			get { return this.changeData.Action(this.changeIndex); }
		}

		public void GetNewData(out TRecord data) {
			if(this.Action == SnapTableAction.Delete) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongNewData);
			}
			this.changeData.GetNewData(this.changeIndex, out data);
		}

		public void GetOldData(out TRecord data) {
			if(this.Action == SnapTableAction.Insert) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongOldRow);
			}
			this.changeData.GetOldData(this.changeIndex, out data);
		}

		public TField GetNewField<TField>(IField<TRecord, TField> field) {
			if(this.Action == SnapTableAction.Delete) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongNewData);
			}
			return this.changeData.GetNewField<TField>(this.changeIndex, field);
		}

		public TField GetOldField<TField>(IField<TRecord, TField> field) {
			if(this.Action == SnapTableAction.Insert) {
				throw new InvalidOperationException(Properties.Resources.ErrorWrongOldRow);
			}
			return this.changeData.GetOldField<TField>(this.changeIndex, field);
		}
	}
}
