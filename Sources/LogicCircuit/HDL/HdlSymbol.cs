// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Globalization;

namespace LogicCircuit {
	public class HdlSymbol {
		private class ConnectionComparer : IEqualityComparer<Connection> {
			public bool Equals(Connection? x, Connection? y)  => x!.OutJam == y!.OutJam && x.InJam == y.InJam;
			public int GetHashCode(Connection obj) => obj.OutJam.GetHashCode() ^ obj.InJam.GetHashCode();
		}

		public HdlExport HdlExport { get; }
		public CircuitSymbol CircuitSymbol { get; }
		public IList<Connection> Connections { get; }
		private readonly HashSet<Connection> connectionSet = new HashSet<Connection>(new ConnectionComparer());
		private readonly List<HdlConnection> hdlConnections;
		public IList<HdlConnection> HdlConnections => this.hdlConnections;

		public int Order { get; set; }

		public string Name {
			get {
				Circuit circuit = this.CircuitSymbol.Circuit;
				if(circuit is Splitter splitter) {
					return string.Format(CultureInfo.InvariantCulture, "Splitter{0}x{1}{2}", splitter.BitWidth, splitter.PinCount, splitter.Clockwise);
				}
				return circuit.Name;
			}
		}

		public HdlSymbol(HdlExport export, CircuitSymbol symbol) {
			this.HdlExport = export;
			this.CircuitSymbol = symbol;
			this.Connections = new List<Connection>();
			this.hdlConnections = new List<HdlConnection>();
		}

		public bool Add(Connection connection) {
			if(this.connectionSet.Add(connection)) {
				this.Connections.Add(connection);
				this.HdlConnections.Add(new HdlConnection(this, connection));
				return true;
			}
			return false;
		}

		public void SortConnections() {
			int compare(HdlConnection x, HdlConnection y) {
				if(x.SymbolJam.Pin.PinType == PinType.Input && y.SymbolJam.Pin.PinType != PinType.Input) {
					return -1;
				}
				if(x.SymbolJam.Pin.PinType != PinType.Input && y.SymbolJam.Pin.PinType == PinType.Input) {
					return 1;
				}
				return StringComparer.Ordinal.Compare(x.SymbolJam.Pin.Name, y.SymbolJam.Pin.Name);
			}

			this.hdlConnections.Sort(compare);
		}
	}
}
