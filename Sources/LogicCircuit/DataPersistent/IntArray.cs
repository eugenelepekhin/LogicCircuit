using System;

namespace LogicCircuit.DataPersistent {
	internal class IntArray {
		public static readonly IField<int, int> Field = new IntField();

		private class IntField : IField<int, int> {
			public string Name => "Int";

			public int Order { get; set; }

			public int DefaultValue => 0;

			public int GetValue(ref int record) => record;

			public void SetValue(ref int record, int value) => record = value;

			public int Compare(ref int data1, ref int data2) => Math.Sign((long)data1 - (long)data2);

			public int Compare(int x, int y) => Math.Sign((long)x - (long)y);
		}

		private readonly SnapTable<int> table;

		public IntArray(SnapStore store, string name, int size) {
			this.table = new SnapTable<int>(store, name, size, new IField<int>[] { IntArray.Field }, false);
		}

		public int Length => this.table.LatestCount(); // Note! The length of the table is never changed.

		public int Value(int index, int version) => this.table.GetField<int>(new RowId(index), IntArray.Field, version);

		public void SetValue(int index, int value) => this.table.SetField<int>(new RowId(index), IntArray.Field, value);
	}
}
