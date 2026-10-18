// Ignore Spelling: Verilog

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LogicCircuit.UnitTest {
	[TestClass]
	[DeploymentItem(@"Properties\VerilogTests.CircuitProject")]
	public class HDLVerilogTest {
		public TestContext TestContext { get; set; }
		private CircuitProject circuitProject;

		private void Message(string message) {
			this.TestContext.WriteLine(message);
			//Debug.WriteLine(message);
		}

		private CircuitProject LoadCircuitProject() {
			if(this.circuitProject == null) {
				this.circuitProject = ProjectTester.LoadDeployedFile(this.TestContext, "VerilogTests.CircuitProject", null);
			}
			return this.circuitProject;
		}

		private string HdlFolder() {
			string path = Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName);
			if(!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			return path;
		}

		[STATestMethod]
		public void ExportWithoutTestBench() {
			CircuitProject project = this.LoadCircuitProject();
			VerilogExport export = new VerilogExport(false, true, true, this.Message, this.Message, this.Message);
			string folder = this.HdlFolder();
			foreach(LogicalCircuit circuit in project.LogicalCircuitSet) {
				if(circuit.Category == "Simple") {
					Assert.IsTrue(export.CanExport(circuit));
					Assert.IsTrue(export.ExportCircuit(circuit, folder, false, false, i => true, () => {}));
				}
			}
			Assert.AreEqual(0, Directory.EnumerateFiles(folder, "*_TestBench.*").Count());
		}

		[STATestMethod]
		public void ExportWithTestBench() {
			CircuitProject project = this.LoadCircuitProject();
			VerilogExport export = new VerilogExport(true, true, true, this.Message, this.Message, this.Message);
			string folder = this.HdlFolder();
			int count = 0;
			foreach(LogicalCircuit circuit in project.LogicalCircuitSet) {
				if(circuit.Category == "Simple") {
					Assert.IsTrue(export.CanExport(circuit));
					Assert.IsTrue(export.ExportCircuit(circuit, folder, false, false, i => true, () => {}));
					count++;
				}
			}
			Assert.AreEqual(count, Directory.EnumerateFiles(folder, "*_TestBench.*").Count());
		}

		[STATestMethod]
		public void ExportErrors() {
			string errorText = null;
			void error(string text) {
				this.Message(text);
				errorText = text;
			}

			Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
			CircuitProject project = this.LoadCircuitProject();
			string folder = this.HdlFolder();
			List<LogicalCircuit> list = project.LogicalCircuitSet.Where(c => c.Category == "Errors").ToList();
			foreach(LogicalCircuit circuit in list) {
				errorText = null;
				VerilogExport export = new VerilogExport(false, true, true, this.Message, error, this.Message);
				bool success = export.ExportCircuit(circuit, folder, false, false, i => true, () => {});
				Assert.IsFalse(success, $"Export of circuit {circuit.Name} expected to fail");
				Assert.IsNotNull(errorText, $"Export of circuit {circuit.Name} expected to produce error text");
				string pattern = circuit.Note;
				Assert.IsFalse(string.IsNullOrWhiteSpace(pattern), $"Circuit {circuit.Name} missing error pattern in Description");
				Regex regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);
				Assert.IsTrue(regex.IsMatch(errorText), $"Export of circuit {circuit.Name} is missing expected error: {pattern}");
			}
		}

		[STATestMethod]
		public void ExportWarnings() {
			string warningText = null;
			void warning(string text) {
				this.Message(text);
				warningText = text;
			}

			Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
			CircuitProject project = this.LoadCircuitProject();
			string folder = this.HdlFolder();
			List<LogicalCircuit> list = project.LogicalCircuitSet.Where(c => c.Category == "Warnings").ToList();
			foreach(LogicalCircuit circuit in list) {
				warningText = null;
				VerilogExport export = new VerilogExport(false, true, true, this.Message, this.Message, warning);
				bool success = export.ExportCircuit(circuit, folder, false, false, i => true, () => {});
				Assert.IsTrue(success, $"Export of circuit {circuit.Name} expected to pass");
				Assert.IsNotNull(warningText, $"Export of circuit {circuit.Name} expected to produce warning text");
				string pattern = circuit.Note;
				Assert.IsFalse(string.IsNullOrWhiteSpace(pattern), $"Circuit {circuit.Name} missing error pattern in Description");
				Regex regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);
				Assert.IsTrue(regex.IsMatch(warningText), $"Export of circuit {circuit.Name} is missing expected warning {pattern}");
			}
		}

		[STATestMethod]
		public void ExportTestBenchErrors() {
			string errorText = null;
			void error(string text) {
				this.Message(text);
				errorText = text;
			}

			Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
			CircuitProject project = this.LoadCircuitProject();
			string folder = this.HdlFolder();
			List<LogicalCircuit> list = project.LogicalCircuitSet.Where(c => c.Category == "TestBenchErrors").ToList();
			foreach(LogicalCircuit circuit in list) {
				errorText = null;
				VerilogExport export = new VerilogExport(true, true, true, this.Message, error, this.Message);
				bool success = export.ExportCircuit(circuit, folder, false, false, i => true, () => {});
				Assert.IsFalse(success, $"Export of circuit {circuit.Name} expected to fail");
				Assert.IsNotNull(errorText, $"Export of circuit {circuit.Name} expected to produce error text");
				string pattern = circuit.Note;
				Assert.IsFalse(string.IsNullOrWhiteSpace(pattern), $"Circuit {circuit.Name} missing error pattern in Description");
				Regex regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);
				Assert.IsTrue(regex.IsMatch(errorText), $"Export of circuit {circuit.Name} is missing expected error: {pattern}");
			}
		}

		[STATestMethod]
		public void FlagExportToHDL() {
			StringBuilder messageText = new();
			void message(string text) {
				this.Message(text);
				messageText.AppendLine(text);
			}

			Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
			CircuitProject project = this.LoadCircuitProject();
			string folder = this.HdlFolder();
			List<LogicalCircuit> list = project.LogicalCircuitSet.Where(c => c.Category == "FlagExportToHDL").ToList();
			for(int i = 0; i < 2; i++) { // do it twice to call export for one only vs all defined by condition: i == 0 in export.ExportCircuit
				foreach(LogicalCircuit circuit in list) {
					string name = circuit.Name;
					bool exportFlag = circuit.ExportToHDL;
					messageText.Clear();
					VerilogExport export = new VerilogExport(false, true, true, message, this.Message, this.Message);
					bool success = export.ExportCircuit(circuit, folder, i == 0, false, p => true, () => {});
					Assert.IsTrue(success, $"Export of circuit {circuit.Name} failed");

					if(!exportFlag) {
						StringAssert.Contains(messageText.ToString(), string.Format("Circuit {0} is excluded from HDL export", name), StringComparison.InvariantCultureIgnoreCase,
							"Unexpected message"
						);
					}
					Assert.AreEqual(exportFlag, File.Exists(Path.Combine(folder, name + ".sv")));
				}
			}
		}
	}
}
