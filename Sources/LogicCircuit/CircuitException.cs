using System;
using System.Collections.Generic;

namespace LogicCircuit {

	public enum Cause {
		Unknown,
		AssertionFailed,
		//Store reasons
		IncompatibleNamespace,
		NullViolation,
		ForeignKeyViolation,
		UniqueViolation,
		//ItemStore reasons
		ManyPrimaryKeys,
		WrongModelType,
		//ModelManager
		UnknownVersion,
		CorruptedFile,
		//User error
		UserError,
		//Script errors
		OperationCanceled,
		XsltError,
		SqlError
	}

	public class CircuitException : Exception {
		private Cause cause;
		public Cause Cause { get { return this.cause; } }

		public CircuitException(Cause cause, Exception innerException, string message) : base(message, innerException) {
			this.cause = cause;
		}
		//public CircuitException(Cause cause, Exception innerException, string message, params object[] args) : this(
		//    cause, innerException, string.Format(CultureInfo.CurrentUICulture, message, args)
		//) {
		//}
		public CircuitException(Cause cause, string message) : this(cause, null, message) {
		}
		//public CircuitException(Cause cause, string message, params object[] args) : this(
		//    cause, null, message, args
		//) {
		//}
		public CircuitException(Cause cause, Exception innerException) : this(cause, innerException, cause.ToString()) {
		}
		public CircuitException(Cause cause) : this(cause, (Exception)null) {}

		public virtual string UserMessage() {
			return this.Message;
		}
	}

	//-------------------------------------------------------------------------

	//[Serializable]
	internal class AssertException : CircuitException {
		public AssertException(string message) : base(Cause.AssertionFailed, message) {}
		public AssertException() : base(Cause.AssertionFailed) {}
		public override string UserMessage() {
			if(this.Message.Length <= 0) {
				return this.GetType().Name;
			}
			return this.Message;
		}
	}
}
