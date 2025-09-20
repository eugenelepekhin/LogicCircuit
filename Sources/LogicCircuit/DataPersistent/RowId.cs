using System;

namespace DataPersistent {
	public readonly struct RowId : IComparable<RowId>, IEquatable<RowId> {

		internal static readonly RowId Empty = new RowId(-1);

		private readonly int rowId;

		internal RowId(int rowId) {
			this.rowId = rowId;
		}

		public override bool Equals(object? obj) {
			if(obj is RowId other) {
				return this.rowId == other.rowId;
			}
			return false;
		}

		public bool Equals(RowId other) => this.rowId == other.rowId;

		public override int GetHashCode() => this.rowId;

		public static bool operator ==(RowId left, RowId right) => left.rowId == right.rowId;

		public static bool operator !=(RowId left, RowId right) => left.rowId != right.rowId;

		public bool IsEmpty => this.rowId == -1;

		public int CompareTo(RowId other) => this.rowId - other.rowId;

		public static bool operator <(RowId left, RowId right) => left.rowId < right.rowId;

		public static bool operator >(RowId left, RowId right) => left.rowId > right.rowId;

		public static bool operator <=(RowId left, RowId right) => left.rowId <= right.rowId;

		public static bool operator >=(RowId left, RowId right) => left.rowId >= right.rowId;

		internal int Value => this.rowId;

		public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "Row[{0}]", this.rowId);
	}
}
