using System.Xml;

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
		[DeploymentItem("Properties\\From2.0.0.6Conversion.CircuitProject")]
		public void ConvertDescription2NoteTest() {
			string file = "From2.0.0.6Conversion.CircuitProject";
			string projectText = File.ReadAllText(Path.Combine(this.TestContext.DeploymentDirectory, file));
			this.AssertFileVersion(projectText, "http://LogicCircuit.net/2.0.0.6/CircuitProject.xsd");
			ProjectTester tester = new ProjectTester(ProjectTester.LoadDeployedFile(this.TestContext, file, "Test Circuit"));
			Assert.AreEqual("Project Description\n<xml>text</xml>", tester.Project.Note);
			Assert.AreEqual("Test Circuit description\n<xml>node</xml>", tester.Project.LogicalCircuit.Note);
		}
	}
}
