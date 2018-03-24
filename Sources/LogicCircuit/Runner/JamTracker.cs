using System;
using System.Collections.Generic;

namespace LogicCircuit {
	/// <summary>
	/// Allow to keep list of observed jams during walking through conductors of the circuits.
	/// </summary>
	internal class JamTracker {
		private class JamBit {
			private readonly CircuitMap map;
			private readonly Jam inJam;
			private readonly Jam outJam;
			private readonly int bit;

			public JamBit(CircuitMap map, Jam inJam, Jam outJam, int bit) {
				Tracer.Assert(map.Circuit == inJam.CircuitSymbol.LogicalCircuit);
				Tracer.Assert(outJam == null || map.Circuit == outJam.CircuitSymbol.LogicalCircuit);
				Tracer.Assert(0 <= bit && bit < inJam.Pin.BitWidth);
				this.map = map;
				this.inJam = inJam;
				this.outJam = outJam;
				this.bit = bit;
			}

			public override int GetHashCode() {
				if(this.outJam != null) {
					return this.map.GetHashCode() ^ this.inJam.GetHashCode() ^ this.outJam.GetHashCode() ^ this.bit;
				} else {
					return this.map.GetHashCode() ^ this.inJam.GetHashCode() ^ this.bit;
				}
			}

			public override bool Equals(object obj) {
				JamBit other = (JamBit)obj;
				return this.map == other.map && this.inJam == other.inJam && this.outJam == other.outJam && this.bit == other.bit;
			}
		}

		private HashSet<JamBit> jamConnected = new HashSet<JamBit>();

		public bool WasTracked(CircuitMap map, Jam inJam, Jam outJam, int bit) {
			return !this.jamConnected.Add(new JamBit(map, inJam, outJam, bit));
		}
	}
}
