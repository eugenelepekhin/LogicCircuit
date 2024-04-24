using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for CircuitMapTest and is intended
	///to contain all CircuitMapTest Unit Tests
	///</summary>
	[STATestClass]
	public class CircuitMapTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		private void CircuitMapPerfTest(string project, string initialCircuit, int maxCount, int maxSeconds) {
			CircuitProject circuitProject = ProjectTester.LoadDeployedFile(this.TestContext, project, initialCircuit);
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
		[DeploymentItem("Properties\\IntegerCalculator.CircuitProject")]
		public void CircuitMapApplyPerfTest1() {
			Console.WriteLine(Thread.CurrentThread.GetApartmentState());
			this.CircuitMapPerfTest("IntegerCalculator.CircuitProject", "Test Computer", 50, 15);
		}

		/// <summary>
		///A test for Apply
		///</summary>
		[TestMethod()]
		[DeploymentItem("Properties\\ExternalCalculator.CircuitProject")]
		public void CircuitMapApplyPerfTest2() {
			this.CircuitMapPerfTest("ExternalCalculator.CircuitProject", "Main", 50, 20);
		}

		private void CircuitMapCleanUpTest(CircuitProject circuitProject, string logicalCircuitName, int expectedFunctions) {
			ProjectTester.SwitchTo(circuitProject, logicalCircuitName);

			CircuitMap map = new CircuitMap(circuitProject.ProjectSet.Project.LogicalCircuit);
			CircuitState state = map.Apply(CircuitRunner.HistorySize);

			Assert.AreEqual(expectedFunctions, state.Functions.Count(), "wrong number of functions");
		}

		/// <summary>
		///A test for Correctness of clean up of gates with all output disconnected.
		///</summary>
		[TestMethod()]
		[DeploymentItem("Properties\\CircuitMapCleanUpTest.CircuitProject")]
		public void CircuitMapCleanUpTest() {
			CircuitProject circuitProject = ProjectTester.LoadDeployedFile(this.TestContext, "CircuitMapCleanUpTest.CircuitProject", null);

			this.CircuitMapCleanUpTest(circuitProject, "1. Empty 1", 0);
			Assert.AreEqual(0, circuitProject.ProjectSet.Project.LogicalCircuit.CircuitSymbols().Count());

			this.CircuitMapCleanUpTest(circuitProject, "2. Empty 2", 0);
			Assert.AreEqual(7, circuitProject.ProjectSet.Project.LogicalCircuit.CircuitSymbols().Count());

			this.CircuitMapCleanUpTest(circuitProject, "3. Single Out", 3);
			Assert.AreEqual(4, circuitProject.ProjectSet.Project.LogicalCircuit.CircuitSymbols().Count());

			this.CircuitMapCleanUpTest(circuitProject, "4. Chain Out", 8);
			Assert.AreEqual(11, circuitProject.ProjectSet.Project.LogicalCircuit.CircuitSymbols().Count());
		}

		[TestMethod()]
		[DeploymentItem("Properties\\CircuitMapTests.CircuitProject")]
		public void CircuitMapDeepWireLoopTest() {
			ProjectTester tester = new ProjectTester(ProjectTester.LoadDeployedFile(this.TestContext, "CircuitMapTests.CircuitProject", "DeepWireLoopTest"));
			InputSocket input = new InputSocket(tester.Input[0]);
			OutputSocket target = new OutputSocket(tester.Output[0]);
			Action<int> test = value => {
				input.Value = value;
				tester.CircuitState.Evaluate(true);
				Assert.AreEqual(value, target.BinaryInt());
			};
			tester.CircuitProject.InOmitTransaction(() => {
				for(int i = 0; i < 10; i++) {
					test(i & 1);
				}
			});
		}
	}
}
