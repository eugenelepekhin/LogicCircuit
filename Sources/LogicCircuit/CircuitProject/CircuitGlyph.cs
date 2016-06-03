using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LogicCircuit {
	public abstract class CircuitGlyph : Symbol {

		private List<Jam>[] jams;
		private bool isUpdated;

		protected abstract Circuit SymbolCircuit { get; }
		private Circuit circuit;
		public Circuit Circuit {
			get {
				if(this.circuit == null) {
					this.circuit = this.SymbolCircuit;
				}
				return this.circuit;
			}
		}

		public abstract GridPoint Point { get; set; }

		private FrameworkElement glyph = null;

		public override FrameworkElement Glyph {
			get { return this.glyph ?? (this.glyph = this.Circuit.CreateGlyph(this)); }
		}

		public override bool HasCreatedGlyph { get { return this.glyph != null; } }

		public void Reset() {
			this.glyph = null;
		}

		public void GuaranteeGlyph() {
			if(this.glyph == null) {
				this.glyph = this.Circuit.CreateGlyph(this);
			}
		}

		public abstract void Invalidate();

		public FrameworkElement ProbeView { get; set; }

		protected CircuitGlyph() : base() {
		}

		private IList<Jam> Left {
			get {
				this.Update();
				return this.jams[(int)PinSide.Left];
			}
		}
		private IList<Jam> Top {
			get {
				this.Update();
				return this.jams[(int)PinSide.Top];
			}
		}
		private IList<Jam> Right {
			get {
				this.Update();
				return this.jams[(int)PinSide.Right];
			}
		}
		private IList<Jam> Bottom {
			get {
				this.Update();
				return this.jams[(int)PinSide.Bottom];
			}
		}

		public IEnumerable<Jam> Jams() {
			this.Update();
			for(int i = 0; i < this.jams.Length; i++) {
				foreach(Jam jam in this.jams[i]) {
					yield return jam;
				}
			}
		}

		public Jam Jam(BasePin pin) {
			this.Update();
			foreach(List<Jam> list in this.jams) {
				foreach(Jam jam in list) {
					if(jam.Pin == pin) {
						return jam;
					}
				}
			}
			return null;
		}

		public void ResetJams() {
			this.isUpdated = false;
			this.Invalidate();
		}

		private void Update() {
			if(!this.isUpdated) {
				if(this.jams == null) {
					this.jams = new List<Jam>[] { new List<Jam>(), new List<Jam>(), new List<Jam>(), new List<Jam>() };
				} else {
					foreach(List<Jam> j in this.jams) {
						j.Clear();
					}
				}
				IList<Jam> list = this.jams[(int)PinSide.Left];
				foreach(BasePin pin in this.Circuit.Left) {
					list.Add(this.CreateJam(pin));
				}
				list = this.jams[(int)PinSide.Top];
				foreach(BasePin pin in this.Circuit.Top) {
					list.Add(this.CreateJam(pin));
				}
				list = this.jams[(int)PinSide.Right];
				foreach(BasePin pin in this.Circuit.Right) {
					list.Add(this.CreateJam(pin));
				}
				list = this.jams[(int)PinSide.Bottom];
				foreach(BasePin pin in this.Circuit.Bottom) {
					list.Add(this.CreateJam(pin));
				}
				this.isUpdated = true;
			}
		}

		private Jam CreateJam(BasePin pin) {
			if(pin is Pin) {
				LogicalCircuit logicalCircuit = (LogicalCircuit)this.Circuit;
				CircuitSymbol pinSymbol = logicalCircuit.CircuitProject.CircuitSymbolSet.SelectByCircuit(pin).First();
				Jam innerJam = pinSymbol.Jams().First();
				return new LogicalJamItem(pin, this, innerJam);
			}
			return new JamItem(pin, this);
		}

		private static FrameworkElement Skin(Canvas canvas, string skin) {
			FrameworkElement shape = Symbol.Skin(skin);
			shape.Width = canvas.Width;
			shape.Height = canvas.Height;
			canvas.Children.Add(shape);
			Panel.SetZIndex(shape, 0);
			// Use name scope from shape in canvas, so ProbeView can be found from canvas
			NameScope.SetNameScope(canvas, NameScope.GetNameScope(shape));
			return shape;
		}

		private static bool AddJam(Canvas canvas, IEnumerable<Jam> list, Action<Jam, TextBlock> notationPosition) {
			bool hasNotation = false;
			foreach(Jam jam in list) {
				Ellipse ellipse = new Ellipse();
				ellipse.DataContext = jam;
				ellipse.Width = ellipse.Height = Symbol.PinRadius * 2;
				Canvas.SetLeft(ellipse, Symbol.ScreenPoint(jam.X) - Symbol.PinRadius);
				Canvas.SetTop(ellipse, Symbol.ScreenPoint(jam.Y) - Symbol.PinRadius);
				canvas.Children.Add(ellipse);
				if(jam.Pin.Inverted) {
					ellipse.Fill = Symbol.JamInvertedFill;
					ellipse.Stroke = Symbol.JamStroke;
					ellipse.StrokeThickness = 1;
					Panel.SetZIndex(ellipse, 1);
				} else {
					ellipse.Fill = Symbol.JamDirectFill;
					Panel.SetZIndex(ellipse, -1);
				}
				ellipse.ToolTip = jam.Pin.ToolTip;
				string jamNotation = jam.Pin.JamNotation;
				if(!string.IsNullOrEmpty(jamNotation)) {
					Tracer.Assert(notationPosition != null); // If pin has notation then it should belong to rectangular rendering circuit.
					TextBlock text = new TextBlock();
					text.Foreground = Brushes.Black;
					int len = (jam.Pin.PinSide == PinSide.Top || jam.Pin.PinSide == PinSide.Bottom) ? 4 : 2;
					text.Text = jamNotation.Substring(0, Math.Min(len, jamNotation.Length));
					text.ToolTip = jam.Pin.ToolTip;
					text.FontSize = 8;
					Panel.SetZIndex(text, 1);
					notationPosition(jam, text);
					canvas.Children.Add(text);
					hasNotation = true;
				}
			}
			return hasNotation;
		}

		private void InitGlyphCanvas(Canvas canvas, CircuitGlyph mainSymbol) {
			Panel.SetZIndex(canvas, this.Z);
			canvas.DataContext = mainSymbol;
			canvas.Width = Symbol.ScreenPoint(this.Circuit.SymbolWidth);
			canvas.Height = Symbol.ScreenPoint(this.Circuit.SymbolHeight);
			if(this == mainSymbol) {
				canvas.ToolTip = this.Circuit.ToolTip;
			}
			canvas.RenderTransform = new RotateTransform();
		}

		private Canvas CreateGlyphCanvas(CircuitGlyph mainSymbol) {
			Canvas canvas = new Canvas();
			this.InitGlyphCanvas(canvas, mainSymbol);
			return canvas;
		}

		private DisplayCanvas CreateDisplayCanvas(CircuitGlyph mainSymbol) {
			DisplayCanvas canvas = new DisplayCanvas();
			this.InitGlyphCanvas(canvas, mainSymbol);
			return canvas;
		}

		public FrameworkElement CreateButtonGlyph(CircuitGlyph mainSymbol) {
			Tracer.Assert(this.Circuit is CircuitButton);
			Canvas canvas = this.CreateGlyphCanvas(mainSymbol);
			if(this == mainSymbol) {
				CircuitGlyph.AddJam(canvas, this.Jams(), null);
			}
			ButtonControl buttonControl = (ButtonControl)CircuitGlyph.Skin(canvas, SymbolShape.Button);
			Panel.SetZIndex(buttonControl, 0);
			buttonControl.Content = this.Circuit.Notation;
			buttonControl.Width = canvas.Width;
			buttonControl.Height = canvas.Height;
			if(this == mainSymbol) {
				this.ProbeView = buttonControl;
			}
			this.UpdateButtonGlyph(canvas);
			return canvas;
		}

		public void UpdateButtonGlyph(Panel panel) {
			Tracer.Assert(panel != null);
			CircuitButton button = this.Circuit as CircuitButton;
			Tracer.Assert(button != null);
			if(button.IsToggle) {
				if(panel.Children.Count < 3) {
					panel.Children.Add(CircuitGlyph.Skin<Border>(SymbolShape.ToggleLed));
				}
			} else {
				if(2 < panel.Children.Count) {
					UIElement rect = panel.Children[2];
					Tracer.Assert(rect is Border);
					panel.Children.Remove(rect);
				}
			}
		}

		public FrameworkElement CreateSensorGlyph(string skin) {
			Canvas canvas = this.CreateGlyphCanvas(this);
			CircuitGlyph.AddJam(canvas, this.Jams(), null);
			FrameworkElement shape = CircuitGlyph.Skin(canvas, skin);
			FrameworkElement probeView = shape.FindName("ProbeView") as FrameworkElement;
			Tracer.Assert(probeView != null);
			this.ProbeView = probeView;

			TextBlock textBlock = probeView as TextBlock;
			if(textBlock != null) {
				textBlock.Text = Sensor.UnknownValue;
			} else {
				TextBox textBox = (TextBox)probeView;
				textBox.Text = Sensor.UnknownValue;
			}

			TextBlock notation = shape.FindName("Notation") as TextBlock;
			Tracer.Assert(notation != null);
			notation.Text = this.Circuit.Notation;
			return canvas;
		}

		public FrameworkElement CreateSimpleGlyph(string skin, CircuitGlyph mainSymbol) {
			Canvas canvas = this.CreateGlyphCanvas(mainSymbol);
			if(this == mainSymbol) {
				CircuitGlyph.AddJam(canvas, this.Jams(), null);
			}
			FrameworkElement shape = CircuitGlyph.Skin(canvas, skin);
			FrameworkElement probeView = shape.FindName("ProbeView") as FrameworkElement;
			if(probeView != null) {
				if(this == mainSymbol) {
					this.ProbeView = probeView;
				}
				TextBlock textBlock = probeView as TextBlock;
				if(textBlock != null) {
					textBlock.Text = this.Circuit.Notation;
				}
			}
			return canvas;
		}

		public FrameworkElement CreateLedMatrixGlyph(CircuitGlyph mainSymbol) {
			Canvas canvas = this.CreateGlyphCanvas(mainSymbol);
			if(this == mainSymbol) {
				CircuitGlyph.AddJam(canvas, this.Jams(), null);
			}
			FrameworkElement shape = CircuitGlyph.Skin(canvas, SymbolShape.LedMatrix);
			UniformGrid grid = shape.FindName("ProbeView") as UniformGrid;
			Tracer.Assert(grid != null);
			if(this == mainSymbol) {
				this.ProbeView = grid;
			}
			LedMatrix matrix = (LedMatrix)this.Circuit;
			grid.Columns = matrix.Columns;
			grid.Rows = matrix.Rows;
			string skin = (matrix.CellShape == LedMatrixCellShape.Round) ? SymbolShape.LedMatrixRoundCell : SymbolShape.LedMatrixRectCell;
			int cellCount = matrix.Rows * matrix.Columns;
			for(int i = 0; i < cellCount; i++) {
				grid.Children.Add(Symbol.Skin(skin));
			}
			return canvas;
		}

		public FrameworkElement CreateRectangularGlyph() {
			Canvas canvas = this.CreateGlyphCanvas(this);
			canvas.Background = Symbol.CircuitFill;
			bool ln = CircuitGlyph.AddJam(canvas, this.Left, (j, t) => { Canvas.SetLeft(t, Symbol.PinRadius); Canvas.SetTop(t, Symbol.ScreenPoint(j.Y) - 2 * Symbol.PinRadius); });
			bool tn = CircuitGlyph.AddJam(canvas, this.Top, (j, t) => { Canvas.SetLeft(t, Symbol.ScreenPoint(j.X) - Symbol.PinRadius); Canvas.SetTop(t, Symbol.PinRadius); });
			bool rn = CircuitGlyph.AddJam(canvas, this.Right, (j, t) => { Canvas.SetRight(t, Symbol.PinRadius); Canvas.SetTop(t, Symbol.ScreenPoint(j.Y) - 2 * Symbol.PinRadius); });
			bool bn = CircuitGlyph.AddJam(canvas, this.Bottom, (j, t) => { Canvas.SetLeft(t, Symbol.ScreenPoint(j.X) - Symbol.PinRadius); Canvas.SetBottom(t, Symbol.PinRadius); });
			FrameworkElement shape = CircuitGlyph.Skin(canvas, SymbolShape.Rectangular);
			TextBlock text = shape.FindName("Notation") as TextBlock;
			if(text != null) {
				text.Margin = new Thickness(ln ? 10 : 5, tn ? 10 : 5, rn ? 10 : 5, bn ? 10 : 5);
				text.Text = this.Circuit.Notation;
			}
			return canvas;
		}

		public FrameworkElement CreateShapedGlyph(string skin) {
			Gate gate = this.Circuit as Gate;
			Tracer.Assert(gate != null);
			Canvas canvas = this.CreateGlyphCanvas(this);
			CircuitGlyph.AddJam(canvas, this.Jams(), null);
			FrameworkElement shape = CircuitGlyph.Skin(canvas, skin);
			int top = Math.Max(0, gate.InputCount - 3) / 2;
			int bottom = Math.Max(0, gate.InputCount - 3 - top);
			Rectangle topLine = shape.FindName("topLine") as Rectangle;
			if(topLine != null) {
				topLine.Height = Symbol.ScreenPoint(top);
			}
			Rectangle bottomLine = shape.FindName("bottomLine") as Rectangle;
			if(bottomLine != null) {
				bottomLine.Height = Symbol.ScreenPoint(bottom);
			}
			return canvas;
		}

		public FrameworkElement CreateDisplayGlyph(CircuitGlyph mainSymbol) {
			Tracer.Assert(mainSymbol != null);
			List<CircuitSymbol> list = ((LogicalCircuit)this.Circuit).CircuitSymbols().Where(s => s.Circuit.IsValidDisplay()).ToList();
			GridPoint offset = Symbol.GridPoint(list.Select(s => s.Bounds()).Aggregate((r1, r2) => Rect.Union(r1, r2)).TopLeft);
			DisplayCanvas canvas = this.CreateDisplayCanvas(mainSymbol);

			if(this == mainSymbol) {
				Border border = Symbol.Skin<Border>(SymbolShape.DisplayBackground);
				border.Width = canvas.Width;
				border.Height = canvas.Height;
				canvas.Children.Add(border);

				CircuitGlyph.AddJam(canvas, this.Left, (j, t) => { Canvas.SetLeft(t, Symbol.PinRadius); Canvas.SetTop(t, Symbol.ScreenPoint(j.Y) - 2 * Symbol.PinRadius); });
				CircuitGlyph.AddJam(canvas, this.Top, (j, t) => { Canvas.SetLeft(t, Symbol.ScreenPoint(j.X) - Symbol.PinRadius); Canvas.SetTop(t, Symbol.PinRadius); });
				CircuitGlyph.AddJam(canvas, this.Right, (j, t) => { Canvas.SetRight(t, Symbol.PinRadius); Canvas.SetTop(t, Symbol.ScreenPoint(j.Y) - 2 * Symbol.PinRadius); });
				CircuitGlyph.AddJam(canvas, this.Bottom, (j, t) => { Canvas.SetLeft(t, Symbol.ScreenPoint(j.X) - Symbol.PinRadius); Canvas.SetBottom(t, Symbol.PinRadius); });

				border = Symbol.Skin<Border>(SymbolShape.DisplayBorder);
				border.Width = canvas.Width;
				border.Height = canvas.Height;
				Panel.SetZIndex(border, int.MaxValue - 1);
				canvas.Children.Add(border);
			}

			foreach(CircuitSymbol symbol in list) {
				FrameworkElement display = symbol.Circuit.CreateDisplay(symbol, mainSymbol);
				Canvas.SetLeft(display, Symbol.ScreenPoint(symbol.X - offset.X));
				Canvas.SetTop(display, Symbol.ScreenPoint(symbol.Y - offset.Y));
				display.RenderTransformOrigin = Symbol.RotationCenter(symbol.Circuit.SymbolWidth, symbol.Circuit.SymbolHeight);
				RotateTransform rotation = (RotateTransform)display.RenderTransform;
				rotation.Angle = Symbol.Angle(symbol.Rotation);
				canvas.AddDisplay(symbol, display);
			}

			return canvas;
		}

		private class JamItem : Jam {
			public JamItem(BasePin pin, CircuitGlyph symbol) {
				Tracer.Assert(pin.Circuit == symbol.Circuit);
				this.Pin = pin;
				this.CircuitSymbol = symbol;
			}
		}

		private class LogicalJamItem : JamItem {
			private readonly Jam innerJam;
			public override Jam InnerJam { get { return this.innerJam; } }

			public LogicalJamItem(BasePin pin, CircuitGlyph symbol, Jam innerJam) : base(pin, symbol) {
				Tracer.Assert(innerJam != null && innerJam.CircuitSymbol.LogicalCircuit == symbol.Circuit);
				this.innerJam = innerJam;
			}
		}
	}
}
