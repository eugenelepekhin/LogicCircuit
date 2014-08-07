using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This class intended to test various version conversions.
	/// Not all conversion are covered by this test.
	/// </summary>
	[TestClass]
	public class ConversionTest {
		public TestContext TestContext { get; set; }

		private void AssertFileVersion(string projectText, string expectedNamespace) {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(projectText);
			Assert.AreEqual(expectedNamespace, xml.DocumentElement.NamespaceURI, "Incorrect file version.");
		}

		/// <summary>
		/// In version 2.0.0.7 of the model project and logical circuit descriptions were renamed to Note. Check correctness of the conversion.
		/// </summary>
		[TestMethod]
		public void ConvertDescription2NoteTest() {
			string projectText = Properties.Resources.From2_0_0_6Conversion;
			this.AssertFileVersion(projectText, "http://LogicCircuit.net/2.0.0.6/CircuitProject.xsd");
			ProjectTester tester = new ProjectTester(this.TestContext, projectText, "Test Circuit");
			Assert.AreEqual("Project Description\n<xml>text</xml>", tester.Project.Note);
			Assert.AreEqual("Test Circuit description\n<xml>node</xml>", tester.Project.LogicalCircuit.Note);
		}
	}
}
