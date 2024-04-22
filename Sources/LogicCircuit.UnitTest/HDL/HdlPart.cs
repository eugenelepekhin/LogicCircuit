// Ignore Spelling: Hdl

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlPart : HdlItem {
		public class Connection {
			public HdlPart Parent { get; }
			public HdlJam Jam { get; }
			public HdlIOPin JamsPin { get; private set; }
			public int BitWidth { get; private set; }

			public HdlJam Pin { get; }
			public HdlIOPin PinsPin { get; private set; }

			public Connection(HdlPart parent, HdlJam jam, HdlJam pin) {
				this.Parent = parent;
				this.Jam = jam;
				this.Pin = pin;
			}

			public bool LinkJam(HdlChip chip) {
				HdlIOPin pin = chip.Pin(this.Jam.Name);
				if(pin != null) {
					this.JamsPin = pin;
					this.BitWidth = (this.Jam.IsBitRange) ? 1 + this.Jam.Last - this.Jam.First : pin.BitWidth;
					if(pin.BitWidth < this.BitWidth) {
						chip.HdlContext.Error($"Specified range [{this.Jam.First}..{this.Jam.Last}] is bigger than pin's {pin.Name} bit width [{pin.BitWidth}] on part {this.Parent.Name}");
					}
					return true;
				} else {
					chip.HdlContext.Error($"Pin {this.Jam.Name} not found on chip {chip.Name}");
					return false;
				}
			}

			public bool LinkPin(HdlChip chip) {
				HdlIOPin pin = chip.Pin(this.Pin.Name);
				if(pin != null) {
					this.PinsPin = pin;
					return true;
				}
				// Assuming internal pin will be managed in HdlState
				return true;
			}
		}

		public string Name { get; }
		public HdlChip Parent { get; }
		public HdlChip Chip { get; private set; }

		private readonly List<Connection> connections = new List<Connection>();
		public IEnumerable<Connection> Connections => this.connections;

		public int Index { get; }

		public HdlPart(HdlContext hdlContext, HdlChip parent, string name, int index) : base(hdlContext) {
			this.Name = name;
			this.Parent = parent;
			this.Index = index;
		}

		public void AddConnection(HdlJam jam, HdlJam pin) {
			this.connections.Add(new Connection(this, jam, pin));
		}

		public bool Link() {
			HdlChip chip = this.HdlContext.Chip(this.Name);
			if(chip != null) {
				bool success = chip.Link();
				foreach(Connection conn in this.connections) {
					success &= conn.LinkJam(chip);
					success &= conn.LinkPin(this.Parent);
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
