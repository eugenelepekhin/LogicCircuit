// Ignore Spelling: Verilog

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
		public void ExportWithoutTestBenchTest() {
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
	}
}
