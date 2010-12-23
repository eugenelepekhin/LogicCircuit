using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class Editor {
		private class WireMarker : Marker {
			public Wire Wire { get; private set; }
			public override Symbol Symbol { get { return this.Wire; } }

			public Canvas MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }
			
			public WireMarker(Wire wire) {
				this.Wire = wire;
				Line line = Symbol.Skin<Line>(SymbolShape.MarkerLine);
				line.DataContext = this;
				line.X1 = Symbol.ScreenPoint(this.Wire.X1);
				line.Y1 = Symbol.ScreenPoint(this.Wire.Y1);
				line.X2 = Symbol.ScreenPoint(this.Wire.X2);
				line.Y2 = Symbol.ScreenPoint(this.Wire.Y2);
				Rectangle p1 = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				Rectangle p2 = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.MarkerGlyph = new Canvas();
				this.MarkerGlyph.Children.Add(line);
				this.MarkerGlyph.Children.Add(p1);
				this.MarkerGlyph.Children.Add(p2);
				p1.Width = p2.Width = p1.Height = p2.Height = 2 * Symbol.PinRadius;
				Canvas.SetLeft(p1, Symbol.ScreenPoint(this.Wire.X1) - Symbol.PinRadius);
				Canvas.SetTop(p1, Symbol.ScreenPoint(this.Wire.Y1) - Symbol.PinRadius);
				Canvas.SetLeft(p2, Symbol.ScreenPoint(this.Wire.X2) - Symbol.PinRadius);
				Canvas.SetTop(p2, Symbol.ScreenPoint(this.Wire.Y2) - Symbol.PinRadius);
			}

			public override void Move(Editor editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(Editor editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public override void Shift(int dx, int dy) {
				this.Wire.Shift(dx, dy);
				Tracer.Fail();
			}
		}
	}
}
