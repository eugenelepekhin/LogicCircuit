using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class FunctionXor : CircuitFunction {
		public static FunctionXor Create(CircuitState circuitState, int[] parameter, int result) {
			return new FunctionXorCommon(circuitState, parameter, result);
		}

		protected FunctionXor(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

		public override string ReportName { get { return Properties.Resources.ReportGateName(Properties.Resources.GateXorName, this.ParameterCount); } }

		private sealed class FunctionXorCommon : FunctionXor {
			public FunctionXorCommon(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {}

			public override bool Evaluate() {
				return this.SetResult0(CircuitFunction.FromBool((this.Count(State.On1) & 1) == 1));
			}
		}
	}
}
