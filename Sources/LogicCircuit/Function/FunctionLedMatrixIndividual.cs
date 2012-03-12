using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace LogicCircuit {
	public class FunctionLedMatrixIndividual : Probe, IFunctionVisual {
		private readonly CircuitSymbol circuitSymbol;
		private readonly int leds;
		private readonly int colors;
		private readonly Color[] color;
		private readonly Brush[] brush;
		private readonly int[] state;
		
		public FunctionLedMatrixIndividual(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, parameter) {
			this.circuitSymbol = symbol;
			LedMatrix matrix = (LedMatrix)symbol.Circuit;
			this.leds = matrix.Rows * matrix.Columns;
			this.colors = matrix.Colors;
			this.color = new Color[1 << this.colors];
			this.brush = new Brush[1 << this.colors];
			this.brush[0] = (Brush)App.Current.FindResource("LedMatrixOff");
			this.state = new int[this.leds];
			Color[] bitColor = new Color[] { matrix.Color1, matrix.Color2, matrix.Color3 };
			for(int i = 1; i < this.color.Length; i++) {
				Color c = Colors.Black;
				for(int j = 0; j < this.colors; j++) {
					if((i & (1 << j)) != 0) {
						c += bitColor[j];
					}
				}
				this.color[i] = c;
			}
		}

		public bool Invalid { get; set; }

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		public void Redraw() {
			UniformGrid back = (UniformGrid)this.circuitSymbol.ProbeView;
			for(int i = 0; i < this.leds; i++) {
				int value = 0;
				for(int j = 0; j < this.colors; j++) {
					if(this[i * this.colors + j] == State.On1) {
						value |= 1 << j;
					}
				}
				if(this.state[i] != value) {
					this.state[i] = value;
					Shape shape = (Shape)back.Children[i];
					if(this.brush[value] == null) {
						this.brush[value] = new SolidColorBrush(this.color[value]);
					}
					shape.Fill = this.brush[value];
				}
			}
		}

		public void TurnOn() {
		}

		public void TurnOff() {
			UniformGrid back = (UniformGrid)this.circuitSymbol.ProbeView;
			foreach(Shape shape in back.Children) {
				shape.Fill = this.brush[0];
			}
		}
	}
}
