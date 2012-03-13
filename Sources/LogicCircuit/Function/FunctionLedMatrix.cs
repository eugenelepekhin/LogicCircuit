using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace LogicCircuit {
	public abstract class FunctionLedMatrix : Probe, IFunctionVisual {
		private readonly UniformGrid grid;
		private readonly Color[] color;
		private readonly Brush[] brush;

		protected int BitPerLed { get; private set; }
		public bool Invalid { get; set; }

		protected FunctionLedMatrix(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, parameter) {
			this.grid = (UniformGrid)symbol.ProbeView;
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.BitPerLed = matrix.Colors;
			this.color = new Color[1 << this.BitPerLed];
			this.brush = new Brush[1 << this.BitPerLed];
			this.brush[0] = (Brush)App.Current.FindResource("LedMatrixOff");
			Color[] bitColor = new Color[] { matrix.Color1, matrix.Color2, matrix.Color3 };
			for(int i = 1; i < this.color.Length; i++) {
				Color c = Colors.Black;
				for(int j = 0; j < this.BitPerLed; j++) {
					if((i & (1 << j)) != 0) {
						c += bitColor[j];
					}
				}
				this.color[i] = c;
			}
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		protected void Fill(int index, int value) {
			if(this.brush[value] == null) {
				this.brush[value] = new SolidColorBrush(this.color[value]);
			}
			((Shape)this.grid.Children[index]).Fill = this.brush[value];
		}

		public abstract void Redraw();

		public void TurnOn() {
		}

		public void TurnOff() {
			foreach(Shape shape in this.grid.Children) {
				shape.Fill = this.brush[0];
			}
		}
	}
}
