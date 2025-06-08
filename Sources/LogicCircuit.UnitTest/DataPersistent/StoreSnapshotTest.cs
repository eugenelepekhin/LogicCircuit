using LogicCircuit.DataPersistent;
using LogicCircuit.UnitTest.DataPersistent.UnitTestSnapStore;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class StoreSnapshotTest {

		/// <summary>
		/// A test for StoreSnapshot parameterless Constructor
		/// </summary>
		[TestMethod]
		public void Constructor1Test() {
			StoreSnapshot store = new StoreSnapshot();
			// there no tables yet
			Assert.AreEqual<int>(0, store.Tables.Count());

			// check that it is impossible to start transaction on freshly created store.
			// To start transaction one needs to freeze the store first.
			bool started = false;
			Assert.Throws<InvalidOperationException>(() => started = store.StartTransaction());
			Assert.IsFalse(started);
		}

		/// <summary>
		/// A test for StoreSnapshot copy Constructor
		/// </summary>
		[TestMethod]
		public void Constructor2Test() {
			StoreSnapshot store1 = new StoreSnapshot();
			StoreSnapshot store2 = null;

			// Not frozen store can't be copied
			Assert.Throws<InvalidOperationException>(() => store2 = new StoreSnapshot(store1));
			Assert.IsNull(store2);

			TableSnapshot<PersonData> table1 = new TableSnapshot<PersonData>(store1, "Person", PersonData.Fields());
			Assert.Throws<InvalidOperationException>(() => store2 = new StoreSnapshot(store1));
			Assert.IsNull(store2);

			TableSnapshot<PhoneData> table2 = new TableSnapshot<PhoneData>(store1, "Phone", PhoneData.Fields());
			Assert.Throws<InvalidOperationException>(() => store2 = new StoreSnapshot(store1));
			Assert.IsNull(store2);

			store1.FreezeShape();

			// now the copy can be created
			store2 = new StoreSnapshot(store1);
			Assert.AreEqual<int>(2, store1.Tables.Count());
			Assert.AreEqual<int>(2, store2.Tables.Count());
			Assert.IsTrue(store1.IsFrozen);
			Assert.IsTrue(store2.IsFrozen);
		}

		/// <summary>
		/// A test for StoreSnapshot FreezeShape
		/// </summary>
		[TestMethod]
		public void FreezeShapeTest() {
			StoreSnapshot store = new StoreSnapshot();
			Assert.AreEqual<int>(0, store.Tables.Count());

			// can't freeze empty store
			Assert.Throws<InvalidOperationException>(() => store.FreezeShape());

			TableSnapshot<PersonData> table1 = new TableSnapshot<PersonData>(store, "Person", PersonData.Fields());
			TableSnapshot<PhoneData> table2 = new TableSnapshot<PhoneData>(store, "Phone", PhoneData.Fields());
			store.FreezeShape();
			Assert.AreEqual<int>(2, store.Tables.Count());

			// frozen store does not accept any new tables, indexes and foreign keys
			Assert.Throws<InvalidOperationException>(() => new TableSnapshot<PersonData>(store, "table", PersonData.Fields()));
			Assert.Throws<InvalidOperationException>(() => table1.MakeUnique<int>("pk", PersonData.PersonIdField.Field));
			Assert.Throws<InvalidOperationException>(() => table1.MakeUnique<int>("pk", PersonData.PersonIdField.Field, true));
			Assert.Throws<InvalidOperationException>(() => table1.MakeUnique<int>("pk", PersonData.PersonIdField.Field, false));
			Assert.Throws<InvalidOperationException>(() => table2.CreateForeignKey<int>("fk", table1, PhoneData.PersonIdField.Field, ForeignKeyAction.Cascade));
			Assert.Throws<InvalidOperationException>(() => table2.CreateForeignKey<int>("fk", table1, PhoneData.PersonIdField.Field, ForeignKeyAction.Restrict));
			Assert.Throws<InvalidOperationException>(() => table2.CreateForeignKey<int>("fk", table1, PhoneData.PersonIdField.Field, ForeignKeyAction.SetDefault));
		}

		/// <summary>
		/// A test for StoreSnapshot version management
		/// </summary>
		[TestMethod]
		public void VersionTest() {
			int data = 1;
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "table", IntField.Field);
			store.FreezeShape();

			// check version information of empty store
			Assert.AreEqual(0, store.Version);
			Assert.AreEqual(0, store.LatestAvailableVersion);
			Assert.AreEqual(0, table.Rows.Count());

			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			Assert.IsFalse(store.Undo());
			Assert.IsFalse(store.Undo());
			Assert.IsFalse(store.Redo());
			Assert.IsFalse(store.Redo());

			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			Assert.AreEqual(0, store.Version);
			Assert.AreEqual(0, store.LatestAvailableVersion);
			Assert.AreEqual(0, table.Rows.Count());

			// make a new transaction
			Assert.IsTrue(store.StartTransaction());
			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			RowId r1 = table.Insert(ref data);
			Assert.AreEqual(1, store.Commit());
			Assert.AreEqual(1, store.Version);
			Assert.AreEqual(1, store.LatestAvailableVersion);
			Assert.AreEqual(1, table.Rows.Count());
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			// undo and check state
			Assert.IsTrue(store.Undo());
			Assert.AreEqual(2, store.Version);
			Assert.AreEqual(2, store.LatestAvailableVersion);
			Assert.AreEqual(0, table.Rows.Count());
			Assert.IsFalse(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.IsTrue(table.IsDeleted(r1));

			// redo and check state
			Assert.IsTrue(store.Redo());
			Assert.AreEqual(3, store.Version);
			Assert.AreEqual(3, store.LatestAvailableVersion);
			Assert.AreEqual(1, table.Rows.Count());
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			Assert.IsFalse(table.IsDeleted(r1));
			Assert.AreEqual(data, table.GetField<int>(r1, IntField.Field));
		}

		/// <summary>
		/// A test for StoreSnapshot Undo and Redo
		/// </summary>
		[TestMethod]
		public void UndoRedoTest() {
			RowId r1, r2;
			int data = 1;
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "table", IntField.Field);
			store.FreezeShape();

			// transaction 1
			Assert.IsTrue(store.StartTransaction());
			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			r1 = table.Insert(ref data);
			Assert.AreEqual(1, store.Commit());
			Assert.AreEqual(1, table.GetField(r1, IntField.Field));
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			// transaction 2
			Assert.IsTrue(store.StartTransaction());
			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			table.SetField(r1, IntField.Field, 2);
			Assert.AreEqual(2, store.Commit());
			Assert.AreEqual(2, table.GetField(r1, IntField.Field));
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			// transaction 3
			Assert.IsTrue(store.StartTransaction());
			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			table.SetField(r1, IntField.Field, 3);
			Assert.AreEqual(3, store.Commit());
			Assert.AreEqual(3, table.GetField(r1, IntField.Field));
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);

			// transaction 4 - undo
			Assert.IsTrue(store.Undo());
			Assert.AreEqual(4, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.AreEqual(2, table.GetField(r1, IntField.Field));

			// transaction 5 - undo
			Assert.IsTrue(store.Undo());
			Assert.AreEqual(5, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.AreEqual(1, table.GetField(r1, IntField.Field));

			// transaction 6 - undo
			Assert.IsTrue(store.Undo());
			Assert.AreEqual(6, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsFalse(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.IsTrue(table.IsDeleted(r1));
			Assert.AreEqual(0, table.Rows.Count());

			// transaction 7 - redo
			Assert.IsTrue(store.Redo());
			Assert.AreEqual(7, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.AreEqual(1, table.GetField(r1, IntField.Field));

			// transaction 8 - redo
			Assert.IsTrue(store.Redo());
			Assert.AreEqual(8, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.AreEqual(2, table.GetField(r1, IntField.Field));

			// transaction 9 - redo
			Assert.IsTrue(store.Redo());
			Assert.AreEqual(9, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			Assert.AreEqual(3, table.GetField(r1, IntField.Field));

			// transaction 10 - undo
			Assert.IsTrue(store.Undo());
			Assert.AreEqual(10, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsTrue(store.CanRedo);
			Assert.AreEqual(2, table.GetField(r1, IntField.Field));

			// transaction 11 edit
			Assert.IsTrue(store.StartTransaction());
			Assert.IsFalse(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			data = 10;
			r2 = table.Insert(ref data);
			Assert.AreEqual(11, store.Commit());
			Assert.AreEqual(11, store.Version);
			Assert.AreEqual(store.Version, store.LatestAvailableVersion);
			Assert.IsTrue(store.CanUndo);
			Assert.IsFalse(store.CanRedo);
			Assert.AreEqual(2, table.Rows.Count());
		}
	}
}
