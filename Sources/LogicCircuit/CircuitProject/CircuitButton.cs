using System;
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

		public override bool IsSmallSymbol { get { return true; } }

		public override string Name {
			get { return Resources.NameButton; }
			set { throw new NotSupportedException(); }
		}

		public override string ToolTip { get { return Resources.ToolTipButton(this.Notation); } }

		public override string Category {
			get { return Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.CircuitButtonSet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateButtonGlyph();
		}

		partial void OnCircuitButtonChanged() {
			int count = 0;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				Tracer.Assert(count++ == 0, "Only one symbol expected");
				ButtonControl button = (ButtonControl)symbol.ProbeView;
				button.Content = this.Notation;
				symbol.Glyph.ToolTip = this.ToolTip;
			}
		}
	}

	public partial class CircuitButtonSet {
		public void Load(XmlNodeList list) {
			CircuitButtonData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private CircuitButton Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, CircuitButtonData.CircuitButtonIdField.Field)
			};
			CircuitButton button = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CircuitProject.DevicePinSet.Create(button, PinType.Output, 1);
			return button;
		}

		public CircuitButton Create(string notation) {
			CircuitButton button = this.CreateItem(Guid.NewGuid(), notation);
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
	}
}
