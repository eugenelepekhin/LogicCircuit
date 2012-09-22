using System;

namespace LogicCircuit {
	public class DialogRom : DialogMemoryEditor {
		public DialogRom(Memory memory) : base(memory) {
			Tracer.Assert(!memory.Writable);
			this.Title = Properties.Resources.ROMNotation;
		}
	}
}
