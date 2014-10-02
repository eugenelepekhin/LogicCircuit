using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class WirePledge : Marker {
			private readonly Line markerLine;
			public override FrameworkElement Glyph { get { return this.markerLine; } }

			private GridPoint Point1 { get { return Symbol.GridPoint(new Point(this.markerLine.X1, this.markerLine.Y1)); } }
			private GridPoint Point2 { get { return Symbol.GridPoint(new Point(this.markerLine.X2, this.markerLine.Y2)); } }

			public WirePledge(Point point) : base(null) {
				Point start = Symbol.ScreenPoint(Symbol.GridPoint(point));
				this.markerLine = new Line() {
					X1 = start.X,
					Y1 = start.Y,
					X2 = point.X,
					Y2 = point.Y,
					DataContext = this,
					Stroke = Symbol.WireStroke,
					StrokeThickness = 1,
				};
			}

			public override void Move(EditorDiagram editor, Point point) {
				Point end = Symbol.ScreenPoint(Symbol.GridPoint(point));
				this.markerLine.X2 = end.X;
				this.markerLine.Y2 = end.Y;
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				this.Move(editor, point);
				Wire wire = editor.CreateWire(this.Point1, this.Point2);
				if(wire == null) {
					wire = editor.FindWireNear(point);
					if(wire != null) {
						editor.Select(wire);
					}
				}
			}

			public override void CancelMove(Panel selectionLayer) {
				selectionLayer.Children.Remove(this.Glyph);
			}

			public override void Refresh() {
				throw new InvalidOperationException();
			}
		}
	}
}
