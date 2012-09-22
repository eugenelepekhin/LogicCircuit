using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionOrNot : CircuitFunction {
		public FunctionOrNot(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}
		public override bool Evaluate() {
			return this.SetResult(CircuitFunction.Not(this.Or()));
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateOrNotName, this.ParameterCount); } }
	}
}
