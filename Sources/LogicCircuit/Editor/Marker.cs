using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private abstract class Marker {
			public abstract Symbol Symbol { get; }
			public abstract FrameworkElement Glyph { get; }
			public abstract void Move(EditorDiagram editor, Point point);
			public abstract void Commit(EditorDiagram editor, Point point, bool withWires);
			public abstract void Shift(int dx, int dy);
			public virtual void CancelMove(Panel selectionLayer) {
			}
		}

		private interface IRectangleMarker {
			Rect SymbolRect { get; }
			double X { get; set; }
			double Y { get; set; }
			double W { get; set; }
			double H { get; set; }
			Rectangle Rectangle { get; }
			void PositionGlyph();
			Symbol Symbol { get; }
			void CommitResize(EditorDiagram editor);
			Rect ResizedRect();
		}

		private class ResizeMarker : Marker {
			private static readonly Action<IRectangleMarker, Point>[] move = new Action<IRectangleMarker, Point>[] {
				(marker, point) => { marker.X = point.X - marker.SymbolRect.X; marker.Y = point.Y - marker.SymbolRect.Y; },
				(marker, point) => { marker.Y = point.Y - marker.SymbolRect.Y; },
				(marker, point) => { marker.W = point.X - marker.SymbolRect.Right; marker.Y = point.Y - marker.SymbolRect.Y; },

				(marker, point) => { marker.X = point.X - marker.SymbolRect.X; },
				null,
				(marker, point) => { marker.W = point.X - marker.SymbolRect.Right; },

				(marker, point) => { marker.X = point.X - marker.SymbolRect.X; marker.H = point.Y - marker.SymbolRect.Bottom; },
				(marker, point) => { marker.H = point.Y - marker.SymbolRect.Bottom; },
				(marker, point) => { marker.W = point.X - marker.SymbolRect.Right; marker.H = point.Y - marker.SymbolRect.Bottom; }
			};

			private static readonly Cursor[]  cursors = new Cursor[] {
				Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW,
				Cursors.SizeWE, null, Cursors.SizeWE,
				Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE
			};

			private readonly IRectangleMarker parent;
			public override Symbol Symbol { get { return this.parent.Symbol; } }

			private readonly Rectangle rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
			public override FrameworkElement Glyph { get { return this.rectangle; } }

			private readonly int x;
			private readonly int y;

			/// <summary>
			/// Creates marker
			/// </summary>
			/// <param name="parent"></param>
			/// <param name="x">0 - leftmost position on the parent. 1 - center of the edge. 2 - rightmost position</param>
			/// <param name="y">0 - topmost position on the parent. 1 - center of the edge. 2 - bottommost position</param>
			public ResizeMarker(IRectangleMarker parent, int x, int y) {
				Tracer.Assert(0 <= x && x <= 2 && 0 <= y && y <= 2);
				this.parent = parent;
				this.x = x;
				this.y = y;
				this.rectangle.DataContext = this;
				Panel.SetZIndex(this.rectangle, 1);
				this.rectangle.Width = this.rectangle.Height = 2 * Symbol.PinRadius;
				this.rectangle.Cursor = ResizeMarker.cursors[this.x + this.y * 3];
				Tracer.Assert(this.rectangle.Cursor != null);
			}

			public void PositionGlyph() {
				Canvas.SetLeft(this.rectangle, this.parent.Rectangle.Width * this.x / 2 - Symbol.PinRadius);
				Canvas.SetTop(this.rectangle, this.parent.Rectangle.Height * this.y / 2 - Symbol.PinRadius);
			}

			public override void Move(EditorDiagram editor, Point point) {
				if(editor.SelectionCount > 1) {
					editor.MoveSelection(point);
				} else {
					ResizeMarker.move[this.x + this.y * 3](this.parent, point);
					this.parent.PositionGlyph();
				}
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				if(editor.SelectionCount > 1) {
					editor.CommitMove(point, withWires);
				} else {
					this.parent.CommitResize(editor);
				}
			}

			public override void Shift(int dx, int dy) {
				throw new InvalidOperationException();
			}
		}
	}
}
