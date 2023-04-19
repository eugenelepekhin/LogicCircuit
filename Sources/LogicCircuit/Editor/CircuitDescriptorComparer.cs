using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit {
	internal sealed class CircuitDescriptorComparer : IComparer<IDescriptor> {
		public static readonly CircuitDescriptorComparer Comparer = new CircuitDescriptorComparer();

		public int Compare(IDescriptor? x, IDescriptor? y) {
			Debug.Assert(x != null && y != null);
			int r = StringComparer.Ordinal.Compare(x.Circuit.Category, y.Circuit.Category);
			if(r == 0) {
				return StringComparer.Ordinal.Compare(x.Circuit.Name, y.Circuit.Name);
			}
			return r;
		}
	}
}
