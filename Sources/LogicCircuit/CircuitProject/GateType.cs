using System;

namespace LogicCircuit {
	public enum GateType {
		// Future extensions should add new types, not insert in the middle, unless old file conversion defined.
		Nop,
		Clock,
		Not,
		Or,
		And,
		Xor,
		Odd,	// This one should not be used anymore
		Even,	// This one should not be used anymore
		Led,
		Probe,	// This one should not be used anymore
		TriState
	}
}
