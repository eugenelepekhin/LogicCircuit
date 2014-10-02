using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class AreaMarker : Marker {
			private readonly Point point0;
			private readonly Rectangle markerGlyph;

			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			public AreaMarker(Point point) : base(null) {
				this.point0 = point;
				this.markerGlyph = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.markerGlyph.DataContext = this;
				this.PositionGlyph(point);
			}

			public override void Move(EditorDiagram editor, Point point) {
				this.PositionGlyph(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.Select(new Rect(this.point0, point));
			}

			private void PositionGlyph(Point point) {
				Rect rect = new Rect(this.point0, point);
				Canvas.SetLeft(this.markerGlyph, rect.X);
				Canvas.SetTop(this.markerGlyph, rect.Y);
				this.markerGlyph.Width = rect.Width;
				this.markerGlyph.Height = rect.Height;
			}

			public override void CancelMove(Panel selectionLayer) {
				selectionLayer.Children.Remove(this.Glyph);
			}

			public override void Refresh() {
				throw new InvalidOperationException();
			}
		}
	}
}
