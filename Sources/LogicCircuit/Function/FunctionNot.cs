using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionNot : CircuitFunction {
		private readonly int param0;

		public FunctionNot(CircuitState circuitState, int parameter, int result) : base(circuitState, parameter, result) {
			this.param0 = parameter;
		}
		public override bool Evaluate() {
			return this.SetResult0(CircuitFunction.Not(this.CircuitState[this.param0]));
		}

		public override string ReportName { get { return Properties.Resources.GateNotName; } }
	}
}
