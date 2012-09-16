using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "RAM")]
	public class DialogRAM : DialogMemoryEditor {
		public DialogRAM(Memory memory) : base(memory) {
			Tracer.Assert(memory.Writable);
			this.Title = LogicCircuit.Resources.RAMNotation;
		}
	}
}
