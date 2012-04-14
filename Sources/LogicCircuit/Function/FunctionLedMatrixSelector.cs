using System;

namespace LogicCircuit {
	public class FunctionLedMatrixSelector : FunctionLedMatrix {
		private readonly State[] row;
		private readonly int[] column;
		private readonly bool[] columnChanged;
		private readonly int rowParameter;

		/// <summary>
		/// Creates function. Assumes parameter layout: first goes columns states starting from column 0, bit 0 to bit 2. After all columns goes rows they are one bit wide.
		/// </summary>
		/// <param name="circuitState"></param>
		/// <param name="symbol"></param>
		/// <param name="parameter"></param>
		public FunctionLedMatrixSelector(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, symbol, parameter) {
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.row = new State[matrix.Rows];
			int columns = matrix.Columns;
			this.column = new int[columns];
			this.columnChanged = new bool[columns];
			this.rowParameter = columns * this.BitPerLed;
		}

		public override void Redraw() {
			// track changes in the column state parameters
			for(int i = 0; i < this.column.Length; i++) {
				int value = 0;
				for(int j = 0; j < this.BitPerLed; j++) {
					if(this[i * this.BitPerLed + j] == State.On1) {
						value |= 1 << j;
					}
				}
				if(this.columnChanged[i] = (value != this.column[i])) {
					this.column[i] = value;
				}
			}
			for(int i = 0; i < this.row.Length; i++) {
				State rowState = this[rowParameter + i];
				if(this.row[i] != rowState) {
					// state of row was changed
					this.row[i] = rowState;
					if(rowState == State.On1) {
						// set all the columns values
						for(int j = 0; j < this.column.Length; j++) {
							this.Fill(i * this.column.Length + j, this.column[j]);
						}
					} else { // rowState == State.On0 or Off that is same as 0 here.
						for(int j = 0; j < this.column.Length; j++) {
							this.Fill(i * this.column.Length + j, 0);
						}
					}
				} else if(rowState == State.On1) {
					// row state was not changed so update all the columns that was changed in this row
					for(int j = 0; j < this.column.Length; j++) {
						if(this.columnChanged[j]) {
							this.Fill(i * this.column.Length + j, this.column[j]);
						}
					}
				}
			}
		}
	}
}
