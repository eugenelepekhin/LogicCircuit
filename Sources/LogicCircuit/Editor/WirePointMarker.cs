using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class WirePointMarker : Marker {
			public WireMarker Parent { get; private set; }
			private readonly Func<Point> getPoint;
			private readonly Action<Point> setPoint;
			private readonly Action<int, int> shift;
			public Func<GridPoint> WirePoint { get; private set; }

			private readonly Rectangle markerGlyph;

			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			public WirePointMarker(WireMarker parent, Func<Point> getPoint, Action<Point> setPoint, Action<int, int> shift, Func<GridPoint> wirePoint) : base(parent.Symbol) {
				this.Parent = parent;
				this.getPoint = getPoint;
				this.setPoint = setPoint;
				this.shift = shift;
				this.WirePoint = wirePoint;
				this.markerGlyph = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.markerGlyph.DataContext = this;
				this.markerGlyph.Width = this.markerGlyph.Height = 2 * Symbol.PinRadius;
			}

			public override void Move(EditorDiagram editor, Point point) {
				if(editor.SelectionCount > 1) {
					base.Move(editor, point);
				} else {
					this.setPoint(point);
					this.Refresh();
				}
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				if(editor.SelectionCount > 1) {
					base.Commit(editor, point, withWires);
				} else {
					editor.CommitMove(point, withWires, this);
					if(this.Parent.Wire.IsDeleted()) {
						editor.Unselect(this.Parent.Wire);
					} else {
						this.Parent.Refresh();
					}
				}
			}

			public void Shift(int dx, int dy) {
				this.shift(dx, dy);
			}

			public override void CancelMove(Panel selectionLayer) {
				this.Parent.Refresh();
			}

			public override void Refresh() {
				Point point = this.getPoint();
				Canvas.SetLeft(this.markerGlyph, point.X - Symbol.PinRadius);
				Canvas.SetTop(this.markerGlyph, point.Y - Symbol.PinRadius);
			}
		}
	}
}
