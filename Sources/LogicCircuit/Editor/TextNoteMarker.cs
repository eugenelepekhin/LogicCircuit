using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class TextNoteMarker : Marker {

			private static readonly Action<TextNoteMarker, Point>[] move = new Action<TextNoteMarker, Point>[] {
				(marker, point) => { marker.x = point.X - Symbol.ScreenPoint(marker.TextNote.X); marker.y = point.Y - Symbol.ScreenPoint(marker.TextNote.Y); },
				(marker, point) => { marker.y = point.Y - Symbol.ScreenPoint(marker.TextNote.Y); },
				(marker, point) => { marker.w = point.X - Symbol.ScreenPoint(marker.TextNote.X + marker.TextNote.Width); marker.y = point.Y - Symbol.ScreenPoint(marker.TextNote.Y); },

				(marker, point) => { marker.x = point.X - Symbol.ScreenPoint(marker.TextNote.X); },
				null,
				(marker, point) => { marker.w = point.X - Symbol.ScreenPoint(marker.TextNote.X + marker.TextNote.Width); },

				(marker, point) => { marker.x = point.X - Symbol.ScreenPoint(marker.TextNote.X); marker.h = point.Y - Symbol.ScreenPoint(marker.TextNote.Y + marker.TextNote.Height); },
				(marker, point) => { marker.h = point.Y - Symbol.ScreenPoint(marker.TextNote.Y + marker.TextNote.Height); },
				(marker, point) => { marker.w = point.X - Symbol.ScreenPoint(marker.TextNote.X + marker.TextNote.Width); marker.h = point.Y - Symbol.ScreenPoint(marker.TextNote.Y + marker.TextNote.Height); }
			};

			public TextNote TextNote { get; private set; }
			public override Symbol Symbol { get { return this.TextNote; } }

			public Canvas MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			private readonly Rectangle rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);

			private double x = 0;
			private double y = 0;
			private double w = 0;
			private double h = 0;

			private readonly ResizeMarker[] resizeMarker;

			public TextNoteMarker(TextNote textNote) {
				this.TextNote = textNote;
				this.MarkerGlyph = new Canvas();
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.ToolTip = "text note";
				Panel.SetZIndex(this.rectangle, 0);
				this.MarkerGlyph.Children.Add(this.rectangle);
				this.resizeMarker = new ResizeMarker[] {
					new ResizeMarker(this, 0, 0), new ResizeMarker(this, 1, 0), new ResizeMarker(this, 2, 0),
					new ResizeMarker(this, 0, 1), new ResizeMarker(this, 2, 1),
					new ResizeMarker(this, 0, 2), new ResizeMarker(this, 1, 2), new ResizeMarker(this, 2, 2)
				};
				foreach(ResizeMarker marker in this.resizeMarker) {
					this.MarkerGlyph.Children.Add(marker.Glyph);
				}
				this.PositionGlyph();
			}

			public override void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public override void Shift(int dx, int dy) {
				this.TextNote.Shift(dx, dy);
				this.PositionTextNoteGliph();

			}

			private void PositionTextNoteGliph() {
				this.TextNote.PositionGlyph();
				this.PositionGlyph();
			}

			public void PositionGlyph() {
				double sx = Symbol.ScreenPoint(this.TextNote.X) + this.x;
				double sy = Symbol.ScreenPoint(this.TextNote.Y) + this.y;
				double sw = Symbol.ScreenPoint(this.TextNote.Width) - this.x + this.w;
				double sh = Symbol.ScreenPoint(this.TextNote.Height) - this.y + this.h;
				Rect rect = new Rect(new Point(sx, sy), new Point(sx + sw, sy + sh));

				Canvas.SetLeft(this.MarkerGlyph, rect.X);
				Canvas.SetTop(this.MarkerGlyph, rect.Y);
				this.rectangle.Width = rect.Width;
				this.rectangle.Height = rect.Height;

				foreach(ResizeMarker marker in this.resizeMarker) {
					marker.PositionGlyph();
				}
			}

			public Rect ResizedRect() {
				this.x = 0;
				this.y = 0;
				this.w = 0;
				this.h = 0;
				
				return new Rect(Canvas.GetLeft(this.MarkerGlyph), Canvas.GetTop(this.MarkerGlyph), this.rectangle.Width, this.rectangle.Height);
			}
		
			private class ResizeMarker : Marker {
				private static readonly Cursor[]  cursors = new Cursor[] {
					Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW,
					Cursors.SizeWE, null, Cursors.SizeWE,
					Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE
				};

				private readonly TextNoteMarker parent;
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
				public ResizeMarker(TextNoteMarker parent, int x, int y) {
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
					Canvas.SetLeft(this.rectangle, this.parent.rectangle.Width * this.x / 2 - Symbol.PinRadius);
					Canvas.SetTop(this.rectangle, this.parent.rectangle.Height * this.y / 2 - Symbol.PinRadius);
				}

				public override void Move(EditorDiagram editor, Point point) {
					if(editor.SelectionCount > 1) {
						editor.MoveSelection(point);
					} else {
						TextNoteMarker.move[this.x + this.y * 3](this.parent, point);
						this.parent.PositionGlyph();
					}
				}

				public override void Commit(EditorDiagram editor, Point point, bool withWires) {
					if(editor.SelectionCount > 1) {
						editor.CommitMove(point, withWires);
					} else {
						editor.CommitMove(this.parent);
						this.PositionGlyph();
					}
				}

				public override void Shift(int dx, int dy) {
					throw new InvalidOperationException();
				}
			}
		}
	}
}
