using System;

namespace ItemWrapper.Generator {
	public class Error : Exception {
		public Error(string message) : base(message) { }
		public Error(string format, params object[] args) : this(string.Format(format, args)) { }
	}

	public class Usage : Error {
		public Usage(string format, params object[] args) : base(format, args) { }
	}
}
