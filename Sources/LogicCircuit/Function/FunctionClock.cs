using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionClock : CircuitFunction {
		private bool state;
		public FunctionClock(CircuitState circuitState, int result) : base(circuitState, null, new int[] { result }) {
			this.state = false;
		}
		public void Flip() {
			this.state = !this.state;
		}
		public override bool Evaluate() {
			return this.SetResult(CircuitFunction.FromBool(this.state));
		}

		public override string ReportName { get { return Resources.GateClockName; } }
	}
}
