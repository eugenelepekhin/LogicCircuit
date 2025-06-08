using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class RowIdTest {
		[TestMethod]
		public void ConstructorTest() {
			RowId id1 = new RowId();
			Assert.AreEqual<int>(0, id1.Value);

			RowId id2 = new RowId(12);
			Assert.AreEqual<int>(12, id2.Value);

			RowId id3 = new RowId(-160);
			Assert.AreEqual<int>(-160, id3.Value);

			RowId id4 = new RowId(int.MaxValue);
			Assert.AreEqual<int>(int.MaxValue, id4.Value);

			RowId id5 = new RowId(int.MinValue);
			Assert.AreEqual<int>(int.MinValue, id5.Value);
		}

		[TestMethod()]
		public void EqualsTest() {
			RowId id1 = new RowId(12);
			RowId id2 = new RowId(12);
			RowId id3 = new RowId(11);

			Assert.IsTrue(id1.Equals(id1));
			Assert.IsTrue(id1.Equals(id2));
			Assert.IsTrue(id2.Equals(id1));
			Assert.IsFalse(id1.Equals(id1.Value));
			Assert.IsFalse(id1.Equals(id3));
			Assert.IsFalse(id3.Equals(id1));

			Assert.AreEqual<int>(id1.Value, id1.GetHashCode());
			Assert.AreEqual<int>(id2.Value, id2.GetHashCode());
			Assert.AreEqual<int>(id3.Value, id3.GetHashCode());

			Assert.IsTrue(id1 == id2);
			Assert.IsTrue(id2 == id1);
			Assert.IsFalse(id1 != id2);
			Assert.IsFalse(id2 != id1);

			Assert.IsTrue(id1 != id3);
			Assert.IsTrue(id3 != id1);
			Assert.IsFalse(id1 == id3);
			Assert.IsFalse(id3 == id1);
		}
	}
}
