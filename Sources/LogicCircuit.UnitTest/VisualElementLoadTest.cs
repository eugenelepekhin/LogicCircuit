using System;
using System.Linq;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// Test of loading of projects with visual elements like 7 segment display.
	/// </summary>
	[TestClass()]
	public class VisualElementLoadTest {

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
		/// A test for ProjectTester
		/// </summary>
		[TestMethod()]
		public void VisualElementLoadProjectTesterTest() {
			ProjectTester tester = new ProjectTester(this.TestContext, Properties.Resources.VisualElementLoadTest, null);
			Assert.AreEqual(1, tester.Input.Length);
			Assert.AreEqual(1, tester.Output.Length);
			Assert.IsTrue(tester.Input.All(f => f != null));
			Assert.IsTrue(tester.Output.All(f => f != null));

			bool succes = tester.CircuitState.Evaluate(true);
			Assert.IsTrue(succes);
			Assert.AreEqual(1, tester.Input[0].Value);
			Assert.AreEqual(2, tester.Output[0].Pack());
		}
	}
}
