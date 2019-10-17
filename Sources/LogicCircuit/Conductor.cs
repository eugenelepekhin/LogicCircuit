using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicCircuit {
	public class Conductor {

		private readonly HashSet<Wire> wire = new HashSet<Wire>();
		private readonly Dictionary<GridPoint, int> pointMap = new Dictionary<GridPoint, int>();

		public void Add(Wire item) {
			if(this.wire.Add(item)) {
				void add(GridPoint point) {
					if(this.pointMap.TryGetValue(point, out int count)) {
						this.pointMap[point] = count + 1;
					} else {
						this.pointMap.Add(point, 1);
					}
				}

				add(item.Point1);
				add(item.Point2);
			}
		}

		public IEnumerable<Wire> Wires { get { return this.wire; } }
		public IEnumerable<GridPoint> Points { get { return this.pointMap.Keys; } }

		public bool Contains(GridPoint gridPoint) {
			return this.pointMap.ContainsKey(gridPoint);
		}

		public bool Contains(Wire symbol) {
			return this.wire.Contains(symbol);
		}

		public int JunctionCount(GridPoint point) {
			if(this.pointMap.TryGetValue(point, out int count)) {
				return count;
			}
			return 0;
		}

		public IEnumerable<GridPoint> JunctionPoints(int minCount) => this.pointMap.Where(pair => minCount <= pair.Value).Select(pair => pair.Key);
	}
}
