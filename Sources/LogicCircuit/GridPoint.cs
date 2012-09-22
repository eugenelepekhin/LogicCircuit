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

		public static bool operator ==(GridPoint point1, GridPoint point2) {
			return point1.X == point2.X && point1.Y == point2.Y;
		}

		public static bool operator !=(GridPoint point1, GridPoint point2) {
			return point1.X != point2.X || point1.Y != point2.Y;
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is GridPoint)) {
				return false;
			}
			GridPoint point = (GridPoint)obj;
			return this == point;
		}

		public override int GetHashCode() {
			return this.X ^ this.Y;
		}

		// This is used to display coordinates to user. Do not make it debug only.
		public override string ToString() {
			return Properties.Resources.GridPoint(this.X, this.Y);
		}

		public GridPoint Offset(int x, int y) {
			return new GridPoint(this.X + x, this.Y + y);
		}
	}
}
