using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	public abstract class Probe : CircuitFunction {
		private State[] state;

		protected Probe(CircuitState circuitState, int[] parameter) : base(circuitState, parameter, null) {
			this.Init(parameter != null ? parameter.Length : 0);
			Debug.Assert(this.state != null);
		}
		protected Probe(CircuitState circuitState, int parameter) : base(circuitState, new int[] { parameter }, null) {
			this.Init(1);
			Debug.Assert(this.state != null);
		}
		private void Init(int count) {
			if(0 < count) {
				this.state = new State[count];
			} else {
				this.state = Array.Empty<State>();
			}
		}

		public string ToText() {
			return CircuitFunction.ToText(this.state, false);
		}

		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		protected bool GetState() {
			return this.GetProbeState(this.state);
		}

		public int BitWidth { get { return this.state != null ? this.state.Length : 0; } }
		public State this[int index] { get { return this.state[index]; } }

		protected void CopyTo(State[] copy) {
			if(copy == null || copy.Length != this.state.Length) {
				throw new ArgumentOutOfRangeException(nameof(copy));
			}
			this.state.CopyTo(copy, 0);
		}
	}
}
