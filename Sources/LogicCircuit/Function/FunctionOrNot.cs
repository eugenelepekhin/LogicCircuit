using System;

namespace LogicCircuit {
	public abstract class FunctionOrNot : CircuitFunction {
		public static FunctionOrNot Create(CircuitState circuitState, int[] parameter, int result) {
			switch(parameter.Length) {
			case 1: return new FunctionOrNot1(circuitState, parameter, result);
			case 2: return new FunctionOrNot2(circuitState, parameter, result);
			case 3: return new FunctionOrNot3(circuitState, parameter, result);
			case 4: return new FunctionOrNot4(circuitState, parameter, result);
			case 5: return new FunctionOrNot5(circuitState, parameter, result);
			default: return new FunctionOrNotCommon(circuitState, parameter, result);
			}
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateOrNotName, this.ParameterCount); } }

		protected FunctionOrNot(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		private sealed class FunctionOrNotCommon : FunctionOrNot {
			public FunctionOrNotCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(CircuitFunction.Not(this.Or()));
			}
		}

		private sealed class FunctionOrNot1 : FunctionOrNot {
			private readonly int param0;

			public FunctionOrNot1(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 1);
				this.param0 = parameter[0];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionOrNot2 : FunctionOrNot {
			private readonly int param0;
			private readonly int param1;

			public FunctionOrNot2(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 2);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1) ||
					(this.CircuitState[this.param1] == State.On1)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionOrNot3 : FunctionOrNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;

			public FunctionOrNot3(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 3);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1) ||
					(this.CircuitState[this.param1] == State.On1) ||
					(this.CircuitState[this.param2] == State.On1)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionOrNot4 : FunctionOrNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;

			public FunctionOrNot4(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 4);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
				this.param3 = parameter[3];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1) ||
					(this.CircuitState[this.param1] == State.On1) ||
					(this.CircuitState[this.param2] == State.On1) ||
					(this.CircuitState[this.param3] == State.On1)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionOrNot5 : FunctionOrNot {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;
			private readonly int param4;

			public FunctionOrNot5(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 5);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
				this.param2 = parameter[2];
				this.param3 = parameter[3];
				this.param4 = parameter[4];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1) ||
					(this.CircuitState[this.param1] == State.On1) ||
					(this.CircuitState[this.param2] == State.On1) ||
					(this.CircuitState[this.param3] == State.On1) ||
					(this.CircuitState[this.param4] == State.On1)
					? State.On0 : State.On1
				);
			}
		}
	}
}
