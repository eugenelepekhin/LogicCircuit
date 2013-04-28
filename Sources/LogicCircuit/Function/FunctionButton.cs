using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionVisual {

		private static Brush[] stateBrush = null;

		private CircuitSymbol circuitSymbol;
		public bool IsToggle { get; private set; }

		public FunctionButton(CircuitState circuitState, CircuitSymbol symbol, int result) : base(circuitState, State.On0, result) {
			this.circuitSymbol = symbol;
			this.IsToggle = ((CircuitButton)symbol.Circuit).IsToggle;
			if(this.IsToggle && FunctionButton.stateBrush == null) {
				FunctionButton.stateBrush = new Brush[] {
					(Brush)App.CurrentApp.FindResource("Led7SegmentOff"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn0"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn1")
				};
			}
		}

		public bool Invalid { get; set; }

		public override string ReportName { get { return Properties.Resources.NameButton; } }

		private void SymbolPress() {
			if(this.IsToggle) {
				this.SetState(CircuitFunction.Not(this.State));
				this.Invalid = true;
			} else {
				this.SetState(State.On1);
			}
		}

		private void SymbolRelease() {
			if(!this.IsToggle) {
				this.SetState(State.On0);
			}
		}

		private void StateChangedAction(CircuitSymbol symbol, bool isPressed) {
			if(isPressed) {
				this.SymbolPress();
			} else {
				this.SymbolRelease();
			}
		}

		public void TurnOn() {
			ButtonControl button = (ButtonControl)this.circuitSymbol.ProbeView;
			button.ButtonStateChanged = this.StateChangedAction;
		}

		public void TurnOff() {
			if(this.circuitSymbol.HasCreatedGlyph) {
				ButtonControl button = (ButtonControl)this.circuitSymbol.ProbeView;
				button.ButtonStateChanged = null;
				this.DrawState(State.Off);
			}
		}

		public void Redraw() {
			this.DrawState(this.State);
		}

		private void DrawState(State state) {
			if(this.IsToggle) {
				Canvas canvas = (Canvas)this.circuitSymbol.Glyph;
				Border border = (Border)canvas.Children[2];
				Tracer.Assert(border != null);
				border.Background = FunctionButton.stateBrush[(int)state];
			}
		}
	}
}
