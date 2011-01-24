using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionInteractive, IFunctionVisual {

		public CircuitSymbol CircuitSymbol { get; private set; }

		public FunctionButton(CircuitState circuitState, CircuitSymbol symbol, int result) : base(circuitState, State.On0, result) {
			Tracer.Assert(symbol.ProbeView != null);
			this.CircuitSymbol = symbol;
		}

		public void OnSymbolPress() {
			this.SetState(State.On1);
		}

		public void OnSymbolRelease() {
			this.SetState(State.On0);
		}

		public void OnSymbolDoubleClick() {
		}

		public void TurnOn() {
			ButtonControl button = this.CircuitSymbol.ProbeView as ButtonControl;
			if(button != null) {
				button.Clickable = true;
			}
		}

		public void TurnOff() {
			ButtonControl button = this.CircuitSymbol.ProbeView as ButtonControl;
			if(button != null) {
				button.Clickable = false;
			}
		}

		public void Redraw() {
		}
	}
}
