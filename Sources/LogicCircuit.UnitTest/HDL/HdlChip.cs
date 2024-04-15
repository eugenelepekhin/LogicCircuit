using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlChip : HdlItem {
		public string Name { get; }
		
		private List<HdlIOPin> pins = new List<HdlIOPin>();
		public IEnumerable<HdlIOPin> Pins => this.pins;
		public int PinsCount => this.pins.Count;
		public IEnumerable<HdlIOPin> Inputs => this.pins.Where(p => p.Type == HdlIOPin.PinType.Input);
		public IEnumerable<HdlIOPin> Outputs => this.pins.Where(p => p.Type == HdlIOPin.PinType.Output);
		public IEnumerable<HdlIOPin> Internals => this.pins.Where(p => p.Type == HdlIOPin.PinType.Internal);
		public int InputsCount => this.Inputs.Count();
		public int OutputsCount => this.Outputs.Count();
		public int InternalsCount => this.Internals.Count();


		private List<HdlPart> parts = new List<HdlPart>();
		public IEnumerable<HdlPart > Parts => this.parts;

		private bool isLinked;

		public HdlChip(HdlContext context, string name) : base(context) {
			this.Name = name;
		}

		public HdlIOPin AddPin(string name, int bitWidth, HdlIOPin.PinType pinType) {
			if(this.pins.Any(p => p.Name == name)) {
				this.HdlContext.Error($"Pin {name} redefined on chip {this.Name}");
				return null;
			} else {
				HdlIOPin pin = new HdlIOPin(this.HdlContext, this, name, bitWidth, pinType);
				this.pins.Add(pin);
				return pin;
			}
		}

		public HdlIOPin Pin(string name) => this.pins.FirstOrDefault(p => p.Name == name);

		public void AddPart(HdlPart part) {
			this.parts.Add(part);
		}

		public virtual bool Link() {
			bool success = true;
			if(!this.isLinked) {
				this.isLinked = true;
				foreach(HdlPart part in this.parts) {
					success &= part.Link();
				}
			}
			return success;
		}

		public virtual bool Evaluate(HdlState state) {
			bool changed;
			do {
				changed = false;
				foreach(HdlPart part in this.parts) {
					HdlState partState = state.PartState(part);
					foreach(HdlPart.Connection connection in part.Connections.Where(c => c.JamsPin.Type == HdlIOPin.PinType.Input)) {
						changed |= state.Assign(connection);
					}
					changed |= part.Chip.Evaluate(partState);
					foreach(HdlPart.Connection connection in part.Connections.Where(c => c.JamsPin.Type == HdlIOPin.PinType.Output)) {
						changed |= state.Assign(connection);
					}
				}
			} while(changed);
			return changed;
		}

		public override string ToString() {
			string inputPins = (0 < this.InputsCount) ? $"\tIN {string.Join(", ", this.Inputs)};\n" : string.Empty;
			string outputPins = (0 < this.OutputsCount) ? $"\tOUT {string.Join(", ", this.Outputs)};\n" : string.Empty;
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
