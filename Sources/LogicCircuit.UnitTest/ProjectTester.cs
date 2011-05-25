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
			// First save project text to test directory
			string path = Path.Combine(testContext.TestRunDirectory, "ProjectTester.xml");
			File.WriteAllText(path, projectText, Encoding.UTF8);
			// Load if from test directory
			this.CircuitProject = CircuitProject.Create(path);
			if(initialCircuit != null) {
				LogicalCircuit circuit = this.CircuitProject.LogicalCircuitSet.FindByName(initialCircuit);
				Assert.IsNotNull(circuit, "initial circuit not found in the project");
				this.CircuitProject.InOmitTransaction(() => this.CircuitProject.ProjectSet.Project.LogicalCircuit = circuit);
			}
			// Init App resources so all the visual elements will be able to find them
			if(App.CurrentApp == null) {
				App app = new App();
				if(Application.ResourceAssembly == null) {
					Application.ResourceAssembly = typeof(App).Assembly;
					app.InitializeComponent();
				}
			}
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet) {
				symbol.GuaranteeGlyph();
			}

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
	}
}
