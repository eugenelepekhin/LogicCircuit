using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogCircuit.xaml
	/// </summary>
	public partial class DialogCircuit : Window {
		public const double MaxShapeDescriptorHeight = 4 * Symbol.GridSize; // 4 - Height of standard symbol with up to 3 pins on left or right.
		public const double MaxShapeDescriptorWidth = DialogCircuit.MaxShapeDescriptorHeight * 3;
		public const double MinShapeDescriptorWidth = 3 * Symbol.GridSize; // 3 - Width of standard symbol with up to 2 pins on top or bottom.

		public class ShapeDescriptor : EnumDescriptor<CircuitShape> {

			private Func<FrameworkElement> createGliph;

			public FrameworkElement Gliph => this.createGliph();

			public ShapeDescriptor(CircuitShape shape, string text, Func<FrameworkElement> createGliph) : base(shape, text) {
				this.createGliph = createGliph;
			}

			private static FrameworkElement Resize(FrameworkElement element) {
				if(DialogCircuit.MaxShapeDescriptorHeight < element.Height || DialogCircuit.MaxShapeDescriptorWidth < element.Width) {
					double x = DialogCircuit.MaxShapeDescriptorWidth / element.Width;
					double y = DialogCircuit.MaxShapeDescriptorHeight / element.Height;
					double zoom = Math.Min(x, y);
					element.LayoutTransform = new ScaleTransform(zoom, zoom);
				}
				return element;
			}

			private static FrameworkElement CreateGlyph(LogicalCircuit logicalCircuit, bool invertIsDisplay, Func<CircuitGlyph, FrameworkElement> create) {
				try {
					logicalCircuit.InvertIsDisplay = invertIsDisplay;
					logicalCircuit.ResetPins();
					CircuitGlyph circuitGlyph = new LogicalCircuitDescriptor(logicalCircuit, s => false).CircuitGlyph;
					return ShapeDescriptor.Resize(create(circuitGlyph));
				} finally {
					logicalCircuit.InvertIsDisplay = false;
					logicalCircuit.ResetPins();
					logicalCircuit.CircuitProject.CircuitSymbolSet.ValidateAll();
				}
			}

			public static FrameworkElement RectangularGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, logicalCircuit.IsDisplay, glyph => glyph.CreateRectangularGlyph());
			public static FrameworkElement DisplayGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, !logicalCircuit.IsDisplay, glyph => glyph.CreateDisplayGlyph(glyph));
			public static FrameworkElement MuxGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, logicalCircuit.IsDisplay, glyph => glyph.CreateMuxGlyph());
			public static FrameworkElement DemuxGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, logicalCircuit.IsDisplay, glyph => glyph.CreateDemuxGlyph());
			public static FrameworkElement AluGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, logicalCircuit.IsDisplay, glyph => glyph.CreateAluGlyph());
			public static FrameworkElement FlipFlopGlyph(LogicalCircuit logicalCircuit) => ShapeDescriptor.CreateGlyph(logicalCircuit, logicalCircuit.IsDisplay, glyph => glyph.CreateFlipFlopGlyph());
		}

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private readonly LogicalCircuit logicalCircuit;

		public IEnumerable<ShapeDescriptor> CircuitShapes { get; }

		public DialogCircuit(LogicalCircuit logicalCircuit) {
			List<ShapeDescriptor> shapes = new List<ShapeDescriptor>() {
				new ShapeDescriptor(CircuitShape.Rectangular, Properties.Resources.GateShapeRectangular, () => ShapeDescriptor.RectangularGlyph(logicalCircuit)),
				new ShapeDescriptor(CircuitShape.Display, Properties.Resources.SymbolShapeDisplay, () => ShapeDescriptor.DisplayGlyph(logicalCircuit)),
				new ShapeDescriptor(CircuitShape.Mux, Properties.Resources.SymbolShapeMux, () => ShapeDescriptor.MuxGlyph(logicalCircuit)),
				new ShapeDescriptor(CircuitShape.Demux, Properties.Resources.SymbolShapeDemux, () => ShapeDescriptor.DemuxGlyph(logicalCircuit)),
				new ShapeDescriptor(CircuitShape.Alu, Properties.Resources.SymbolShapeAlu, () => ShapeDescriptor.AluGlyph(logicalCircuit)),
				new ShapeDescriptor(CircuitShape.FlipFlop, Properties.Resources.SymbolShapeFlipFlop, () => ShapeDescriptor.FlipFlopGlyph(logicalCircuit)),
			};

			
			bool canBeDisplay = false;
			try {
				logicalCircuit.InvertIsDisplay = !logicalCircuit.IsDisplay;
				canBeDisplay = logicalCircuit.ContainsDisplays();
			} finally {
				logicalCircuit.InvertIsDisplay = false;
			}
			if(!canBeDisplay) {
				shapes.RemoveAt(1);
			}
			this.CircuitShapes = shapes.AsReadOnly();

			this.DataContext = this;
			this.InitializeComponent();
			this.logicalCircuit = logicalCircuit;
			this.name.Text = this.logicalCircuit.Name;
			this.notation.Text = this.logicalCircuit.Notation;

			HashSet<string> set = new HashSet<string>(this.logicalCircuit.CircuitProject.LogicalCircuitSet.Select(c => c.Category)) {
				string.Empty
			};
			foreach(string s in set.OrderBy(s => s)) {
				this.category.Items.Add(s);
			}

			this.category.Text = this.logicalCircuit.Category;
			this.shapes.SelectedItem = this.CircuitShapes.FirstOrDefault(d => d.Value == this.logicalCircuit.CircuitShape) ?? this.CircuitShapes.First();
			this.description.Text = this.logicalCircuit.Note;

			IEnumerable<Pin> pins(PinSide pinSide) => this.logicalCircuit.Pins.Where(pin => pin.PinSide == pinSide).Select(pin => (Pin)pin);
			this.leftPins.SetPins(pins(PinSide.Left));
			this.rightPins.SetPins(pins(PinSide.Right));
			this.topPins.SetPins(pins(PinSide.Top));
			this.bottomPins.SetPins(pins(PinSide.Bottom));

			bool isFixed = this.IsOrderFixed();
			if(isFixed) {
				this.leftPins.FixOrder();
				this.rightPins.FixOrder();
				this.topPins.FixOrder();
				this.bottomPins.FixOrder();
			}

			this.checkBoxGraphOrder.IsChecked = !isFixed;

			this.Loaded += new RoutedEventHandler(this.DialogCircuitLoaded);
		}

		private void DialogCircuitLoaded(object sender, RoutedEventArgs e) {
			ControlTemplate template = this.category.Template;
			if(template != null) {
				if(template.FindName("PART_EditableTextBox", this.category) is TextBox textBox) {
					SpellCheck spellCheck = textBox.SpellCheck;
					if(spellCheck != null) {
						spellCheck.IsEnabled = true;
					}
				}
			}
		}

		private void CheckBoxGraphOrderClick(object sender, RoutedEventArgs e) {
			try {
				if(!this.checkBoxGraphOrder.IsChecked.Value) {
					this.leftPins.FixOrder();
					this.rightPins.FixOrder();
					this.topPins.FixOrder();
					this.bottomPins.FixOrder();
				} else {
					this.leftPins.Reset();
					this.rightPins.Reset();
					this.topPins.Reset();
					this.bottomPins.Reset();
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private bool IsOrderFixed() => this.leftPins.IsOrderFixed() || this.rightPins.IsOrderFixed() || this.topPins.IsOrderFixed() || this.bottomPins.IsOrderFixed();

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string notation = this.notation.Text.Trim();
				string category = this.category.Text.Trim();
				category = category.Substring(0, Math.Min(category.Length, 64)).Trim();
				ShapeDescriptor shape = (ShapeDescriptor)this.shapes.SelectedItem;
				string description = this.description.Text.Trim();
				bool leftChanged = this.leftPins.HasChanges();
				bool rightChanged = this.rightPins.HasChanges();
				bool topChanged = this.topPins.HasChanges();
				bool bottomChanged = this.bottomPins.HasChanges();

				if(this.logicalCircuit.Name != name || this.logicalCircuit.Notation != notation ||
					this.logicalCircuit.Category != category || this.logicalCircuit.CircuitShape != shape.Value || this.logicalCircuit.Note != description ||
					leftChanged || rightChanged || topChanged || bottomChanged
				) {
					this.logicalCircuit.CircuitProject.InTransaction(() => {
						this.logicalCircuit.Rename(name);
						this.logicalCircuit.Notation = notation;
						this.logicalCircuit.Category = category;
						this.logicalCircuit.CircuitShape = shape.Value;
						this.logicalCircuit.Note = description;
						this.logicalCircuit.CircuitProject.CollapsedCategorySet.Purge();
						if(leftChanged) this.leftPins.Update();
						if(rightChanged) this.rightPins.Update();
						if(topChanged) this.topPins.Update();
						if(bottomChanged) this.bottomPins.Update();
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
