using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogicCircuit {
	public class PinOrderDescriptor : CircuitDescriptor<Pin> {
		public static IComparer<PinOrderDescriptor> Comparer { get; } = new PinOrderComparer();

		private class PinOrderComparer : IComparer<PinOrderDescriptor>, IComparer {
			public int Compare(PinOrderDescriptor x, PinOrderDescriptor y) => x.CompareTo(y);
			public int Compare(object x, object y) => this.Compare((PinOrderDescriptor)x, (PinOrderDescriptor)y);
		}

		private int index;
		public int Index {
			get => this.index;
			set {
				if(this.index != value) {
					this.index = value;
					this.NotifyPropertyChanged(nameof(this.Index));
				}
			}
		}

		private readonly int x;
		private readonly int y;
		private readonly string name;

		public PinOrderDescriptor(Pin pin) : base(pin) {
			this.index = pin.Index;
			this.name = pin.Name;
			CircuitSymbolSet symbolSet = pin.CircuitProject.CircuitSymbolSet;
			CircuitSymbol symbol = symbolSet.SelectByCircuit(pin).FirstOrDefault();
			this.x = symbol.X;
			this.y = symbol.Y;
		}

		public int CompareTo(PinOrderDescriptor other) {
			int i = this.index - other.index;
			if(i == 0) {
				i = this.y - other.y;
				if(i == 0) {
					i = this.x - other.x;
					if(i == 0) {
						i = StringComparer.Ordinal.Compare(this.name, other.name);
					}
				}
			}
			return i;
		}

		public override bool CategoryExpanded { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		protected override Pin GetCircuitToDrop(CircuitProject circuitProject) => throw new InvalidOperationException();

		#if DEBUG
			public override string ToString() => this.Circuit.ToString();
		#endif
	}
}
