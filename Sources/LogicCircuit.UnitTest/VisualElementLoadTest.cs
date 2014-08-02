using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// Test of loading of projects with visual elements like 7 segment display.
	/// </summary>
	[TestClass()]
	public class VisualElementLoadTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// A test for ProjectTester
		/// </summary>
		[TestMethod()]
		public void VisualElementLoadProjectTesterTest() {
			ProjectTester tester = new ProjectTester(this.TestContext, Properties.Resources.VisualElementLoadTest, null);
			Assert.AreEqual(1, tester.Input.Length);
			Assert.AreEqual(1, tester.Output.Length);
			Assert.IsTrue(tester.Input.All(f => f != null));
			Assert.IsTrue(tester.Output.All(f => f != null));

			Assert.IsTrue(tester.CircuitState.Evaluate(true));
			Assert.AreEqual(1, tester.Input[0].Value);
			Assert.AreEqual(2, tester.Output[0].Pack());

			tester.CircuitProject.InTransaction(() => tester.Input[0].Value = 0);
			Assert.IsTrue(tester.CircuitState.Evaluate(true));
			Assert.AreEqual(0, tester.Input[0].Value);
			Assert.AreEqual(1, tester.Output[0].Pack());
		}
	}
}
