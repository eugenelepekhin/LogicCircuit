using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for testing of conversion from version 2.0.0.5
	///</summary>
	[TestClass()]
	public class ProbeConverterTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		[TestMethod()]
		public void ProbeConvertionTest() {
			string projectText = Properties.Resources.ProbeConvertTest;
			this.AssertFileVersion(projectText);
			ProjectTester tester = new ProjectTester(this.TestContext, projectText, null);
			Assert.AreEqual<int>(3, tester.CircuitProject.CircuitProbeSet.Count(), "Expecting 3 probes");
			Assert.AreEqual(3, tester.CircuitProject.CircuitSymbolSet.Where(symbol => symbol.Circuit is CircuitProbe).Count(), "Expecting 3 probe symbols");
			List<CircuitSymbol> symbols = tester.CircuitProject.CircuitProbeSet.Select(probe => tester.CircuitProject.CircuitSymbolSet.SelectByCircuit(probe).First()).ToList();
			Assert.AreEqual(3, symbols.Count);
			Assert.AreEqual(2, symbols.Where(symbol => symbol.LogicalCircuit == tester.CircuitProject.ProjectSet.Project.LogicalCircuit).Count(), "Expecting 2 symbols on main diagram");
		}

		private void AssertFileVersion(string projectText) {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(projectText);
			Assert.AreEqual("http://LogicCircuit.net/2.0.0.5/CircuitProject.xsd", xml.DocumentElement.NamespaceURI, "Incorrect file version. File should be of 2.0.0.5 version for this test");
		}
	}
}
