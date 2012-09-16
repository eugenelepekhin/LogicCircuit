using System;

namespace LogicCircuit {
	public class DialogRam : DialogMemoryEditor {
		public DialogRam(Memory memory) : base(memory) {
			Tracer.Assert(memory.Writable);
			this.Title = LogicCircuit.Resources.RAMNotation;
		}
	}
}
