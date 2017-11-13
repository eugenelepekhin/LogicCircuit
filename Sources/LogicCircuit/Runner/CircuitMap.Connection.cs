using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	partial class CircuitMap {
		private class Connection {
			public Jam InJam { get; private set; }
			public Jam OutJam { get; private set; }

			public Connection(Jam inJam, Jam outJam) {
				Tracer.Assert(inJam != null && inJam.EffectivePinType != PinType.Output);
				Tracer.Assert(outJam != null && outJam.EffectivePinType != PinType.Input);
				this.InJam = inJam;
				this.OutJam = outJam;
			}
		}

		private class ConnectionSet {
			private class JamBit {
				private readonly CircuitMap map;
				private readonly Jam jam;
				private readonly int bit;

				public JamBit(CircuitMap map, Jam jam, int bit) {
					Tracer.Assert(map.Circuit == jam.CircuitSymbol.LogicalCircuit);
					Tracer.Assert(0 <= bit && bit < jam.Pin.BitWidth);
					this.map = map;
					this.jam = jam;
					this.bit = bit;
				}

				public override int GetHashCode() {
					return this.map.GetHashCode() ^ this.jam.GetHashCode() ^ this.bit;
				}

				public override bool Equals(object obj) {
					JamBit other = (JamBit)obj;
					return this.map == other.map && this.jam == other.jam && this.bit == other.bit;
				}
			}

			private HashSet<LogicalCircuit> connected = new HashSet<LogicalCircuit>();
			private Dictionary<Jam, Dictionary<Jam, Connection>> outputs = new Dictionary<Jam, Dictionary<Jam, Connection>>();
			private HashSet<JamBit> jamConnected = new HashSet<JamBit>();

			public bool IsConnected(LogicalCircuit logicalCircuit) {
				return this.connected.Contains(logicalCircuit);
			}

			public void MarkConnected(LogicalCircuit logicalCircuit) {
				Tracer.Assert(!this.IsConnected(logicalCircuit));
				this.connected.Add(logicalCircuit);
			}

			public Connection Connect(Jam inputJam, Jam outputJam) {
				Connection connection;
				Dictionary<Jam, Connection> inputs;
				if(this.outputs.TryGetValue(outputJam, out inputs)) {
					if(inputs.TryGetValue(inputJam, out connection)) {
						return connection;
					}
				} else {
					inputs = new Dictionary<Jam, Connection>();
					this.outputs.Add(outputJam, inputs);
				}
				connection = new Connection(inputJam, outputJam);
				inputs.Add(inputJam, connection);
				return connection;
			}

			public IEnumerable<Connection> SelectByOutput(Jam outputJam) {
				Dictionary<Jam, Connection> inputs;
				if(this.outputs.TryGetValue(outputJam, out inputs)) {
					return inputs.Values;
				}
				return Enumerable.Empty<Connection>();
			}

			public bool IsConnected(CircuitMap map, Jam jam, int bit) {
				return !this.jamConnected.Add(new JamBit(map, jam, bit));
			}

			#if DEBUG
				[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
				public string Dump {
					get {
						System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
						Func<Jam, string> trim = jam => regex.Replace(jam.ToString(), " ").Trim();
						System.Text.StringBuilder text = new System.Text.StringBuilder();
						System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
						foreach(Dictionary<Jam, Connection> dic in this.outputs.Values) {
							foreach(Connection con in dic.Values) {
								text.AppendFormat(culture, "{0} -> {1}", trim(con.InJam), trim(con.OutJam));
								text.AppendLine();
							}
						}
						return text.ToString();
					}
				}
			#endif
		}
	}
}
