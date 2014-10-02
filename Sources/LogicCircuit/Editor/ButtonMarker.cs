using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private sealed class ButtonMarker : CircuitMarker, ISizableMarker {
			private Rect symbolRect;
			private readonly Rectangle rectangle;
			private readonly ResizeMarker<ButtonMarker>[] resizeMarker;

			public Size Size { get { return new Size(this.rectangle.Width, this.rectangle.Height); } }

			public ButtonMarker(CircuitSymbol symbol) : base(symbol, new Canvas()) {
				Tracer.Assert(symbol.Circuit is CircuitButton);

				this.resizeMarker = new ResizeMarker<ButtonMarker>[] {
					new ResizeMarker<ButtonMarker>(this, 0, 0), new ResizeMarker<ButtonMarker>(this, 1, 0), new ResizeMarker<ButtonMarker>(this, 2, 0),
					new ResizeMarker<ButtonMarker>(this, 0, 1), new ResizeMarker<ButtonMarker>(this, 2, 1),
					new ResizeMarker<ButtonMarker>(this, 0, 2), new ResizeMarker<ButtonMarker>(this, 1, 2), new ResizeMarker<ButtonMarker>(this, 2, 2)
				};

				this.rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				Panel.SetZIndex(this.rectangle, 0);

				Canvas markerCanvas = (Canvas)this.Glyph;
				markerCanvas.Children.Add(this.rectangle);

				foreach(ResizeMarker<ButtonMarker> marker in this.resizeMarker) {
					markerCanvas.Children.Add(marker.Glyph);
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
				editor.CommitMove(this, withWires);
			}

			public Rect ResizedRect() {
				int x = Symbol.GridPoint(Canvas.GetLeft(this.Glyph));
				int y = Symbol.GridPoint(Canvas.GetTop(this.Glyph));
				int w = Math.Max(2, Math.Min(Symbol.GridPoint(this.rectangle.Width), CircuitButton.MaxWidth));
				int h = Math.Max(2, Math.Min(Symbol.GridPoint(this.rectangle.Height), CircuitButton.MaxHeight));
				Rect rect = new Rect(Symbol.ScreenPoint(x), Symbol.ScreenPoint(y), Symbol.ScreenPoint(w), Symbol.ScreenPoint(h));
				if(this.CircuitSymbol.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(-Symbol.Angle(this.CircuitSymbol.Rotation), x, y, w, h));
				}
				return rect;
			}

			public override void Refresh() {
				CircuitButton button = (CircuitButton)this.CircuitSymbol.Circuit;
				Rect rect = new Rect(
					Symbol.ScreenPoint(this.CircuitSymbol.X),
					Symbol.ScreenPoint(this.CircuitSymbol.Y),
					Symbol.ScreenPoint(button.Width),
					Symbol.ScreenPoint(button.Height)
				);
				if(this.CircuitSymbol.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(this.CircuitSymbol.Rotation, this.CircuitSymbol.X, this.CircuitSymbol.Y, button.Width, button.Height));
				}
				this.symbolRect = rect;
				this.PositionGlyph(rect);
			}

			private void PositionGlyph(Rect rect) {
				Canvas.SetLeft(this.Glyph, rect.X);
				Canvas.SetTop(this.Glyph, rect.Y);
				this.rectangle.Width = rect.Width;
				this.rectangle.Height = rect.Height;
				foreach(ResizeMarker<ButtonMarker> marker in this.resizeMarker) {
					marker.Refresh();
				}
			}
		}
	}
}
