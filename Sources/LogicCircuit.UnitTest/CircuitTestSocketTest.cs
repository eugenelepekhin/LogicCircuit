using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for CircuitTestSocket and is intended
	/// to contain all CircuitTestSocket Unit Tests
	/// </summary>
	[TestClass()]
	public class CircuitTestSocketTest {
		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		private static CircuitProject project;
		private CircuitProject CircuitProject {
			get {
				if(CircuitTestSocketTest.project == null) {
					CircuitTestSocketTest.project = ProjectTester.Load(this.TestContext, Properties.Resources.CircuitTestSocketTest, null);
				}
				return CircuitTestSocketTest.project;
			}
		}

		private void AssertTestable(CircuitProject project, string circuitName, bool isTestable) {
			ProjectTester.SwitchTo(project, circuitName);
			Assert.AreEqual(isTestable, CircuitTestSocket.IsTestable(project.ProjectSet.Project.LogicalCircuit),
				"{0} should {1}be testable", project.ProjectSet.Project.LogicalCircuit.Name, isTestable ? "" : "not "
			);
		}

		/// <summary>
		/// A test for IsTestable
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketIsTestableTest() {
			this.AssertTestable(this.CircuitProject, "No Input Output", false);
			this.AssertTestable(this.CircuitProject, "No Input", false);
			this.AssertTestable(this.CircuitProject, "No Output", false);
			this.AssertTestable(this.CircuitProject, "Unconnected", true);
			this.AssertTestable(this.CircuitProject, "1 Bit Wire", true);
			this.AssertTestable(this.CircuitProject, "1 bit 2 entry AND", true);
			this.AssertTestable(this.CircuitProject, "8 bit adder", true);
		}

		/// <summary>
		/// A test for CircuitTestSocket Constructor
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketConstructorTest() {
			ProjectTester.SwitchTo(this.CircuitProject, "Unconnected");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			Assert.AreEqual(0, s1.Inputs.Count());
			Assert.AreEqual(0, s1.Outputs.Count());

			ProjectTester.SwitchTo(this.CircuitProject, "1 Bit Wire");
			CircuitTestSocket s2 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			Assert.AreEqual(1, s2.Inputs.Count());
			Assert.AreEqual(1, s2.Outputs.Count());
			Assert.AreEqual(1, s2.Inputs.Sum(i => i.Pin.BitWidth));
			Assert.AreEqual(1, s2.Outputs.Sum(i => i.Pin.BitWidth));

			ProjectTester.SwitchTo(this.CircuitProject, "8 bit adder");
			CircuitTestSocket s3 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			Assert.AreEqual(3, s3.Inputs.Count());
			Assert.AreEqual(3, s3.Outputs.Count());
			Assert.AreEqual(17, s3.Inputs.Sum(i => i.Pin.BitWidth));
			Assert.AreEqual(10, s3.Outputs.Sum(i => i.Pin.BitWidth));
		}

		/// <summary>
		/// A test for BuildTruthTable in single threaded case.
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketBuildTruthTable1Test() {
			ProjectTester.SwitchTo(this.CircuitProject, "1 Bit Wire");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(s1);
			Predicate<TruthState> predicate = parser.Parse("q=x", true);
			double progress = -1;
			bool truncated;
			IList<TruthState> table1 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				null,
				2,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table1 != null && table1.Count == 2);
			Assert.IsTrue(table1[0].Input[0] == 0 && table1[0].Output[0] == 0);
			Assert.IsTrue(table1[1].Input[0] == 1 && table1[1].Output[0] == 1);

			IList<TruthState> table2 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				predicate,
				2,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table2 != null && table2.Count == 0);
		}

		/// <summary>
		/// A test for BuildTruthTable in single threaded case.
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketBuildTruthTable2Test() {
			ProjectTester.SwitchTo(this.CircuitProject, "1 bit 2 entry AND");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(s1);
			Predicate<TruthState> predicate = parser.Parse("q=a&b", true);
			double progress = -1;
			bool truncated;
			IList<TruthState> table1 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				null,
				4,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table1 != null && table1.Count == 4);
			Assert.IsTrue(table1[0].Input[0] == 0 && table1[0].Input[1] == 0 && table1[0].Output[0] == 0);
			Assert.IsTrue(table1[1].Input[0] == 0 && table1[1].Input[1] == 1 && table1[1].Output[0] == 0);
			Assert.IsTrue(table1[2].Input[0] == 1 && table1[2].Input[1] == 0 && table1[2].Output[0] == 0);
			Assert.IsTrue(table1[3].Input[0] == 1 && table1[3].Input[1] == 1 && table1[3].Output[0] == 1);

			IList<TruthState> table2 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				predicate,
				1,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table2 != null && table2.Count == 0);
		}

		/// <summary>
		/// A test for BuildTruthTable in single threaded case.
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketBuildTruthTable3Test() {
			ProjectTester.SwitchTo(this.CircuitProject, "1 bit full adder");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(s1);
			Predicate<TruthState> predicate = parser.Parse("outC << 1 | s = inC + a + b", true);
			double progress = -1;
			bool truncated;
			IList<TruthState> table1 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				null,
				8,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table1 != null && table1.Count == 8);
			foreach(TruthState state in table1) {
				Assert.IsFalse(predicate(state));
			}

			IList<TruthState> table2 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				predicate,
				1,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table2 != null && table2.Count == 0);
		}

		/// <summary>
		/// A test for BuildTruthTable in multi threaded case.
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketBuildTruthTable4Test() {
			ProjectTester.SwitchTo(this.CircuitProject, "8 bit adder");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(s1);
			Predicate<TruthState> predicate = parser.Parse("outC << 8 | s = inC + a + b && v = !(a & 0x80 != b & 0x80 || a & 0x80 == s & 0x80)", true);
			double progress = -1;
			bool truncated;
			IList<TruthState> table1 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				null,
				1 << 17,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table1 != null && table1.Count == (1 << 17));
			foreach(TruthState state in table1) {
				Assert.IsFalse(predicate(state));
			}

			IList<TruthState> table2 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				predicate,
				1,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table2 != null && table2.Count == 0);
		}

		/// <summary>
		/// A test for BuildTruthTable in multi threaded case with truncation.
		/// </summary>
		[TestMethod()]
		public void CircuitTestSocketBuildTruthTable5Test() {
			ProjectTester.SwitchTo(this.CircuitProject, "8 bit adder");
			CircuitTestSocket s1 = new CircuitTestSocket(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(s1);
			Predicate<TruthState> predicate = parser.Parse("outC << 8 | s = inC + a + b && v = !(a & 0x80 != b & 0x80 || a & 0x80 == s & 0x80)", true);
			double progress = -1;
			bool truncated;
			int maxSize = 128;
			IList<TruthState> table1 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				null,
				maxSize,
				out truncated
			);
			Assert.IsTrue(truncated);
			Assert.IsTrue(table1 != null && table1.Count == maxSize);
			foreach(TruthState state in table1) {
				Assert.IsFalse(predicate(state));
			}

			IList<TruthState> table2 = s1.BuildTruthTable(
				p => { Assert.IsTrue(0 <= p && p <= 100); progress = p; },
				() => true,
				predicate,
				1,
				out truncated
			);
			Assert.IsTrue(Math.Abs(progress - 100) < 2);
			Assert.IsTrue(!truncated);
			Assert.IsTrue(table2 != null && table2.Count == 0);
		}
	}
}
