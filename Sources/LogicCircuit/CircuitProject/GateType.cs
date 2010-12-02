using System;

namespace LogicCircuit {
	public enum GateType {
		// Future extentions should add new types, not insert in the middle, unless old file conversion defined.
		Nop,
		Clock,
		Not,
		Or,
		And,
		Xor,
		Odd,
		Even,
		Led,
		Probe,
		TriState
	}
}
