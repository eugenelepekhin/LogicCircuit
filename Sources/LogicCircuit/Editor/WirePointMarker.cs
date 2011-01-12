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

			public override Symbol Symbol { get { return this.Parent.Symbol; } }

			public Rectangle MarkerGlyph { get; private set; }
			
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			public WirePointMarker(WireMarker parent, Func<Point> getPoint, Action<Point> setPoint, Action<int, int> shift, Func<GridPoint> wirePoint) {
				this.Parent = parent;
				this.getPoint = getPoint;
				this.setPoint = setPoint;
				this.shift = shift;
				this.WirePoint = wirePoint;
				this.MarkerGlyph = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.Width = this.MarkerGlyph.Height = 2 * Symbol.PinRadius;
			}

			public override void Move(EditorDiagram editor, Point point) {
				if(editor.SelectionCount > 1) {
					editor.MoveSelection(point);
				} else {
					this.setPoint(point);
					this.PositionGlyph();
				}
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				if(editor.SelectionCount > 1) {
					editor.CommitMove(point, withWires);
				} else {
					editor.CommitMove(point, withWires, this);
					if(this.Parent.Wire.IsDeleted()) {
						editor.Unselect(this.Parent.Wire);
					} else {
						this.Parent.PositionGlyph();
					}
				}
			}

			public override void Shift(int dx, int dy) {
				this.shift(dx, dy);
			}

			public void PositionGlyph() {
				Point point = this.getPoint();
				Canvas.SetLeft(this.MarkerGlyph, point.X - Symbol.PinRadius);
				Canvas.SetTop(this.MarkerGlyph, point.Y - Symbol.PinRadius);
			}
		}
	}
}
