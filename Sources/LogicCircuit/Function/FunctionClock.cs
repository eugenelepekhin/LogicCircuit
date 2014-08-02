using System;

namespace LogicCircuit {
	public class FunctionClock : CircuitFunction, IFunctionClock {
		private bool state;
		public FunctionClock(CircuitState circuitState, int result) : base(circuitState, null, new int[] { result }) {
			this.state = false;
		}
		public bool Flip() {
			this.state = !this.state;
			return true;
		}
		public override bool Evaluate() {
			return this.SetResult0(CircuitFunction.FromBool(this.state));
		}

		public override string ReportName { get { return Properties.Resources.GateClockName; } }
	}
}
