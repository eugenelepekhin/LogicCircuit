using System;

namespace LogicCircuit {
	public class FunctionRom : FunctionMemory {
		public FunctionRom(CircuitState circuitState, int[] address, int[] result, Memory memory) : base(circuitState, address, null, result, 0, memory) {
		}

		public override bool Evaluate() {
			return this.Read();
		}

		public override string ReportName { get { return Properties.Resources.ReportMemoryName(Properties.Resources.ROMNotation, this.AddressBitWidth, this.DataBitWidth); } }
	}
}
