using System;

namespace LogicCircuit.DataPersistent {
	public struct RowId : IComparable<RowId> {

		internal static readonly RowId Empty = new RowId(-1);

		private int rowId;

		internal RowId(int rowId) {
			this.rowId = rowId;
		}

		public override bool Equals(object obj) {
			if(obj is RowId) {
				RowId other = (RowId)obj;
				return this.rowId == other.rowId;
			}
			return false;
		}

		public override int GetHashCode() {
			return this.rowId;
		}

		public static bool operator ==(RowId left, RowId right) {
			return left.rowId == right.rowId;
		}

		public static bool operator !=(RowId left, RowId right) {
			return left.rowId != right.rowId;
		}

		public bool IsEmpty { get { return this.rowId == -1; } }

		public int CompareTo(RowId other) {
			return this.rowId - other.rowId;
		}

		public static bool operator <(RowId left, RowId right) {
			return left.rowId < right.rowId;
		}

		public static bool operator >(RowId left, RowId right) {
			return left.rowId > right.rowId;
		}

		public static bool operator <=(RowId left, RowId right) {
			return left.rowId <= right.rowId;
		}

		public static bool operator >=(RowId left, RowId right) {
			return left.rowId >= right.rowId;
		}

		internal int Value { get { return this.rowId; } }

		public override string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Row[{0}]", this.rowId);
		}
	}
}
