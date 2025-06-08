using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class ForeignKeyTest {
		private struct NodeData {
			public RowId NextRowId;
			public int Data;

			public class NextRowIdField : RowIdField<NodeData> {
				public static readonly NextRowIdField Field = new NextRowIdField();
				private NextRowIdField() : base("NextRowId") { }
				public override RowId GetValue(ref NodeData record) => record.NextRowId;
				public override void SetValue(ref NodeData record, RowId value) => record.NextRowId = value;
			}
			public class DataField : IField<NodeData, int> {
				public static readonly DataField Field = new DataField();
				public int DefaultValue => 0;
				public int GetValue(ref NodeData record) => record.Data;
				public void SetValue(ref NodeData record, int value) => record.Data = value;
				public string Name => "Data";
				public int Order { get; set; }
				public int Compare(ref NodeData data1, ref NodeData data2) => Math.Sign(data1.Data - (long)data2.Data);
				public int Compare(int x, int y) => Math.Sign(x - (long)y);
			}

			public static TableSnapshot<NodeData> CreateTable(StoreSnapshot store, string name) => new TableSnapshot<NodeData>(store, name, NextRowIdField.Field, DataField.Field);

			public static RowId Insert(TableSnapshot<NodeData> table, RowId nextRowId, int data) {
				NodeData nodeData = new NodeData() {
					NextRowId = nextRowId,
					Data = data
				};
				return table.Insert(ref nodeData);
			}

			public static RowId Insert(TableSnapshot<NodeData> table, int data) => NodeData.Insert(table, NextRowIdField.Field.DefaultValue, data);
		}

		/// <summary>
		/// Check foreign key throws when child is default and no parent with default primary key is exist.
		/// </summary>
		[TestMethod]
		public void ForeignKeyWithoutDefaultTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<NodeData> table = NodeData.CreateTable(store, "t1");
			table.MakeAutoUnique();
			table.CreateForeignKey<RowId>("fk", table, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId r1 = NodeData.Insert(table, 1);
			Assert.Throws<ForeignKeyViolationException>(() => store.PrepareCommit());
			table.SetField(r1, NodeData.NextRowIdField.Field, r1);
			store.Commit();
		}

		/// <summary>
		/// Check foreign key allow child to be default when no parent with default primary key is exist.
		/// </summary>
		[TestMethod]
		public void ForeignKeyWithDefaultTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<NodeData> table = NodeData.CreateTable(store, "t1");
			table.MakeAutoUnique();
			table.CreateForeignKey<RowId>("fk", table, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade, true);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId r1 = NodeData.Insert(table, 1);
			store.PrepareCommit();
			store.Commit();
		}
	}
}
