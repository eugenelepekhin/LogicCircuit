﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class Editor {
		private class CircuitSymbolMarker : Marker {
			public CircuitSymbol CircuitSymbol { get; private set; }
			public override Symbol Symbol { get { return this.CircuitSymbol; } }

			public Rectangle MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			public CircuitSymbolMarker(CircuitSymbol symbol) {
				this.CircuitSymbol = symbol;
				this.MarkerGlyph = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.Width = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolWidth) + 2 * Symbol.PinRadius;
				this.MarkerGlyph.Height = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolHeight) + 2 * Symbol.PinRadius;
				Canvas.SetLeft(this.MarkerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.X) - Symbol.PinRadius);
				Canvas.SetTop(this.MarkerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.Y) - Symbol.PinRadius);
			}

			public override void Move(Editor editor, Point point) {
				throw new NotImplementedException();
			}

			public override void Commit(Editor editor, Point point) {
				throw new NotImplementedException();
			}
		}
	}
}
