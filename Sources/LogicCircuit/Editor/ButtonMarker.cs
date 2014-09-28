using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class ButtonMarker : CircuitSymbolMarker, IRectangleMarker {
			public Rect SymbolRect { get; private set; }

			public double X { get; set; }
			public double Y { get; set; }
			public double W { get; set; }
			public double H { get; set; }

			private Canvas MarkerCanvas { get { return (Canvas)this.MarkerGlyph; } }
			public Rectangle Rectangle { get; private set; }

			private readonly ResizeMarker[] resizeMarker;

			public ButtonMarker(CircuitSymbol symbol) : base(symbol, new Canvas()) {
				CircuitButton button = symbol.Circuit as CircuitButton;
				Tracer.Assert(button != null);
				this.Rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);
				Panel.SetZIndex(this.Rectangle, 0);
				this.MarkerCanvas.Children.Add(this.Rectangle);

				this.resizeMarker = new ResizeMarker[] {
					new ResizeMarker(this, 0, 0), new ResizeMarker(this, 1, 0), new ResizeMarker(this, 2, 0),
					new ResizeMarker(this, 0, 1), new ResizeMarker(this, 2, 1),
					new ResizeMarker(this, 0, 2), new ResizeMarker(this, 1, 2), new ResizeMarker(this, 2, 2)
				};
				foreach(ResizeMarker marker in this.resizeMarker) {
					this.MarkerCanvas.Children.Add(marker.Glyph);
				}

				this.SnapToButton();
				this.PositionGlyph();
			}

			public void PositionGlyph() {
				Rect symbolRect = this.SymbolRect;
				double sx = symbolRect.X + this.X;
				double sy = symbolRect.Y + this.Y;
				double sw = symbolRect.Width - this.X + this.W;
				double sh = symbolRect.Height - this.Y + this.H;
				Rect rect = new Rect(new Point(sx, sy), new Point(sx + sw, sy + sh));

				Canvas.SetLeft(this.MarkerGlyph, rect.X);
				Canvas.SetTop(this.MarkerGlyph, rect.Y);
				this.Rectangle.Width = rect.Width;
				this.Rectangle.Height = rect.Height;

				foreach(ResizeMarker marker in this.resizeMarker) {
					marker.PositionGlyph();
				}
			}

			public void CommitResize(EditorDiagram editor) {
				editor.CommitMove(this);
			}

			private void SnapToButton() {
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
				this.SymbolRect = rect;
			}

			public Rect ResizedRect() {
				CircuitButton button = (CircuitButton)this.CircuitSymbol.Circuit;
				this.X = 0;
				this.Y = 0;
				this.W = 0;
				this.H = 0;

				Rect rect = new Rect(Canvas.GetLeft(this.MarkerGlyph), Canvas.GetTop(this.MarkerGlyph), this.Rectangle.Width, this.Rectangle.Height);
				if(this.CircuitSymbol.Rotation != Rotation.Up) {
					rect = Symbol.Transform(rect, Symbol.RotationTransform(-Symbol.Angle(this.CircuitSymbol.Rotation), this.CircuitSymbol.X, this.CircuitSymbol.Y, button.Width, button.Height));
				}
				return rect;
			}

			public void Refresh() {
				this.X = 0;
				this.Y = 0;
				this.W = 0;
				this.H = 0;

				this.SnapToButton();
				this.PositionGlyph();
			}
		}
	}
}
