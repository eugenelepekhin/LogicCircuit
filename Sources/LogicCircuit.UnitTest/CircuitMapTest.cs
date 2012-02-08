using System;
using System.Diagnostics;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for CircuitMapTest and is intended
	///to contain all CircuitMapTest Unit Tests
	///</summary>
	[TestClass()]
	public class CircuitMapTest {

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
		///A test for Apply
		///</summary>
		[TestMethod()]
		public void CircuitMapApplyPerfTest() {
			CircuitProject circuitProject = ProjectTester.Load(this.TestContext, Properties.Resources.IntegerCalculator, "Test Computer");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int max = 50;
			for(int i = 0; i < max; i++) {
				CircuitMap circuitMap = new CircuitMap(circuitProject.ProjectSet.Project.LogicalCircuit);
				Assert.IsNotNull(circuitMap);
				CircuitState circuitState = circuitMap.Apply(CircuitRunner.HistorySize);
				Assert.IsNotNull(circuitState);
				circuitMap.TurnOn();
			}
			stopwatch.Stop();
			this.TestContext.WriteLine("{0} CircuitMap(s) created applied in {1} - {2:N2} sec per each map", max, stopwatch.Elapsed, stopwatch.Elapsed.TotalSeconds / max);
			Assert.IsTrue(stopwatch.Elapsed < new TimeSpan(0, 0, 15), "CircuitMap was created and applied too slow");
		}
	}
}
