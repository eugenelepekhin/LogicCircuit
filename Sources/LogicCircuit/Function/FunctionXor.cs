using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionXor : CircuitFunction {
		public FunctionXor(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}
		public override bool Evaluate() {
			return this.SetResult(CircuitFunction.FromBool((this.Count(State.On1) == 1)));
		}

		public override string ReportName { get { return Resources.ReportGateName(Resources.GateXorName, this.ParameterCount); } }
	}
}
