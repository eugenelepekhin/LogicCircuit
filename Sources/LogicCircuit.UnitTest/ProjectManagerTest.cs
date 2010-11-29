using System;
using System.Linq;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for ProjectManager and is intended
	/// to contain all ProjectManager Unit Tests
	/// </summary>
	[TestClass()]
	public class ProjectManagerTest {

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
		/// A test for CreateProject
		/// </summary>
		[TestMethod()]
		public void ProjectManagerConstructorTest() {
			//ProjectManager target = new ProjectManager();
			//Assert.AreEqual(1, target.CircuitProject.ProjectSet.Count());
			//Assert.AreEqual(142, target.CircuitProject.GateSet.Count());
		}

		/// <summary>
		/// A test for CreateProject
		/// </summary>
		[TestMethod()]
		public void CreateProjectTest() {
			//ProjectManager target = new ProjectManager();
			//Guid old = target.CircuitProject.ProjectSet.First().ProjectId;
			//target.CreateProject();
			//Assert.AreNotEqual(old, target.CircuitProject.ProjectSet.First().ProjectId);
			//Assert.AreEqual(1, target.CircuitProject.ProjectSet.Count());
			//Assert.AreEqual(142, target.CircuitProject.GateSet.Count());
		}
	}
}
