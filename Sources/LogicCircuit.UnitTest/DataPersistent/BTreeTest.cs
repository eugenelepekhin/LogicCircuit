namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class BTreeTest {
		private Pin CreatePin(LogicalCircuit circuit) {
			Pin pin = circuit.CircuitProject.PinSet.Create(circuit, PinType.Input, 1);
			circuit.CircuitProject.CircuitSymbolSet.Create(pin, circuit, 10, 20);
			return pin;
		}

		[TestMethod]
		public void BTreeInsertTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit1 = project.ProjectSet.Project.LogicalCircuit;
			LogicalCircuit circuit2 = null;
			List<Pin> pins1 = new List<Pin>();
			List<Pin> pins2 = new List<Pin>();
			project.InTransaction(() => {
				circuit2 = project.LogicalCircuitSet.Create();
				for(int i = 0; i < 10; i++) {
					pins1.Add(this.CreatePin(circuit1));
					pins2.Add(this.CreatePin(circuit2));
				}
			});
			Assert.IsNotNull(circuit2);
			List<Pin> actual1 = circuit1.Pins.Select(p => (Pin)p).ToList();
			List<Pin> actual2 = circuit2.Pins.Select(p => (Pin)p).ToList();
			pins1.Sort(PinComparer.Comparer);
			pins2.Sort(PinComparer.Comparer);
			actual1.Sort(PinComparer.Comparer);
			actual2.Sort(PinComparer.Comparer);
			CollectionAssert.AreEqual(pins1, actual1);
			CollectionAssert.AreEqual(pins2, actual2);
		}

		[TestMethod]
		public void BTreeDeleteTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit1 = project.ProjectSet.Project.LogicalCircuit;
			LogicalCircuit circuit2 = null;
			List<Pin> pins1 = new List<Pin>();
			List<Pin> pins2 = new List<Pin>();
			project.InTransaction(() => {
				circuit2 = project.LogicalCircuitSet.Create();
				for(int i = 0; i < 10; i++) {
					pins1.Add(this.CreatePin(circuit1));
					pins2.Add(this.CreatePin(circuit2));
				}
			});
			Assert.IsNotNull(circuit2);

			project.InTransaction(() => {
				foreach(Pin pin in pins2.Reverse<Pin>()) {
					pin.Delete();
				}
			});
			Assert.AreEqual(0, circuit2.Pins.Count());

			project.InTransaction(() => {
				foreach(Pin pin in pins1.Reverse<Pin>()) {
					pin.Delete();
				}
			});
			Assert.AreEqual(0, circuit1.Pins.Count());

			pins1.Clear();
			pins2.Clear();
			project.InTransaction(() => {
				circuit2 = project.LogicalCircuitSet.Create();
				for(int i = 0; i < 10; i++) {
					pins1.Add(this.CreatePin(circuit1));
					pins2.Add(this.CreatePin(circuit2));
				}
			});

			project.InTransaction(() => {
				foreach(Pin pin in pins2) {
					pin.Delete();
				}
			});
			Assert.AreEqual(0, circuit2.Pins.Count());

			project.InTransaction(() => {
				foreach(Pin pin in pins1) {
					pin.Delete();
				}
			});
			Assert.AreEqual(0, circuit1.Pins.Count());
		}

		[TestMethod]
		public void ExistsTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit1 = project.ProjectSet.Project.LogicalCircuit;
			LogicalCircuit circuit2 = null;
			List<Pin> pins = new List<Pin>();

			Assert.IsFalse(project.PinSet.Table.Exists(PinData.CircuitIdField.Field, circuit1.CircuitId));

			project.InTransaction(() => {
				circuit2 = project.LogicalCircuitSet.Create();
				for(int i = 0; i < 10; i++) {
					pins.Add(this.CreatePin(circuit1));
				}
			});
			Assert.IsNotNull(circuit2);

			Assert.IsTrue(project.PinSet.Table.Exists(PinData.CircuitIdField.Field, circuit1.CircuitId));
			Assert.IsFalse(project.PinSet.Table.Exists(PinData.CircuitIdField.Field, circuit2.CircuitId));
		}
	}
}
