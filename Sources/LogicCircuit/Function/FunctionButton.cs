using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionVisual {

		private static Brush[] stateBrush = null;

		private List<CircuitSymbol> circuitSymbol;
		private bool isToggle;

		public FunctionButton(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int result) : base(circuitState, State.On0, result) {
			this.circuitSymbol = symbols.ToList();
			this.isToggle = ((CircuitButton)this.circuitSymbol[0].Circuit).IsToggle;
			if(this.isToggle && FunctionButton.stateBrush == null) {
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
			if(this.isToggle) {
				this.SetState(CircuitFunction.Not(this.State));
				this.Invalid = true;
			} else {
				this.SetState(State.On1);
			}
		}

		private void SymbolRelease() {
			if(!this.isToggle) {
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
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					ButtonControl button = this.ProbeView(symbol);
					button.ButtonStateChanged = this.StateChangedAction;
					this.DrawState(button, State.Off);
				}
			}
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					ButtonControl button = this.ProbeView(symbol);
					button.ButtonStateChanged = null;
					this.DrawState(button, State.Off);
				}
			}
		}

		public void Redraw() {
			if(this.isToggle) {
				ButtonControl button = null;
				if(this.circuitSymbol.Count == 1) {
					button = (ButtonControl)this.circuitSymbol[0].ProbeView;
				} else {
					LogicalCircuit currentCircuit = this.circuitSymbol[0].LogicalCircuit.CircuitProject.ProjectSet.Project.LogicalCircuit;
					CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
					button = this.ProbeView(symbol);
				}
				this.DrawState(button, this.State);
			}
		}

		private void DrawState(ButtonControl button, State state) {
			if(this.isToggle) {
				Canvas canvas = (Canvas)button.Parent;
				Border border = (Border)canvas.Children[canvas.Children.Count - 1];
				Tracer.Assert(border != null);
				border.Background = FunctionButton.stateBrush[(int)state];
			}
		}

		private ButtonControl ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (ButtonControl)this.circuitSymbol[0].ProbeView;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (ButtonControl)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
