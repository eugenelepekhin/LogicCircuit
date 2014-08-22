using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class WireMarker : Marker {
			public Wire Wire { get; private set; }
			public override Symbol Symbol { get { return this.Wire; } }

			public Canvas MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			private Line Line { get; set; }

			public WirePointMarker Point1 { get; private set; }
			public WirePointMarker Point2 { get; private set; }

			public WireMarker(Wire wire) {
				this.Wire = wire;
				this.MarkerGlyph = new Canvas();
				this.MarkerGlyph.DataContext = this;

				this.Line = Symbol.Skin<Line>(SymbolShape.MarkerLine);
				this.Point1 = new WirePointMarker(this,
					getPoint: () => new Point(this.Line.X1, this.Line.Y1),
					setPoint: p => { this.Line.X1 = p.X; this.Line.Y1 = p.Y; },
					shift:    (dx, dy) => { this.Wire.X1 += dx; this.Wire.Y1 += dy; this.PositionWireGlyph(); },
					wirePoint:() => this.Wire.Point1
				);
				this.Point2 = new WirePointMarker(this,
					getPoint: () => new Point(this.Line.X2, this.Line.Y2),
					setPoint: p => { this.Line.X2 = p.X; this.Line.Y2 = p.Y; },
					shift:    (dx, dy) => { this.Wire.X2 += dx; this.Wire.Y2 += dy; this.PositionWireGlyph(); },
					wirePoint:() => this.Wire.Point2
				);

				Panel.SetZIndex(this.Line, 0);
				Panel.SetZIndex(this.Point1.MarkerGlyph, 1);
				Panel.SetZIndex(this.Point2.MarkerGlyph, 1);

				this.MarkerGlyph.Children.Add(this.Line);
				this.MarkerGlyph.Children.Add(this.Point1.MarkerGlyph);
				this.MarkerGlyph.Children.Add(this.Point2.MarkerGlyph);

				this.PositionGlyph();
			}

			public override void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public override void Shift(int dx, int dy) {
				this.Wire.Shift(dx, dy);
				this.PositionWireGlyph();
			}

			public void PositionGlyph() {
				this.Line.X1 = Symbol.ScreenPoint(this.Wire.X1);
				this.Line.Y1 = Symbol.ScreenPoint(this.Wire.Y1);
				this.Line.X2 = Symbol.ScreenPoint(this.Wire.X2);
				this.Line.Y2 = Symbol.ScreenPoint(this.Wire.Y2);
				this.Point1.PositionGlyph();
				this.Point2.PositionGlyph();
			}

			private void PositionWireGlyph() {
				this.Wire.PositionGlyph();
				this.PositionGlyph();
			}
		}
	}
}
