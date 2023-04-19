using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LogicCircuit {
	internal sealed class PinComparer : IComparer<BasePin> {
		public static readonly PinComparer Comparer = new PinComparer();

		public int Compare(BasePin? x, BasePin? y) {
			Debug.Assert(x != null && y != null);
			Tracer.Assert(x.GetType() == y.GetType());
			if(x is DevicePin dp1) {
				DevicePin dp2 = (DevicePin)y;
				return dp1.Order - dp2.Order;
			} else {
				Pin xp = (Pin)x;
				Pin yp = (Pin)y;
				int index = xp.Index - yp.Index;
				if(0 == index) {
					CircuitSymbolSet symbolSet = x.CircuitProject.CircuitSymbolSet;
					Tracer.Assert(symbolSet == y.CircuitProject.CircuitSymbolSet);
					CircuitSymbol? s1 = symbolSet.SelectByCircuit(x).FirstOrDefault();
					CircuitSymbol? s2 = symbolSet.SelectByCircuit(y).FirstOrDefault();
					if(s1 != null && s2 != null) {
						int d = s1.Y - s2.Y;
						if(d == 0) {
							d = s1.X - s2.X;
							if(d == 0) {
								return StringComparer.Ordinal.Compare(x.Name, y.Name);
							}
						}
						return d;
					}
					return StringComparer.Ordinal.Compare(x.Name, y.Name);
				}
				return index;
			}
		}
	}
}
