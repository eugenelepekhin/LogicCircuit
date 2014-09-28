using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class CircuitButton {
		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Name {
			get { return Properties.Resources.NameButton; }
			set { throw new NotSupportedException(); }
		}

		public override string ToolTip { get { return Circuit.BuildToolTip(Properties.Resources.ToolTipButton(this.Notation), this.Note); } }

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.CircuitButtonSet.Copy(this);
		}

		public override bool IsDisplay {
			get { return true; }
			set { base.IsDisplay = value; }
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			return base.CircuitSymbolWidth(Math.Max(defaultWidth, Math.Min(this.Width, 10)));
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			return base.CircuitSymbolHeight(Math.Max(defaultHeight, Math.Min(this.Height, 10)));
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateButtonGlyph(symbol);
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateButtonGlyph(mainSymbol);
		}

		partial void OnCircuitButtonChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class CircuitButtonSet {
		private CircuitButton Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, CircuitButtonData.CircuitButtonIdField.Field)
			};
			CircuitButton button = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePin(button);
			return button;
		}

		public CircuitButton Create(string notation, bool isToggle, PinSide pinSide) {
			CircuitButton button = this.CreateItem(Guid.NewGuid(), notation, isToggle, pinSide,
				CircuitButtonData.WidthField.Field.DefaultValue,
				CircuitButtonData.HeightField.Field.DefaultValue,
				CircuitButtonData.NoteField.Field.DefaultValue
			);
			this.CreateDevicePin(button);
			return button;
		}

		private void CreateDevicePin(CircuitButton button) {
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(button, PinType.Output, 1);
			pin.PinSide = button.PinSide;
		}

		public CircuitButton Copy(CircuitButton other) {
			CircuitButtonData data;
			other.CircuitProject.CircuitButtonSet.Table.GetData(other.CircuitButtonRowId, out data);
			if(this.FindByCircuitButtonId(data.CircuitButtonId) != null) {
				data.CircuitButtonId = Guid.NewGuid();
			}
			data.CircuitButton = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<CircuitButtonData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
