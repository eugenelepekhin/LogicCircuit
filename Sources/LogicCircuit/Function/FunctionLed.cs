using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogicCircuit {
	public class FunctionLed : Probe, IFunctionVisual {

		private static Brush[] stateBrush = null;

		private readonly List<CircuitSymbol> circuitSymbol;
		private readonly Project project;
		private LogicalCircuit lastLogicalCircuit = null;
		private Shape lastShape;

		public FunctionLed(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int parameter) : base(circuitState, parameter) {
			if(FunctionLed.stateBrush == null) {
				FunctionLed.stateBrush = new Brush[3];
				FunctionLed.stateBrush[(int)State.Off] = (Brush)App.CurrentApp.FindResource("LedOff");
				FunctionLed.stateBrush[(int)State.On0] = (Brush)App.CurrentApp.FindResource("LedOn0");
				FunctionLed.stateBrush[(int)State.On1] = (Brush)App.CurrentApp.FindResource("LedOn1");
			}
			this.circuitSymbol = symbols.ToList();
			this.project = this.circuitSymbol[0].LogicalCircuit.CircuitProject.ProjectSet.Project;
		}

		public bool Invalid { get; set; }

		public override string ReportName { get { return Properties.Resources.GateLedName; } }

		public void Redraw() {
			LogicalCircuit currentCircuit = this.project.LogicalCircuit;
			if(this.lastLogicalCircuit != currentCircuit) {
				this.lastLogicalCircuit = currentCircuit;
				this.lastShape = null;
			}
			if(this.lastShape == null) {
				if(this.circuitSymbol.Count == 1) {
					this.lastShape = (Shape)this.circuitSymbol[0].ProbeView;
				} else {
					CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
					this.lastShape = this.ProbeView(symbol);
				}
			}
			this.lastShape.Fill = FunctionLed.stateBrush[(int)this[0]];
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		public void TurnOn() {
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					Shape shape = this.ProbeView(symbol);
					shape.Fill = FunctionLed.stateBrush[(int)State.Off];
				}
			}
		}

		private Shape ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (Shape)this.circuitSymbol[0].ProbeView;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (Shape)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
