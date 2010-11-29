using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LogicCircuit {
	public abstract class Jam {
		public BasePin Pin { get; protected set; }
		public CircuitGlyph CircuitGlyph { get; protected set; }
		public int X { get { return this.Pin.GridPoint.X; } }
		public int Y { get { return this.Pin.GridPoint.Y; } }

		public int Z { get { return this.Pin.Inverted ? 2 : 0; } }

		public GridPoint AbsolutePoint {
			get { return this.CircuitGlyph.Point.Offset(this.X, this.Y); }
		}
	}
}
