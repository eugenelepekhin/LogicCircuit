using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionTriState : CircuitFunction {
		public FunctionTriState(CircuitState circuitState, int parameter, int enable, int result) : base(
			circuitState, new int[] { parameter, enable }, result
		) {
		}
		public override bool Evaluate() {
			return this.SetResult(this.ControlledState(State.On1));
		}
	}
}
