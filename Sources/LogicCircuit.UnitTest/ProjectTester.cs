using System.Windows;
using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// Helper class to load and prepare testing of project
	/// </summary>
	public class ProjectTester {
		public CircuitProject CircuitProject { get; private set; }
		public CircuitMap CircuitMap { get; private set; }
		public CircuitState CircuitState { get; private set; }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }
		public FunctionConstant[] Input { get; private set; }
		public FunctionProbe[] Output { get; private set; }

		public ProjectTester(CircuitProject circuitProject) {
			this.CircuitProject = circuitProject;

			// Create map and state
			this.CircuitMap = new CircuitMap(this.CircuitProject.ProjectSet.Project.LogicalCircuit);
			this.CircuitState = this.CircuitMap.Apply(CircuitRunner.HistorySize);

			// Init controlling points of the main circuit: constants and probes
			List<CircuitSymbol> inputSymbol = new List<CircuitSymbol>();
			foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
				if(symbol.Circuit is Constant) {
					inputSymbol.Add(symbol);
				}
			}
			CircuitSymbolComparer circuitSymbolComparer = new CircuitSymbolComparer(true);
			inputSymbol.Sort(circuitSymbolComparer);
			this.Input = inputSymbol.Select(s => this.CircuitMap.FunctionConstant(s)).ToArray();

			List<CircuitSymbol> outputSymbol = new List<CircuitSymbol>();
			foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
				if(symbol.Circuit is CircuitProbe) {
					outputSymbol.Add(symbol);
				}
			}
			outputSymbol.Sort(circuitSymbolComparer);
			this.Output = outputSymbol.Select(s => this.CircuitMap.FunctionProbe(s)).ToArray();

			this.CircuitMap.TurnOn();
		}

		public static CircuitProject LoadDeployedFile(TestContext testContext, string projectFile, string initialCircuit) {
			string path = Path.Combine(testContext.DeploymentDirectory, projectFile);
			CircuitProject circuitProject = CircuitProject.Create(path);
			ProjectTester.InitResources();
			if(initialCircuit != null) {
				ProjectTester.SwitchTo(circuitProject, initialCircuit);
			}
			ProjectTester.GuaranteeGlyph(circuitProject);
			return circuitProject;
		}

		public static LogicalCircuit SwitchTo(CircuitProject circuitProject, string logicalCircuitName) {
			Assert.IsNotNull(logicalCircuitName);
			LogicalCircuit circuit = circuitProject.LogicalCircuitSet.FindByName(logicalCircuitName);
			Assert.IsNotNull(circuit, "Circuit {0} not found in the project", logicalCircuitName);
			if(circuitProject.ProjectSet.Project.LogicalCircuit != circuit) {
				circuitProject.InOmitTransaction(() => circuitProject.ProjectSet.Project.LogicalCircuit = circuit);
			}
			ProjectTester.GuaranteeGlyph(circuitProject);
			return circuit;
		}

		private static void GuaranteeGlyph(CircuitProject circuitProject) {
			foreach(CircuitSymbol symbol in circuitProject.ProjectSet.Project.LogicalCircuit.CircuitSymbols()) {
				symbol.GuaranteeGlyph();
			}
		}

		public static void InitResources() {
			// Init App resources so all the visual elements will be able to find them
			if(App.CurrentApp == null) {
				App app = new App();
				if(Application.ResourceAssembly == null) {
					Application.ResourceAssembly = typeof(App).Assembly;
					app.InitializeComponent();
				}
			}
		}

		public static bool Equal(CircuitProject x, CircuitProject y) {
			Assert.IsNotNull(x);
			Assert.IsNotNull(y);

			return (
				ProjectTester.Equal(x.ProjectSet.Table, y.ProjectSet.Table) &&
				ProjectTester.Equal(x.CollapsedCategorySet.Table, y.CollapsedCategorySet.Table) &&
				ProjectTester.EqualCount(x.CircuitSet.Table, y.CircuitSet.Table) &&
				ProjectTester.EqualCount(x.DevicePinSet.Table, y.DevicePinSet.Table) &&
				ProjectTester.EqualCount(x.GateSet.Table, y.GateSet.Table) &&
				ProjectTester.Equal(x.LogicalCircuitSet.Table, y.LogicalCircuitSet.Table) &&
				ProjectTester.Equal(x.PinSet.Table, y.PinSet.Table) &&
				ProjectTester.Equal(x.ConstantSet.Table, y.ConstantSet.Table) &&
				ProjectTester.Equal(x.ConstantSet.Table, y.ConstantSet.Table) &&
				ProjectTester.Equal(x.CircuitButtonSet.Table, y.CircuitButtonSet.Table) &&
				ProjectTester.Equal(x.MemorySet.Table, y.MemorySet.Table) &&
				ProjectTester.Equal(x.LedMatrixSet.Table, y.LedMatrixSet.Table) &&
				ProjectTester.Equal(x.SplitterSet.Table, y.SplitterSet.Table) &&
				ProjectTester.Equal(x.CircuitSymbolSet.Table, y.CircuitSymbolSet.Table) &&
				ProjectTester.Equal(x.WireSet.Table, y.WireSet.Table) &&
				ProjectTester.Equal(x.TextNoteSet.Table, y.TextNoteSet.Table)
			);
		}

		private static bool EqualCount<T>(TableSnapshot<T> x, TableSnapshot<T> y) where T:struct {
			return x.Count() == y.Count();
		}

		private static bool Equal<T>(TableSnapshot<T> x, TableSnapshot<T> y) where T:struct {
			return ProjectTester.EqualCount(x, y) && x.Zip(y, (RowId xr, RowId yr) => {
				T xd, yd;
				x.GetData(xr, out xd);
				y.GetData(yr, out yd);
				foreach(IField<T> field in x.Fields) {
					if(field.Compare(ref xd, ref yd) != 0) {
						return false;
					}
				}
				return true;
			}).All(r => r);
		}
	}
}
