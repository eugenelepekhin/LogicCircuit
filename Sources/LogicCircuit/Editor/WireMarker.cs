using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private sealed class WireMarker : Marker {
			private readonly Line line;

			private readonly Canvas markerGlyph;
			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			public Wire Wire { get { return (Wire)this.Symbol; } }

			public WirePointMarker Point1 { get; private set; }
			public WirePointMarker Point2 { get; private set; }

			public WireMarker(Wire wire) : base(wire) {
				this.markerGlyph = new Canvas();
				this.markerGlyph.DataContext = this;

				this.line = Symbol.Skin<Line>(SymbolShape.MarkerLine);
				this.Point1 = new WirePointMarker(this,
					getPoint: () => new Point(this.line.X1, this.line.Y1),
					setPoint: p => { this.line.X1 = p.X; this.line.Y1 = p.Y; },
					shift:    (dx, dy) => { this.Wire.X1 += dx; this.Wire.Y1 += dy; this.PositionGlyphs(); },
					wirePoint:() => this.Wire.Point1
				);
				this.Point2 = new WirePointMarker(this,
					getPoint: () => new Point(this.line.X2, this.line.Y2),
					setPoint: p => { this.line.X2 = p.X; this.line.Y2 = p.Y; },
					shift:    (dx, dy) => { this.Wire.X2 += dx; this.Wire.Y2 += dy; this.PositionGlyphs(); },
					wirePoint:() => this.Wire.Point2
				);

				Panel.SetZIndex(this.line, 0);
				Panel.SetZIndex(this.Point1.Glyph, 1);
				Panel.SetZIndex(this.Point2.Glyph, 1);

				this.markerGlyph.Children.Add(this.line);
				this.markerGlyph.Children.Add(this.Point1.Glyph);
				this.markerGlyph.Children.Add(this.Point2.Glyph);

				this.Refresh();
			}

			public override void Refresh() {
				this.line.X1 = Symbol.ScreenPoint(this.Wire.X1);
				this.line.Y1 = Symbol.ScreenPoint(this.Wire.Y1);
				this.line.X2 = Symbol.ScreenPoint(this.Wire.X2);
				this.line.Y2 = Symbol.ScreenPoint(this.Wire.Y2);
				this.Point1.Refresh();
				this.Point2.Refresh();
			}

			private void PositionGlyphs() {
				this.Wire.PositionGlyph();
				this.Refresh();
			}
		}
	}
}
