using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit.DataPersistent {
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class SnapStoreException : Exception {
		public SnapStoreException(string message) : base(message) {
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	public class UniqueViolationException : SnapStoreException {
		public string ConstraintName { get; private set; }
		public UniqueViolationException(string name) : base(Properties.Resources.UniqueConstraintViolation(name)) {
			this.ConstraintName = name;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	public class ForeignKeyViolationException : SnapStoreException {
		public string ConstraintName { get; private set; }
		public ForeignKeyViolationException(string name) : base(Properties.Resources.ForeignKeyViolation(name)) {
			this.ConstraintName = name;
		}
	}
}
