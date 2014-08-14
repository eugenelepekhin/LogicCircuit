using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	public interface IDescriptor {
		Circuit Circuit { get; }
		string Category { get; }
		void CreateSymbol(EditorDiagram editor, GridPoint point);
	}

	public abstract class Descriptor {
		public abstract bool CategoryExpanded { get; set; }

		protected static CircuitProject CircuitProject {
			get {
				Mainframe mainframe = App.Mainframe;
				if(mainframe != null) {
					Editor editor = mainframe.Editor;
					if(editor != null) {
						return editor.CircuitProject;
					}
				}
				return null;
			}
		}
	}

	public abstract class CircuitDescriptor<T> : Descriptor, IDescriptor, INotifyPropertyChanged where T:Circuit {
		public event PropertyChangedEventHandler PropertyChanged;

		public T Circuit { get; private set; }
		Circuit IDescriptor.Circuit { get { return this.Circuit; } }
		public CircuitGlyph CircuitGlyph { get; private set; }

		public string Category { get; set; }

		protected abstract T GetCircuitToDrop(CircuitProject circuitProject);

		public void CreateSymbol(EditorDiagram editor, GridPoint point) {
			CircuitProject project = editor.CircuitProject;
			project.InTransaction(() => project.CircuitSymbolSet.Create(this.GetCircuitToDrop(project), editor.Project.LogicalCircuit, point.X, point.Y));
		}

		protected CircuitDescriptor(T circuit) {
			this.Circuit = circuit;
			this.Category = circuit.Category;
			this.CircuitGlyph = new CircuitDescriptorGlyph(this);
		}

		public void ResetGlyph() {
			this.CircuitGlyph.ResetJams();
			this.CircuitGlyph.Invalidate();
		}

		protected void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected void InTransaction(Action action) {
			CircuitProject project = this.Circuit.CircuitProject;
			if(project.StartTransaction()) {
				try {
					action();
				} catch {
					project.Rollback();
					throw;
				} finally {
					if(project.IsEditor) {
						project.Omit();
					}
				}
			} else {
				Tracer.Fail();
			}
		}

		private class CircuitDescriptorGlyph : CircuitGlyph {
			private readonly CircuitDescriptor<T> circuitDescriptor;

			public CircuitDescriptorGlyph(CircuitDescriptor<T> circuitDescriptor) {
				this.circuitDescriptor = circuitDescriptor;
			}
			protected override Circuit SymbolCircuit { get { return this.circuitDescriptor.Circuit; } }
			protected override LogicalCircuit SymbolLogicalCircuit { get { throw new InvalidOperationException(); } set { throw new InvalidOperationException(); } }
			public override void Shift(int dx, int dy) { throw new InvalidOperationException(); }
			public override GridPoint Point {
				get { return new GridPoint(0, 0); }
				set { throw new InvalidOperationException(); }
			}
			public override int Z { get { return 0; } }

			public override void PositionGlyph() {
				throw new InvalidOperationException();
			}

			public override void DeleteSymbol() {
				throw new InvalidOperationException();
			}

			public override Symbol CopyTo(LogicalCircuit target) {
				throw new InvalidOperationException();
			}

			public override void Invalidate() {
				this.Reset();
				this.NotifyPropertyChanged("Glyph");
			}

			public override Rect Bounds() {
				throw new InvalidOperationException();
			}
		}
	}

	public abstract class PrimitiveCircuitDescriptor<T> : CircuitDescriptor<T> where T:Circuit {
		protected  PrimitiveCircuitDescriptor(T circuit) : base(circuit) {
		}

		public override bool CategoryExpanded {
			get {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					return !circuitProject.ProjectSet.Project.CategoryPrimitivesCollapsed;
				}
				return true;
			}
			set {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					Project project = circuitProject.ProjectSet.Project;
					if(value != (!project.CategoryPrimitivesCollapsed)) {
						circuitProject.InOmitTransaction(() => project.CategoryPrimitivesCollapsed = !value);
					}
				}
			}
		}
	}

	public abstract class IOCircuitDescriptor<T> : CircuitDescriptor<T> where T:Circuit {
		protected  IOCircuitDescriptor(T circuit) : base(circuit) {
		}

		public override bool CategoryExpanded {
			get {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					return !circuitProject.ProjectSet.Project.CategoryInputOutputCollapsed;
				}
				return true;
			}
			set {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					Project project = circuitProject.ProjectSet.Project;
					if(value != (!project.CategoryInputOutputCollapsed)) {
						circuitProject.InOmitTransaction(() => project.CategoryInputOutputCollapsed = !value);
					}
				}
			}
		}

		protected void RefreshGlyph() {
			this.Circuit.ResetPins();
			this.ResetGlyph();
		}
	}

	public class GateDescriptor : PrimitiveCircuitDescriptor<Gate> {
		private static readonly int[] inputCountRange = PinDescriptor.NumberRange(2, Gate.MaxInputCount);
		
		public int InputCount { get; set; }
		public IEnumerable<int> InputCountRange { get; private set; }
		public int InputCountRangeLength { get; private set; }

		public GateDescriptor(Gate gate, string note) : base(gate) {
			switch(gate.GateType) {
			case GateType.Clock:
			case GateType.Not:
			case GateType.Led:
			case GateType.TriState1:
			case GateType.TriState2:
				this.InputCountRangeLength = 0;
				break;
			case GateType.Or:
			case GateType.And:
			case GateType.Xor:
				this.InputCountRange = GateDescriptor.inputCountRange;
				this.InputCountRangeLength = GateDescriptor.inputCountRange.Length;
				break;
			default:
				Tracer.Fail();
				break;
			}
			this.InputCount = gate.InputCount;
			gate.Note = note;
		}

		protected override Gate GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.GateSet.Gate(this.Circuit.GateType, this.InputCount, this.Circuit.InvertedOutput);
		}
	}

	public class ProbeDescriptor : IOCircuitDescriptor<CircuitProbe> {
		public string Name { get; set; }

		public ProbeDescriptor(CircuitProject circuitProject) : base(circuitProject.CircuitProbeSet.Create(null)) {
			this.Name = string.Empty;
			this.Circuit.Note = Properties.Resources.ToolTipDescriptorProbe;
		}

		protected override CircuitProbe GetCircuitToDrop(CircuitProject circuitProject) {
			string name = this.Name;
			this.Name = string.Empty;
			this.NotifyPropertyChanged("Name");
			return circuitProject.CircuitProbeSet.Create(name);
		}
	}

	public class ButtonDescriptor : IOCircuitDescriptor<CircuitButton> {
		public string Notation { get; set; }
		public bool IsToggle {
			get { return this.Circuit.IsToggle; }
			set {
				if(this.IsToggle != value) {
					this.InTransaction(() => {
						this.Circuit.IsToggle = value;
					});
					this.RefreshGlyph();
				}
			}
		}

		private EnumDescriptor<PinSide> pinSide;
		public EnumDescriptor<PinSide> PinSide {
			get { return this.pinSide; }
			set {
				if(this.pinSide != value) {
					this.InTransaction(() => {
						this.Circuit.Pins.First().PinSide = value.Value;
					});
					this.pinSide = value;
					this.RefreshGlyph();
				}
			}
		}

		public ButtonDescriptor(CircuitProject circuitProject) : base(circuitProject.CircuitButtonSet.Create(string.Empty, false, LogicCircuit.PinSide.Right)) {
			this.Notation = string.Empty;
			this.pinSide = PinDescriptor.PinSideDescriptor(LogicCircuit.PinSide.Right);
		}

		protected override CircuitButton GetCircuitToDrop(CircuitProject circuitProject) {
			string notation = (this.Notation ?? string.Empty).Trim();
			if(!string.IsNullOrEmpty(notation)) {
				this.Notation = string.Empty;
				this.NotifyPropertyChanged("Notation");
			}
			return circuitProject.CircuitButtonSet.Create(notation, this.IsToggle, this.PinSide.Value);
		}
	}

	public class ConstantDescriptor : IOCircuitDescriptor<Constant> {
		public int BitWidth { get; set; }
		public string Value { get; set; }

		private EnumDescriptor<PinSide> pinSide;
		public EnumDescriptor<PinSide> PinSide {
			get { return this.pinSide; }
			set {
				if(this.pinSide != value) {
					this.InTransaction(() => {
						this.Circuit.Pins.First().PinSide = value.Value;
					});
					this.pinSide = value;
					this.RefreshGlyph();
				}
			}
		}

		public ConstantDescriptor(CircuitProject circuitProject) : base(circuitProject.ConstantSet.Create(1, 0, LogicCircuit.PinSide.Right)) {
			this.BitWidth = 1;
			this.Value = "0";
			this.pinSide = PinDescriptor.PinSideDescriptor(LogicCircuit.PinSide.Right);
		}

		protected override Constant GetCircuitToDrop(CircuitProject circuitProject) {
			int value;
			if(!int.TryParse(this.Value, NumberStyles.HexNumber, Properties.Resources.Culture, out value)) {
				value = 0;
			}
			return circuitProject.ConstantSet.Create(this.BitWidth, value, this.PinSide.Value);
		}
	}

	public class SensorDescriptor : IOCircuitDescriptor<Sensor> {
		private static readonly IEnumerable<EnumDescriptor<SensorType>> sensorTypes = new EnumDescriptor<SensorType>[] {
			new EnumDescriptor<SensorType>(LogicCircuit.SensorType.Series, Properties.Resources.SensorTypeSeries),
			new EnumDescriptor<SensorType>(LogicCircuit.SensorType.Random, Properties.Resources.SensorTypeRandom),
			new EnumDescriptor<SensorType>(LogicCircuit.SensorType.Manual, Properties.Resources.SensorTypeManual),
		};

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<EnumDescriptor<SensorType>> SensorTypes { get { return sensorTypes; } }

		private EnumDescriptor<SensorType> sensorType;
		public EnumDescriptor<SensorType> SensorType {
			get { return this.sensorType; }
			set {
				if(this.sensorType != value) {
					this.InTransaction(() => {
						this.Circuit.SensorType = value.Value;
					});
					this.sensorType = value;
					this.RefreshGlyph();
				}
			}
		}
		public int BitWidth { get; set; }

		private EnumDescriptor<PinSide> pinSide;
		public EnumDescriptor<PinSide> PinSide {
			get { return this.pinSide; }
			set {
				if(this.pinSide != value) {
					this.InTransaction(() => {
						this.Circuit.Pins.First().PinSide = value.Value;
					});
					this.pinSide = value;
					this.RefreshGlyph();
				}
			}
		}

		public string Notation { get; set; }

		public SensorDescriptor(CircuitProject circuitProject) : base(circuitProject.SensorSet.Create(LogicCircuit.SensorType.Random, 1, LogicCircuit.PinSide.Right, string.Empty)) {
			this.sensorType = SensorDescriptor.sensorTypes.First(t => t.Value == this.Circuit.SensorType);
			this.BitWidth = this.Circuit.BitWidth;
			this.pinSide = PinDescriptor.PinSideDescriptor(this.Circuit.PinSide);
			this.Notation = string.Empty;
			this.Circuit.Note = Properties.Resources.ToolTipDescriptorSensor;
		}

		protected override Sensor GetCircuitToDrop(CircuitProject circuitProject) {
			string notation = (this.Notation ?? string.Empty).Trim();
			if(!string.IsNullOrEmpty(notation)) {
				this.Notation = string.Empty;
				this.NotifyPropertyChanged("Notation");
			}
			SensorType type = this.SensorType.Value;
			if(type == LogicCircuit.SensorType.Series) {
				type = LogicCircuit.SensorType.Loop;
			}
			return circuitProject.SensorSet.Create(type, this.BitWidth, this.PinSide.Value, notation);
		}
	}

	public class SoundDescriptor : IOCircuitDescriptor<Sound> {

		private EnumDescriptor<PinSide> pinSide;
		public EnumDescriptor<PinSide> PinSide {
			get { return this.pinSide; }
			set {
				if(this.pinSide != value) {
					this.InTransaction(() => {
						this.Circuit.Pins.First().PinSide = value.Value;
					});
					this.pinSide = value;
					this.RefreshGlyph();
				}
			}
		}

		public string Notation { get; set; }

		public SoundDescriptor(CircuitProject circuitProject) : base(circuitProject.SoundSet.Create(LogicCircuit.PinSide.Left, string.Empty)) {
			this.pinSide = PinDescriptor.PinSideDescriptor(this.Circuit.PinSide);
			this.Notation = string.Empty;
		}

		protected override Sound GetCircuitToDrop(CircuitProject circuitProject) {
			string notation = (this.Notation ?? string.Empty).Trim();
			if(!string.IsNullOrEmpty(notation)) {
				this.Notation = string.Empty;
				this.NotifyPropertyChanged("Notation");
			}
			return circuitProject.SoundSet.Create(this.PinSide.Value, notation);
		}
	}

	public class MemoryDescriptor : PrimitiveCircuitDescriptor<Memory> {
		private static readonly int[] addressBitWidthRange = PinDescriptor.AddressBitRange();

		public static IEnumerable<int> AddressBitWidthRange { get { return MemoryDescriptor.addressBitWidthRange; } }

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<EnumDescriptor<bool>> WriteOnList {
			get {
				return new EnumDescriptor<bool>[] {
					new EnumDescriptor<bool>(false, Properties.Resources.WriteOn0),
					new EnumDescriptor<bool>(true, Properties.Resources.WriteOn1)
				};
			}
		}

		public int AddressBitWidth { get; set; }
		public int DataBitWidth { get; set; }

		public MemoryDescriptor(CircuitProject circuitProject, bool writable) : base(circuitProject.MemorySet.Create(writable, 1, 1)) {
			this.AddressBitWidth = 1;
			this.DataBitWidth = 1;
		}

		protected override Memory GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.MemorySet.Create(this.Circuit.Writable, this.AddressBitWidth, this.DataBitWidth);
		}
	}

	public class LedMatrixDescriptor : IOCircuitDescriptor<LedMatrix> {
		private static readonly IEnumerable<EnumDescriptor<LedMatrixType>> matrixTypes = new EnumDescriptor<LedMatrixType>[] {
			new EnumDescriptor<LedMatrixType>(LedMatrixType.Individual, Properties.Resources.LedMatrixTypeIndividual),
			new EnumDescriptor<LedMatrixType>(LedMatrixType.Selector, Properties.Resources.LedMatrixTypeSelector)
		};
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<EnumDescriptor<LedMatrixType>> MatrixTypes { get { return matrixTypes; } }

		private static readonly IEnumerable<int> ledRange = PinDescriptor.NumberRange(LedMatrix.MinLedCount, LedMatrix.MaxLedCount);
		public static IEnumerable<int> RowsRange { get { return LedMatrixDescriptor.ledRange; } }
		public static IEnumerable<int> ColumnsRange { get { return LedMatrixDescriptor.ledRange; } }

		public int Rows { get; set; }
		public int Columns { get; set; }
		public EnumDescriptor<LedMatrixType> MatrixType { get; set; }

		public LedMatrixDescriptor(CircuitProject circuitProject) : base(circuitProject.LedMatrixSet.Create(LedMatrixType.Individual, 4, 4)) {
			this.Rows = 4;
			this.Columns = 4;
			this.MatrixType = LedMatrixDescriptor.LedMatrixTypeDescriptor(LedMatrixType.Individual);
		}

		protected override LedMatrix GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.LedMatrixSet.Create(this.MatrixType.Value, this.Rows, this.Columns);
		}

		public static EnumDescriptor<LedMatrixType> LedMatrixTypeDescriptor(LedMatrixType ledMatrixType) {
			Tracer.Assert(EnumHelper.IsValid(ledMatrixType));
			return LedMatrixDescriptor.MatrixTypes.First(d => d.Value == ledMatrixType);
		}
	}

	public class GraphicsArrayDescriptor : IOCircuitDescriptor<GraphicsArray> {
		private static readonly IEnumerable<int> bitsPerPixel = new int[] { 1, 2, 4, 8 };
		public static IEnumerable<int> BitsPerPixelRange { get { return GraphicsArrayDescriptor.bitsPerPixel; } }

		public int BitsPerPixel { get; set; }

		public GraphicsArrayDescriptor(CircuitProject circuitProject) : base(circuitProject.GraphicsArraySet.Create(1, 85, 64)) {
			this.Circuit.Note = Properties.Resources.ToolTipDescriptorGraphicsArray;
			this.BitsPerPixel = this.Circuit.BitsPerPixel;
		}

		protected override GraphicsArray GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.GraphicsArraySet.Create(this.BitsPerPixel, GraphicsArrayData.WidthField.Field.DefaultValue, GraphicsArrayData.HeightField.Field.DefaultValue);
		}
	}

	public class PinDescriptor : IOCircuitDescriptor<Pin> {
		private static readonly EnumDescriptor<PinSide>[] pinSideRange = new EnumDescriptor<PinSide>[] {
			new EnumDescriptor<PinSide>(LogicCircuit.PinSide.Left, Properties.Resources.PinSideLeft),
			new EnumDescriptor<PinSide>(LogicCircuit.PinSide.Top, Properties.Resources.PinSideTop),
			new EnumDescriptor<PinSide>(LogicCircuit.PinSide.Right, Properties.Resources.PinSideRight),
			new EnumDescriptor<PinSide>(LogicCircuit.PinSide.Bottom, Properties.Resources.PinSideBottom)
		};
		private static readonly int[] bitWidthRange = PinDescriptor.NumberRange(1);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<EnumDescriptor<PinSide>> PinSideRange { get { return PinDescriptor.pinSideRange; } }
		public static IEnumerable<int> BitWidthRange { get { return PinDescriptor.bitWidthRange; } }

		public int BitWidth { get; set; }
		public EnumDescriptor<PinSide> PinSide { get; set; }

		public PinDescriptor(CircuitProject circuitProject, PinType pinType) : base(
			circuitProject.PinSet.Create(circuitProject.ProjectSet.Project.LogicalCircuit, pinType, 1)
		) {
			this.BitWidth = 1;
			this.PinSide = PinDescriptor.PinSideDescriptor((pinType == PinType.Input) ? LogicCircuit.PinSide.Left : LogicCircuit.PinSide.Right);
			this.Circuit.Note = Properties.Resources.ToolTipDescriptorPin;
		}

		protected override Pin GetCircuitToDrop(CircuitProject circuitProject) {
			Pin pin = circuitProject.PinSet.Create(circuitProject.ProjectSet.Project.LogicalCircuit, this.Circuit.PinType, this.BitWidth);
			pin.PinSide = this.PinSide.Value;
			return pin;
		}

		public static EnumDescriptor<PinSide> PinSideDescriptor(PinSide pinSide) {
			Tracer.Assert(EnumHelper.IsValid(pinSide));
			return PinDescriptor.PinSideRange.First(d => d.Value == pinSide);
		}

		public static int[] NumberRange(int minBitWidth, int maxBitWidth) {
			Tracer.Assert(0 < minBitWidth && minBitWidth < maxBitWidth);
			int[] range = new int[maxBitWidth - minBitWidth + 1];
			for(int i = 0; i < range.Length; i++) {
				range[i] = i + minBitWidth;
			}
			return range;
		}

		public static int[] NumberRange(int minBitWidth) {
			return PinDescriptor.NumberRange(minBitWidth, BasePin.MaxBitWidth);
		}

		public static int[] AddressBitRange() {
			return PinDescriptor.NumberRange(1, Memory.MaxAddressBitWidth);
		}
	}

	public class SplitterDescriptor : IOCircuitDescriptor<Splitter> {
		public int BitWidth { get; set; }
		public int PinCount { get; set; }
		public DirectionDescriptor Direction { get; set; }

		public IEnumerable<int> BitWidthRange { get; private set; }
		public IEnumerable<int> PinCountRange { get; private set; }
		public IEnumerable<DirectionDescriptor> DirectionRange { get; private set; }

		public SplitterDescriptor(CircuitProject circuitProject) : base(circuitProject.SplitterSet.Create(3, 3, true)) {
			this.PinCountRange = PinDescriptor.NumberRange(2, BasePin.MaxBitWidth);
			this.BitWidthRange = PinDescriptor.NumberRange(1, BasePin.MaxBitWidth / 2);
			this.DirectionRange = new DirectionDescriptor[] {
				new DirectionDescriptor(true, Properties.Resources.SplitterDirectionClockwise, 1),
				new DirectionDescriptor(false, Properties.Resources.SplitterDirectionCounterclockwise, -1)
			};

			this.PinCount = 3;
			this.BitWidth = 1;
			this.Direction = this.DirectionRange.First();
			this.Circuit.Note = Properties.Resources.ToolTipDescriptorSplitter;
		}

		protected override Splitter GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.SplitterSet.Create(this.BitWidth * this.PinCount, this.PinCount, this.Direction.Value);
		}

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class DirectionDescriptor : EnumDescriptor<bool> {
			public double Flip { get; private set; }
			public DirectionDescriptor(bool clockwise, string text, int flip) : base(clockwise, text) {
				Tracer.Assert(flip == 1 || flip == -1);
				this.Flip = flip;
			}
		}
	}

	public class LogicalCircuitDescriptor : CircuitDescriptor<LogicalCircuit> {
		public LogicalCircuitDescriptor(LogicalCircuit logicalCircuit, Predicate<string> isReserved) : base(logicalCircuit) {
			if(isReserved(logicalCircuit.Category)) {
				this.Category = Properties.Resources.CategoryDuplicate(logicalCircuit.Category);
			}
		}

		public bool IsCurrent { get { return this.Circuit == this.Circuit.CircuitProject.ProjectSet.Project.LogicalCircuit; } }

		protected override LogicalCircuit GetCircuitToDrop(CircuitProject circuitProject) {
			return this.Circuit;
		}

		public void NotifyCurrentChanged() {
			this.NotifyPropertyChanged("IsCurrent");
		}

		public override bool CategoryExpanded {
			get {
				CircuitProject circuitProject = this.Circuit.CircuitProject;
				if(circuitProject != null) {
					return !circuitProject.CollapsedCategorySet.IsCollapsed(this.Circuit.Category);
				}
				return true;
			}
			set {
				CircuitProject circuitProject = this.Circuit.CircuitProject;
				if(circuitProject != null) {
					circuitProject.CollapsedCategorySet.SetCollapsed(this.Circuit.Category, !value);
				}
			}
		}
	}

	public class TextNoteDescriptor : Descriptor, IDescriptor {

		public Circuit Circuit { get; private set; }
		public string Category { get { return this.Circuit.Category; } }
		public override bool CategoryExpanded {
			get {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					return !circuitProject.ProjectSet.Project.CategoryTextNoteCollapsed;
				}
				return true;
			}
			set {
				CircuitProject circuitProject = Descriptor.CircuitProject;
				if(circuitProject != null) {
					Project project = circuitProject.ProjectSet.Project;
					if(value != (!project.CategoryTextNoteCollapsed)) {
						circuitProject.InOmitTransaction(() => project.CategoryTextNoteCollapsed = !value);
					}
				}
			}
		}

		public TextNoteDescriptor(CircuitProject circuitProject) {
			// create dummy circuit to provide category and name for sorting and displaying in list of circuits descriptors
			LogicalCircuit circuit = circuitProject.LogicalCircuitSet.Create();
			circuit.Category = Properties.Resources.TextNotation;
			circuit.Name = Properties.Resources.TextNotation;
			this.Circuit = circuit;
		}

		public void CreateSymbol(EditorDiagram editor, GridPoint point) {
			CircuitProject project = editor.CircuitProject;
			DialogText dialog = new DialogText(null);
			bool? result = editor.Mainframe.ShowDialog(dialog);
			if(result.HasValue && result.Value && TextNote.IsValidText(dialog.Document)) {
				project.InTransaction(() => project.TextNoteSet.Create(editor.Project.LogicalCircuit, point, dialog.Document));
			}
		}
	}
}
