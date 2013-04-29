using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public class FunctionLedMatrixSelector : FunctionLedMatrix, IFunctionClock {
		private readonly State[] row;
		private readonly int[] column;
		private readonly bool[] columnChanged;
		private readonly int[] cell;
		private readonly int[] cellFlip;
		private readonly int rowParameter;
		private int flip;
		private LogicalCircuit lastLogicalCircuit = null;

		/// <summary>
		/// Creates function. Assumes parameter layout: first goes columns states starting from column 0, bit 0 to bit 2. After all columns goes rows they are one bit wide.
		/// </summary>
		/// <param name="circuitState"></param>
		/// <param name="symbol"></param>
		/// <param name="parameter"></param>
		public FunctionLedMatrixSelector(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int[] parameter) : base(circuitState, symbols, parameter) {
			LedMatrix matrix = this.Matrix;
			this.row = new State[matrix.Rows];
			int columns = matrix.Columns;
			this.column = new int[columns];
			this.columnChanged = new bool[columns];
			this.cell = new int[this.column.Length * this.row.Length];
			this.cellFlip = new int[this.column.Length * this.row.Length];
			this.rowParameter = columns * this.BitPerLed;
		}

		public bool Flip() {
			this.flip++;
			this.Invalid = true;
			return false;
		}

		public override void Redraw() {
			LogicalCircuit current = this.CurrentLogicalCircuit;
			if(current != this.lastLogicalCircuit) {
				this.lastLogicalCircuit = current;
				for(int i = 0; i < this.row.Length; i++) {
					this.row[i] = (State)(0xFF);
				}
				for(int i = 0; i < this.column.Length; i++) {
					this.column[i] = -1;
				}
			}
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
							int value = this.column[j];
							int index = i * this.column.Length + j;
							if(value != 0) {
								this.Fill(index, value);
							} else {
								this.cellFlip[index] = this.flip;
							}
							this.cell[index] = value;
						}
					} else { // rowState == State.On0 or Off that is same as 0 here.
						for(int j = 0; j < this.column.Length; j++) {
							int index = i * this.column.Length + j;
							this.cell[index] = 0;
							this.cellFlip[index] = this.flip;
						}
					}
				} else if(rowState == State.On1) {
					// row state was not changed so update all the columns that was changed in this row
					for(int j = 0; j < this.column.Length; j++) {
						if(this.columnChanged[j]) {
							int value = this.column[j];
							int index = i * this.column.Length + j;
							if(value != 0) {
								this.Fill(index, value);
							} else {
								this.cellFlip[index] = this.flip;
							}
							this.cell[index] = value;
						}
					}
				}
			}
			int toOff = this.flip - this.row.Length;
			for(int i = 0; i < this.cellFlip.Length; i++) {
				if(this.cell[i] == 0 && this.cellFlip[i] == toOff) {
					this.Fill(i, 0);
				}
			}
		}
	}
}
