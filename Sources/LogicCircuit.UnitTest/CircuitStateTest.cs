using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LogicCircuit.UnitTest {


	/// <summary>
	///This is a test class for CircuitStateTest and is intended
	///to contain all CircuitStateTest Unit Tests
	///</summary>
	[TestClass()]
	public class CircuitStateTest {

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
		///A test for CircuitState Constructor
		///</summary>
		[TestMethod()]
		public void CircuitStateConstructorTest() {
			int reserveState = 3;
			CircuitState target = new CircuitState(reserveState);
			Assert.AreEqual(reserveState, target.Count);
		}

		/// <summary>
		///A test for EndDefinition
		///</summary>
		[TestMethod()]
		public void EndDefinitionTest() {
			ProjectTester.InitResources();
			CircuitProject project = CircuitProject.Create(null);
			CircuitButton button = null;
			CircuitSymbol buttonSymbol = null;
			Gate led = null;
			CircuitSymbol ledSymbol = null;

			project.InTransaction(() => {
				button = project.CircuitButtonSet.Create("b", false);
				buttonSymbol = project.CircuitSymbolSet.Create(button, project.ProjectSet.Project.LogicalCircuit, 1, 1);
				led = project.GateSet.Gate(GateType.Led, 1, false);
				ledSymbol = project.CircuitSymbolSet.Create(led, project.ProjectSet.Project.LogicalCircuit, 5, 1);
			});

			CircuitState target = new CircuitState(3);
			int buttonResult = target.ReserveState();
			FunctionButton functionButton = new FunctionButton(target, buttonSymbol, buttonResult);
			FunctionLed functionLed = new FunctionLed(target, ledSymbol, buttonResult);
			target.EndDefinition();

			Assert.IsTrue(functionButton.Dependant != null && functionButton.Dependant.Length == 1 && functionButton.Dependant[0] == functionLed);
			Assert.IsTrue(functionLed.Dependant == null);
		}
	}
}
