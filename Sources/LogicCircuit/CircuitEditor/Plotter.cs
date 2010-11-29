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
			return xGrid * Plotter.GridSize;
		}

		public static Point ScreenPoint(int xGrid, int yGrid) {
			return new Point(xGrid * Plotter.GridSize, yGrid * Plotter.GridSize);
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
		/// <param name="p0">Start point of both vectors</param>
		/// <param name="p1">The end point of first vector</param>
		/// <param name="p2">The end point of second vector</param>
		/// <returns>if +1 then vector p0-p1 is clockwise from p0-p2; if -1, it is counterclockwise</returns>
		public static int CrossProductSign(Point p0, Point p1, Point p2) {
			return Math.Sign((p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y));
		}

		/// <summary>
		/// Checks if two line segments are intersected
		/// </summary>
		/// <param name="p1">First vertex of first line segment</param>
		/// <param name="p2">Second vertex of first line segment</param>
		/// <param name="p3">First vertex of second line segment</param>
		/// <param name="p4">Second vertex of second line segment</param>
		/// <returns>true if line segment (p1, p2) is intersected with line segment (p3, p4)</returns>
		public static bool Intersected(Point p1, Point p2, Point p3, Point p4) {
			//1. Find if bounding rectangles intersected
			double x1 = Math.Min(p1.X, p2.X);
			double y1 = Math.Min(p1.Y, p2.Y);
			double x2 = Math.Max(p1.X, p2.X);
			double y2 = Math.Max(p1.Y, p2.Y);

			double x3 = Math.Min(p3.X, p4.X);
			double y3 = Math.Min(p3.Y, p4.Y);
			double x4 = Math.Max(p3.X, p4.X);
			double y4 = Math.Max(p3.Y, p4.Y);
			if((x2 >= x3) && (x4 >= x1) && (y2 >= y3) && (y4 >= y1)) {
				//2. now, two line segments intersected if each straddles the line containing the other
				// let's use CrossRoductSign for this purpose.
				return(
					(Plotter.CrossProductSign(p1, p2, p3) * Plotter.CrossProductSign(p1, p2, p4) <= 0) &&
					(Plotter.CrossProductSign(p3, p4, p1) * Plotter.CrossProductSign(p3, p4, p2) <= 0)
				);
			}
			return false;
		}

		/// <summary>
		/// Checks if line segment (p1, p2) intersets with rectangle r
		/// </summary>
		/// <param name="p1">First vertex of the line segment</param>
		/// <param name="p2">Second vertex of the line segment</param>
		/// <param name="r">Rectangle</param>
		/// <returns>true if line segment and rectangle have intersection</returns>
		public static bool Intersected(Point p1, Point p2, Rect r) {
			if(r.Contains(p1) || r.Contains(p2)) {
				return true;
			}
			Point r1 = new Point(r.X, r.Y);
			Point r2 = new Point(r.X + r.Width, r.Y);
			Point r3 = new Point(r.X + r.Width, r.Y + r.Height);
			Point r4 = new Point(r.X, r.Y + r.Height);
			return(
				Plotter.Intersected(p1, p2, r1, r2) ||
				Plotter.Intersected(p1, p2, r2, r3) ||
				Plotter.Intersected(p1, p2, r3, r4) ||
				Plotter.Intersected(p1, p2, r4, r1)
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
			return null;
		}
	}
}
