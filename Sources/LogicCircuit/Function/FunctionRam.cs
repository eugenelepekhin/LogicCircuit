using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class FunctionRam : FunctionMemory {
		public FunctionRam(CircuitState circuitState, int[] address, int[] inputData, int[] outputData, int write, bool writeOn1) : base(
			circuitState, address, inputData, outputData, write, writeOn1
		) {
		}

		public override bool Evaluate() {
			if(this.IsWriteAllowed) {
				this.Write();
			}
			return this.Read();
		}
	}
}
