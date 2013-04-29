using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogicCircuit {
	public class Function7Segment : Probe, IFunctionVisual {

		private static Brush[] stateBrush = null;

		private List<CircuitSymbol> circuitSymbol;

		public Function7Segment(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int[] parameter) : base(circuitState, parameter) {
			if(Function7Segment.stateBrush == null) {
				Function7Segment.stateBrush = new Brush[3];
				Function7Segment.stateBrush[(int)State.Off] = (Brush)App.CurrentApp.FindResource("Led7SegmentOff");
				Function7Segment.stateBrush[(int)State.On0] = (Brush)App.CurrentApp.FindResource("Led7SegmentOn0");
				Function7Segment.stateBrush[(int)State.On1] = (Brush)App.CurrentApp.FindResource("Led7SegmentOn1");
			}
			this.circuitSymbol = symbols.ToList();
		}

		public bool Invalid { get; set; }

		public override string ReportName { get { return Properties.Resources.Gate7SegName; } }

		public void Redraw() {
			Canvas back = null;
			if(this.circuitSymbol.Count == 1) {
				back = (Canvas)this.circuitSymbol[0].ProbeView;
			} else {
				LogicalCircuit currentCircuit = this.circuitSymbol[0].LogicalCircuit.CircuitProject.ProjectSet.Project.LogicalCircuit;
				CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
				back = this.ProbeView(symbol);
			}
			Tracer.Assert(back.Children.Count == this.BitWidth);
			for(int i = 0; i < this.BitWidth; i++) {
				Function7Segment.SetVisual((Shape)back.Children[i], this[i]);
			}
		}

		private static void SetVisual(Shape shape, State state) {
			Brush brush = Function7Segment.stateBrush[(int)state];
			Line line = shape as Line;
			if(line != null) {
				line.Stroke = brush;
			} else {
				shape.Fill = brush;
			}
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				this.Invalid = true;
			}
			return false;
		}

		public void TurnOn() {
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					Canvas back = this.ProbeView(symbol);
					for(int i = 0; i < 8; i++) {
						Function7Segment.SetVisual((Shape)back.Children[i], State.Off);
					}
				}
			}
		}

		private Canvas ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (Canvas)this.circuitSymbol[0].ProbeView;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (Canvas)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
