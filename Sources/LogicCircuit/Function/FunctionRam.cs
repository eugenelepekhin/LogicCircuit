using System;

namespace LogicCircuit {
	public class FunctionRam : FunctionMemory {
		public FunctionRam(CircuitState circuitState, int[] address, int[] inputData, int[] outputData, int write, Memory memory) : base(
			circuitState, address, inputData, outputData, write, memory
		) {
		}

		public override bool Evaluate() {
			if(this.IsWriteAllowed) {
				this.Write();
			}
			return this.Read();
		}

		public override string ReportName { get { return Properties.Resources.ReportMemoryName(Properties.Resources.RAMNotation, this.AddressBitWidth, this.DataBitWidth); } }
	}
}
