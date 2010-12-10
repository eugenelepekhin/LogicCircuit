using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public class ConductorMap {

		private Dictionary<GridPoint, Conductor> map = new Dictionary<GridPoint, Conductor>();
		private HashSet<Conductor> list = new HashSet<Conductor>();

		public ConductorMap(LogicalCircuit logicalCircuit) {
			foreach(Wire wire in logicalCircuit.Wires()) {
				Tracer.Assert(wire.Point1 != wire.Point2);
				Conductor conductor;
				if(this.TryGetValue(wire.Point1, out conductor)) {
					Conductor other;
					if(!this.TryGetValue(wire.Point2, out other)) {
						this.map.Add(wire.Point2, conductor);
					} else if(conductor != other) {
						conductor = this.Join(conductor, other);
					}
				} else if(this.TryGetValue(wire.Point2, out conductor)) {
					this.map.Add(wire.Point1, conductor);
				} else {
					conductor = new Conductor();
					this.map.Add(wire.Point1, conductor);
					this.map.Add(wire.Point2, conductor);
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
	}
}
