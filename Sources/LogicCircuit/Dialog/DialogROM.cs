using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ROM")]
	public class DialogROM : DialogMemoryEditor {
		public DialogROM(Memory memory) : base(memory) {
			Tracer.Assert(!memory.Writable);
			this.Title = LogicCircuit.Resources.ROMNotation;
		}
	}
}
