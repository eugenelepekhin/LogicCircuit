using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class CircuitFunction {
		private int[] parameter;
		private int[] result;
		public  CircuitState CircuitState { get; private set; }

		public IEnumerable<int> Parameter { get { return this.parameter; } }
		public IEnumerable<int> Result { get { return this.result; } }

		public long Iteration { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public CircuitFunction[] Dependant { get; set; }

		protected CircuitFunction(CircuitState circuitState, int[] parameter, int[] result) {
			this.CircuitState = circuitState;
			this.parameter = (parameter == null) ? new int[0] : parameter;
			this.result = (result == null) ? new int[0] : result;
			this.CircuitState.DefineFunction(this);
		}
		protected CircuitFunction(CircuitState circuitState, int[] parameter, int minimumParameterCount, int result) : this(circuitState, parameter, new int[] { result }) {
			if(parameter == null) {
				throw new ArgumentNullException("parameter");
			}
			if(parameter.Length < minimumParameterCount) {
				throw new ArgumentException(Properties.Resources.FunctionParameter(this.Name, minimumParameterCount));
			}
		}
		protected CircuitFunction(CircuitState circuitState, int[] parameter, int result) : this(circuitState, parameter, 1, result) {
		}
		protected CircuitFunction(CircuitState circuitState, int parameter, int result) : this(circuitState, new int[] { parameter }, new int[] { result }) {
		}

		public abstract bool Evaluate();

		public string Name { get { return this.GetType().Name; } }
		public abstract string ReportName { get; }

		public int ParameterCount { get { return this.parameter.Length; } }
		public int ResultCount { get { return this.result.Length; } }

		public static State FromBool(bool value) {
			return value ? State.On1 : State.On0;
		}

		public static char ToChar(State state) {
			// The chars are hardcoded if needed must be localized.
			switch(state) {
			case State.Off:
				return '-';
			case State.On0:
				return '0';
			case State.On1:
				return '1';
			default:
				throw CircuitFunction.BadState(state);
			}
		}

		#if false
			public static State FromChar(char c) {
				switch(c) {
				case '-':
					return State.Off;
				case '0':
					return State.On0;
				case '1':
					return State.On1;
				default:
					throw new Exception(Properties.Resources.UnknownStateCharacter(c));
				}
			}
		#endif

		protected bool SetResult(int index, State state) {
			if(this.CircuitState[this.result[index]] != state) {
				this.CircuitState[this.result[index]] = state;
				return true;
			}
			return false;
		}
		protected bool SetResult(State state) {
			return this.SetResult(0, state);
		}
		/// <summary>
		/// Sets multi-bit result from the 32 bit parameter
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		protected bool SetResult(int state) {
			bool changed = false;
			for(int i = 0; i < this.result.Length; i++) {
				changed |= this.SetResult(i, CircuitFunction.FromBool((state & (1 << i)) != 0));
			}
			return changed;
		}

		protected static Exception BadState(State state) {
			return new AssertException(Properties.Resources.UnknownState(state));
		}

		protected State TriStateGroup() {
			State state = State.Off;
			foreach(int index in this.parameter) {
				switch(this.CircuitState[index]) {
				case State.On0:
					return State.On0;
				case State.On1:
					state = State.On1;
					break;
				case State.Off:
					break;
				default:
					throw CircuitFunction.BadState(this.CircuitState[index]);
				}
			}
			return state;
		}

		protected State And() {
			for(int i = 0; i < this.parameter.Length; i++) {
				if(this.CircuitState[this.parameter[i]] == State.On0) {
					return State.On0;
				}
			}
			return State.On1;
		}

		protected State Or() {
			for(int i = 0; i < this.parameter.Length; i++) {
				if(this.CircuitState[this.parameter[i]] == State.On1) {
					return State.On1;
				}
			}
			return State.On0;
		}

		protected static State Not(State state) {
			switch(state) {
			case State.Off:
				return State.On0;
			case State.On0:
				return State.On1;
			case State.On1:
				return State.On0;
			}
			throw CircuitFunction.BadState(state);
		}

		protected State Not() {
			return CircuitFunction.Not(this.CircuitState[this.parameter[0]]);
		}

		protected State ControlledState(State enable) {
			return (this.CircuitState[this.parameter[1]] == enable) ? this.CircuitState[this.parameter[0]] : State.Off;
		}

		protected int Count(State state) {
			int count = 0;
			foreach(int index in this.parameter) {
				if(this.CircuitState[index] == state) {
					count++;
				}
			}
			return count;
		}

		#if DEBUG
			public override string ToString() {
				StringBuilder text = new StringBuilder();
				text.Append(this.Name);
				#if false
					//replace this block with some other representation of path in the running tree
					if(!string.IsNullOrEmpty(this.Label)) {
						text.Append('<');
						text.Append(this.Label);
						text.Append('>');
					}
				#endif
				text.Append("(");
				bool comma = false;
				foreach(int p in this.parameter) {
					if(comma) {
						text.Append(", ");
					} else {
						comma = true;
					}
					text.Append(p);
				}
				text.Append(") -> [");
				comma = false;
				foreach(int r in this.result) {
					if(comma) {
						text.Append(", ");
					} else {
						comma = true;
					}
					text.Append(r);
				}
				text.Append("]");
				return text.ToString();
			}
		#endif
	}
}
