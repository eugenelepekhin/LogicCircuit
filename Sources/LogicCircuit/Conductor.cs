using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public class Conductor {

		private HashSet<Wire> wire = new HashSet<Wire>();
		private HashSet<GridPoint> point = new HashSet<GridPoint>();

		public void Add(Wire item) {
			if(this.wire.Add(item)) {
				this.point.Add(item.Point1);
				this.point.Add(item.Point2);
			}
		}

		public IEnumerable<Wire> Wires { get { return this.wire; } }
		public IEnumerable<GridPoint> Points { get { return this.point; } }

		public bool Contains(GridPoint gridPoint) {
			return this.point.Contains(gridPoint);
		}

		public bool Contains(Wire symbol) {
			return this.wire.Contains(symbol);
		}
	}
}
