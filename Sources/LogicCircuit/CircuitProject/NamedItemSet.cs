using System;

namespace LogicCircuit {
	public abstract class NamedItemSet {
		protected abstract bool Exists(string name);
		protected abstract bool Exists(string name, Circuit group);

		public string UniqueName(string prefix) {
			string uniqueName = prefix;
			int order = 1;
			while(this.Exists(uniqueName)) {
				uniqueName = prefix + order++;
			}
			return uniqueName;
		}

		public string UniqueName(string prefix, Circuit group) {
			string uniqueName = prefix;
			int order = 1;
			while(this.Exists(uniqueName, group)) {
				uniqueName = prefix + order++;
			}
			return uniqueName;
		}
	}
}
