using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit {
	public class CircuitSymbolComparer : IComparer<CircuitSymbol> {

		private readonly bool yPrecedence;

		public CircuitSymbolComparer(bool yPrecedence) {
			this.yPrecedence = yPrecedence;
		}

		public int Compare(CircuitSymbol? x, CircuitSymbol? y) {
			Debug.Assert(x != null && y != null);
			if(this.yPrecedence) {
				int d = x.Y - y.Y;
				if(d == 0) {
					d = x.X - y.X;
				}
				return d;
			} else {
				int d = x.X - y.X;
				if(d == 0) {
					d = x.Y - y.Y;
				}
				return d;
			}
		}
	}
}
