using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private abstract class CircuitMarker : Marker {
			public CircuitSymbol CircuitSymbol { get { return (CircuitSymbol)this.Symbol; } }

			private readonly FrameworkElement markerGlyph;
			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			protected CircuitMarker(CircuitSymbol symbol, FrameworkElement markerGlyph) : base(symbol) {
				this.markerGlyph = markerGlyph;
				this.markerGlyph.DataContext = this;
			}

			public override void Refresh() {
				this.markerGlyph.Width = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolWidth) + 2 * Symbol.PinRadius;
				this.markerGlyph.Height = Symbol.ScreenPoint(this.CircuitSymbol.Circuit.SymbolHeight) + 2 * Symbol.PinRadius;
				Canvas.SetLeft(this.markerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.X) - Symbol.PinRadius);
				Canvas.SetTop(this.markerGlyph, Symbol.ScreenPoint(this.CircuitSymbol.Y) - Symbol.PinRadius);
				this.markerGlyph.RenderTransformOrigin = Symbol.MarkerRotationCenter(this.CircuitSymbol.Circuit.SymbolWidth, this.CircuitSymbol.Circuit.SymbolHeight);
				this.markerGlyph.ToolTip = this.CircuitSymbol.Circuit.ToolTip;
			}
		}

		private sealed class CircuitSymbolMarker : CircuitMarker {
			public CircuitSymbolMarker(CircuitSymbol symbol) : base(symbol, Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle)) {
				this.Glyph.RenderTransform = this.CircuitSymbol.Glyph.RenderTransform;
				this.Refresh();
			}
		}
	}
}
