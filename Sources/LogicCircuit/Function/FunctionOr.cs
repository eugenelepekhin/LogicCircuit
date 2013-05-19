using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class FunctionOr : CircuitFunction {
		public static FunctionOr Create(CircuitState circuitState, int[] parameter, int result) {
			switch(parameter.Length) {
			case 1: return new FunctionOr1(circuitState, parameter, result);
			case 2: return new FunctionOr2(circuitState, parameter, result);
			case 3: return new FunctionOr3(circuitState, parameter, result);
			case 4: return new FunctionOr4(circuitState, parameter, result);
			case 5: return new FunctionOr5(circuitState, parameter, result);
			default: return new FunctionOrCommon(circuitState, parameter, result);
			}
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateOrName, this.ParameterCount); } }

		protected FunctionOr(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		private sealed class FunctionOrCommon : FunctionOr {
			public FunctionOrCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(this.Or());
			}
		}

		private sealed class FunctionOr1 : FunctionOr {
			private readonly int param0;

			public FunctionOr1(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 1);
				this.param0 = parameter[0];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionOr2 : FunctionOr {
			private readonly int param0;
			private readonly int param1;

			public FunctionOr2(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 2);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
			}

			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On1) ||
					(this.CircuitState[this.param1] == State.On1)
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionOr3 : FunctionOr {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;

			public FunctionOr3(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionOr4 : FunctionOr {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;

			public FunctionOr4(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On1 : State.On0
				);
			}
		}

		private sealed class FunctionOr5 : FunctionOr {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;
			private readonly int param4;

			public FunctionOr5(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On1 : State.On0
				);
			}
		}
	}
}
