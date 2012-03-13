using System;

namespace LogicCircuit {
	public class FunctionLedMatrixSelector : FunctionLedMatrix {
		private readonly State[] column;
		private readonly int[] row;
		private readonly bool[] rowChanged;
		private readonly int columnParameter;

		/// <summary>
		/// Creates function. Assumes parameter layout: first goes rows states starting from row 0 starting from bit 0 to bit 2. After all rows goes columns they are one bit wide.
		/// </summary>
		/// <param name="circuitState"></param>
		/// <param name="symbol"></param>
		/// <param name="parameter"></param>
		public FunctionLedMatrixSelector(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, symbol, parameter) {
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.column = new State[matrix.Columns];
			int rows = matrix.Rows;
			this.row = new int[rows];
			this.rowChanged = new bool[rows];
			this.columnParameter = rows * this.BitPerLed;
		}

		public override void Redraw() {
			// track changes in the row state parameters
			for(int i = 0; i < this.row.Length; i++) {
				int value = 0;
				for(int j = 0; j < this.BitPerLed; j++) {
					if(this[i * this.BitPerLed + j] == State.On1) {
						value |= 1 << j;
					}
				}
				if(this.rowChanged[i] = (value != this.row[i])) {
					this.row[i] = value;
				}
			}
			for(int i = 0; i < this.column.Length; i++) {
				State columnState = this[columnParameter + i];
				if(this.column[i] != columnState) {
					// state of column was changed
					this.column[i] = columnState;
					if(columnState == State.On1) {
						// set all the rows values
						for(int j = 0; j < this.row.Length; j++) {
							this.Fill(j * this.row.Length + i, this.row[j]);
						}
					} else { // columnState == State.On0 or Off that is same as 0 here.
						for(int j = 0; j < this.row.Length; j++) {
							this.Fill(j * this.row.Length + i, 0);
						}
					}
				} else if(columnState == State.On1) {
					// column state was not changed so update all the rows for this column that was changed
					for(int j = 0; j < this.row.Length; j++) {
						if(this.rowChanged[j]) {
							this.Fill(j * this.row.Length + i, this.row[j]);
						}
					}
				}
			}
		}
	}
}
