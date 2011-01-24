using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LogicCircuit {
	public class FunctionConstant : CircuitFunction, IFunctionInteractive {

		public CircuitSymbol CircuitSymbol { get; private set; }

		public FunctionConstant(CircuitState circuitState, CircuitSymbol symbol, int[] result) : base(circuitState, null, result) {
			Tracer.Assert(symbol.ProbeView != null);
			this.CircuitSymbol = symbol;
			Tracer.Assert(this.BitWidth == result.Length);
		}

		public Constant Constant { get { return (Constant)this.CircuitSymbol.Circuit; } }
		public int BitWidth { get { return this.Constant.BitWidth; } }
		public int Value {
			get { return this.Constant.ConstantValue; }
			set {
				int old = this.Value;
				this.Constant.ConstantValue = value;
				if(old != this.Value) {
					((TextBlock)this.CircuitSymbol.ProbeView).Text =  this.Constant.Notation;
					this.CircuitSymbol.Glyph.ToolTip = this.Constant.ToolTip;
					this.CircuitState.MarkUpdated(this);
				}
			}
		}

		public void OnSymbolPress() {
		}

		public void OnSymbolRelease() {
		}

		public void OnSymbolDoubleClick() {
			this.Constant.CircuitProject.InTransaction(() => this.Value++);
		}

		public override bool Evaluate() {
			return this.SetResult(this.Value);
		}
	}
}
