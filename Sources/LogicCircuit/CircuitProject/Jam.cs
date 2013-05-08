using System;
using System.Windows;
using System.Windows.Media;

namespace LogicCircuit {
	public abstract class Jam {
		public BasePin Pin { get; protected set; }
		public CircuitGlyph CircuitSymbol { get; protected set; }
		public int X { get { return this.Pin.GridPoint.X; } }
		public int Y { get { return this.Pin.GridPoint.Y; } }

		public int Z { get { return this.Pin.Inverted ? 2 : 0; } }

		public PinType EffectivePinType {
			get { return (this.Pin is Pin) ? PinType.None : this.Pin.PinType; }
		}

		public GridPoint AbsolutePoint {
			get {
				CircuitSymbol symbol = this.CircuitSymbol as CircuitSymbol;
				if(symbol != null && symbol.Rotation != Rotation.Up) {
					Matrix matrix = Symbol.RotationTransform(symbol.Rotation, symbol.X, symbol.Y, symbol.Circuit.SymbolWidth, symbol.Circuit.SymbolHeight);
					return Symbol.GridPoint(matrix.Transform(Symbol.ScreenPoint(symbol.Point.Offset(this.X, this.Y))));
				} else {
					return this.CircuitSymbol.Point.Offset(this.X, this.Y);
				}
			}

		}

		public bool IsValid(int bitNumber) {
			if(0 <= bitNumber) {
				if(bitNumber < this.Pin.BitWidth) {
					return true;
				}
				return this.CircuitSymbol.Circuit is CircuitProbe;
			}
			return false;
		}

		public virtual Jam InnerJam {
			get { throw new InvalidOperationException("Should not be called. Only override call is allowed."); }
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} of {1} on {2}", this.GetType().Name, this.Pin.ToString(), this.CircuitSymbol.ToString());
			}
		#endif
	}
}
