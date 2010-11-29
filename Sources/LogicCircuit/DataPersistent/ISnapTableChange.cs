using System;

namespace LogicCircuit.DataPersistent {
	internal interface ISnapTableChange<TRecord> where TRecord:struct {
		RowId RowId(int changeIndex);
		SnapTableAction Action(int changeIndex);
		void GetNewData(int changeIndex, out TRecord data);
		void GetOldData(int changeIndex, out TRecord data);
		TField GetNewField<TField>(int changeIndex, IField<TRecord, TField> field);
		TField GetOldField<TField>(int changeIndex, IField<TRecord, TField> field);
	}
}
