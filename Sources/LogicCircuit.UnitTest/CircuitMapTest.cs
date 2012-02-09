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

		private void CircuitMapPerfTest(string project, string initialCircuit, int maxCount, int maxSeconds) {
			CircuitProject circuitProject = ProjectTester.Load(this.TestContext, project, initialCircuit);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for(int i = 0; i < maxCount; i++) {
				CircuitMap circuitMap = new CircuitMap(circuitProject.ProjectSet.Project.LogicalCircuit);
				CircuitState circuitState = circuitMap.Apply(CircuitRunner.HistorySize);
				Assert.IsNotNull(circuitState);
				circuitMap.TurnOn();
			}
			stopwatch.Stop();
			this.TestContext.WriteLine("{0} CircuitMap(s) created and applied in {1} - {2:N2} sec per each map", maxCount, stopwatch.Elapsed, stopwatch.Elapsed.TotalSeconds / maxCount);
			Assert.IsTrue(stopwatch.Elapsed < new TimeSpan(0, 0, maxSeconds), "CircuitMap was created and applied successfully but too slow");
		}

		/// <summary>
		///A test for Apply
		///</summary>
		[TestMethod()]
		public void CircuitMapApplyPerfTest1() {
			this.CircuitMapPerfTest(Properties.Resources.IntegerCalculator, "Test Computer", 50, 15);
		}

		/// <summary>
		///A test for Apply
		///</summary>
		[TestMethod()]
		public void CircuitMapApplyPerfTest2() {
			this.CircuitMapPerfTest(Properties.Resources.ExternalCalculator, "Main", 10, 25);
		}
	}
}
