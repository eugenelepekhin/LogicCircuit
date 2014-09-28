using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class TextNoteMarker : Marker, IRectangleMarker {
			public TextNote TextNote { get; private set; }
			public override Symbol Symbol { get { return this.TextNote; } }
			public Rect SymbolRect { get; private set; }

			public Canvas MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			public Rectangle Rectangle { get; private set; }

			public double X { get; set; }
			public double Y { get; set; }
			public double W { get; set; }
			public double H { get; set; }

			private readonly ResizeMarker[] resizeMarker;

			public TextNoteMarker(TextNote textNote) {
				this.TextNote = textNote;
				this.Rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.MarkerGlyph = new Canvas();
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.ToolTip = Properties.Resources.TextNotation;
				Panel.SetZIndex(this.Rectangle, 0);
				this.MarkerGlyph.Children.Add(this.Rectangle);
				this.resizeMarker = new ResizeMarker[] {
					new ResizeMarker(this, 0, 0), new ResizeMarker(this, 1, 0), new ResizeMarker(this, 2, 0),
					new ResizeMarker(this, 0, 1), new ResizeMarker(this, 2, 1),
					new ResizeMarker(this, 0, 2), new ResizeMarker(this, 1, 2), new ResizeMarker(this, 2, 2)
				};
				foreach(ResizeMarker marker in this.resizeMarker) {
					this.MarkerGlyph.Children.Add(marker.Glyph);
				}
				this.SnapToTextNote();
				this.PositionGlyph();
			}

			private void SnapToTextNote() {
				Rect rect = new Rect(
					Symbol.ScreenPoint(this.TextNote.X),
					Symbol.ScreenPoint(this.TextNote.Y),
					Symbol.ScreenPoint(this.TextNote.Width),
					Symbol.ScreenPoint(this.TextNote.Height)
				);
				if(this.TextNote.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(this.TextNote.Rotation, this.TextNote.X, this.TextNote.Y, this.TextNote.Width, this.TextNote.Height));
				}
				this.SymbolRect = rect;
			}

			public Rect ResizedRect() {
				this.X = 0;
				this.Y = 0;
				this.W = 0;
				this.H = 0;

				Rect rect = new Rect(Canvas.GetLeft(this.MarkerGlyph), Canvas.GetTop(this.MarkerGlyph), this.Rectangle.Width, this.Rectangle.Height);
				if(this.TextNote.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(-Symbol.Angle(this.TextNote.Rotation), this.TextNote.X, this.TextNote.Y, this.TextNote.Width, this.TextNote.Height));
				}
				return rect;
			}

			public override void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
				this.SnapToTextNote();
			}
			public void CommitResize(EditorDiagram editor) {
				editor.CommitMove(this);
			}

			public override void Shift(int dx, int dy) {
				this.TextNote.Shift(dx, dy);
				this.SnapToTextNote();
				this.PositionTextNoteGliph();

			}

			private void PositionTextNoteGliph() {
				this.TextNote.PositionGlyph();
				this.PositionGlyph();
			}

			public void PositionGlyph() {
				double sx = this.SymbolRect.X + this.X;
				double sy = this.SymbolRect.Y + this.Y;
				double sw = this.SymbolRect.Width - this.X + this.W;
				double sh = this.SymbolRect.Height - this.Y + this.H;
				Rect rect = new Rect(new Point(sx, sy), new Point(sx + sw, sy + sh));

				Canvas.SetLeft(this.MarkerGlyph, rect.X);
				Canvas.SetTop(this.MarkerGlyph, rect.Y);
				this.Rectangle.Width = rect.Width;
				this.Rectangle.Height = rect.Height;

				foreach(ResizeMarker marker in this.resizeMarker) {
					marker.PositionGlyph();
				}
			}

			public void Refresh() {
				this.X = 0;
				this.Y = 0;
				this.W = 0;
				this.H = 0;

				this.SnapToTextNote();
				this.PositionGlyph();
			}
		}
	}
}
