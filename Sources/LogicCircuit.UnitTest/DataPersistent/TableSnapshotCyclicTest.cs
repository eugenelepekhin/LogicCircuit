using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class TableSnapshotCyclicTest {
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

			public static readonly IField<NodeData>[] Fields = new IField<NodeData>[] { NextRowIdField.Field, DataField.Field };

			public static RowId Insert(TableSnapshot<NodeData> table, int data) {
				NodeData nodeData = new NodeData() {
					Data = data
				};
				RowId rowId = table.Insert(ref nodeData);
				table.SetField<RowId>(rowId, NextRowIdField.Field, rowId);
				return rowId;
			}

			public static RowId Insert(TableSnapshot<NodeData> table, RowId nextRowId, int data) {
				NodeData nodeData = new NodeData() {
					NextRowId = nextRowId,
					Data = data
				};
				return table.Insert(ref nodeData);
			}
		}

		private void AssertSelection(IEnumerable<RowId> selection, params RowId[] expected) {
			CollectionAssert.AreEquivalent(expected, selection.ToArray(), "Selection does not match expected rows.");
		}

		/// <summary>
		/// Check of self referring table delete. The root is a record with itself as a parent.
		/// </summary>
		[TestMethod]
		public void TableSnapshotCyclicDeleteTreeTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<NodeData> tree = new TableSnapshot<NodeData>(store, "Tree", NodeData.Fields);
			tree.MakeAutoUnique();
			tree.CreateForeignKey<RowId>("FK_TreeParent", tree, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId root = NodeData.Insert(tree, 10);
			RowId row1Id = NodeData.Insert(tree, root, 20);
			RowId row2Id = NodeData.Insert(tree, row1Id, 30);
			RowId row3Id = NodeData.Insert(tree, row2Id, 40);
			RowId row4Id = NodeData.Insert(tree, row1Id, 50);
			store.Commit();

			this.AssertSelection(tree, root, row1Id, row2Id, row3Id, row4Id);
			Assert.AreEqual(root, tree.GetField<RowId>(root, NodeData.NextRowIdField.Field));

			Assert.IsTrue(store.StartTransaction());
			tree.Delete(row2Id);
			store.Commit();

			this.AssertSelection(tree, root, row1Id, row4Id);

			Assert.IsTrue(store.StartTransaction());
			tree.Delete(root);
			store.Commit();

			this.AssertSelection(tree);
		}

		/// <summary>
		/// Check of deletion from chain of 3 tables forming a loop.
		/// </summary>
		[TestMethod]
		public void TableSnapshotCyclicDeleteChainTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<NodeData> node1 = new TableSnapshot<NodeData>(store, "nod1", NodeData.Fields);
			TableSnapshot<NodeData> node2 = new TableSnapshot<NodeData>(store, "nod2", NodeData.Fields);
			TableSnapshot<NodeData> node3 = new TableSnapshot<NodeData>(store, "nod3", NodeData.Fields);
			node1.MakeAutoUnique();
			node2.MakeAutoUnique();
			node3.MakeAutoUnique();
			node2.CreateForeignKey<RowId>("node1-node2", node1, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade);
			node3.CreateForeignKey<RowId>("node2-node3", node2, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade);
			node1.CreateForeignKey<RowId>("node3-node1", node3, NodeData.NextRowIdField.Field, ForeignKeyAction.Cascade);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			NodeData data = new NodeData();
			RowId row1Id = NodeData.Insert(node1, data.NextRowId, 10);
			RowId row2Id = NodeData.Insert(node2, row1Id, 20);
			RowId row3Id = NodeData.Insert(node3, row2Id, 30);
			node1.SetField<RowId>(row1Id, NodeData.NextRowIdField.Field, row3Id);
			store.Commit();

			this.AssertSelection(node1, row1Id);
			this.AssertSelection(node2, row2Id);
			this.AssertSelection(node3, row3Id);

			Assert.IsTrue(store.StartTransaction());
			node2.Delete(row2Id);
			store.Commit();

			this.AssertSelection(node1);
			this.AssertSelection(node2);
			this.AssertSelection(node3);
		}
	}
}
