using System;

namespace ResourceWrapper.Generator {
	public struct Parameter {
		public string Type;
		public string Name;
		public Parameter(string type, string name) {
			this.Type = type;
			this.Name = name;
		}
	}
}
