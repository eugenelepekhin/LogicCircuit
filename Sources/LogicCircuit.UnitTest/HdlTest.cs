// Ignore Spelling: Hdl

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using LogicCircuit.UnitTest.HDL;

namespace LogicCircuit.UnitTest {
	[TestClass]
	[DeploymentItem(@"Properties\HDLTests.CircuitProject")]
	public class HdlTest {
		public TestContext TestContext { get; set; }
		private CircuitProject circuitProject;

		private void Message(string message) {
			this.TestContext.WriteLine(message);
			//Debug.WriteLine(message);
		}

		private CircuitProject LoadCircuitProject() {
			if(this.circuitProject == null) {
				this.circuitProject = ProjectTester.LoadDeployedFile(this.TestContext, "HDLTests.CircuitProject", null);
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

		private void RunTruthTableComparisonForCategory(string category) {
			CircuitProject project = this.LoadCircuitProject();
			string hdlPath = this.HdlFolder();
			List<LogicalCircuit> circuits = project.LogicalCircuitSet.Where(c => c.Category == category).ToList();
			this.SortByNote(circuits);
			foreach(LogicalCircuit circuit in circuits) {
				ProjectTester.SwitchTo(project, circuit.Name);
				N2TExport export = new N2TExport(false, true, this.Message, this.Message, this.Message);
				bool success = export.ExportCircuit(circuit, hdlPath, false, false, null, () => {});
				Assert.IsTrue(success);
				HdlContext context = new HdlContext(hdlPath, this.Message);
				HdlState hdlState = context.Load(circuit.Name);
				Assert.IsNotNull(hdlState);
				this.Message($"Circuit {circuit.Name} HDL:");
				this.Message(hdlState.Chip.ToString());
				List<TruthState> hdlTable = hdlState.BuildTruthTable();

				CircuitTestSocket socket = new CircuitTestSocket(circuit);
				IList<TruthState> lcTable = socket.BuildTruthTable(d => {}, () => true, s => true, 1 << circuit.Pins.Where(p => p.PinType == PinType.Input).Sum(p => p.BitWidth), out bool truncated);

				bool same = TruthState.AreEqual(lcTable, hdlTable);
				Assert.IsTrue(same);
			}
		}

		private HdlState LoadState(string circuitName) {
			CircuitProject project = this.LoadCircuitProject();
			string hdlPath = this.HdlFolder();
			LogicalCircuit circuit = ProjectTester.SwitchTo(project, circuitName);
			N2TExport export = new N2TExport(false, false, this.Message, this.Message, this.Message);
			bool success = export.ExportCircuit(circuit, hdlPath, false, false, null, () => {});
			Assert.IsTrue(success);
			HdlContext context = new HdlContext(hdlPath, this.Message);
			HdlState state = context.Load(circuit.Name);
			Assert.IsNotNull(state);
			this.Message($"Circuit {circuit.Name} HDL:");
			this.Message(state.Chip.ToString());
			return state;
		}

		[STATestMethod]
		public void HdlTestTruthTables() {
			this.RunTruthTableComparisonForCategory("Test");
		}


		[STATestMethod]
		public void HdlTestTruthTables2() {
			this.RunTruthTableComparisonForCategory("Test2");
		}

		[STATestMethod]
		public void HdlTestErrors() {
			string text = null;
			void error(string message) {
				this.Message(message);
				text = message;
			}
			Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
			CircuitProject project = this.LoadCircuitProject();
			string hdlPath = this.HdlFolder();
			List<LogicalCircuit> circuits = project.LogicalCircuitSet.Where(c => c.Category == "Error").ToList();
			this.SortByNote(circuits);
			foreach(LogicalCircuit circuit in circuits) {
				ProjectTester.SwitchTo(project, circuit.Name);
				N2TExport export = new N2TExport(false, false, this.Message, error, this.Message);
				bool success = export.ExportCircuit(circuit, hdlPath, false, false, null, () => {});
				Assert.IsFalse(success);

				TextNote note = circuit.TextNotes().FirstOrDefault();
				Assert.IsNotNull(note);
				if(note != null) {
					XmlDocument xml = XmlHelper.LoadXml(note.Note);
					XmlNode node = xml.DocumentElement.FirstChild;
					Assert.IsNotNull(node);
					if(node != null) {
						string pattern = node.InnerText;
						Assert.IsTrue(Regex.IsMatch(text, pattern), $"Circuit: {circuit.Name} has unmatched pattern: \"{pattern}\" and text: \"{text}\"");
					}
				}
			}
		}

		[STATestMethod]
		public void HdlTestMultiInputGates() {
			this.RunTruthTableComparisonForCategory("BigGates");
		}

		[STATestMethod]
		public void HdlTestLocale() {
			ProjectTester.InitResources();
			CultureInfo cultureInfo = Properties.Resources.Culture;
			try {
				Properties.Resources.Culture = CultureInfo.GetCultureInfo("ru");
				this.RunTruthTableComparisonForCategory("Test");
				this.RunTruthTableComparisonForCategory("BigGates");
			} finally {
				Properties.Resources.Culture = cultureInfo;
			}
		}

		[STATestMethod]
		public void HdlTestRSFlipFlop1() {
			HdlState state = this.LoadState("RSFlipFlop1");

			void check(int s, int r, int q, int nq) {
				state["S"] = s;
				state["R"] = r;
				Assert.IsTrue(state.Evaluate());
				Assert.AreEqual(q, state["q"]);
				Assert.AreEqual(nq, state["Nq"]);
			}

			check(0, 1, 1, 0);
			check(1, 1, 1, 0);

			check(1, 0, 0, 1);
			check(1, 1, 0, 1);

			check(0, 1, 1, 0);
			check(1, 1, 1, 0);

			check(1, 0, 0, 1);
			check(1, 1, 0, 1);
		}

		[STATestMethod]
		public void HdlTestJKFlipFlop() {
			HdlState state = this.LoadState("JKFlipFlop");

			state["r"] = 0;
			Assert.IsTrue(state.Evaluate());
			Assert.AreEqual(0, state["q"]);
			state["r"] = 1;

			void check(int j, int k, int q, int nq) {
				state["j"] = j;
				state["k"] = k;
				state["clk"] = 0;
				Assert.IsTrue(state.Evaluate());
				state["clk"] = 1;
				Assert.IsTrue(state.Evaluate());
				state["clk"] = 0;
				Assert.IsTrue(state.Evaluate());
				Assert.AreEqual(q, state["q"]);
				Assert.AreEqual(nq, state["nq"]);
			}

			check(0, 1, 0, 1);
			check(0, 0, 0, 1);

			check(1, 0, 1, 0);
			check(0, 0, 1, 0);

			check(1, 1, 0, 1);
			check(1, 1, 1, 0);
			check(1, 1, 0, 1);
			check(1, 1, 1, 0);
		}

		[STATestMethod]
		public void HdlTestCount6() {
			HdlState state = this.LoadState("Count6");

			state["clk"] = 0;
			state["r"] = 0;
			Assert.IsTrue(state.Evaluate());
			Assert.AreEqual(0, state["q"]);
			state["r"] = 1;
			Assert.IsTrue(state.Evaluate());
			Assert.AreEqual(0, state["q"]);

			void check(int q) {
				state["clk"] = 1;
				Assert.IsTrue(state.Evaluate());
				state["clk"] = 0;
				Assert.IsTrue(state.Evaluate());
				Assert.AreEqual(q, state["q"]);
			}

			int max = 1 << 6;
			for(int i = 0; i < max + 10; i++) {
				check((i + 1) % max);
			}
		}

		//[STATestMethod]
		public void HdlTestSingleTest() {
			HdlState state = this.LoadState("MissingXNorJam");
			List<TruthState> hdlTable = state.BuildTruthTable();

			LogicalCircuit circuit = this.LoadCircuitProject().LogicalCircuitSet.FindByName(state.Chip.Name);
			CircuitTestSocket socket = new CircuitTestSocket(circuit);
			IList<TruthState> lcTable = socket.BuildTruthTable(d => {}, () => true, s => true, 1 << circuit.Pins.Where(p => p.PinType == PinType.Input).Sum(p => p.BitWidth), out bool truncated);

			bool same = TruthState.AreEqual(lcTable, hdlTable);
			Assert.IsTrue(same);
		}
	}
}
