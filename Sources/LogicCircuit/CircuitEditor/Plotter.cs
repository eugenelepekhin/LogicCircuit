using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.IO;
using System.Xml;
using System.Windows.Markup;

namespace LogicCircuit {
	public static class Plotter {
		public const int PinRadius = 3;
		public const int GridSize = Plotter.PinRadius * 6;

		public static int LogicalCircuitWidth { get { return 170; } }
		public static int LogicalCircuitHeight { get { return 170; } }
		public static double LogicalCircuitScreenWidth { get { return Plotter.ScreenPoint(Plotter.LogicalCircuitWidth); } }
		public static double LogicalCircuitScreenHeight { get { return Plotter.ScreenPoint(Plotter.LogicalCircuitHeight); } }
		public static Rect LogicalCircuitBackgroundTile { get { return new Rect(0, 0, Plotter.GridSize, Plotter.GridSize); } }

		public static Brush WireStroke { get { return Brushes.Black; } }
		public static Brush JamDirectFill { get { return Brushes.Black; } }
		public static Brush JamInvertedFill { get { return Brushes.White; } }
		public static Brush JamStroke { get { return Brushes.Black; } }

		public static double ScreenPoint(int xGrid) {
			return (double)xGrid * Plotter.GridSize;
		}

		public static Point ScreenPoint(int xGrid, int yGrid) {
			return new Point((double)xGrid * Plotter.GridSize, (double)yGrid * Plotter.GridSize);
		}

		public static Point ScreenPoint(GridPoint point) {
			return Plotter.ScreenPoint(point.X, point.Y);
		}

		public static int GridPoint(double xScreen) {
			return (int)Math.Round(xScreen / Plotter.GridSize);
		}

		public static GridPoint GridPoint(Point screenPoint) {
			return new GridPoint(
				(int)Math.Round(screenPoint.X / Plotter.GridSize),
				(int)Math.Round(screenPoint.Y / Plotter.GridSize)
			);
		}

		/// <summary>
		/// Evaluate relative position of two vectors defined as (p0, p1) and (p0, p2)
		/// </summary>
		/// <param name="point0">Start point of both vectors</param>
		/// <param name="point1">The end point of first vector</param>
		/// <param name="point2">The end point of second vector</param>
		/// <returns>if +1 then vector p0-p1 is clockwise from p0-p2; if -1, it is counterclockwise</returns>
		public static int CrossProductSign(Point point0, Point point1, Point point2) {
			return Math.Sign((point1.X - point0.X) * (point2.Y - point0.Y) - (point2.X - point0.X) * (point1.Y - point0.Y));
		}

		/// <summary>
		/// Checks if two line segments are intersected
		/// </summary>
		/// <param name="point1">First vertex of first line segment</param>
		/// <param name="point2">Second vertex of first line segment</param>
		/// <param name="point3">First vertex of second line segment</param>
		/// <param name="point4">Second vertex of second line segment</param>
		/// <returns>true if line segment (p1, p2) is intersected with line segment (p3, p4)</returns>
		public static bool Intersected(Point point1, Point point2, Point point3, Point point4) {
			//1. Find if bounding rectangles intersected
			double x1 = Math.Min(point1.X, point2.X);
			double y1 = Math.Min(point1.Y, point2.Y);
			double x2 = Math.Max(point1.X, point2.X);
			double y2 = Math.Max(point1.Y, point2.Y);

			double x3 = Math.Min(point3.X, point4.X);
			double y3 = Math.Min(point3.Y, point4.Y);
			double x4 = Math.Max(point3.X, point4.X);
			double y4 = Math.Max(point3.Y, point4.Y);
			if((x2 >= x3) && (x4 >= x1) && (y2 >= y3) && (y4 >= y1)) {
				//2. now, two line segments intersected if each straddles the line containing the other
				// let's use CrossRoductSign for this purpose.
				return(
					(Plotter.CrossProductSign(point1, point2, point3) * Plotter.CrossProductSign(point1, point2, point4) <= 0) &&
					(Plotter.CrossProductSign(point3, point4, point1) * Plotter.CrossProductSign(point3, point4, point2) <= 0)
				);
			}
			return false;
		}

		/// <summary>
		/// Checks if line segment (p1, p2) intersets with rectangle r
		/// </summary>
		/// <param name="point1">First vertex of the line segment</param>
		/// <param name="point2">Second vertex of the line segment</param>
		/// <param name="rect">Rectangle</param>
		/// <returns>true if line segment and rectangle have intersection</returns>
		public static bool Intersected(Point point1, Point point2, Rect rect) {
			if(rect.Contains(point1) || rect.Contains(point2)) {
				return true;
			}
			Point r1 = new Point(rect.X, rect.Y);
			Point r2 = new Point(rect.X + rect.Width, rect.Y);
			Point r3 = new Point(rect.X + rect.Width, rect.Y + rect.Height);
			Point r4 = new Point(rect.X, rect.Y + rect.Height);
			return(
				Plotter.Intersected(point1, point2, r1, r2) ||
				Plotter.Intersected(point1, point2, r2, r3) ||
				Plotter.Intersected(point1, point2, r3, r4) ||
				Plotter.Intersected(point1, point2, r4, r1)
			);
		}

		//---------------------------------------------------------------------

		public static Line CreateGlyph(Wire wire) {
			Line line = new Line();
			line.Stroke = Plotter.WireStroke;
			line.StrokeThickness = 1;
			line.ToolTip = Resources.ToolTipWire;
			line.Tag = wire;
			return line;
		}

		public static FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Circuit circuit = symbol.Circuit;
			if(circuit is LogicalCircuit || circuit is Memory) {
				return Plotter.RectangularGlyph(circuit, symbol);
			}
			Gate gate = circuit as Gate;
			if(gate != null) {
				switch(gate.GateType) {
				case GateType.Clock:
					return Plotter.ClockGlyph(gate, symbol);
				case GateType.Led:
					if(gate.InputCount == 1) {
						return Plotter.LedGlyph(gate, symbol);
					} else {
						Tracer.Assert(gate.InputCount == 8);
						return Plotter.SevenSegmentGlyph(gate, symbol);
					}
				case GateType.Probe:
					return Plotter.ProbeGlyph(gate, symbol);
				default:
					if(Settings.User.GateShape == GateShape.Rectangular) {
						return Plotter.RectangularGlyph(gate, symbol);
					} else {
						return Plotter.ShapedGateGlyph(gate, symbol);
					}
				}
			}
			Pin pin = circuit as Pin;
			if(pin != null) {
				if(pin.PinType == PinType.Input) {
					return Plotter.InputPinGlyph(pin, symbol);
				} else if(pin.PinType == PinType.Output) {
					return Plotter.OutputPinGlyph(pin, symbol);
				} else {
					Tracer.Fail();
				}
			}
			Constant constant = circuit as Constant;
			if(constant != null) {
				return Plotter.ConstantGlyph(constant, symbol);
			}
			CircuitButton button = circuit as CircuitButton;
			if(button != null) {
				return Plotter.ButtonGlyph(button, symbol);
			}
			Splitter splitter = circuit as Splitter;
			if(splitter != null) {
				return Plotter.SplitterGlyph(splitter, symbol);
			}
			Tracer.Fail();
			return null;
		}

		/*public static Rectangle CreateGlyph(CircuitSymbolMarker marker) {
			return null;
		}

		public static Line CreateGlyph(WireMarker marker) {
			return null;
		}

		public static Rectangle CreateGlyph(WirePointMarker marker) {
			return null;
		}*/

		//---------------------------------------------------------------------

		private static FrameworkElement CircuitSkin(Canvas canvas, string skin) {
			FrameworkElement shape = null;
			using(StringReader stringReader = new StringReader(skin)) {
				using(XmlTextReader xmlReader = new XmlTextReader(stringReader)) {
					shape = (FrameworkElement)XamlReader.Load(xmlReader);
				}
			}
			shape.Width = canvas.Width;
			shape.Height = canvas.Height;
			canvas.Children.Add(shape);
			Panel.SetZIndex(shape, 0);
			return shape;
		}

		private static bool AddJam(Canvas canvas, IList<Jam> list, Action<Jam, TextBlock> position = null) {
			bool hasNotation = false;
			if(0 < list.Count) {
				foreach(Jam jam in list) {
					Ellipse ellipse = new Ellipse();
					ellipse.DataContext = jam;
					ellipse.Width = ellipse.Height = Plotter.PinRadius * 2;
					Canvas.SetLeft(ellipse, Plotter.ScreenPoint(jam.X) - Plotter.PinRadius);
					Canvas.SetTop(ellipse, Plotter.ScreenPoint(jam.Y) - Plotter.PinRadius);
					canvas.Children.Add(ellipse);
					if(jam.Pin.Inverted) {
						ellipse.Fill = Plotter.JamInvertedFill;
						ellipse.Stroke = Plotter.JamStroke;
						ellipse.StrokeThickness = 1;
						Panel.SetZIndex(ellipse, 1);
					} else {
						ellipse.Fill = Plotter.JamDirectFill;
						Panel.SetZIndex(ellipse, -1);
					}
					ellipse.ToolTip = jam.Pin.ToolTip;
					if(!string.IsNullOrEmpty(jam.Pin.Notation)) {
						Tracer.Assert(position != null); // If pin has notation then it should belong to rectangualry rendering circuit.
						TextBlock text = new TextBlock();
						text.Text = jam.Pin.Notation.Substring(0, Math.Min(2, jam.Pin.Notation.Length));
						text.ToolTip = jam.Pin.ToolTip;
						text.FontSize = 8;
						Panel.SetZIndex(text, 1);
						position(jam, text);
						canvas.Children.Add(text);
						hasNotation = true;
					}
				}
			}
			return hasNotation;
		}

		private static FrameworkElement RectangularGlyph(Circuit circuit, CircuitGlyph symbol) {
			Tracer.Assert(symbol != null && symbol.Circuit == circuit);
			Tracer.Assert(circuit is Gate || circuit is LogicalCircuit || circuit is Memory);
			Canvas canvas = new Canvas();
			canvas.Background = Brushes.White;
			canvas.DataContext = symbol;
			canvas.Tag = symbol;
			canvas.Width = Plotter.ScreenPoint(circuit.SymbolWidth);
			canvas.Height = Plotter.ScreenPoint(circuit.SymbolHeight);
			bool ln = Plotter.AddJam(canvas, symbol.Left, (j, t) => { Canvas.SetLeft(t, Plotter.PinRadius); Canvas.SetTop(t, Plotter.ScreenPoint(j.Y) - 2 * Plotter.PinRadius); });
			bool tn = Plotter.AddJam(canvas, symbol.Top, (j, t) => { Canvas.SetLeft(t, Plotter.ScreenPoint(j.X) - Plotter.PinRadius); Canvas.SetTop(t, Plotter.PinRadius); });
			bool rn = Plotter.AddJam(canvas, symbol.Right, (j, t) => { Canvas.SetRight(t, Plotter.PinRadius); Canvas.SetTop(t, Plotter.ScreenPoint(j.Y) - 2 * Plotter.PinRadius); });
			bool bn = Plotter.AddJam(canvas, symbol.Bottom, (j, t) => { Canvas.SetLeft(t, Plotter.ScreenPoint(j.X) - Plotter.PinRadius); Canvas.SetBottom(t, Plotter.PinRadius); });

			FrameworkElement shape = Plotter.CircuitSkin(canvas, SymbolShape.Rectangular);
			TextBlock text = shape.FindName("Notation") as TextBlock;
			if(text != null) {
				text.Margin = new Thickness(ln ? 10 : 5, tn ? 10 : 5, rn ? 10 : 5, bn ? 10 : 5);
				text.Text = circuit.Notation;
			}

			canvas.ToolTip = circuit.ToolTip;

			return canvas;
		}

		private static FrameworkElement ClockGlyph(Gate gate, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement LedGlyph(Gate gate, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement ShapedGateGlyph(Gate gate, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement ProbeGlyph(Gate gate, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement SevenSegmentGlyph(Gate gate, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement SplitterGlyph(Splitter splitter, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement ButtonGlyph(CircuitButton button, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement ConstantGlyph(Constant constant, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement OutputPinGlyph(Pin pin, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}

		private static FrameworkElement InputPinGlyph(Pin pin, CircuitGlyph symbol) {
			throw new NotImplementedException();
		}
	}
}
