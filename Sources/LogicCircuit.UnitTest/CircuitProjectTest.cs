﻿using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for CircuitProjectTest and is intended
	///to contain all CircuitProjectTest Unit Tests
	///</summary>
	[TestClass()]
	public class CircuitProjectTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// A test for CircuitProject Constructor
		/// </summary>
		[TestMethod()]
		public void CircuitProjectCreateTest() {
			CircuitProject target = new CircuitProject();
			Assert.IsTrue(target.IsFrozen);
			Assert.IsFalse(target.ProjectSet.Any());
			Assert.IsFalse(target.CircuitSet.Any());
			Assert.IsFalse(target.DevicePinSet.Any());
			Assert.IsFalse(target.GateSet.Any());
			Assert.IsFalse(target.LogicalCircuitSet.Any());
			Assert.IsFalse(target.PinSet.Any());
			Assert.IsFalse(target.ConstantSet.Any());
			Assert.IsFalse(target.CircuitButtonSet.Any());
			Assert.IsFalse(target.MemorySet.Any());
			Assert.IsFalse(target.SplitterSet.Any());
			Assert.IsFalse(target.CircuitSymbolSet.Any());
			Assert.IsFalse(target.WireSet.Any());

			Assert.IsTrue(target.StartTransaction());
		}

		/// <summary>
		/// A test of creating of items in the store
		/// </summary>
		[TestMethod()]
		public void CircuitProjectCreateItemsTest() {
			CircuitProject project = new CircuitProject();
			Assert.IsTrue(project.StartTransaction());
			LogicalCircuit logicalCircuit = project.LogicalCircuitSet.Create();
			Pin pin1 = project.PinSet.Create(logicalCircuit, PinType.Input, 1);
			Pin pin2 = project.PinSet.Create(logicalCircuit, PinType.Output, 10);
			Assert.AreEqual(1, project.LogicalCircuitSet.Count());
			Assert.AreEqual(2, project.PinSet.Count());
			Assert.AreEqual(2, project.DevicePinSet.Count());

			Constant constant = project.ConstantSet.Create(3, 7, PinSide.Right);
			Assert.AreEqual(1, project.ConstantSet.Count());
			Assert.AreEqual(3, project.DevicePinSet.Count());

			CircuitButton button = project.CircuitButtonSet.Create("a", false, PinSide.Right);
			Assert.AreEqual(1, project.CircuitButtonSet.Count());
			Assert.AreEqual(4, project.DevicePinSet.Count());

			Memory rom = project.MemorySet.Create(false, 2, 8);
			Assert.AreEqual(1, project.MemorySet.Count());
			Assert.AreEqual(8, project.DevicePinSet.Count());

			Memory ram = project.MemorySet.Create(true, 4, 4);
			Assert.AreEqual(2, project.MemorySet.Count());
			Assert.AreEqual(14, project.DevicePinSet.Count());

			Splitter splitter = project.SplitterSet.Create(8, 4, true);
			Assert.AreEqual(1, project.SplitterSet.Count());
			Assert.AreEqual(19, project.DevicePinSet.Count());

			CircuitSymbol symbol = project.CircuitSymbolSet.Create(constant, logicalCircuit, 10, 15);
			Assert.AreEqual(1, project.CircuitSymbolSet.Count());

			Wire wire = project.WireSet.Create(logicalCircuit, new GridPoint(10, 20), new GridPoint(30, 40));
			Assert.AreEqual(1, project.WireSet.Count());
		}

		/// <summary>
		/// A test of saving/loading roundtrip
		/// </summary>
		[STATestMethod]
		[DeploymentItem("Properties\\Digital Clock.CircuitProject")]
		public void CircuitProjectSaveLoadTest() {
			string dir = Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName + DateTime.UtcNow.Ticks, "Some Test Sub Directory");
			string file = Path.Combine(dir, "My Test File.CircuitProject");

			// save in inexistent folder
			CircuitProject project1 = CircuitProject.Create(null);
			project1.InTransaction(() => {
				LogicalCircuit main = project1.ProjectSet.Project.LogicalCircuit;
				CircuitButton button = project1.CircuitButtonSet.Create("b", false, PinSide.Right);
				CircuitSymbol buttonSymbol = project1.CircuitSymbolSet.Create(button, main, 1, 2);
				Gate led = project1.GateSet.Gate(GateType.Led, 1, false);
				project1.CircuitSymbolSet.Create(led, main, 6, 2);
				Wire wire = project1.WireSet.Create(main, new GridPoint(3, 3), new GridPoint(6, 3));
			});
			Assert.IsTrue(!Directory.Exists(dir));
			project1.Save(file);
			Assert.IsTrue(File.Exists(file));
			CircuitProject project2 = CircuitProject.Create(file);
			Assert.IsTrue(ProjectTester.Equal(project1, project2));

			// save in existing folder and existing file.
			CircuitProject project3 = ProjectTester.LoadDeployedFile(this.TestContext, "Digital Clock.CircuitProject", null);
			Assert.IsTrue(File.Exists(file));
			project3.Save(file);
			CircuitProject project4 = CircuitProject.Create(file);
			File.Delete(file);
			Assert.IsTrue(ProjectTester.Equal(project3, project4));
		}
	}
}
