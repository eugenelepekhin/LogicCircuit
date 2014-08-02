using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TriState")]
	public class FunctionTriState : CircuitFunction {
		public FunctionTriState(CircuitState circuitState, int parameter, int enable, int result) : base(
			circuitState, new int[] { parameter, enable }, result
		) {
		}
		public override bool Evaluate() {
			return this.SetResult0(this.ControlledState(State.On1));
		}

		public override string ReportName { get { return Properties.Resources.GateTriStateName; } }
	}
}
