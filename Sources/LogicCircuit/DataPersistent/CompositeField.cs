using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit.DataPersistent {

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	partial class TableSnapshot<TRecord> {
		private struct Composite<T1, T2> {
			public T1 t1;
			public T2 t2;
		}

		private interface ICompositeField {
			bool ConsistOf(IField<TRecord> field1, IField<TRecord> field2);
			bool ConsistOf(IField<TRecord> field1, IField<TRecord> field2, IField<TRecord> field3);
		}

		private class CompositeField<T1, T2> : IField<TRecord, Composite<T1, T2>>, ICompositeField {
			private IField<TRecord, T1> f1;
			private IField<TRecord, T2> f2;

			public CompositeField(IField<TRecord, T1> f1, IField<TRecord, T2> f2) {
				this.f1 = f1;
				this.f2 = f2;
				this.Name = f1.Name + "." + f2.Name;
			}

			public Composite<T1, T2> DefaultValue {
				get { return new Composite<T1, T2>() { t1 = this.f1.DefaultValue, t2 = this.f2.DefaultValue }; }
			}

			public Composite<T1, T2> GetValue(ref TRecord record) {
				return new Composite<T1, T2>() {
					t1 = this.f1.GetValue(ref record),
					t2 = this.f2.GetValue(ref record)
				};
			}

			public void SetValue(ref TRecord record, Composite<T1, T2> value) {
				throw new InvalidOperationException();
			}

			public string Name { get; private set; }

			public int Order {
				get { return -1; }
				set { throw new InvalidOperationException(); }
			}

			public int Compare(ref TRecord data1, ref TRecord data2) {
				int result = this.f1.Compare(ref data1, ref data2);
				if(result == 0) {
					return this.f2.Compare(ref data1, ref data2);
				}
				return result;
			}

			public int Compare(Composite<T1, T2> x, Composite<T1, T2> y) {
				int result = this.f1.Compare(x.t1, y.t1);
				if(result == 0) {
					return this.f2.Compare(x.t2, y.t2);
				}
				return result;
			}

			public bool ConsistOf(IField<TRecord> field1, IField<TRecord> field2) {
				return this.f1 == field1 && this.f2 == field2;
			}

			public bool ConsistOf(IField<TRecord> field1, IField<TRecord> field2, IField<TRecord> field3) {
				return false;
			}
		}
	}
}
