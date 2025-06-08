using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class SnapStoreTest {
		[TestMethod]
		public void StartTransactionTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Assert.IsTrue(project.StartTransaction());
			StoreSnapshot snapshot = new StoreSnapshot(project);
			Assert.IsFalse(snapshot.StartTransaction());
			Assert.IsFalse(project.StartTransaction());
			Assert.IsFalse(project.Undo());
		}

		[TestMethod]
		public void OmitTransactionTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Assert.IsFalse(project.Undo());
			project.InOmitTransaction(() => {
				circuit.Name = "Hello, world!";
			});
			Assert.IsFalse(project.Undo());
		}

		[TestMethod]
		public void RollbackTransactionTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			string originalName = circuit.Name;
			Assert.IsTrue(project.StartTransaction());
			circuit.Name = "Hello, world!";
			project.Rollback();
			Assert.AreEqual(originalName, circuit.Name);
			Assert.IsFalse(project.Undo());
		}

		[TestMethod]
		public void UndoRedoTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			string originalName = circuit.Name;
			string newName = "Hello, world!";
			project.InTransaction(() => {
				circuit.Name = newName;
			});
			Assert.AreEqual(newName, circuit.Name);
			Assert.IsTrue(project.Undo());
			Assert.IsFalse(project.IsEditor);
			Assert.IsFalse(project.Undo());
			Assert.AreEqual(originalName, circuit.Name);
			Assert.IsTrue(project.Redo());
			Assert.IsFalse(project.IsEditor);
			Assert.AreEqual(newName, circuit.Name);

			Assert.IsFalse(project.Redo());
		}
	}
}
