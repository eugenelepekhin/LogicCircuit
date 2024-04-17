using System.Text.RegularExpressions;
using System.Xml;
using LogicCircuit.UnitTest.HDL;
using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;

namespace LogicCircuit.UnitTest {
	[TestClass]
	[DeploymentItem(@"Properties\HDLTests.CircuitProject")]
	public class HdlTest {
		public TestContext TestContext { get; set; }

		private void Message(string message) {
			this.TestContext.WriteLine(message);
			//Debug.WriteLine(message);
		}

		private CircuitProject LoadCircuitProject() {
			string file = Path.Combine(this.TestContext.TestRunDirectory, "Out", "HDLTests.CircuitProject");
			CircuitProject project = ProjectTester.Load(this.TestContext, File.ReadAllText(file), null);
			return project;
		}

		private string HdlFolder() {
			string path = Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName);
			if(!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			return path;
		}

		private void SortByNote(List<LogicalCircuit> circuits) {
			double Number(string text) {
				Match match = Regex.Match(text, @"[0-9]+(\.[0-9]+)?");
				if(match.Success) {
					return double.Parse(match.Value);
				}
				return int.MaxValue;
			}
			circuits.Sort((a, b) => Math.Sign(Number(a.Note) - Number(b.Note)));
		}

		[TestMethod]
		public void TestParser() {
			string file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Merge3.hdl";
			//file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Inverter.hdl";
			file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\DInverter.hdl";
			//file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\MergeSplit1_2.hdl";

			string folder = Path.GetDirectoryName(file);
			string name = Path.GetFileNameWithoutExtension(file);

			HdlContext hdl = new HdlContext(folder, message => this.TestContext.WriteLine(message));
			HdlState state = hdl.Load(name);
			if(state != null) {
				string text = state.Chip.ToString();
				this.TestContext.WriteLine(text);

				state.Set(null, state.Chip.Pin("x"), 1);
				state.Chip.Evaluate(state);
				int q = state.Get(null, state.Chip.Pin("q"));
				Assert.AreEqual(1, q);

				state.Set(null, state.Chip.Pin("x"), 0);
				state.Chip.Evaluate(state);
				q = state.Get(null, state.Chip.Pin("q"));
				Assert.AreEqual(0, q);

				List<TruthState> list = state.BuildTruthTable();
				Assert.IsTrue(0 < list.Count);
			}
		}

		[STATestMethod]
		public void HdlTestTruthTables() {
			CircuitProject project = this.LoadCircuitProject();
			string hdlPath = this.HdlFolder();
			List<LogicalCircuit> circuits = project.LogicalCircuitSet.Where(c => c.Category == "Test").ToList();
			this.SortByNote(circuits);
			foreach(LogicalCircuit circuit in circuits) {
				ProjectTester.SwitchTo(project, circuit.Name);
				N2TExport export = new N2TExport(false, false, this.Message, this.Message);
				bool success = export.ExportCircuit(circuit, hdlPath, false);
				Assert.IsTrue(success);
				HdlContext context = new HdlContext(hdlPath, this.Message);
				HdlState hdlState = context.Load(circuit.Name);
				this.Message($"Circuit {circuit.Name} HDL:");
				this.Message(hdlState.Chip.ToString());
				List<TruthState> hdlTable = hdlState.BuildTruthTable();

				CircuitTestSocket socket = new CircuitTestSocket(circuit);
				IList<TruthState> lcTable = socket.BuildTruthTable(d => {}, () => true, s => true, 1 << circuit.Pins.Where(p => p.PinType == PinType.Input).Sum(p => p.BitWidth), out bool truncated);

				bool same = TruthState.AreEqual(lcTable, hdlTable);
				Assert.IsTrue(same);
			}
		}

		[STATestMethod]
		public void HdlTestErrors() {
			string text = null;
			void error(string message) {
				this.Message(message);
				text = message;
			}
			CircuitProject project = this.LoadCircuitProject();
			string hdlPath = this.HdlFolder();
			List<LogicalCircuit> circuits = project.LogicalCircuitSet.Where(c => c.Category == "Error").ToList();
			this.SortByNote(circuits);
			foreach(LogicalCircuit circuit in circuits) {
				ProjectTester.SwitchTo(project, circuit.Name);
				N2TExport export = new N2TExport(false, false, this.Message, error);
				bool success = export.ExportCircuit(circuit, hdlPath, false);
				Assert.IsFalse(success);

				TextNote note = circuit.TextNotes().FirstOrDefault();
				Assert.IsNotNull(note);
				if(note != null) {
					XmlDocument xml = XmlHelper.LoadXml(note.Note);
					XmlNode node = xml.DocumentElement.FirstChild;
					Assert.IsNotNull(node);
					if(node != null) {
						string pattern = node.InnerText;
						Assert.IsTrue(Regex.IsMatch(text, pattern));
					}
				}
			}
		}
	}
}
