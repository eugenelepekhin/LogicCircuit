using System;
using System.Linq;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for CircuitProjectTest and is intended
	///to contain all CircuitProjectTest Unit Tests
	///</summary>
	[TestClass()]
	public class CircuitProjectTest {

		#region Additional test attributes
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

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

			Constant constant = project.ConstantSet.Create(3, 7);
			Assert.AreEqual(1, project.ConstantSet.Count());
			Assert.AreEqual(3, project.DevicePinSet.Count());

			CircuitButton button = project.CircuitButtonSet.Create("a");
			Assert.AreEqual(1, project.CircuitButtonSet.Count());
			Assert.AreEqual(4, project.DevicePinSet.Count());

			Memory rom = project.MemorySet.Create(false, 2, 8);
			Assert.AreEqual(1, project.MemorySet.Count());
			Assert.AreEqual(6, project.DevicePinSet.Count());

			Memory ram = project.MemorySet.Create(true, 4, 4);
			Assert.AreEqual(2, project.MemorySet.Count());
			Assert.AreEqual(10, project.DevicePinSet.Count());

			Splitter splitter = project.SplitterSet.Create(8, 4, CircuitRotation.Down);
			Assert.AreEqual(1, project.SplitterSet.Count());
			Assert.AreEqual(15, project.DevicePinSet.Count());

			CircuitSymbol symbol = project.CircuitSymbolSet.Create(constant, logicalCircuit, 10, 15);
			Assert.AreEqual(1, project.CircuitSymbolSet.Count());

			Wire wire = project.WireSet.Create(logicalCircuit, new GridPoint(10, 20), new GridPoint(30, 40));
			Assert.AreEqual(1, project.WireSet.Count());
		}
	}
}
