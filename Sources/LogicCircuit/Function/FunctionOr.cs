using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionOr : CircuitFunction {
		public FunctionOr(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}
		public override bool Evaluate() {
			return this.SetResult(this.Or());
		}

		public override string ReportName { get { return Resources.ReportGateName(Resources.GateOrName, this.ParameterCount); } }
	}
}
