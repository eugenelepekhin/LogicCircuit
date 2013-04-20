using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
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

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateButtonGlyph(symbol);
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateButtonGlyph(mainSymbol);
		}

		partial void OnCircuitButtonChanged() {
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
			this.CircuitProject.DevicePinSet.Create(button, PinType.Output, 1);
			return button;
		}

		public CircuitButton Create(string notation, bool isToggle) {
			CircuitButton button = this.CreateItem(Guid.NewGuid(), notation, isToggle, CircuitButtonData.NoteField.Field.DefaultValue);
			this.CircuitProject.DevicePinSet.Create(button, PinType.Output, 1);
			return button;
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
