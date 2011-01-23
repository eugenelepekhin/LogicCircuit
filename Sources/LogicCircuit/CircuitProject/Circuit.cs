using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		public abstract FrameworkElement CreateGlyph(CircuitGlyph symbol);

		public virtual bool IsSmallSymbol { get { return false; } }

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
				int width;
				int height;
				if(this.IsSmallSymbol) {
					Splitter splitter = this as Splitter;
					if(splitter != null) {
						width = Math.Max(top.Count, bottom.Count) + 1;
						height = Math.Max(left.Count, right.Count) + 1;
						Debug.Assert(width == 1 && 2 < height || height == 1 && 2 < width);
					} else {
						Debug.Assert(this.Pins.Count() == 1);
						height = width = 2;
					}
				} else {
					width = Math.Max(2, Math.Max(top.Count, bottom.Count)) + 1;
					height = Math.Max(3, Math.Max(left.Count, right.Count)) + 1;
				}
				this.symbolWidth = width;
				this.symbolHeight = height;

				if(0 < left.Count) {
					Circuit.UpdatePin(height, left, y => new GridPoint(0, y));
				}
				if(0 < top.Count) {
					Circuit.UpdatePin(width, top, x => new GridPoint(x, 0));
				}
				if(0 < right.Count) {
					Circuit.UpdatePin(height, right, y => new GridPoint(width, y));
				}
				if(0 < bottom.Count) {
					Circuit.UpdatePin(width, bottom, x => new GridPoint(x, height));
				}

				this.isUpdated = true;
			}
		}

		public abstract Circuit CopyTo(CircuitProject project);

		protected void InvalidateDistinctSymbol() {
			int count = 0;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				Tracer.Assert(count++ == 0, "Only one symbol is expected");
				symbol.Invalidate();
			}
		}

		private class PinComparer : IComparer<BasePin> {
			public static readonly PinComparer Comparer = new PinComparer();

			public int Compare(BasePin x, BasePin y) {
				Tracer.Assert(x.GetType() == y.GetType());
				DevicePin dp1 = x as DevicePin;
				if(dp1 != null) {
					DevicePin dp2 = (DevicePin)y;
					return dp1.Order - dp2.Order;
				} else {
					CircuitSymbolSet symbolSet = x.CircuitProject.CircuitSymbolSet;
					Tracer.Assert(symbolSet == y.CircuitProject.CircuitSymbolSet);
					CircuitSymbol s1 = symbolSet.SelectByCircuit(x).FirstOrDefault();
					CircuitSymbol s2 = symbolSet.SelectByCircuit(y).FirstOrDefault();
					if(s1 != null && s2 != null) {
						int d = s1.Y - s2.Y;
						if(d == 0) {
							d = s1.X - s2.X;
							if(d == 0) {
								return StringComparer.Ordinal.Compare(x.Name, y.Name);
							}
						}
						return d;
					}
					return StringComparer.Ordinal.Compare(x.Name, y.Name);
				}
			}
		}
	}

	public partial class CircuitSet {
	}
}
