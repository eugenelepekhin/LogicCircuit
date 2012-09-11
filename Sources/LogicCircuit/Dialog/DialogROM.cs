using System;

namespace LogicCircuit {
	public class DialogROM : DialogMemoryEditor {
		public DialogROM(Memory memory) : base(memory) {
			Tracer.Assert(!memory.Writable);
			this.Title = LogicCircuit.Resources.ROMNotation;
		}
	}
}
