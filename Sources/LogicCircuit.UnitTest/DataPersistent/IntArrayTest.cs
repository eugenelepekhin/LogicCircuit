using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class IntArrayTest {
		[TestMethod]
		public void IntArrayConstructorTest() {
			StoreSnapshot store = new StoreSnapshot();
			IntArray array = new IntArray(store.SnapStore, "test", 3);
			store.FreezeShape();

			Assert.AreEqual<int>(3, array.Length);
			Assert.AreEqual<int>(0, array.Value(0, store.Version));
			Assert.AreEqual<int>(0, array.Value(1, store.Version));
			Assert.AreEqual<int>(0, array.Value(2, store.Version));
		}

		[TestMethod]
		public void IntArrayIndexerTest() {
			StoreSnapshot store = new StoreSnapshot();
			IntArray array = new IntArray(store.SnapStore, "test", 3);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			array.SetValue(0, 10);
			array.SetValue(1, 21);
			array.SetValue(2, 32);
			store.Commit();

			Assert.AreEqual<int>(10, array.Value(0, store.Version));
			Assert.AreEqual<int>(21, array.Value(1, store.Version));
			Assert.AreEqual<int>(32, array.Value(2, store.Version));

			Assert.IsTrue(store.StartTransaction());
			array.SetValue(0, 230);
			array.SetValue(1, 341);
			array.SetValue(2, 452);
			store.Commit();

			Assert.AreEqual<int>(230, array.Value(0, store.Version));
			Assert.AreEqual<int>(341, array.Value(1, store.Version));
			Assert.AreEqual<int>(452, array.Value(2, store.Version));

			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(10, array.Value(0, store.Version));
			Assert.AreEqual<int>(21, array.Value(1, store.Version));
			Assert.AreEqual<int>(32, array.Value(2, store.Version));

			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(0, array.Value(0, store.Version));
			Assert.AreEqual<int>(0, array.Value(1, store.Version));
			Assert.AreEqual<int>(0, array.Value(2, store.Version));

			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(10, array.Value(0, store.Version));
			Assert.AreEqual<int>(21, array.Value(1, store.Version));
			Assert.AreEqual<int>(32, array.Value(2, store.Version));

			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(230, array.Value(0, store.Version));
			Assert.AreEqual<int>(341, array.Value(1, store.Version));
			Assert.AreEqual<int>(452, array.Value(2, store.Version));
		}
	}
}
