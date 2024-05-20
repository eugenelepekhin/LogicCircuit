using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public class OneToMany<TOne, TMany> : Dictionary<TOne, ICollection<TMany>> where TOne : notnull where TMany : notnull {
		private readonly Func<ICollection<TMany>> createCollection;
		private readonly Func<ICollection<TMany>, TMany, bool> add;

		public OneToMany(bool uniqueCollection) {
			this.createCollection = uniqueCollection ? () => new HashSet<TMany>() : () => new List<TMany>();
			this.add = uniqueCollection ? (collection, item) => ((HashSet<TMany>)collection).Add(item) : (collection, item) => { collection.Add(item); return true; };
		}

		public OneToMany() : this(false) {
		}

		public bool Add(TOne key, TMany value) {
			ICollection<TMany>? list;
			if(!this.TryGetValue(key, out list)) {
				list = this.createCollection();
				this.Add(key, list);
			}
			return this.add(list, value);
		}

		public bool Remove(TOne key, TMany value) {
			ICollection<TMany>? list;
			if(this.TryGetValue(key, out list)) {
				bool result = list.Remove(value);
				if(list.Count == 0) {
					this.Remove(key);
				}
				return result;
			}
			return false;
		}
	}
}
