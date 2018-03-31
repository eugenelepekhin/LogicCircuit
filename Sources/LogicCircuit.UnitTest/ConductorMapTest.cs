using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for ConductorMap and is intended
	/// to contain all ConductorMap and Conductor Unit Tests
	/// </summary>
	[TestClass]
	public class ConductorMapTest {
		[TestMethod]
		public void ConductorMapEmptyTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			ConductorMap target = new ConductorMap(circuit);

			Assert.AreEqual(0, target.Conductors.Count());
		}

		[TestMethod]
		public void ConductorMapOneWireTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Wire wire = null;
			project.InTransaction(() => wire = project.WireSet.Create(circuit, new GridPoint(1, 2), new GridPoint(3, 4)));
			Assert.IsNotNull(wire);
			ConductorMap target = new ConductorMap(circuit);

			Assert.AreEqual(1, target.Conductors.Count());
			Conductor conductor;
			bool success = target.TryGetValue(new GridPoint(3, 4), out conductor);
			Assert.IsTrue(success);
			Assert.AreEqual(1, conductor.Wires.Count());
			Assert.AreSame(wire, conductor.Wires.First());
		}

		[TestMethod]
		public void ConductorMapTwoWiresOneConductorTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Wire wire1 = null;
			Wire wire2 = null;
			project.InTransaction(() => wire1 = project.WireSet.Create(circuit, new GridPoint(1, 2), new GridPoint(3, 4)));
			project.InTransaction(() => wire2 = project.WireSet.Create(circuit, new GridPoint(5, 6), new GridPoint(3, 4)));
			Assert.IsNotNull(wire1);
			Assert.IsNotNull(wire2);

			ConductorMap target = new ConductorMap(circuit);

			Assert.AreEqual(1, target.Conductors.Count());

			Conductor conductor;
			Assert.IsTrue(target.TryGetValue(new GridPoint(3, 4), out conductor));
			Assert.AreEqual(2, conductor.Wires.Count());
			Assert.AreEqual(3, conductor.Points.Count());
		}

		[TestMethod]
		public void ConductorMapTwoWiresTwoConductorTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Wire wire1 = null;
			Wire wire2 = null;
			project.InTransaction(() => wire1 = project.WireSet.Create(circuit, new GridPoint(1, 2), new GridPoint(3, 4)));
			project.InTransaction(() => wire2 = project.WireSet.Create(circuit, new GridPoint(5, 6), new GridPoint(7, 8)));
			Assert.IsNotNull(wire1);
			Assert.IsNotNull(wire2);

			ConductorMap target = new ConductorMap(circuit);

			Assert.AreEqual(2, target.Conductors.Count());

			Conductor conductor1;
			Assert.IsTrue(target.TryGetValue(new GridPoint(3, 4), out conductor1));
			Assert.AreEqual(1, conductor1.Wires.Count());
			Assert.AreEqual(2, conductor1.Points.Count());

			Conductor conductor2;
			Assert.IsTrue(target.TryGetValue(new GridPoint(3, 4), out conductor2));
			Assert.AreEqual(1, conductor2.Wires.Count());
			Assert.AreEqual(2, conductor2.Points.Count());
		}

		[TestMethod]
		public void ConductorMapThreeWiresJoinTest() {
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			Wire wire1 = null;
			Wire wire2 = null;
			Wire wire3 = null;
			project.InTransaction(() => wire1 = project.WireSet.Create(circuit, new GridPoint(1, 2), new GridPoint(3, 4)));
			project.InTransaction(() => wire2 = project.WireSet.Create(circuit, new GridPoint(5, 6), new GridPoint(7, 8)));

			ConductorMap target0 = new ConductorMap(circuit);
			Assert.AreEqual(2, target0.Conductors.Count());

			project.InTransaction(() => wire3 = project.WireSet.Create(circuit, new GridPoint(5, 6), new GridPoint(1, 2)));

			ConductorMap target = new ConductorMap(circuit);

			Assert.AreEqual(1, target.Conductors.Count());
			Conductor conductor;
			Assert.IsTrue(target.TryGetValue(new GridPoint(7, 8), out conductor));
			Assert.AreEqual(3, conductor.Wires.Count());
			Assert.AreEqual(4, conductor.Points.Count());
		}
	}
}
