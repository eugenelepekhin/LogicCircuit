using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class TextNoteMarker : Marker, IRectangleMarker {
			public TextNote TextNote { get { return (TextNote)this.Symbol; } }

			private Rect symbolRect;

			private readonly Canvas markerGlyph;
			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			public Rectangle Rectangle { get; private set; }

			private readonly ResizeMarker<TextNoteMarker>[] resizeMarker;

			public TextNoteMarker(TextNote textNote) : base(textNote) {
				this.Rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.markerGlyph = new Canvas();
				this.markerGlyph.DataContext = this;
				this.markerGlyph.ToolTip = Properties.Resources.TextNotation;
				Panel.SetZIndex(this.Rectangle, 0);
				this.markerGlyph.Children.Add(this.Rectangle);
				this.resizeMarker = new ResizeMarker<TextNoteMarker>[] {
					new ResizeMarker<TextNoteMarker>(this, 0, 0), new ResizeMarker<TextNoteMarker>(this, 1, 0), new ResizeMarker<TextNoteMarker>(this, 2, 0),
					new ResizeMarker<TextNoteMarker>(this, 0, 1), new ResizeMarker<TextNoteMarker>(this, 2, 1),
					new ResizeMarker<TextNoteMarker>(this, 0, 2), new ResizeMarker<TextNoteMarker>(this, 1, 2), new ResizeMarker<TextNoteMarker>(this, 2, 2)
				};
				foreach(ResizeMarker<TextNoteMarker> marker in this.resizeMarker) {
					this.markerGlyph.Children.Add(marker.Glyph);
				}

				this.Refresh();
			}

			public void Resize(double x1, double y1, double x2, double y2) {
				Point p1 = this.symbolRect.TopLeft;
				Point p2 = this.symbolRect.BottomRight;
				if(!double.IsNaN(x1)) {
					p1.X = x1;
				}
				if(!double.IsNaN(y1)) {
					p1.Y = y1;
				}
				if(!double.IsNaN(x2)) {
					p2.X = x2;
				}
				if(!double.IsNaN(y2)) {
					p2.Y = y2;
				}
				this.PositionGlyph(new Rect(p1, p2));
			}

			public void CommitResize(EditorDiagram editor) {
				editor.CommitMove(this);
			}

			public Rect ResizedRect() {
				Rect rect = new Rect(Canvas.GetLeft(this.Glyph), Canvas.GetTop(this.Glyph), this.Rectangle.Width, this.Rectangle.Height);
				if(this.TextNote.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(-Symbol.Angle(this.TextNote.Rotation), this.TextNote.X, this.TextNote.Y, this.TextNote.Width, this.TextNote.Height));
				}
				return rect;
			}

			public override void Refresh() {
				Rect rect = new Rect(
					Symbol.ScreenPoint(this.TextNote.X),
					Symbol.ScreenPoint(this.TextNote.Y),
					Symbol.ScreenPoint(this.TextNote.Width),
					Symbol.ScreenPoint(this.TextNote.Height)
				);
				if(this.TextNote.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(this.TextNote.Rotation, this.TextNote.X, this.TextNote.Y, this.TextNote.Width, this.TextNote.Height));
				}
				this.symbolRect = rect;
				this.PositionGlyph(rect);
			}

			private void PositionGlyph(Rect rect) {
				Canvas.SetLeft(this.Glyph, rect.X);
				Canvas.SetTop(this.Glyph, rect.Y);
				this.Rectangle.Width = rect.Width;
				this.Rectangle.Height = rect.Height;
				foreach(ResizeMarker<TextNoteMarker> marker in this.resizeMarker) {
					marker.Refresh();
				}
			}
		}
	}
}
