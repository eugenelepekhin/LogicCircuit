using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicCircuit {
	public class ConductorMap {

		private readonly Dictionary<GridPoint, Conductor> map = new Dictionary<GridPoint, Conductor>();
		private readonly HashSet<Conductor> list = new HashSet<Conductor>();

		public ConductorMap(LogicalCircuit logicalCircuit) {
			foreach(Wire wire in logicalCircuit.Wires()) {
				GridPoint p1 = wire.Point1;
				GridPoint p2 = wire.Point2;
				Tracer.Assert(p1 != p2);
				Conductor conductor;
				if(this.TryGetValue(p1, out conductor)) {
					if(!this.TryGetValue(p2, out Conductor other)) {
						this.map.Add(p2, conductor);
					} else if(conductor != other) {
						conductor = this.Join(conductor, other);
					}
				} else if(this.TryGetValue(p2, out conductor)) {
					this.map.Add(p1, conductor);
				} else {
					conductor = new Conductor();
					this.map.Add(p1, conductor);
					this.map.Add(p2, conductor);
					this.list.Add(conductor);
				}
				conductor.Add(wire);
			}
		}

		private Conductor Join(Conductor conductor, Conductor other) {
			Tracer.Assert(conductor != other);
			foreach(Wire wire in other.Wires) {
				conductor.Add(wire);
				this.map[wire.Point1] = conductor;
				this.map[wire.Point2] = conductor;
			}
			this.list.Remove(other);
			return conductor;
		}

		public bool TryGetValue(GridPoint point, out Conductor conductor) {
			return this.map.TryGetValue(point, out conductor);
		}

		public IEnumerable<Conductor> Conductors { get { return this.list; } }

		public int JunctionCount(GridPoint point) {
			if(this.TryGetValue(point, out Conductor conductor)) {
				return conductor.JunctionCount(point);
			}
			return 0;
		}

		public IEnumerable<GridPoint> JunctionPoints(int minCount) => this.Conductors.SelectMany(conductor => conductor.JunctionPoints(minCount));
	}
}
