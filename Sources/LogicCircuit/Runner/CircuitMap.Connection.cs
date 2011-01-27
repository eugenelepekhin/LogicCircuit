using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicCircuit {
	partial class CircuitMap {
		private class Connection {
			public Jam InJam { get; private set; }
			public Jam OutJam { get; private set; }

			public Connection(Jam inJam, Jam outJam) {
				Tracer.Assert(inJam != null && inJam.Pin.PinType != PinType.Output);
				Tracer.Assert(outJam != null && outJam.Pin.PinType != PinType.Input);
				this.InJam = inJam;
				this.OutJam = outJam;
			}
		}

		private class ConnectionSet {
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
		}
	}
}
