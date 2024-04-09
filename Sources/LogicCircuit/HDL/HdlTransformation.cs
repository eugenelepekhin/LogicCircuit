// Ignore Spelling: Hdl

using System.Collections.Generic;
using System.Linq;

namespace LogicCircuit {
	public abstract class HdlTransformation : T4Transformation {
		public IEnumerable<HdlSymbol> InputPins { get; }
		public IEnumerable<HdlSymbol> OutputPins { get; }
		public IEnumerable<HdlSymbol> Parts { get; }
		public bool HasInputPins => this.InputPins.Any();
		public bool HasOutputPins => this.OutputPins.Any();

		protected HdlTransformation(IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) {
			this.InputPins = inputPins;
			this.OutputPins = outputPins;
			this.Parts = parts;
		}
	}
}
