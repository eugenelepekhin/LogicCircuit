using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public struct GridPoint {
		public int X { get; private set; }
		public int Y { get; private set; }

		public GridPoint(int x, int y) : this() {
			this.X = x;
			this.Y = y;
		}

		public static bool operator ==(GridPoint p1, GridPoint p2) {
			return p1.X == p2.X && p1.Y == p2.Y;
		}

		public static bool operator !=(GridPoint p1, GridPoint p2) {
			return p1.X != p2.X || p1.Y != p2.Y;
		}

		public override bool Equals(object o) {
			if((o == null) || !(o is GridPoint)) {
				return false;
			}
			GridPoint point = (GridPoint)o;
			return this == point;
		}

		public override int GetHashCode() {
			return this.X ^ this.Y;
		}

		public override string ToString() {
			return string.Format("({0}, {1})", this.X, this.Y);
		}

		public GridPoint Offset(int x, int y) {
			return new GridPoint(this.X + x, this.Y + y);
		}
	}
}
