namespace LogicCircuit.UnitTest.HDL {
	internal class HdlPart : HdlItem {
		public readonly struct Connection {
			public HdlJam Jam { get; }
			public HdlJam Pin { get; }
			public Connection(HdlJam jam, HdlJam pin) {
				this.Jam = jam;
				this.Pin = pin;
			}
		}

		public string Name { get; }
		public HdlChip Chip { get; set; }

		private readonly List<Connection> connections = new List<Connection>();
		public IEnumerable<Connection> Connections => this.connections;

		public HdlPart(HdlContext hdlContext, string name) : base(hdlContext) {
			this.Name = name;
		}

		public void AddConnection(HdlJam jam, HdlJam pin) {
			jam.Jam = jam;
			jam.Pin = pin;
			pin.Jam = jam;
			pin.Pin = pin;
			this.connections.Add(new Connection(jam, pin));
		}

		public override string ToString() {
			return $"{this.Name}({string.Join(", ", this.Connections.Select(c => $"{c.Jam}={c.Pin}"))});";
		}
	}
}
