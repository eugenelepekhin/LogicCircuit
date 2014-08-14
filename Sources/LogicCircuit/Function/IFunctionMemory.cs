using System;

namespace LogicCircuit {
	public interface IFunctionMemory {
		int AddressBitWidth { get; }
		int DataBitWidth { get; }
		int this[int index] { get; }

	}
}
