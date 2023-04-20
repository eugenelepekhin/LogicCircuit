using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ItemWrapper.Generator {
	[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
	public class GeneratorException : Exception {
		public GeneratorException(string message) : base(message) { }
		public GeneratorException(string format, params object[] args) : this(string.Format(CultureInfo.InvariantCulture, format, args)) { }
	}

	[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
	public class UsageException : GeneratorException {
		public UsageException(string format, params object[] args) : base(format, args) { }
	}
}
