using System.Diagnostics;
using System.Text.RegularExpressions;
using LogicCircuit.UnitTest.HDL;
using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class HdlTest {
		public TestContext TestContext { get; set; }

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

		private void Message(string message) {
			this.TestContext.WriteLine(message);
			//Debug.WriteLine(message);
		}

		[STATestMethod]
		public void TestOverall() {
			double Number(string text) {
				Match match = Regex.Match(text, @"[0-9]+(\.[0-9]+)?");
				if(match.Success) {
					return double.Parse(match.Value);
				}
				return int.MaxValue;
			}
			string lcFile = """C:\Projects\CircuitProjects\HDLTests.CircuitProject""";
			CircuitProject project = ProjectTester.Load(this.TestContext, File.ReadAllText(lcFile), null);
			string hdlPath = Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName);
			if(!Directory.Exists(hdlPath)) {
				Directory.CreateDirectory(hdlPath);
			}
			List<LogicalCircuit> circuits = project.LogicalCircuitSet.Where(c => c.Category == "Test").ToList();
			circuits.Sort((a, b) => Math.Sign(Number(a.Note) - Number(b.Note)));
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
	}
}
