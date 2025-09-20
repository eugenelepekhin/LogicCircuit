using System.Xml;
using DataPersistent;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This class intended to test various version conversions.
	/// Not all conversion are covered by this test.
	/// </summary>
	[TestClass]
	public class ConversionTest {
		public TestContext TestContext { get; set; }

		private Dictionary<string, int> tableCounts = [];

		private void AssertFileVersion(string projectText, string expectedNamespace) {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(projectText);
			Assert.AreEqual(expectedNamespace, xml.DocumentElement.NamespaceURI, "Incorrect file version.");
		}

		private void AssertEqual<TRecord>(TableSnapshot<TRecord> expected, TableSnapshot<TRecord> actual) where TRecord:struct {
			this.TestContext.WriteLine("Comparing table {0}", expected.Name);
			int count = expected.Count();
			Assert.AreEqual(count, actual.Count(), "Row count mismatch for table {0}", expected.Name);
			int max = 0;
			if(this.tableCounts.TryGetValue(expected.Name, out int maxCount)) {
				max = maxCount;
			}
			this.tableCounts[expected.Name] = Math.Max(max, count);

			List<IField<TRecord>> fields = expected.Fields.Where(f => f is IFieldSerializer<TRecord>).ToList();
			if(0 < fields.Count) {
				List<TRecord> expectedData = new List<TRecord>(count);
				List<TRecord> actualData = new List<TRecord>(count);
			
				foreach(RowId rowId in expected.Rows) {
					TRecord data;
					expected.GetData(rowId, out data);
					expectedData.Add(data);
				}
				foreach(RowId rowId in actual.Rows) {
					TRecord data;
					actual.GetData(rowId, out data);
					actualData.Add(data);
				}
				expectedData.Sort((x, y) => fields[0].Compare(ref x, ref y));
				actualData.Sort((x, y) => fields[0].Compare(ref x, ref y));

				for(int i = 0; i < expectedData.Count; i++) {
					for(int j = 0; j < fields.Count; j++) {
						IField<TRecord> field = fields[j];
						TRecord expectedDataItem = expectedData[i];
						TRecord actualDataItem = actualData[i];
						int res = field.Compare(ref expectedDataItem, ref actualDataItem);
						if(res != 0) {
							this.TestContext.WriteLine("Field {0} mismatch for row {1} in table {2}: expected={3}, actual={4}", field.Name, i, expected.Name, expectedDataItem, actualDataItem);
						}
						Assert.AreEqual(0, res, "Field {0} mismatch for row {1} in table {2}", field.Name, i, expected.Name);
					}
				}
			}
		}

		private void AssertEqual(CircuitProject expected, CircuitProject actual)  {
			int tableCount = 0;

			tableCount++; this.AssertEqual(expected.ProjectSet.Table, actual.ProjectSet.Table);
			tableCount++; this.AssertEqual(expected.CollapsedCategorySet.Table, actual.CollapsedCategorySet.Table);
			tableCount++; this.AssertEqual(expected.LogicalCircuitSet.Table, actual.LogicalCircuitSet.Table);
			tableCount++; this.AssertEqual(expected.PinSet.Table, actual.PinSet.Table);
			tableCount++; this.AssertEqual(expected.CircuitProbeSet.Table, actual.CircuitProbeSet.Table);
			tableCount++; this.AssertEqual(expected.ConstantSet.Table, actual.ConstantSet.Table);
			tableCount++; this.AssertEqual(expected.CircuitButtonSet.Table, actual.CircuitButtonSet.Table);
			tableCount++; this.AssertEqual(expected.MemorySet.Table, actual.MemorySet.Table);
			tableCount++; this.AssertEqual(expected.LedMatrixSet.Table, actual.LedMatrixSet.Table);
			tableCount++; this.AssertEqual(expected.SplitterSet.Table, actual.SplitterSet.Table);
			tableCount++; this.AssertEqual(expected.SensorSet.Table, actual.SensorSet.Table);
			tableCount++; this.AssertEqual(expected.SoundSet.Table, actual.SoundSet.Table);
			tableCount++; this.AssertEqual(expected.GraphicsArraySet.Table, actual.GraphicsArraySet.Table);
			tableCount++; this.AssertEqual(expected.CircuitSymbolSet.Table, actual.CircuitSymbolSet.Table);
			tableCount++; this.AssertEqual(expected.WireSet.Table, actual.WireSet.Table);
			tableCount++; this.AssertEqual(expected.TextNoteSet.Table, actual.TextNoteSet.Table);

			// these are not serialized in the project file, but still must be equal
			tableCount++; this.AssertEqual(expected.CircuitSet.Table, actual.CircuitSet.Table);
			tableCount++; this.AssertEqual(expected.DevicePinSet.Table, actual.DevicePinSet.Table);
			tableCount++; this.AssertEqual(expected.GateSet.Table, actual.GateSet.Table);

			Assert.AreEqual(expected.Tables.Count(), tableCount, "Table count mismatch. Expected {0}, actual {1}", expected.Tables.Count(), tableCount);
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

		/// <summary>
		/// In version 2.0.0.14 file format changed to allow attributes instead of elements. Test correctness of the conversion.
		/// </summary>
		[TestMethod]
		[DeploymentItem("Properties", "Originals")]
		public void RoundTripConversionTest() {
			this.tableCounts.Clear();
			string originals = Path.Combine(this.TestContext.DeploymentDirectory, "Originals");
			string conveted = Path.Combine(this.TestContext.DeploymentDirectory, "Converted");
			foreach(string oldFile in Directory.GetFiles(originals, "*.CircuitProject")) {
				this.TestContext.WriteLine("Testing conversion of file: {0}", Path.GetFileName(oldFile));
				CircuitProject circuitProject1 = CircuitProject.Create(oldFile);
				string newFile = Path.Combine(conveted, Path.GetFileName(oldFile));
				circuitProject1.Save(newFile);
				CircuitProject circuitProject2 = CircuitProject.Create(newFile);
				this.AssertEqual(circuitProject1, circuitProject2);
			}

			this.TestContext.WriteLine("");
			this.TestContext.WriteLine("Table counts in files");
			foreach(var kv in this.tableCounts.OrderBy(kv => kv.Key)) {
				this.TestContext.WriteLine("{1,5:d} {0}", kv.Key, kv.Value);
			}
		}
	}
}
