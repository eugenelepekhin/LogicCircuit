using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace LogicCircuit {
	public abstract class Symbol : INotifyPropertyChanged {

		public const int PinRadius = 3;
		public const int GridSize = Symbol.PinRadius * 6;

		public const int LogicalCircuitGridWidth = 170;
		public const int LogicalCircuitGridHeight = 170;
		public static double LogicalCircuitWidth { get { return Symbol.ScreenPoint(Symbol.LogicalCircuitGridWidth); } }
		public static double LogicalCircuitHeight { get { return Symbol.ScreenPoint(Symbol.LogicalCircuitGridHeight); } }
		public static Rect LogicalCircuitBackgroundTile { get { return new Rect(0, 0, Symbol.GridSize, Symbol.GridSize); } }

		public static Brush CircuitFill { get { return Brushes.White; } }
		public static Brush WireStroke { get { return Brushes.Black; } }
		public static Brush JamDirectFill { get { return Brushes.Black; } }
		public static Brush JamInvertedFill { get { return Brushes.White; } }
		public static Brush JamStroke { get { return Brushes.Black; } }

		public static T Skin<T>(string skinPath) where T:FrameworkElement {
			return (T)Application.LoadComponent(new Uri(skinPath, UriKind.Relative));
		}

		public static FrameworkElement Skin(string skinPath) {
			return Symbol.Skin<FrameworkElement>(skinPath);
		}

		public static double ScreenPoint(int xGrid) {
			return (double)xGrid * Symbol.GridSize;
		}

		public static Point ScreenPoint(int xGrid, int yGrid) {
			return new Point((double)xGrid * Symbol.GridSize, (double)yGrid * Symbol.GridSize);
		}

		public static Point ScreenPoint(GridPoint point) {
			return Symbol.ScreenPoint(point.X, point.Y);
		}

		public static int GridPoint(double xScreen) {
			return (int)Math.Round(xScreen / Symbol.GridSize);
		}

		public static GridPoint GridPoint(Point screenPoint) {
			return new GridPoint(
				(int)Math.Round(screenPoint.X / Symbol.GridSize),
				(int)Math.Round(screenPoint.Y / Symbol.GridSize)
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
					(Symbol.CrossProductSign(point1, point2, point3) * Symbol.CrossProductSign(point1, point2, point4) <= 0) &&
					(Symbol.CrossProductSign(point3, point4, point1) * Symbol.CrossProductSign(point3, point4, point2) <= 0)
				);
			}
			return false;
		}

		/// <summary>
		/// Checks if line segment (p1, p2) intersects with rectangle r
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
				Symbol.Intersected(point1, point2, r1, r2) ||
				Symbol.Intersected(point1, point2, r2, r3) ||
				Symbol.Intersected(point1, point2, r3, r4) ||
				Symbol.Intersected(point1, point2, r4, r1)
			);
		}

		public static bool IsHorizontal(IRotatable symbol) {
			Rotation rotation = symbol.Rotation;
			return rotation == Rotation.Right || rotation == Rotation.Left;
		}

		public static double Angle(Rotation rotation) {
			switch(rotation) {
			case Rotation.Up:	return 0;
			case Rotation.Right:return 90;
			case Rotation.Down:	return 180;
			case Rotation.Left:	return -90;
			}
			Tracer.Fail("Invalid Rotation");
			return 0;
		}

		public static Point RotationCenter(int width, int height) {
			if((width & 1) == (height & 1)) {
				return new Point(0.5, 0.5);
			}
			return new Point(
				(double)(width / 2) / (double)width,
				(double)(height / 2) / (double)height
			);
		}

		public static Point MarkerRotationCenter(int width, int height) {
			if((width & 1) == (height & 1)) {
				return new Point(0.5, 0.5);
			}
			if((width & 1) == 0) {
				return new Point(
					0.5,
					(Symbol.ScreenPoint(height / 2) + Symbol.PinRadius) / (Symbol.ScreenPoint(height) + 2 * Symbol.PinRadius)
				);
			} else {
				return new Point(
					(Symbol.ScreenPoint(width / 2) + Symbol.PinRadius) / (Symbol.ScreenPoint(width) + 2 * Symbol.PinRadius),
					0.5
				);
			}
		}

		public static Matrix RotationTransform(double rotation, int x, int y, int width, int height) {
			Point origin = Symbol.RotationCenter(width, height);
			Matrix matrix = new Matrix();
			matrix.RotateAt(rotation,
				Symbol.ScreenPoint(x) + Symbol.ScreenPoint(width) * origin.X,
				Symbol.ScreenPoint(y) + Symbol.ScreenPoint(height) * origin.Y
			);
			return matrix;
		}

		public static Matrix RotationTransform(Rotation rotation, int x, int y, int width, int height) {
			return Symbol.RotationTransform(Symbol.Angle(rotation), x, y, width, height);
		}

		public static Rect Transform(Rect rect, Matrix matrix) {
			return new Rect(matrix.Transform(rect.TopLeft), matrix.Transform(rect.BottomRight));
		}

		//---------------------------------------------------------------------

		public event PropertyChangedEventHandler PropertyChanged;

		protected Symbol() {
		}

		protected abstract LogicalCircuit SymbolLogicalCircuit { get; set; }
		private LogicalCircuit logicalCircuit;
		public LogicalCircuit  LogicalCircuit {
			get {
				if(this.logicalCircuit == null) {
					this.logicalCircuit = this.SymbolLogicalCircuit;
				}
				return this.logicalCircuit;
			}
			set {
				Tracer.Assert(this.logicalCircuit == null);
				this.logicalCircuit = this.SymbolLogicalCircuit = value;
			}
		}
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LogicCircuit")]
		public LogicalCircuit CachedLogicCircuit { get { return this.logicalCircuit; } }

		public abstract void Shift(int dx, int dy);
		public abstract int Z { get; }

		public abstract FrameworkElement Glyph { get; }
		public abstract bool HasCreatedGlyph { get; }
		public abstract void PositionGlyph();

		public abstract Rect Bounds();

		public abstract void DeleteSymbol();

		protected void NotifyPropertyChanged(string name) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		protected bool HasListener { get { return this.PropertyChanged != null; } }

		public abstract Symbol CopyTo(LogicalCircuit target);
	}
}
