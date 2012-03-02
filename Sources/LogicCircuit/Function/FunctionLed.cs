using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogicCircuit {
	public class FunctionLed : Probe, IFunctionVisual {

		private static Brush[] stateBrush = null;

		public CircuitSymbol CircuitSymbol { get; private set; }

		public FunctionLed(CircuitState circuitState, CircuitSymbol symbol, int parameter) : base(circuitState, parameter) {
			if(FunctionLed.stateBrush == null) {
				FunctionLed.stateBrush = new Brush[3];
				FunctionLed.stateBrush[(int)State.Off] = (Brush)App.CurrentApp.FindResource("LedOff");
				FunctionLed.stateBrush[(int)State.On0] = (Brush)App.CurrentApp.FindResource("LedOn0");
				FunctionLed.stateBrush[(int)State.On1] = (Brush)App.CurrentApp.FindResource("LedOn1");
			}
			this.CircuitSymbol = symbol;
		}

		public bool Invalid { get; set; }

		public void Redraw() {
			Shape shape = (Shape)this.CircuitSymbol.ProbeView;
			shape.Fill = FunctionLed.stateBrush[(int)this[0]];
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
			if(this.CircuitSymbol.HasCreatedGlyph) {
				Shape shape = (Shape)this.CircuitSymbol.ProbeView;
				shape.Fill = FunctionLed.stateBrush[(int)State.Off];
			}
		}
	}
}
