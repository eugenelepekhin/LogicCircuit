using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	public interface IDescriptor {
		Circuit Circuit { get; }
		void CreateSymbol(EditorDiagram editor, GridPoint point);
	}
	
	public abstract class CircuitDescriptor<T> : IDescriptor where T:Circuit {
		public T Circuit { get; private set; }
		Circuit IDescriptor.Circuit { get { return this.Circuit; } }
		public CircuitGlyph CircuitGlyph { get; private set; }

		protected abstract T GetCircuitToDrop(CircuitProject circuitProject);

		public void CreateSymbol(EditorDiagram editor, GridPoint point) {
			CircuitProject project = editor.CircuitProject;
			project.InTransaction(() => project.CircuitSymbolSet.Create(this.GetCircuitToDrop(project), editor.Project.LogicalCircuit, point.X, point.Y));
		}

		protected CircuitDescriptor(T circuit) {
			this.Circuit = circuit;
			this.CircuitGlyph = new CircuitDescriptorGlyph(this);
		}

		public void ResetGlyph() {
			this.CircuitGlyph.ResetJams();
			this.CircuitGlyph.Invalidate();
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
		}
	}

	public class GateDescriptor : CircuitDescriptor<Gate> {
		public int InputCount { get; set; }
		public IEnumerable<int> InputCountRange { get; private set; }
		public int InputCountRangeLength { get; private set; }

		public GateDescriptor(Gate gate) : base(gate) {
			switch(gate.GateType) {
			case GateType.Clock:
			case GateType.Not:
			case GateType.Led:
			case GateType.Probe:
			case GateType.TriState:
				this.InputCountRangeLength = 0;
				break;
			case GateType.Or:
			case GateType.And:
			case GateType.Xor:
			case GateType.Odd:
			case GateType.Even:
				this.InputCountRange = GateDescriptor.PinRange(2, Gate.MaxInputCount);
				this.InputCountRangeLength = this.InputCountRange.Count();
				break;
			default:
				Tracer.Fail();
				break;
			}
			this.InputCount = gate.InputCount;
		}

		protected override Gate GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.GateSet.Gate(this.Circuit.GateType, this.InputCount, this.Circuit.InvertedOutput);
		}

		private static List<int> PinRange(int min, int max) {
			List<int> range = new List<int>();
			if(min < max) {
				for(int i = min; i <= max; i++) {
					range.Add(i);
				}
			} else {
				range.Add(min);
			}
			return range;
		}
	}

	public class ButtonDescriptor : CircuitDescriptor<CircuitButton> {
		public string Notation { get; set; }

		public ButtonDescriptor(CircuitProject circuitProject) : base(circuitProject.CircuitButtonSet.Create(string.Empty)) {
			this.Notation = string.Empty;
		}

		protected override CircuitButton GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.CircuitButtonSet.Create(this.Notation);
		}
	}

	public class ConstantDescriptor : CircuitDescriptor<Constant> {
		public int BitWidth { get; set; }
		public string Value { get; set; }

		public IEnumerable<int> BitWidthRange { get; private set; }

		public ConstantDescriptor(CircuitProject circuitProject) : base(circuitProject.ConstantSet.Create(1, 0)) {
			this.BitWidth = 1;
			this.Value = "0";
			this.BitWidthRange = PinDescriptor.BitRange(1);
		}

		protected override Constant GetCircuitToDrop(CircuitProject circuitProject) {
			int value;
			if(!int.TryParse(this.Value, NumberStyles.HexNumber, Resources.Culture, out value)) {
				value = 0;
			}
			return circuitProject.ConstantSet.Create(this.BitWidth, value);
		}
	}

	public class MemoryDescriptor : CircuitDescriptor<Memory> {
		public int AddressBitWidth { get; set; }
		public int DataBitWidth { get; set; }

		public IEnumerable<int> AddressBitWidthRange { get; private set; }
		public IEnumerable<int> DataBitWidthRange { get; private set; }

		public MemoryDescriptor(CircuitProject circuitProject, bool writable) : base(circuitProject.MemorySet.Create(writable, 1, 1)) {
			this.AddressBitWidth = 1;
			this.DataBitWidth = 1;
			this.AddressBitWidthRange = PinDescriptor.AddressBitRange();
			this.DataBitWidthRange = PinDescriptor.BitRange(1);
		}

		protected override Memory GetCircuitToDrop(CircuitProject circuitProject) {
			return circuitProject.MemorySet.Create(this.Circuit.Writable, this.AddressBitWidth, this.DataBitWidth);
		}
	}

	public class PinDescriptor : CircuitDescriptor<Pin> {
		public int BitWidth { get; set; }
		public PinSide PinSide { get; set; }

		public IEnumerable<int> BitWidthRange { get; private set; }
		public IEnumerable<PinSide> PinSideRange { get; set; }

		public PinDescriptor(CircuitProject circuitProject, PinType pinType) : base(
			circuitProject.PinSet.Create(circuitProject.ProjectSet.Project.LogicalCircuit, pinType, 1)
		) {
			this.BitWidth = 1;
			this.PinSide = (pinType == PinType.Input) ? PinSide.Left : PinSide.Right;
			this.BitWidthRange = PinDescriptor.BitRange(1);
			this.PinSideRange = (PinSide[])Enum.GetValues(typeof(PinSide));
		}

		protected override Pin GetCircuitToDrop(CircuitProject circuitProject) {
			Pin pin = circuitProject.PinSet.Create(circuitProject.ProjectSet.Project.LogicalCircuit, this.Circuit.PinType, this.BitWidth);
			pin.PinSide = this.PinSide;
			return pin;
		}

		public static int[] BitRange(int minBitWidth) {
			Tracer.Assert(0 < minBitWidth && minBitWidth < BasePin.MaxBitWidth);
			int[] range = new int[BasePin.MaxBitWidth - minBitWidth + 1];
			for(int i = 0; i < range.Length; i++) {
				range[i] = i + minBitWidth;
			}
			return range;
		}

		public static int[] AddressBitRange() {
			int[] range = new int[Memory.MaxAddressBitWidth];
			for(int i = 0; i < range.Length; i++) {
				range[i] = i + 1;
			}
			return range;
		}
	}

	public class SplitterDescriptor : CircuitDescriptor<Splitter> {
		public int BitWidth { get; set; }
		public int PinCount { get; set; }
		public CircuitRotation CircuitRotation { get; set; }

		public IEnumerable<int> BitWidthRange { get; private set; }
		public IEnumerable<int> PinCountRange { get; private set; }
		public IEnumerable<CircuitRotation> CircuitRotationRange { get; private set; }

		public SplitterDescriptor(CircuitProject circuitProject) : base(circuitProject.SplitterSet.Create(3, 3, CircuitRotation.Left)) {
			this.BitWidth = 3;
			this.PinCount = 3;
			this.CircuitRotation = LogicCircuit.CircuitRotation.Left;
			this.BitWidthRange = PinDescriptor.BitRange(2);

			int[] pinRange = new int[Gate.MaxInputCount - 2];
			for(int i = 0; i < pinRange.Length; i++) {
				pinRange[i] = i + 2;
			}
			this.PinCountRange = pinRange;

			this.CircuitRotationRange = (CircuitRotation[])Enum.GetValues(typeof(CircuitRotation));
		}

		protected override Splitter GetCircuitToDrop(CircuitProject circuitProject) {
			if(this.BitWidth < this.PinCount) {
				throw new CircuitException(Cause.UserError, Resources.ErrorWrongSplitter);
			}
			return circuitProject.SplitterSet.Create(this.BitWidth, this.PinCount, this.CircuitRotation);
		}
	}

	public class LogicalCircuitDescriptor : CircuitDescriptor<LogicalCircuit>, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public LogicalCircuitDescriptor(LogicalCircuit logicalCircuit) : base(logicalCircuit) {
		}

		public bool IsCurrent { get { return this.Circuit == this.Circuit.CircuitProject.ProjectSet.Project.LogicalCircuit; } }

		protected override LogicalCircuit GetCircuitToDrop(CircuitProject circuitProject) {
			return this.Circuit;
		}

		public void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public void NotifyCurrentChanged() {
			this.NotifyPropertyChanged("IsCurrent");
		}
	}

	public class TextNoteDescriptor : IDescriptor {

		public Circuit Circuit { get; private set; }

		public TextNoteDescriptor(CircuitProject circuitProject) {
			// create dummy circuit to provide category and name for sorting and displaying in list of circuits descriptors
			LogicalCircuit circuit = circuitProject.LogicalCircuitSet.Create();
			circuit.Category = Resources.TextNotation;
			circuit.Name = Resources.TextNotation;
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
