using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LogicCircuit {
	public abstract class FunctionLedMatrix : Probe, IFunctionVisual {
		private static Brush[] brush;

		private readonly List<CircuitSymbol> circuitSymbol;
		private readonly Project project;
		protected LedMatrix Matrix { get; private set; }
		protected int BitPerLed { get; private set; }
		protected LogicalCircuit CurrentLogicalCircuit { get { return this.project.LogicalCircuit; } }

		public bool Invalid { get; set; }

		public override string ReportName { get { return Properties.Resources.NameLedMatrix; } }

		protected FunctionLedMatrix(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int[] parameter) : base(circuitState, parameter) {
			this.circuitSymbol = symbols.ToList();
			this.Matrix = (LedMatrix)this.circuitSymbol[0].Circuit;
			this.BitPerLed = this.Matrix.Colors;
			this.project = this.circuitSymbol[0].LogicalCircuit.CircuitProject.ProjectSet.Project;
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		protected void Fill(int index, int value) {
			UniformGrid grid = null;
			if(this.circuitSymbol.Count == 1) {
				grid = (UniformGrid)this.circuitSymbol[0].ProbeView;
			} else {
				LogicalCircuit currentCircuit = this.CurrentLogicalCircuit;
				CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
				grid = this.ProbeView(symbol);
			}
			((Shape)grid.Children[index]).Fill = FunctionLedMatrix.brush[value];
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
			if(FunctionLedMatrix.brush != null) {
				foreach(CircuitSymbol symbol in this.circuitSymbol) {
					if(symbol.HasCreatedGlyph) {
						UniformGrid grid = this.ProbeView(symbol);
						foreach(Shape shape in grid.Children) {
							shape.Fill = FunctionLedMatrix.brush[0];
						}
					}
				}
			}
		}

		private UniformGrid ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (UniformGrid)this.circuitSymbol[0].ProbeView;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (UniformGrid)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
