using System;
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
			ProjectTester tester = new ProjectTester(this.TestContext, Properties.Resources.IntegerCalculator, null);

			//Assert.Inconclusive("Verify the correctness of this test method.");
		}
	}
}
