using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private abstract class Marker {
			public Symbol Symbol { get; private set; }
			public abstract FrameworkElement Glyph { get; }

			protected Marker(Symbol symbol) {
				this.Symbol = symbol;
			}

			public virtual void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public virtual void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public virtual void CancelMove(Panel selectionLayer) {
			}

			public abstract void Refresh();
		}

		private interface ISizableMarker {
			Size Size { get; }
			void Resize(double x1, double y1, double x2, double y2);
			void CommitResize(EditorDiagram editor);
		}

		private class ResizeMarker<TParent> : Marker where TParent: Marker, ISizableMarker {
			private static readonly Action<TParent, Point>[] move = new Action<TParent, Point>[] {
				(marker, point) => marker.Resize(point.X, point.Y, double.NaN, double.NaN),
				(marker, point) => marker.Resize(double.NaN, point.Y, double.NaN, double.NaN),
				(marker, point) => marker.Resize(double.NaN, point.Y, point.X, double.NaN),

				(marker, point) => marker.Resize(point.X, double.NaN, double.NaN, double.NaN),
				null,
				(marker, point) => marker.Resize(double.NaN, double.NaN, point.X, double.NaN),

				(marker, point) => marker.Resize(point.X, double.NaN, double.NaN, point.Y),
				(marker, point) => marker.Resize(double.NaN, double.NaN, double.NaN, point.Y),
				(marker, point) => marker.Resize(double.NaN, double.NaN, point.X, point.Y)
			};


			private static readonly Cursor[]  cursors = new Cursor[] {
				Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW,
				Cursors.SizeWE, null, Cursors.SizeWE,
				Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE
			};

			private readonly TParent parent;

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
			public ResizeMarker(TParent parent, int x, int y) : base(parent.Symbol) {
				Tracer.Assert(0 <= x && x <= 2 && 0 <= y && y <= 2);
				this.parent = parent;
				this.x = x;
				this.y = y;
				this.rectangle.DataContext = this;
				Panel.SetZIndex(this.rectangle, 1);
				this.rectangle.Width = this.rectangle.Height = 2 * Symbol.PinRadius;
				this.rectangle.Cursor = ResizeMarker<TParent>.cursors[this.x + this.y * 3];
				Tracer.Assert(this.rectangle.Cursor != null);
			}

			public override void Move(EditorDiagram editor, Point point) {
				if(editor.SelectionCount > 1) {
					base.Move(editor, point);
				} else {
					ResizeMarker<TParent>.move[this.x + this.y * 3](this.parent, point);
				}
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				if(editor.SelectionCount > 1) {
					base.Commit(editor, point, withWires);
				} else {
					this.parent.CommitResize(editor);
				}
			}

			public override void Refresh() {
				Size size = this.parent.Size;
				Canvas.SetLeft(this.rectangle, size.Width * this.x / 2 - Symbol.PinRadius);
				Canvas.SetTop(this.rectangle, size.Height * this.y / 2 - Symbol.PinRadius);
			}
		}
	}
}
