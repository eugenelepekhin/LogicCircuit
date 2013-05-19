using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class FunctionAnd : CircuitFunction {
		public static FunctionAnd Create(CircuitState circuitState, int[] parameter, int result) {
			switch(parameter.Length) {
			case 1: return new FunctionAnd1(circuitState, parameter, result);
			case 2: return new FunctionAnd2(circuitState, parameter, result);
			case 3: return new FunctionAnd3(circuitState, parameter, result);
			case 4: return new FunctionAnd4(circuitState, parameter, result);
			case 5: return new FunctionAnd5(circuitState, parameter, result);
			default: return new FunctionAndCommon(circuitState, parameter, result);
			}
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateAndName, this.ParameterCount); } }

		protected FunctionAnd(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		private sealed class FunctionAndCommon : FunctionAnd {
			public FunctionAndCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(this.And());
			}
		}

		private sealed class FunctionAnd1 : FunctionAnd {
			private readonly int param0;

			public FunctionAnd1(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 1);
				this.param0 = parameter[0];
			}
			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionAnd2 : FunctionAnd {
			private readonly int param0;
			private readonly int param1;

			public FunctionAnd2(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
				Tracer.Assert(parameter.Length == 2);
				this.param0 = parameter[0];
				this.param1 = parameter[1];
			}
			public override bool Evaluate() {
				return this.SetResult0(
					(this.CircuitState[this.param0] == State.On0) ||
					(this.CircuitState[this.param1] == State.On0)
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionAnd3 : FunctionAnd {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;

			public FunctionAnd3(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionAnd4 : FunctionAnd {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;

			public FunctionAnd4(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On0 : State.On1
				);
			}
		}

		private sealed class FunctionAnd5 : FunctionAnd {
			private readonly int param0;
			private readonly int param1;
			private readonly int param2;
			private readonly int param3;
			private readonly int param4;

			public FunctionAnd5(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
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
					? State.On0 : State.On1
				);
			}
		}
	}
}
