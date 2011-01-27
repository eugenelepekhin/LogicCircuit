using System;
using System.Collections.Generic;

namespace LogicCircuit {
	internal sealed class JamComparer : IComparer<Jam> {
		public static readonly IComparer<Jam> Comparer = new JamComparer();

		public int Compare(Jam x, Jam y) {
			return PinComparer.Comparer.Compare(x.Pin, y.Pin);
		}
	}
}
