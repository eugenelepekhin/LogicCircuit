﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit {
	internal sealed class JamComparer : IComparer<Jam> {
		public static readonly IComparer<Jam> Comparer = new JamComparer();

		public int Compare(Jam? x, Jam? y) {
			Debug.Assert(x != null && y != null);
			return PinComparer.Comparer.Compare(x.Pin, y.Pin);
		}
	}
}
