using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlPart : HdlItem {
		public class Connection {
			public HdlJam Jam { get; }
			public HdlIOPin JamsPin { get; private set; }
			public bool JamIsOutput { get; private set; }
			public int BitWidth { get; private set; }

			public HdlJam Pin { get; }
			public HdlIOPin PinsPin { get; private set; }
			public bool PinIsOutput { get; private set; }
			public bool PinIsInternal => this.PinsPin == null;

			public Connection(HdlJam jam, HdlJam pin) {
				this.Jam = jam;
				this.Pin = pin;
			}

			public bool LinkJam(HdlChip chip) {
				bool byName(HdlIOPin pin) => pin.Name == this.Jam.Name;

				HdlIOPin pin = chip.Inputs.FirstOrDefault(byName);
				if(pin != null) {
					this.JamsPin = pin;
					this.BitWidth = pin.BitWidth;
					return true;
				}
				pin = chip.Outputs.FirstOrDefault(byName);
				if(pin != null) {
					this.JamsPin = pin;
					this.BitWidth = pin.BitWidth;
					this.JamIsOutput = true;
					return true;
				}
				chip.HdlContext.Error($"Input/Output pin {this.Jam.Name} not found on chip {chip.Name}");
				return false;
			}

			public bool LinkPin(HdlChip chip) {
				bool byName(HdlIOPin pin) => pin.Name == this.Pin.Name;

				HdlIOPin pin = chip.Inputs.FirstOrDefault(byName);
				if(pin != null) {
					this.PinsPin = pin;
					return true;
				}

				pin = chip.Outputs.FirstOrDefault(byName);
				if(pin != null) {
					this.PinsPin = pin;
					this.PinIsOutput = true;
					return true;
				}
				return true;
			}
		}

		public string Name { get; }
		public HdlChip Parent { get; }
		public HdlChip Chip { get; private set; }

		private readonly List<Connection> connections = new List<Connection>();
		public IEnumerable<Connection> Connections => this.connections;

		public HdlPart(HdlContext hdlContext, HdlChip parent, string name) : base(hdlContext) {
			this.Name = name;
			this.Parent = parent;
		}

		public void AddConnection(HdlJam jam, HdlJam pin) {
			this.connections.Add(new Connection(jam, pin));
		}

		public bool Link(HdlChip parent) {
			HdlChip chip = this.HdlContext.Chip(this.Name);
			if(chip != null) {
				bool success = chip.Link();
				foreach(Connection conn in this.connections) {
					success &= conn.LinkJam(chip);
					success &= conn.LinkPin(parent);
				}
				this.Chip = chip;
				return success;
			}
			return false;
		}

		public override string ToString() {
			return $"{this.Name}({string.Join(", ", this.Connections.Select(c => $"{c.Jam}={c.Pin}"))});";
		}
	}
}
