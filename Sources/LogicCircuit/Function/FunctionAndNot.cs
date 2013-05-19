using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class FunctionAndNot : CircuitFunction {
		public static FunctionAndNot Create(CircuitState circuitState, int[] parameter, int result) {
			switch(parameter.Length) {
			case 1: return new FunctionAndNot1(circuitState, parameter, result);
			case 2: return new FunctionAndNot2(circuitState, parameter, result);
			case 3: return new FunctionAndNot3(circuitState, parameter, result);
			case 4: return new FunctionAndNot4(circuitState, parameter, result);
			case 5: return new FunctionAndNot5(circuitState, parameter, result);
			default: return new FunctionAndNotCommon(circuitState, parameter, result);
			}
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateAndNotName, this.ParameterCount); } }

		protected FunctionAndNot(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		private sealed class FunctionAndNotCommon : FunctionAndNot {
			public FunctionAndNotCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(CircuitFunction.Not(this.And()));
			}
		}

		private sealed class FunctionAndNot1 : FunctionAndNot {
			private readonly int param0;

			public FunctionAndNot1(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 1);
				this.param0 = parameter[0];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionAndNot2 : FunctionAndNot {
			private readonly int param0;
			private readonly int param1;

			public FunctionAndNot2(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 2);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0) ||
					(this.CircuitState[this.param1] == State.On0)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionAndNot3 : FunctionAndNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;

			public FunctionAndNot3(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 3);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0) ||
					(this.CircuitState[this.param1] == State.On0) ||
					(this.CircuitState[this.param2] == State.On0)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionAndNot4 : FunctionAndNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;

			public FunctionAndNot4(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 4);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
				this.param3 = parameter[3];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0) ||
					(this.CircuitState[this.param1] == State.On0) ||
					(this.CircuitState[this.param2] == State.On0) ||
					(this.CircuitState[this.param3] == State.On0)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionAndNot5 : FunctionAndNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;
			private readonly int param4;

			public FunctionAndNot5(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 5);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
				this.param3 = parameter[3];
				this.param4 = parameter[4];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0) ||
					(this.CircuitState[this.param1] == State.On0) ||
					(this.CircuitState[this.param2] == State.On0) ||
					(this.CircuitState[this.param3] == State.On0) ||
					(this.CircuitState[this.param4] == State.On0)
					? State.On1 : State.On0
				);
			}
		}
	}
}
