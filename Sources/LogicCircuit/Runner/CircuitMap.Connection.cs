﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LogicCircuit {
	public class Connection {
		public Jam InJam { get; private set; }
		public Jam OutJam { get; private set; }

		public Connection(Jam inJam, Jam outJam) {
			Tracer.Assert(inJam != null && inJam.EffectivePinType != PinType.Output);
			Tracer.Assert(outJam != null && outJam.EffectivePinType != PinType.Input);
			Debug.Assert(inJam != null && outJam != null);
			this.InJam = inJam;
			this.OutJam = outJam;
		}

		#if DEBUG
			public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "Connect {0} -> {1}", this.OutJam.ToString(), this.InJam.ToString());
		#endif
	}

	public class ConnectionSet {
		private HashSet<LogicalCircuit> connected = new HashSet<LogicalCircuit>();
		private Dictionary<Jam, Dictionary<Jam, Connection>> outputs = new Dictionary<Jam, Dictionary<Jam, Connection>>();

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
			if(this.outputs.TryGetValue(outputJam, out inputs!)) {
				if(inputs.TryGetValue(inputJam, out connection!)) {
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
			if(this.outputs.TryGetValue(outputJam, out Dictionary<Jam, Connection>? inputs)) {
				Debug.Assert(inputs != null);
				return inputs.Values;
			}
			return Enumerable.Empty<Connection>();
		}

		#if DEBUG
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public string Dump {
				get {
					System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
					string trim(Jam jam) => regex.Replace(jam.ToString(), " ").Trim();
					System.Text.StringBuilder text = new System.Text.StringBuilder();
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
					foreach(Dictionary<Jam, Connection> dictionary in this.outputs.Values) {
						foreach(Connection con in dictionary.Values) {
							text.AppendFormat(culture, "{0} -> {1}", trim(con.OutJam), trim(con.InJam));
							text.AppendLine();
						}
					}
					return text.ToString();
				}
			}
		#endif
	}
}
