using System;

namespace LogicCircuit.DataPersistent {
	internal class IntArray {
		public static readonly IField<int, int> Field = new IntField();

		private class IntField : IField<int, int> {
			public string Name { get { return "Int"; } }

			public int Order { get; set; }

			public int DefaultValue { get { return 0; } }

			public int GetValue(ref int record) {
				return record;
			}

			public void SetValue(ref int record, int value) {
				record = value;
			}

			public int Compare(ref int data1, ref int data2) {
				return Math.Sign((long)data1 - (long)data2);
			}

			public int Compare(int x, int y) {
				return Math.Sign((long)x - (long)y);
			}
		}

		private readonly SnapTable<int> table;

		public IntArray(SnapStore store, string name, int size) {
			this.table = new SnapTable<int>(store, name, size, new IField<int>[] { IntArray.Field }, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Length {
			// Note! The length of the table is never changed.
			get { return this.table.LatestCount(); }
		}

		public int Value(int index, int version) {
			return this.table.GetField<int>(new RowId(index), IntArray.Field, version);
		}

		public void SetValue(int index, int value) {
			this.table.SetField<int>(new RowId(index), IntArray.Field, value);
		}
	}
}
