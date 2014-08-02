using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace LogicCircuit {
	public partial class Circuit {
		private List<BasePin>[] pins;
		private bool isUpdated;

		private int symbolWidth;
		private int symbolHeight;

		public int SymbolWidth {
			get {
				this.Update();
				return this.symbolWidth;
			}
		}

		public int SymbolHeight {
			get{
				this.Update();
				return this.symbolHeight;
			}
		}

		public abstract string Name { get; set; }
		public abstract string Notation { get; set; }
		public abstract string ToolTip { get; }
		public abstract string Category { get; set; }

		public virtual bool IsDisplay {
			get { return false; }
			set { throw new InvalidOperationException(); }
		}

		public virtual FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			throw new InvalidOperationException();
		}

		public abstract FrameworkElement CreateGlyph(CircuitGlyph symbol);

		public virtual IEnumerable<BasePin> Pins {
			get { return this.CircuitProject.DevicePinSet.SelectByCircuit(this); }
		}

		public IList<BasePin> Left {
			get {
				this.Update();
				return this.pins[(int)PinSide.Left];
			}
		}

		public IList<BasePin> Top {
			get {
				this.Update();
				return this.pins[(int)PinSide.Top];
			}
		}

		public IList<BasePin> Right {
			get {
				this.Update();
				return this.pins[(int)PinSide.Right];
			}
		}

		public IList<BasePin> Bottom {
			get {
				this.Update();
				return this.pins[(int)PinSide.Bottom];
			}
		}

		public virtual void ResetPins() {
			this.isUpdated = false;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				symbol.ResetJams();
			}
		}

		private static void UpdatePin(int sideSize, List<BasePin> pin, Func<int, GridPoint> point) {
			if(pin.Count == 1) {
				pin[0].GridPoint = point(sideSize / 2);
			} else {
				int d = (sideSize - 2) / (pin.Count - 1);
				for(int i = 0; i < pin.Count; i++) {
					pin[i].GridPoint = point(i * d + 1);
				}
			}
		}

		protected virtual int CircuitSymbolWidth(int defaultWidth) {
			return Math.Max(2, defaultWidth);
		}

		protected virtual int CircuitSymbolHeight(int defaultHeight) {
			return Math.Max(2, defaultHeight);
		}

		private void Update() {
			if(!this.isUpdated) {
				if(this.pins == null) {
					this.pins = new List<BasePin>[] { new List<BasePin>(), new List<BasePin>(), new List<BasePin>(), new List<BasePin>() };
				} else {
					foreach(List<BasePin> list in this.pins) {
						list.Clear();
					}
				}
				foreach(BasePin pin in this.Pins) {
					this.pins[(int)pin.PinSide].Add(pin);
				}
				foreach(List<BasePin> list in this.pins) {
					list.Sort(PinComparer.Comparer);
				}
				List<BasePin> left = this.pins[(int)PinSide.Left];
				List<BasePin> top = this.pins[(int)PinSide.Top];
				List<BasePin> right = this.pins[(int)PinSide.Right];
				List<BasePin> bottom = this.pins[(int)PinSide.Bottom];

				this.symbolWidth = this.CircuitSymbolWidth(Math.Max(top.Count, bottom.Count) + 1);
				this.symbolHeight = this.CircuitSymbolHeight(Math.Max(left.Count, right.Count) + 1);

				if(0 < left.Count) {
					Circuit.UpdatePin(this.symbolHeight, left, y => new GridPoint(0, y));
				}
				if(0 < top.Count) {
					Circuit.UpdatePin(this.symbolWidth, top, x => new GridPoint(x, 0));
				}
				if(0 < right.Count) {
					Circuit.UpdatePin(this.symbolHeight, right, y => new GridPoint(this.symbolWidth, y));
				}
				if(0 < bottom.Count) {
					Circuit.UpdatePin(this.symbolWidth, bottom, x => new GridPoint(x, this.symbolHeight));
				}

				this.isUpdated = true;
			}
		}

		public abstract Circuit CopyTo(LogicalCircuit target);

		protected void InvalidateDistinctSymbol() {
			int count = 0;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				Tracer.Assert(count++ == 0, "Only one symbol is expected");
				symbol.Invalidate();
				if(this.IsDisplay && symbol.LogicalCircuit.IsDisplay) {
					this.CircuitProject.LogicalCircuitSet.Invalidate(symbol.LogicalCircuit);
				}
			}
		}

		protected static string BuildToolTip(string mandatoryPart, string optionalPart) {
			if(!string.IsNullOrWhiteSpace(optionalPart)) {
				return mandatoryPart + "\n" + optionalPart;
			}
			return mandatoryPart;
		}

		public virtual bool Similar(Circuit other) {
			return this == other || this.GetType() == other.GetType();
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: \"{1}\"", this.GetType().Name, this.Name);
			}
		#endif
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class CircuitSet {
	}
}
