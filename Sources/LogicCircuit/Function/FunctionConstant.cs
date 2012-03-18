using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LogicCircuit {
	public class FunctionConstant : CircuitFunction, IFunctionVisual {

		public CircuitSymbol CircuitSymbol { get; private set; }

		public FunctionConstant(CircuitState circuitState, CircuitSymbol symbol, int[] result) : base(circuitState, null, result) {
			this.CircuitSymbol = symbol;
			Tracer.Assert(this.BitWidth == result.Length);
		}

		public Constant Constant { get { return (Constant)this.CircuitSymbol.Circuit; } }
		public int BitWidth { get { return this.Constant.BitWidth; } }
		public int Value {
			get { return this.Constant.ConstantValue; }
			set {
				int newValue = Constant.Normalize(value, this.Constant.BitWidth);
				if(newValue != this.Constant.ConstantValue) {
					this.Constant.ConstantValue = newValue;
					this.CircuitState.MarkUpdated(this);
					this.Invalid = true;
				}
			}
		}

		public bool Invalid { get; set; }

		public override string ReportName { get { return Resources.NameConstant; } }

		public override bool Evaluate() {
			return this.SetResult(this.Value);
		}

		public void TurnOn() {
			// Nothing to do
		}

		public void TurnOff() {
			if(this.CircuitSymbol.HasCreatedGlyph) {
				this.Redraw();
			}
		}

		public void Redraw() {
			if(this.CircuitSymbol.ProbeView != null) {
				((TextBlock)this.CircuitSymbol.ProbeView).Text =  this.Constant.Notation;
				this.CircuitSymbol.Glyph.ToolTip = this.Constant.ToolTip;
			}
		}
	}
}
