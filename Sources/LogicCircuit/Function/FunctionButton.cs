using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionVisual {

		public CircuitSymbol CircuitSymbol { get; private set; }

		public FunctionButton(CircuitState circuitState, CircuitSymbol symbol, int result) : base(circuitState, State.On0, result) {
			this.CircuitSymbol = symbol;
		}

		public void SymbolPress() {
			this.SetState(State.On1);
		}

		public void SymbolRelease() {
			this.SetState(State.On0);
		}

		public void TurnOn() {
			this.CircuitSymbol.GuaranteeGlyph();
			ButtonControl button = (ButtonControl)this.CircuitSymbol.ProbeView;
			button.Clickable = true;
		}

		public void TurnOff() {
			ButtonControl button = (ButtonControl)this.CircuitSymbol.ProbeView;
			button.Clickable = false;
		}

		public void Redraw() {
		}
	}
}
