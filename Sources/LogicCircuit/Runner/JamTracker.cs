using System;
using System.Collections.Generic;

namespace LogicCircuit {
	/// <summary>
	/// Allow to keep list of observed jams during walking through conductors of the circuits.
	/// </summary>
	internal class JamTracker {
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

		private HashSet<JamBit> jamConnected = new HashSet<JamBit>();

		public bool WasTracked(CircuitMap map, Jam jam, int bit) {
			return !this.jamConnected.Add(new JamBit(map, jam, bit));
		}
	}
}
