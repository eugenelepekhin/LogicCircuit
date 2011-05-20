﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionTriStateGroup : CircuitFunction {
		public FunctionTriStateGroup(CircuitState circuitState, int[] parameter, int result) : base(circuitState, parameter, result) {
		}
		public override bool Evaluate() {
			return this.SetResult(this.And(State.Off));
		}
	}
}