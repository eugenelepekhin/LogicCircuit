using System;

namespace LogicCircuit.DataPersistent {
	internal interface ITableChange<TRecord> where TRecord:struct {
		SnapTableAction Action(RowId rowId);
		void GetNewData(RowId rowId, out TRecord data);
		void GetOldData(RowId rowId, out TRecord data);
		TField GetNewField<TField>(RowId rowId, IField<TRecord, TField> field);
		TField GetOldField<TField>(RowId rowId, IField<TRecord, TField> field);
	}
}
