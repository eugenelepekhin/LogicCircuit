using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class AreaMarker : Marker {

			public override Symbol Symbol { get { throw new InvalidOperationException(); } }
			public Rectangle MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }
			public Point Point0 { get; private set; }

			public AreaMarker(Point point) {
				this.Point0 = point;
				this.MarkerGlyph = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.MarkerGlyph.DataContext = this;
				this.PositionGlyph(point);
			}

			public override void Move(EditorDiagram editor, Point point) {
				this.PositionGlyph(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.Select(new Rect(this.Point0, point));
			}

			public override void Shift(int dx, int dy) {
				throw new InvalidOperationException();
			}

			private void PositionGlyph(Point point) {
				Rect rect = new Rect(this.Point0, point);
				Canvas.SetLeft(this.MarkerGlyph, rect.X);
				Canvas.SetTop(this.MarkerGlyph, rect.Y);
				this.MarkerGlyph.Width = rect.Width;
				this.MarkerGlyph.Height = rect.Height;
			}

			public override void CancelMove(Panel selectionLayer) {
				selectionLayer.Children.Remove(this.Glyph);
			}
		}
	}
}
