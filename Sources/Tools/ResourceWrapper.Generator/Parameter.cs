using System.Diagnostics.CodeAnalysis;

namespace ResourceWrapper.Generator {
	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
	public struct Parameter {
		public string Type { get; }
		public string Name { get; }
		public Parameter(string type, string name) {
			this.Type = type;
			this.Name = name;
		}
	}
}
