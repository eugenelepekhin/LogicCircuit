using DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class ChangeEnumeratorTest {
		private void Insert(TableSnapshot<int> table, params int[] list) {
			bool commit = (table.StoreSnapshot.SnapStore.Editor == null);
			if(commit) {
				Assert.IsTrue(table.StoreSnapshot.StartTransaction());
			}
			foreach(int v in list) {
				int value = v;
				table.Insert(ref value);
			}
			if(commit) {
				table.StoreSnapshot.Commit();
			}
		}

		[TestMethod]
		public void CtorTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			TableSnapshot<int> unchanged = new TableSnapshot<int>(store, "unchanged", IntArray.Field);
			store.FreezeShape();

			// version 0 will never has any changes
			IEnumerator<TableChange<int>> enumerator = table.GetChanges(store.Version);
			Assert.IsNull(enumerator);

			// lets make some changes
			bool started = store.StartTransaction();
			Assert.IsTrue(started);
			int data = 10;
			RowId rowId = table.Insert(ref data);
			Assert.AreEqual<int>(0, rowId.Value);
			store.Commit();

			// if there were no changes on the table during transaction then enumerator should be null
			enumerator = unchanged.GetChanges(store.Version);
			Assert.IsNull(enumerator);

			// if it was changes  for the table then enumerator should be created
			IEnumerator<TableChange<int>> changes = table.GetChanges(store.Version);
			Assert.IsNotNull(changes);

			// uncommitted changes also can be enumerated in the editor thread...
			started = store.StartTransaction();
			Assert.IsTrue(started);
			data = 20;
			table.Insert(ref data);
			changes = table.GetChanges(store.Version);
			Assert.IsNotNull(changes);
			Assert.IsTrue(changes.MoveNext());
			TableChange<int> c = changes.Current;
			Assert.AreEqual<int>(1, c.RowId.Value);
			Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, c.Action);
			Assert.IsFalse(changes.MoveNext());

			//... but not from any other thread...
			changes = null;
			Thread thread = new Thread(
				new ThreadStart(
					delegate () {
						Assert.Throws<InvalidOperationException>(() => changes = table.GetChanges(store.SnapStore.Version));
					}
				)
			);
			thread.Start();
			thread.Join();
			Assert.IsNull(changes);

			//...however committed changes can be enumerated from any thread
			thread = new Thread(
				new ThreadStart(
					() => changes = table.GetChanges(store.SnapStore.Version - 1)
				)
			);
			thread.Start();
			thread.Join();
			Assert.IsNotNull(changes);
		}

		[TestMethod]
		public void CurrentTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();
			Assert.AreEqual<int>(0, store.Version);

			// lets make some changes
			this.Insert(table, 20, 200, 2000);
			Assert.AreEqual<int>(1, store.Version);

			bool started = store.StartTransaction();
			Assert.IsTrue(started);
			bool updated = table.SetField<int>(new RowId(1), IntArray.Field, 333);
			Assert.IsTrue(updated);
			store.Commit();
			Assert.AreEqual<int>(2, store.Version);

			IEnumerator<TableChange<int>> enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);

			bool moved = enumerator.MoveNext();
			Assert.IsTrue(moved);
			//now it is the only change is current
			TableChange<int> change = enumerator.Current;
			Assert.AreEqual(1, change.RowId.Value);
			Assert.AreEqual(SnapTableAction.Update, change.Action);

			//this is the last change so:
			moved = enumerator.MoveNext();
			Assert.IsFalse(moved);
		}

		[TestMethod]
		public void MoveNextTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			// lets make some changes
			// version 1
			this.Insert(table, 20, 200, 2000);
			Assert.AreEqual<int>(1, store.Version);

			// version 2
			bool started = store.StartTransaction();
			Assert.IsTrue(started);
			bool updated = table.SetField<int>(new RowId(1), IntArray.Field, 333);
			Assert.IsTrue(updated);
			store.Commit();
			Assert.AreEqual<int>(2, store.Version);

			IEnumerator<TableChange<int>> enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);

			// version 2 has one change so first move is available
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual<int>(1, enumerator.Current.RowId.Value);
			Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
			// and second is not
			Assert.IsFalse(enumerator.MoveNext());
			// as well as all others
			for(int i = 0; i < 1000; i++) {
				Assert.IsFalse(enumerator.MoveNext());
			}

			// lets enumerate first transaction it has 3 changes all are inserts
			enumerator = null;
			Assert.IsNull(enumerator);
			enumerator = table.GetChanges(1);
			Assert.IsNotNull(enumerator);
			for(int i = 0; i < 3; i++) {
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual<int>(i, enumerator.Current.RowId.Value);
				Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
			}

			// check how multiple updates get enumerated
			// version 3
			started = store.StartTransaction();
			Assert.IsTrue(started);
			for(int i = 0; i < 3; i++) {
				updated = table.SetField<int>(new RowId(i), IntArray.Field, 30 * (int)Math.Pow(10, i));
				Assert.IsTrue(updated);
			}
			store.Commit();
			enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);
			for(int i = 0; i < 3; i++) {
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual<int>(i, enumerator.Current.RowId.Value);
				Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
			}

			// check mixed editing enumeration
			started = store.StartTransaction();
			Assert.IsTrue(started);

			//insert row 3
			int data = 40;
			Assert.AreEqual<int>(3, table.Insert(ref data).Value);
			//update row 1
			Assert.IsTrue(table.SetField<int>(new RowId(1), IntArray.Field, 500));
			//insert row 4
			data = 400;
			Assert.AreEqual<int>(4, table.Insert(ref data).Value);
			//update row 2
			Assert.IsTrue(table.SetField<int>(new RowId(2), IntArray.Field, 5000));
			//insert row 5
			data = 4000;
			Assert.AreEqual<int>(5, table.Insert(ref data).Value);
			//now delete just inserted row 4
			table.Delete(new RowId(4));
			store.Commit();
			//get changes
			enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);
			HashSet<int> changedRows = new HashSet<int>();
			while(enumerator.MoveNext()) {
				Assert.IsTrue(changedRows.Add(enumerator.Current.RowId.Value), "each row should enumerated just once");
				switch(enumerator.Current.RowId.Value) {
				case 3:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
					break;
				case 1:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 4:
					Assert.Fail("this change should not be enumerated");
					break;
				case 2:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 5:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
					break;
				}
			}
			Assert.AreEqual<int>(4, changedRows.Count, "all changes should be enumerated");

			// undo previous transaction
			store.Undo();
			enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);
			changedRows.Clear();
			while(enumerator.MoveNext()) {
				Assert.IsTrue(changedRows.Add(enumerator.Current.RowId.Value), "each row should enumerated just once");
				switch(enumerator.Current.RowId.Value) {
				case 3:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Delete, enumerator.Current.Action);
					break;
				case 1:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 4:
					Assert.Fail("this change should not be enumerated");
					break;
				case 2:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 5:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Delete, enumerator.Current.Action);
					break;
				}
			}
			Assert.AreEqual<int>(4, changedRows.Count, "all changes should be enumerated");

			//redo previous changes
			store.Redo();
			enumerator = table.GetChanges(store.Version);
			Assert.IsNotNull(enumerator);
			changedRows.Clear();
			while(enumerator.MoveNext()) {
				Assert.IsTrue(changedRows.Add(enumerator.Current.RowId.Value), "each row should enumerated just once");
				switch(enumerator.Current.RowId.Value) {
				case 3:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
					break;
				case 1:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 4:
					Assert.Fail("this change should not be enumerated");
					break;
				case 2:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Update, enumerator.Current.Action);
					break;
				case 5:
					Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
					break;
				}
			}
			Assert.AreEqual<int>(4, changedRows.Count, "all changes should be enumerated");
		}
	}
}
