using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private sealed class TextNoteMarker : Marker, ISizableMarker {
			public TextNote TextNote { get { return (TextNote)this.Symbol; } }

			private readonly Canvas markerGlyph;
			public override FrameworkElement Glyph { get { return this.markerGlyph; } }

			private Rect symbolRect;
			private readonly Rectangle rectangle;
			private readonly ResizeMarker<TextNoteMarker>[] resizeMarker;

			public Size Size { get { return new Size(this.rectangle.Width, this.rectangle.Height); } }

			public TextNoteMarker(TextNote textNote) : base(textNote) {
				this.rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				this.markerGlyph = new Canvas();
				this.markerGlyph.DataContext = this;
				this.markerGlyph.ToolTip = Properties.Resources.TextNotation;
				Panel.SetZIndex(this.rectangle, 0);
				this.markerGlyph.Children.Add(this.rectangle);
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

			public void CommitResize(EditorDiagram editor, bool withWires) {
				editor.CommitMove(this);
			}

			public Rect ResizedRect() {
				Rect rect = new Rect(Canvas.GetLeft(this.Glyph), Canvas.GetTop(this.Glyph), this.rectangle.Width, this.rectangle.Height);
				if(this.TextNote.Rotation != Rotation.Up) {
					int x = Symbol.GridPoint(rect.X);
					int y = Symbol.GridPoint(rect.Y);
					int w = Math.Max(Symbol.GridPoint(rect.Width), 1);
					int h = Math.Max(Symbol.GridPoint(rect.Height), 1);
					rect = Symbol.Transform(rect, Symbol.RotationTransform(-Symbol.Angle(this.TextNote.Rotation), x, y, w, h));
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
				this.rectangle.Width = rect.Width;
				this.rectangle.Height = rect.Height;
				foreach(ResizeMarker<TextNoteMarker> marker in this.resizeMarker) {
					marker.Refresh();
				}
			}
		}
	}
}
