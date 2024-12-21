﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class FunctionButton : OneBitConst, IFunctionVisual {

		private static Brush[]? stateBrush;

		private readonly List<CircuitSymbol> circuitSymbol;
		private readonly bool isToggle;
		private readonly bool inverted;
		private readonly Project project;

		public FunctionButton(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int result) : base(circuitState, State.On0, result) {
			this.circuitSymbol = symbols.ToList();
			this.project = this.circuitSymbol[0].LogicalCircuit.CircuitProject.ProjectSet.Project;
			this.isToggle = ((CircuitButton)this.circuitSymbol[0].Circuit).IsToggle;
			this.inverted = ((CircuitButton)this.circuitSymbol[0].Circuit).Inverted;

			if(this.isToggle && FunctionButton.stateBrush == null) {
				FunctionButton.stateBrush = new Brush[] {
					(Brush)App.CurrentApp.FindResource("Led7SegmentOff"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn0"),
					(Brush)App.CurrentApp.FindResource("Led7SegmentOn1")
				};
			}
			if(this.inverted) {
				this.SetState(State.On1);
			}
		}

		public bool Invalid { get; set; }

		public override string ReportName { get { return Properties.Resources.NameButton; } }

		public CircuitSymbol? ButtonSymbol() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.Circuit is CircuitButton) {
					return symbol;
				}
			}
			return null;
		}

		public void StateChangedAction(CircuitSymbol symbol, bool isPressed) {
			if(isPressed) {
				if(this.isToggle) {
					this.SetState(CircuitFunction.Not(this.State));
					this.Invalid = true;
				} else {
					this.SetState(this.inverted ? State.On0 : State.On1);
				}
			} else if(!this.isToggle) {
				this.SetState(this.inverted ? State.On1 : State.On0);
			}
		}

		public void TurnOn() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					ButtonControl button = this.ProbeView(symbol);
					bool turnedOff = (button.ButtonStateChanged == null);
					button.ButtonStateChanged = this.StateChangedAction;
					if(turnedOff && this.isToggle) {
						FunctionButton.DrawState(button, this.State);
					}
				}
			}
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					ButtonControl button = this.ProbeView(symbol);
					button.ButtonStateChanged = null;
					if(this.isToggle) {
						FunctionButton.DrawState(button, State.Off);
					}
				}
			}
		}

		public void Redraw(bool force) {
			if(this.isToggle) {
				ButtonControl? button = null;
				if(this.circuitSymbol.Count == 1) {
					button = (ButtonControl)this.circuitSymbol[0].ProbeView!;
				} else {
					LogicalCircuit currentCircuit = this.project.LogicalCircuit;
					CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
					button = this.ProbeView(symbol);
				}
				FunctionButton.DrawState(button, this.State);
			}
		}

		private static void DrawState(ButtonControl button, State state) {
			Canvas canvas = (Canvas)button.Parent;
			Panel panel = (Panel)canvas.Children[canvas.Children.Count - 1];
			Border border = (Border)panel.Children[0];
			Tracer.Assert(border);
			border.Background = FunctionButton.stateBrush![(int)state];
		}

		private ButtonControl ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (ButtonControl)this.circuitSymbol[0].ProbeView!;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (ButtonControl)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
