using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionVisual {

		private static Brush[] stateBrush = null;

		public CircuitSymbol CircuitSymbol { get; private set; }
		public bool IsToggle { get; private set; }

		public FunctionButton(CircuitState circuitState, CircuitSymbol symbol, int result) : base(circuitState, State.On0, result) {
			this.CircuitSymbol = symbol;
			this.IsToggle = ((CircuitButton)symbol.Circuit).IsToggle;
			if(this.IsToggle && FunctionButton.stateBrush == null) {
				FunctionButton.stateBrush = new Brush[] {
					(Brush)App.CurrentApp.FindResource("Led7SegmentOff"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn0"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn1")
				};
			}
		}

		public void SymbolPress() {
			if(this.IsToggle) {
				this.SetState(CircuitFunction.Not(this.State));
				this.CircuitState.Invalidate(this);
			} else {
				this.SetState(State.On1);
			}
		}

		public void SymbolRelease() {
			if(!this.IsToggle) {
				this.SetState(State.On0);
			}
		}

		public void TurnOn() {
			this.CircuitSymbol.GuaranteeGlyph();
			ButtonControl button = (ButtonControl)this.CircuitSymbol.ProbeView;
			button.Clickable = true;
		}

		public void TurnOff() {
			ButtonControl button = (ButtonControl)this.CircuitSymbol.ProbeView;
			button.Clickable = false;
			this.DrawState(State.Off);
		}

		public void Redraw() {
			this.DrawState(this.State);
		}

		private void DrawState(State state) {
			if(this.IsToggle) {
				Canvas canvas = (Canvas)this.CircuitSymbol.Glyph;
				Border border = (Border)canvas.Children[2];
				Tracer.Assert(border != null);
				border.Background = FunctionButton.stateBrush[(int)state];
			}
		}
	}
}
