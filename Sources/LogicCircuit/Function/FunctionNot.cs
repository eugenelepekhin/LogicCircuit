using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionNot : CircuitFunction {
		public FunctionNot(CircuitState circuitState, int parameter, int result) : base(circuitState, parameter, result) {}
		public override bool Evaluate() {
			return this.SetResult(this.Not());
		}

		public override string ReportName { get { return Properties.Resources.GateNotName; } }
	}
}
