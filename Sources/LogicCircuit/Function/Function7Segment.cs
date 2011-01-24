using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogicCircuit {
	public class Function7Segment : Probe, IFunctionVisual {

		private static Brush[] stateBrush = null;

		public CircuitSymbol CircuitSymbol { get; private set; }
		private volatile bool evaluating;
		private State[] stateCopy;

		public Function7Segment(CircuitState circuitState, CircuitSymbol symbol, int[] parameter) : base(circuitState, parameter) {
			if(Function7Segment.stateBrush == null) {
				Function7Segment.stateBrush = new Brush[3];
				Function7Segment.stateBrush[(int)State.Off] = (Brush)App.CurrentApp.FindResource("Led7SegmentOff");
				Function7Segment.stateBrush[(int)State.On0] = (Brush)App.CurrentApp.FindResource("Led7SegmentOn0");
				Function7Segment.stateBrush[(int)State.On1] = (Brush)App.CurrentApp.FindResource("Led7SegmentOn1");
			}
			Tracer.Assert(symbol.ProbeView != null);
			this.CircuitSymbol = symbol;
			this.stateCopy = new State[this.BitWidth];
		}

		public void Redraw() {
			int count = 5;
			do {
				while(this.evaluating);
				this.CopyTo(this.stateCopy);
			} while(this.evaluating && 0 <= --count);
			Canvas back = this.CircuitSymbol.ProbeView as Canvas;
			Tracer.Assert(back.Children.Count == this.stateCopy.Length);
			if(back != null) {
				for(int i = 0; i < this.stateCopy.Length; i++) {
					Function7Segment.SetVisual((Shape)back.Children[i], this.stateCopy[i]);
				}
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
			this.evaluating = true;
			bool changed = false;
			try {
				changed = this.GetState();
			} finally {
				this.evaluating = false;
			}
			if(changed) {
				this.CircuitState.Invalidate(this);
			}
			return false;
		}

		public void TurnOn() {
			this.evaluating = false;
		}

		public void TurnOff() {
			Canvas back = this.CircuitSymbol.ProbeView as Canvas;
			if(back != null) {
				for(int i = 0; i < 8; i++) {
					Function7Segment.SetVisual((Shape)back.Children[i], State.Off);
				}
			}
		}
	}
}
