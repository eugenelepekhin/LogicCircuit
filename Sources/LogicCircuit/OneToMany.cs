using System.Collections.Generic;

namespace LogicCircuit {
	public class OneToMany<TOne, TMany> : Dictionary<TOne, IList<TMany>> where TOne : notnull {
		public void Add(TOne key, TMany value) {
			IList<TMany>? list;
			if(!this.TryGetValue(key, out list)) {
				list = new List<TMany>();
				this.Add(key, list);
			}
			list.Add(value);
		}

		public bool Remove(TOne key, TMany value) {
			IList<TMany>? list;
			if(this.TryGetValue(key, out list)) {
				return list.Remove(value);
			}
			return false;
		}
	}
}
