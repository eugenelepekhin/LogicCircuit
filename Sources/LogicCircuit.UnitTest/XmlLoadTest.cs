using System;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for Load and save xml files and is intended
	/// to contain all load and save Unit Tests
	/// </summary>
	[TestClass()]
	public class XmlLoadTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// A test for LoadRecord
		/// </summary>
		[TestMethod()]
		public void XmlLoadReadElementTextTest() {
			string text = Properties.Resources.XmlLoadReadElementTextTest;
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(text);
			Assert.AreEqual(@"http://LogicCircuit.net/1.0.0.2/CircuitProject.xsd", xml.DocumentElement.NamespaceURI);

			CircuitProject circuitProject = ProjectTester.Load(this.TestContext, text, null);
			Assert.IsNotNull(circuitProject);

			Assert.AreEqual(1, circuitProject.CircuitButtonSet.Count());
			CircuitButton button = circuitProject.CircuitButtonSet.First();
			Assert.AreEqual(@"<a>", button.Notation);
			Assert.AreEqual(@"<a>b</a>", button.Note);
			Assert.AreEqual(1, circuitProject.CircuitSymbolSet.SelectByCircuit(button).Count());
			CircuitSymbol buttonSymbol = circuitProject.CircuitSymbolSet.SelectByCircuit(button).First();
			Assert.AreEqual(3, buttonSymbol.X);
			Assert.AreEqual(8, buttonSymbol.Y);

			Assert.AreEqual(2, circuitProject.CircuitSymbolSet.Count());
			CircuitSymbol ledSymbol = circuitProject.CircuitSymbolSet.First(s => s != buttonSymbol);
			Assert.IsNotNull(ledSymbol);
			Assert.AreEqual(9, ledSymbol.X);
			Assert.AreEqual(8, ledSymbol.Y);

			Assert.AreEqual(1, circuitProject.WireSet.Count());
			Wire wire = circuitProject.WireSet.First();
			Assert.IsNotNull(wire);
			Assert.AreEqual(5, wire.X1);
			Assert.AreEqual(9, wire.Y1);
			Assert.AreEqual(9, wire.X2);
			Assert.AreEqual(9, wire.Y1);
		}
	}
}
