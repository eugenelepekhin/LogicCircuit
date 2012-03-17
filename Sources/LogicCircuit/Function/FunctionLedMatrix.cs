using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace LogicCircuit {
	public abstract class FunctionLedMatrix : Probe, IFunctionVisual {
		private static Brush[] brush;

		private readonly UniformGrid grid;
		protected int BitPerLed { get; private set; }
		public bool Invalid { get; set; }

		protected FunctionLedMatrix(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, parameter) {
			this.grid = (UniformGrid)symbol.ProbeView;
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.BitPerLed = matrix.Colors;
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		protected void Fill(int index, int value) {
			((Shape)this.grid.Children[index]).Fill = FunctionLedMatrix.brush[value];
		}

		public abstract void Redraw();

		public void TurnOn() {
			if(FunctionLedMatrix.brush == null) {
				Color[] color = new Color[] { Colors.Red, Colors.Lime, Colors.Blue };
				FunctionLedMatrix.brush = new Brush[1 << LedMatrix.MaxBitsPerLed];
				FunctionLedMatrix.brush[0] = (Brush)App.Current.FindResource("LedMatrixOff");
				for(int i = 1; i < FunctionLedMatrix.brush.Length; i++) {
					Color c = Colors.Black;
					for(int j = 0; j < LedMatrix.MaxBitsPerLed; j++) {
						if((i & (1 << j)) != 0) {
							c += color[j];
						}
					}
					FunctionLedMatrix.brush[i] = new SolidColorBrush(c);
				}
			}
		}

		public void TurnOff() {
			if(FunctionLedMatrix.brush == null) {
				foreach(Shape shape in this.grid.Children) {
					shape.Fill = FunctionLedMatrix.brush[0];
				}
			}
		}
	}
}
