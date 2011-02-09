using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LogicCircuit {
	public abstract class Jam {
		public BasePin Pin { get; protected set; }
		public CircuitGlyph CircuitSymbol { get; protected set; }
		public int X { get { return this.Pin.GridPoint.X; } }
		public int Y { get { return this.Pin.GridPoint.Y; } }

		public int Z { get { return this.Pin.Inverted ? 2 : 0; } }

		public GridPoint AbsolutePoint {
			get { return this.CircuitSymbol.Point.Offset(this.X, this.Y); }
		}

		public bool IsValid(int bitNumber) {
			if(0 <= bitNumber) {
				if(bitNumber < this.Pin.BitWidth) {
					return true;
				}
				Gate gate = this.CircuitSymbol.Circuit as Gate;
				return gate != null && gate.GateType == GateType.Probe;
			}
			return false;
		}
	}
}
