// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LogicCircuit {
	public class HdlSymbol {
		private readonly struct JamKey : IEquatable<JamKey> {
			public readonly Jam OutJam;
			public readonly Jam InJam;

			public JamKey(Jam outJam, Jam inJam) {
				this.OutJam = outJam;
				this.InJam = inJam;
			}

			public bool Equals(JamKey other) => this.OutJam == other.OutJam && this.InJam == other.InJam;
			public override bool Equals(object? obj) => obj is JamKey && this.Equals((JamKey)obj);
			public override int GetHashCode() => HashCode.Combine(this.OutJam, this.InJam);

			#if DEBUG
				public override string ToString() => $"{this.OutJam}->{this.InJam}";
			#endif
		}

		private readonly struct JamRange : IEquatable<JamRange> {
			public readonly Jam Jam;
			public readonly HdlConnection.BitRange Range;

			public JamRange(Jam jam, HdlConnection.BitRange range) {
				this.Jam = jam;
				this.Range = range;
			}

			public bool Equals(JamRange other) => this.Jam == other.Jam && this.Range == other.Range;
			public override bool Equals(object? obj) => obj is JamRange other && this.Equals(other);
			public override int GetHashCode() => HashCode.Combine(this.Jam, this.Range);

			#if DEBUG
				public override string ToString() => $"{this.Jam}{this.Range}";
			#endif
		}

		public HdlExport HdlExport { get; }
		public CircuitSymbol CircuitSymbol { get; }
		
		private readonly Dictionary<JamKey, List<HdlConnection>> connections = new Dictionary<JamKey, List<HdlConnection>>();
		private List<HdlConnection>? connectionList;

		public int Order { get; set; }

		public string Name => this.HdlExport.Name(this);

		public HdlSymbol(HdlExport export, CircuitSymbol symbol) {
			this.HdlExport = export;
			this.CircuitSymbol = symbol;
		}

		public void Add(HdlConnection connection) {
			Debug.Assert(this.connectionList == null);
			List<HdlConnection>? list;
			JamKey jamKey = new JamKey(connection.OutJam, connection.InJam);
			if(!this.connections.TryGetValue(jamKey, out list)) {
				list = new List<HdlConnection>();
				this.connections.Add(jamKey, list);
			}
			list.Add(connection);
		}

		public IEnumerable<HdlConnection> Find(Jam outJam, Jam inJam) {
			if(this.connections.TryGetValue(new JamKey(outJam, inJam), out List<HdlConnection>? list)) {
				return list;
			}
			return Enumerable.Empty<HdlConnection>();
		}

		public IEnumerable<HdlConnection> HdlConnections() {
			if(this.connectionList == null) {
				return this.connections.Values.SelectMany(connections => connections);
			}
			return this.connectionList;
		}

		public void SortConnections() {
			int compare(HdlConnection x, HdlConnection y) {
				
				if(x.SymbolJam(this).Pin.PinType == PinType.Input && y.SymbolJam(this).Pin.PinType != PinType.Input) {
					return -1;
				}
				if(x.SymbolJam(this).Pin.PinType != PinType.Input && y.SymbolJam(this).Pin.PinType == PinType.Input) {
					return 1;
				}
				return StringComparer.Ordinal.Compare(x.SymbolJam(this).Pin.Name, y.SymbolJam(this).Pin.Name);
			}

			Debug.Assert(this.connectionList == null);
			this.connectionList = this.HdlConnections().ToList();
			this.connectionList.Sort(compare);

			Dictionary<JamRange, List<HdlConnection>> byRange = new Dictionary<JamRange, List<HdlConnection>>();
			foreach(HdlConnection connection in this.connectionList.Where(c => this == c.OutHdlSymbol)) {
				JamRange range = new JamRange(connection.OutJam, connection.OutBits);
				List<HdlConnection>? list;
				if(!byRange.TryGetValue(range, out list)) {
					list = new List<HdlConnection>();
					byRange.Add(range, list);
				}
				list.Add(connection);
			}
			foreach(List<HdlConnection> list in byRange.Values) {
				if(1 < list.Count) {
					for(int i = 1; i < list.Count; i++) {
						list[i].SkipOutput = true;
					}
				}
			}
		}

		#if DEBUG
			public override string ToString() => $"HdlSymbol of {this.CircuitSymbol.ToString()}";
		#endif
	}
}
