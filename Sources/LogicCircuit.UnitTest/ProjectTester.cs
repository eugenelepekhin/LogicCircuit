using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

		public ProjectTester(TestContext testContext, string projectText, string initialCircuit) {
			this.CircuitProject = ProjectTester.Load(testContext, projectText, initialCircuit);

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
			this.Input = inputSymbol.Select(s => (FunctionConstant)this.CircuitMap.Input(s)).ToArray();

			List<CircuitSymbol> outputSymbol = new List<CircuitSymbol>();
			foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
				Gate g = symbol.Circuit as Gate;
				if(g != null && g.GateType == GateType.Probe) {
					outputSymbol.Add(symbol);
				}
			}
			outputSymbol.Sort(circuitSymbolComparer);
			this.Output = outputSymbol.Select(s => this.CircuitMap.FunctionProbe(s)).ToArray();

			this.CircuitMap.TurnOn();
		}

		public static CircuitProject Load(TestContext testContext, string projectText, string initialCircuit) {
			// First save project text to test directory
			string path = Path.Combine(testContext.TestRunDirectory, string.Format("{0}.{1}.{2}.xml", testContext.FullyQualifiedTestClassName, testContext.TestName, DateTime.UtcNow.Ticks));
			File.WriteAllText(path, projectText, Encoding.UTF8);
			// Load if from test directory
			CircuitProject circuitProject = CircuitProject.Create(path);
			File.Delete(path);
			if(initialCircuit != null) {
				ProjectTester.SwitchTo(circuitProject, initialCircuit);
			}
			ProjectTester.InitResources();
			foreach(CircuitSymbol symbol in circuitProject.CircuitSymbolSet) {
				symbol.GuaranteeGlyph();
			}
			return circuitProject;
		}

		public static LogicalCircuit SwitchTo(CircuitProject circuitProject, string logicalCircuitName) {
			Assert.IsNotNull(logicalCircuitName);
			LogicalCircuit circuit = circuitProject.LogicalCircuitSet.FindByName(logicalCircuitName);
			Assert.IsNotNull(circuit, "Circuit {0} not found in the project", logicalCircuitName);
			if(circuitProject.ProjectSet.Project.LogicalCircuit != circuit) {
				circuitProject.InOmitTransaction(() => circuitProject.ProjectSet.Project.LogicalCircuit = circuit);
			}
			return circuit;
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
	}
}
