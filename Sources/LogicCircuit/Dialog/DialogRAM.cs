using System;

namespace LogicCircuit {
	public class DialogRAM : DialogMemoryEditor {
		public DialogRAM(Memory memory) : base(memory) {
			Tracer.Assert(memory.Writable);
			this.Title = LogicCircuit.Resources.RAMNotation;
		}
	}
}
