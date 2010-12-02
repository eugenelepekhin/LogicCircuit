using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LogicCircuit {
	public static class Plotter {
		public const int PinRadius = 3;
		public const int GridSize = Plotter.PinRadius * 6;

		public static int LogicalCircuitWidth { get { return 170; } }
		public static int LogicalCircuitHeight { get { return 170; } }
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
			Tracer.Assert(symbol != null);
			return null;
		}
	}
}
