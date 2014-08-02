using System;

namespace LogicCircuit {
	public abstract class FunctionXorNot : CircuitFunction {
		public static FunctionXorNot Create(CircuitState circuitState, int[] parameter, int result) {
			return new FunctionXorNotCommon(circuitState, parameter, result);
		}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateXorNotName, this.ParameterCount); } }

		protected FunctionXorNot(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		private sealed class FunctionXorNotCommon : FunctionXorNot {
			public FunctionXorNotCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(CircuitFunction.FromBool((this.Count(State.On1) & 1) != 1));
			}
		}
	}
}
