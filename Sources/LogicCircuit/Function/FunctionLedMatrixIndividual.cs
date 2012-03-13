using System;

namespace LogicCircuit {
	public class FunctionLedMatrixIndividual : FunctionLedMatrix {
		private readonly int[] state;

		public FunctionLedMatrixIndividual(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, symbol, parameter) {
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.state = new int[matrix.Rows * matrix.Columns];
		}

		public override void Redraw() {
			for(int i = 0; i < this.state.Length; i++) {
				int value = 0;
				for(int j = 0; j < this.BitPerLed; j++) {
					if(this[i * this.BitPerLed + j] == State.On1) {
						value |= 1 << j;
					}
				}
				if(this.state[i] != value) {
					this.state[i] = value;
					this.Fill(i, value);
				}
			}
		}
	}
}
