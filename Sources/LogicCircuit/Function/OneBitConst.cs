using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class OneBitConst : CircuitFunction {
		private State state;
		public State State { get { return this.state; } }
		public void SetState(State newState) {
			if(this.state != newState) {
				this.state = newState;
				this.CircuitState.MarkUpdated(this);
			}
		}

		public OneBitConst(CircuitState circuitState, State state, int result) : base(circuitState, null, new int[] { result }) {
			this.state = state;
		}
		public override bool Evaluate() {
			return this.SetResult(this.state);
		}
	}
}
