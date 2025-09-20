using DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	public class IntField : IField<int, int> {
		public static readonly IntField Field = new IntField();
		public string Name => "Int";
		public int Order { get; set; }
		public int DefaultValue { get { return 0; } }
		public int GetValue(ref int record) => record;
		public void SetValue(ref int record, int value) => record = value;
		public int Compare(ref int data1, ref int data2) => Math.Sign((long)data1 - (long)data2);
		public int Compare(int x, int y) => Math.Sign((long)x - (long)y);
	}
}
