namespace LogicCircuit.UnitTest.HDL {
	internal class HdlChip : HdlItem {
		public string Name { get; }
		
		private List<HdlIOPin> inputs = new List<HdlIOPin>();
		public IEnumerable<HdlIOPin> Inputs => this.inputs;

		private List<HdlIOPin> outputs = new List<HdlIOPin>();
		public IEnumerable<HdlIOPin> Outputs => this.outputs;

		private List<HdlPart> parts = new List<HdlPart>();
		public IEnumerable<HdlPart > Parts => this.parts;

		public HdlChip(HdlContext context, string name) : base(context) {
			this.Name = name;
		}

		public void AddInput(HdlIOPin pin) {
			this.inputs.Add(pin);
			pin.Chip = this;
		}

		public void AddOutput(HdlIOPin pin) {
			this.outputs.Add(pin);
			pin.Chip = this;
		}

		public void AddPart(HdlPart part) {
			this.parts.Add(part);
			part.Chip = this;
		}

		public override string ToString() {
			string inputPins = (0 < this.inputs.Count) ? $"\tIN {string.Join(", ", this.Inputs)};\n" : string.Empty;
			string outputPins = (0 < this.outputs.Count) ? $"\tOUT {string.Join(", ", this.Outputs)};\n" : string.Empty;
			return (
				$"CHIP {this.Name} {{\n" +
				inputPins +
				outputPins +
				$"PARTS:\n\t" +
				string.Join("\n\t", this.Parts) +
				"\n}"
			);
		}
	}
}
