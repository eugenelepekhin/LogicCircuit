using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class Probe : CircuitFunction {
		private State[] state;

		protected Probe(CircuitState circuitState, int[] parameter) : base(circuitState, parameter, null) {
			this.Init(parameter != null ? parameter.Length : 0);
		}
		protected Probe(CircuitState circuitState, int parameter) : base(circuitState, new int[] { parameter }, null) {
			this.Init(1);
		}
		private void Init(int count) {
			if(0 < count) {
				this.state = new State[count];
			}
		}

		protected bool GetState() {
			bool changed = false;
			int index = 0;
			foreach(int parameter in this.Parameter) {
				if(this.state[index] != this.CircuitState[parameter]) {
					this.state[index] = this.CircuitState[parameter];
					changed = true;
				}
				index++;
			}
			return changed;
		}

		public int BitWidth { get { return this.state != null ? this.state.Length : 0; } }
		public State this[int index] { get { return this.state[index]; } }

		protected void CopyTo(State[] copy) {
			if(copy == null || copy.Length != this.state.Length) {
				throw new ArgumentOutOfRangeException("copy");
			}
			this.state.CopyTo(copy, 0);
		}
	}
}
