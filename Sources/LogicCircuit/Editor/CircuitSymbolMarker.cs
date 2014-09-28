using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class CircuitSymbolMarker : Marker {
			public CircuitSymbol CircuitSymbol { get; private set; }
			public override Symbol Symbol { get { return this.CircuitSymbol; } }

			public FrameworkElement MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			protected CircuitSymbolMarker(CircuitSymbol symbol, FrameworkElement markerGlyph) {
				this.CircuitSymbol = symbol;
				this.MarkerGlyph = markerGlyph;
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.RenderTransform = this.CircuitSymbol.Glyph.RenderTransform;
				this.Invalidate();
			}

			public CircuitSymbolMarker(CircuitSymbol symbol) : this(symbol, Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle)) {
			}

			public override void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public override void Shift(int dx, int dy) {
				this.CircuitSymbol.Shift(dx, dy);
				this.CircuitSymbol.PositionGlyph();
				this.PositionGlyph();
			}

			public void Invalidate() {
				this.MarkerGlyph.Width = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolWidth) + 2 * Symbol.PinRadius;
				this.MarkerGlyph.Height = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolHeight) + 2 * Symbol.PinRadius;
				this.PositionGlyph();
				this.MarkerGlyph.ToolTip = this.CircuitSymbol.Circuit.ToolTip;
			}

			private void PositionGlyph() {
				Canvas.SetLeft(this.MarkerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.X) - Symbol.PinRadius);
				Canvas.SetTop(this.MarkerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.Y) - Symbol.PinRadius);
				this.MarkerGlyph.RenderTransformOrigin = Symbol.MarkerRotationCenter(this.CircuitSymbol.Circuit.SymbolWidth, this.CircuitSymbol.Circuit.SymbolHeight);
			}
		}
	}
}
