namespace LogicCircuit.UnitTest.HDL {
	internal class HdlChip : HdlItem {
		public string Name { get; }
		
		private List<HdlIOPin> inputs = new List<HdlIOPin>();
		public IEnumerable<HdlIOPin> Inputs => this.inputs;

		private List<HdlIOPin> outputs = new List<HdlIOPin>();
		public IEnumerable<HdlIOPin> Outputs => this.outputs;

		private List<HdlPart> parts = new List<HdlPart>();
		public IEnumerable<HdlPart > Parts => this.parts;

		private bool isLinked;

		public HdlChip(HdlContext context, string name) : base(context) {
			this.Name = name;
		}

		public bool AddInput(HdlIOPin pin) {
			if(this.inputs.Concat(this.outputs).Any(p => p.Name == pin.Name)) {
				this.HdlContext.Error($"Input/Output pin {pin.Name} redefined on chip {this.Name}");
				return false;
			} else {
				this.inputs.Add(pin);
				return true;
			}
		}

		public bool AddOutput(HdlIOPin pin) {
			if(this.inputs.Concat(this.outputs).Any(p => p.Name == pin.Name)) {
				this.HdlContext.Error($"Input/Output pin {pin.Name} redefined on chip {this.Name}");
				return false;
			} else {
				this.outputs.Add(pin);
				return true;
			}
		}

		public void AddPart(HdlPart part) {
			this.parts.Add(part);
		}

		public virtual bool Link() {
			bool success = true;
			if(!this.isLinked) {
				this.isLinked = true;
				foreach(HdlPart part in this.parts) {
					success &= part.Link(this);
				}
			}
			return success;
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
