using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class SnapTableTest {
		private Pin CreatePin(LogicalCircuit circuit) {
			Pin pin = circuit.CircuitProject.PinSet.Create(circuit, PinType.Input, 1);
			circuit.CircuitProject.CircuitSymbolSet.Create(pin, circuit, 10, 20);
			return pin;
		}

		[TestMethod]
		public void ConstructorTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			Assert.AreEqual(store.SnapStore, table.StoreSnapshot.SnapStore);
			Assert.AreEqual<string>("test", table.Name);
			Assert.AreEqual(table, store.Table("test"));

			// we can't create table with the same name
			store = new StoreSnapshot();
			table = new TableSnapshot<int>(store, "test", IntArray.Field);
			TableSnapshot<int> table2 = null;
			Assert.Throws<ArgumentException>(() => table2 = new TableSnapshot<int>(store, "test", IntArray.Field));
			Assert.IsNull(table2);

			// but we can create table with different name
			TableSnapshot<int> table3 = new TableSnapshot<int>(store, "other", IntArray.Field);
			store.FreezeShape();
			Assert.AreEqual(store.SnapStore, table3.StoreSnapshot.SnapStore);
			Assert.AreEqual<string>("other", table3.Name);
			Assert.AreEqual(table3, store.Table("other"));
		}

		[TestMethod]
		public void PushToLogTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			bool started = store.StartTransaction();
			Assert.IsTrue(started);

			// first insert should create a new version record
			int data = 1;
			RowId rowId0 = table.Insert(ref data);
			Assert.AreEqual<int>(0, rowId0.Value);
			data = 10;
			RowId rowId1 = table.Insert(ref data);
			Assert.AreEqual<int>(1, rowId1.Value);
			store.Commit();

			started = store.StartTransaction();
			Assert.IsTrue(started);

			//first update should create a new version record as well as new log record
			bool updated = table.SetField<int>(rowId0, IntArray.Field, 2);
			Assert.IsTrue(updated);

			//next update to the same record will not change log.
			updated = table.SetField<int>(rowId0, IntArray.Field, 3);
			Assert.IsTrue(updated);

			//update to other record will change the log
			updated = table.SetField<int>(rowId1, IntArray.Field, 20);
			Assert.IsTrue(updated);
		}

		[TestMethod]
		public void ValidationTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			bool started = store.StartTransaction();
			Assert.IsTrue(started);
			int data = 5;
			RowId rowId = table.Insert(ref data);
			store.Commit();

			//attempt to edit without transaction should throw
			rowId = new RowId(-1);
			Assert.Throws<InvalidOperationException>(() => { int d = 123; rowId = table.Insert(ref d); });
			Assert.AreEqual<int>(-1, rowId.Value);

			started = store.StartTransaction();
			Assert.IsTrue(started);

			// attempt to edit from other thread should throw
			{
				Exception error = null;
				Thread thread = new Thread(
					new ThreadStart(
						delegate () {
							try {
								rowId = table.Insert(ref data);
								Assert.Fail("previous insert should throw");
							} catch(InvalidOperationException e) {
								error = e;
							} catch(Exception) {
								Assert.Fail("wrong exception");
							}
						}
					)
				);
				rowId = new RowId(-1);
				thread.Start();
				thread.Join();
				Assert.IsNotNull(error);
				Assert.AreEqual<int>(-1, rowId.Value);
			}

			//modification of deleted row should throw
			table.Delete(new RowId(0));
			Assert.IsTrue(table.IsDeleted(new RowId(0)));
			Assert.Throws<ArgumentOutOfRangeException>(() => table.SetField(new RowId(0), IntArray.Field, 1000));
		}

		[TestMethod]
		public void UndoRedoTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			//version 1
			Assert.IsTrue(store.StartTransaction());
			int data = 1;
			Assert.AreEqual<int>(0, table.Insert(ref data).Value);
			data = 20;
			Assert.AreEqual<int>(1, table.Insert(ref data).Value);
			data = 3;
			Assert.AreEqual<int>(2, table.Insert(ref data).Value);
			store.Commit();

			//version 2
			Assert.IsTrue(store.StartTransaction());
			Assert.IsTrue(table.SetField<int>(new RowId(1), IntArray.Field, 21));
			store.Commit();

			//version 3
			Assert.IsTrue(store.StartTransaction());
			Assert.IsTrue(table.SetField<int>(new RowId(1), IntArray.Field, 22));
			store.Commit();

			//undo, version 4
			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(4, store.Version);
			Assert.AreEqual<int>(21, table.GetField<int>(new RowId(1), IntArray.Field));

			//redo, version 5
			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(5, store.Version);
			Assert.AreEqual<int>(22, table.GetField<int>(new RowId(1), IntArray.Field));

			//undo, version 6
			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(6, store.Version);
			Assert.AreEqual<int>(21, table.GetField<int>(new RowId(1), IntArray.Field));

			//undo, version 7
			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(7, store.Version);
			Assert.AreEqual<int>(20, table.GetField<int>(new RowId(1), IntArray.Field));

			//redo, version 8
			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(8, store.Version);
			Assert.AreEqual<int>(21, table.GetField<int>(new RowId(1), IntArray.Field));

			//redo, version 9
			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(9, store.Version);
			Assert.AreEqual<int>(22, table.GetField<int>(new RowId(1), IntArray.Field));

			//undo, version 10
			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(10, store.Version);
			Assert.AreEqual<int>(21, table.GetField<int>(new RowId(1), IntArray.Field));

			//a regular operation should wipe out the last possible redo still sitting on top of the stack
			//version 11
			Assert.IsTrue(store.StartTransaction());
			Assert.IsTrue(table.SetField<int>(new RowId(1), IntArray.Field, 200));
			store.Commit();
			Assert.AreEqual<int>(11, store.Version);
			Assert.AreEqual<int>(200, table.GetField<int>(new RowId(1), IntArray.Field));


			//undo, version 12
			Assert.IsTrue(store.Undo());
			Assert.AreEqual<int>(12, store.Version);
			Assert.AreEqual<int>(21, table.GetField<int>(new RowId(1), IntArray.Field));

			//redo, version 13
			Assert.IsTrue(store.Redo());
			Assert.AreEqual<int>(13, store.Version);
			Assert.AreEqual<int>(200, table.GetField<int>(new RowId(1), IntArray.Field));

			// check undo of insertion
			Assert.IsTrue(store.StartTransaction());
			data = 1000;
			RowId r1 = table.Insert(ref data);
			store.Commit();
			Assert.AreEqual<int>(data, table.GetField<int>(r1, IntArray.Field));
			Assert.IsFalse(table.IsDeleted(r1));

			Assert.IsTrue(store.Undo());
			Assert.IsTrue(table.IsDeleted(r1));

			Assert.IsTrue(store.Undo());
			Assert.IsTrue(table.IsDeleted(r1));
		}

		[TestMethod]
		public void OldRollbackTest() { // this test is coming from the old code base
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			//version 1
			Assert.IsTrue(store.StartTransaction());
			int data = 1;
			Assert.AreEqual<int>(0, table.Insert(ref data).Value);
			data = 20;
			Assert.AreEqual<int>(1, table.Insert(ref data).Value);
			data = 3;
			Assert.AreEqual<int>(2, table.Insert(ref data).Value);
			store.Rollback();

			Assert.AreEqual(0, table.Count());
			Assert.IsTrue(table.IsDeleted(new RowId(0)));
			Assert.IsTrue(table.IsDeleted(new RowId(1)));
			Assert.IsTrue(table.IsDeleted(new RowId(2)));

			// version 2
			Assert.IsTrue(store.StartTransaction());
			data = 1;
			Assert.AreEqual<int>(3, table.Insert(ref data).Value);
			data = 20;
			Assert.AreEqual<int>(4, table.Insert(ref data).Value);
			data = 3;
			Assert.AreEqual<int>(5, table.Insert(ref data).Value);
			store.Commit();

			// version 3
			Assert.IsTrue(store.StartTransaction());
			data = 10;
			Assert.AreEqual<int>(6, table.Insert(ref data).Value);
			Assert.IsTrue(table.SetField<int>(new RowId(4), IntArray.Field, 5000));
			table.Delete(new RowId(3));
			store.Rollback();

			Assert.AreEqual<int>(3, table.Count());
			Assert.IsTrue(table.IsDeleted(new RowId(6)));
			Assert.AreEqual<int>(20, table.GetField<int>(new RowId(4), IntArray.Field));
			Assert.IsFalse(table.IsDeleted(new RowId(3)));
		}

		[TestMethod]
		public void InsertTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;

			Assert.AreEqual(0, circuit.Pins.Count());

			List<Pin> pins = new List<Pin>();

			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					Pin pin = this.CreatePin(circuit);
					pins.Add(pin);
				}
			});

			pins.Sort(PinComparer.Comparer);

			List<Pin> actual = circuit.Pins.Select(p => (Pin)p).ToList();
			actual.Sort(PinComparer.Comparer);

			CollectionAssert.AreEqual(pins, actual);
		}

		[TestMethod]
		public void DeleteTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			List<Pin> pins = new List<Pin>();

			project.InTransaction(() => {
				for(int i = 0; i < 7; i++) {
					Pin pin = this.CreatePin(circuit);
					pins.Add(pin);
				}
			});

			pins.Sort(PinComparer.Comparer);
			Pin deleted = pins[TestHelper.Random.Next(0, pins.Count - 1)];
			project.InTransaction(() => {
				deleted.Delete();
			});
			Assert.IsTrue(deleted.IsDeleted());
			List<Pin> actual = circuit.Pins.Select(p => (Pin)p).ToList();
			actual.Sort(PinComparer.Comparer);
			pins.Remove(deleted);
			CollectionAssert.AreEqual(pins, actual);
		}

		[TestMethod]
		public void IsDeletedTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			List<Pin> pins = new List<Pin>();
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					Pin pin = this.CreatePin(circuit);
					pins.Add(pin);
				}
			});
			pins.Sort(PinComparer.Comparer);
			Pin deleted = pins[TestHelper.Random.Next(0, pins.Count - 1)];
			Assert.IsFalse(deleted.IsDeleted());
			project.InTransaction(() => {
				deleted.Delete();
			});
			Assert.IsTrue(deleted.IsDeleted());
			StoreSnapshot snapshot = new StoreSnapshot(project);
			project.Undo();
			Assert.IsFalse(deleted.IsDeleted());
			ITableSnapshot table = snapshot.Table("Circuit");
			TableSnapshot<CircuitData> pinSnapshot = (TableSnapshot<CircuitData>)table;
			Assert.IsTrue(pinSnapshot.IsDeleted(deleted.CircuitRowId));
			project.Redo();
			Assert.IsTrue(pinSnapshot.IsDeleted(deleted.CircuitRowId));
		}

		[TestMethod]
		public void SetFieldTest() {
			CircuitProject circuitProject = CircuitProject.Create(null);
			Project project = circuitProject.ProjectSet.Project;
			string newName = "Test Project";
			TableSnapshot<ProjectData> table = (TableSnapshot<ProjectData>)circuitProject.Table("Project");
			bool modified = false;
			circuitProject.InTransaction(() => {
				modified = table.SetField(project.ProjectRowId, ProjectData.NameField.Field, newName);
			});
			Assert.IsTrue(modified);
			Assert.AreEqual(newName, project.Name);
			circuitProject.InTransaction(() => {
				modified = table.SetField(project.ProjectRowId, ProjectData.NameField.Field, newName);
			});
			Assert.IsFalse(modified);
		}

		[TestMethod]
		public void GetFieldTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Pin pin = null;
			project.InTransaction(() => {
				pin = this.CreatePin(circuit);
			});
			Assert.IsNotNull(pin);
			string originalName = pin.Name;
			string newName = "Test";
			project.InTransaction(() => {
				pin.Name = newName;
			});
			StoreSnapshot snapshot = new StoreSnapshot(project);
			TableSnapshot<PinData> table = (TableSnapshot<PinData>)snapshot.Table("Pin");
			// get latest field
			Assert.AreEqual(newName, table.GetField<string>(pin.PinRowId, PinData.NameField.Field));

			project.Undo();
			project.Redo();
			project.Undo();

			// get old data
			Assert.AreEqual(newName, table.GetField<string>(pin.PinRowId, PinData.NameField.Field));
			Assert.AreEqual(originalName, pin.Name);

			// check for exception on get deleted data on latest snapshot
			project.InTransaction(() => {
				pin.Delete();
			});
			snapshot = new StoreSnapshot(project);
			table = (TableSnapshot<PinData>)snapshot.Table("Pin");
			string value = null;
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				value = table.GetField<string>(pin.PinRowId, PinData.NameField.Field);
			});
			Assert.IsNull(value);
			// check for exception on get deleted data on previous snapshot
			project.Undo();
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				value = table.GetField<string>(pin.PinRowId, PinData.NameField.Field);
			});
			Assert.IsNull(value);
		}


		[TestMethod]
		public void GetDataTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Pin pin = null;
			project.InTransaction(() => {
				pin = this.CreatePin(circuit);
			});
			Assert.IsNotNull(pin);
			string originalName = pin.Name;
			string newName = "Test";
			project.InTransaction(() => {
				pin.Name = newName;
			});
			StoreSnapshot snapshot = new StoreSnapshot(project);
			TableSnapshot<PinData> table = (TableSnapshot<PinData>)snapshot.Table("Pin");
			PinData data;
			// get latest data
			table.GetData(pin.PinRowId, out data);
			Assert.AreEqual(newName, data.Name);

			project.Undo();
			project.Redo();
			project.Undo();

			// get old data
			table.GetData(pin.PinRowId, out data);
			Assert.AreEqual(newName, data.Name);
			Assert.AreEqual(originalName, pin.Name);

			// check for exception on get deleted data on latest snapshot
			project.InTransaction(() => {
				pin.Delete();
			});
			snapshot = new StoreSnapshot(project);
			table = (TableSnapshot<PinData>)snapshot.Table("Pin");
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				table.GetData(pin.PinRowId, out data);
			});
			// check for exception on get deleted data on previous snapshot
			project.Undo();
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				table.GetData(pin.PinRowId, out data);
			});
		}

		[TestMethod]
		public void RollbackTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			List<Pin> pins = [];
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					pins.Add(this.CreatePin(circuit));
				}
			});
			Pin pinModified = pins[pins.Count / 2];
			Pin pinDeleted = pins[pins.Count / 2 + 1];
			Assert.AreNotEqual(pinModified, pinDeleted);
			Pin pinCreated = null;
			string originalName = pinModified.Name;
			string newName = "Test";

			Assert.Throws<Exception>(() => project.InTransaction(() => {
				pinCreated = this.CreatePin(circuit);
				pinModified.Name = newName;
				pinDeleted.Delete();
				throw new Exception("Rollback test exception");
			}));

			Assert.AreEqual(originalName, pinModified.Name);
			Assert.IsFalse(pinDeleted.IsDeleted());
			Assert.IsNotNull(pinCreated);
			Assert.IsTrue(pinCreated.IsDeleted());
		}

		[TestMethod]
		public void RevertTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			List<Pin> pins = new List<Pin>();
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					pins.Add(this.CreatePin(circuit));
				}
			});
			Pin pinModified = pins[pins.Count / 2];
			Pin pinDeleted = pins[pins.Count / 2 + 1];
			Assert.AreNotEqual(pinModified, pinDeleted);
			Pin pinCreated = null;
			string originalName = pinModified.Name;
			string newName = "Test";
			project.InTransaction(() => {
				pinCreated = this.CreatePin(circuit);
				pinModified.Name = newName;
				pinDeleted.Delete();
			});

			project.Undo();
			Assert.AreEqual(originalName, pinModified.Name);
			Assert.IsFalse(pinDeleted.IsDeleted());
			Assert.IsNotNull(pinCreated);
			Assert.IsTrue(pinCreated.IsDeleted());

			project.Redo();
			project.Undo();
			project.Undo();
			project.Redo();
			project.Redo();
			Assert.AreEqual(newName, pinModified.Name);
			Assert.IsTrue(pinDeleted.IsDeleted());
			Assert.IsNotNull(pinCreated);
			Assert.IsFalse(pinCreated.IsDeleted());
		}
	}
}
