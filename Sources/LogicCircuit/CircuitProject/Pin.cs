using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Pin {
		public LogicalCircuit LogicalCircuit { get { return (LogicalCircuit)this.Circuit; } }
		
		public override bool IsSmallSymbol { get { return true; } }

		public override string Notation {
			get {
				switch(this.PinType) {
				case LogicCircuit.PinType.Input: return Resources.TitlePinInput(this.Name);
				case LogicCircuit.PinType.Output: return Resources.TitlePinOutput(this.Name);
				default:
					Tracer.Fail();
					return string.Empty;
				}
			}
			set { throw new InvalidOperationException(); }
		}

		public override void Delete() {
			this.OnPinChanged();
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override Circuit CopyTo(CircuitProject project) {
			return project.PinSet.Copy(this);
		}

		public void Rename(string name) {
			if(PinData.NameField.Field.Compare(this.Name, name) != 0) {
				this.Name = this.CircuitProject.PinSet.UniqueName(name, this.LogicalCircuit);
			}
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateSimpleGlyph(SymbolShape.Pin);
		}
	}

	public partial class PinSet : NamedItemSet {
		public void Load(XmlNodeList list) {
			PinData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private Pin Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, PinData.PinIdField.Field)
			};
			Pin pin = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePin(pin);
			return pin;
		}

		private void CreateDevicePin(Pin pin) {
			PinType pinType = pin.PinType;
			if(pinType != PinType.None) {
				DevicePin devicePin = this.CircuitProject.DevicePinSet.Create(
					pin, (pinType == PinType.Input) ? PinType.Output : PinType.Input, pin.BitWidth
				);
				devicePin.Inverted = pin.Inverted;
			}
		}

		protected override bool Exists(string name) {
			throw new NotSupportedException();
		}

		protected override bool Exists(string name, Circuit group) {
			return this.FindByCircuitAndName(group, name) != null;
		}

		public Pin Create(LogicalCircuit logicalCircuit, PinType pinType, int bitWidth) {
			Pin pin = this.CreateItem(Guid.NewGuid(), logicalCircuit, bitWidth, pinType,
				BasePin.DefaultSide(pinType),
				PinData.InvertedField.Field.DefaultValue,
				this.UniqueName(BasePin.DefaultName(pinType), logicalCircuit),
				PinData.NoteField.Field.DefaultValue,
				PinData.JamNotationField.Field.DefaultValue
			);
			this.CreateDevicePin(pin);
			return pin;
		}

		public Pin Copy(Pin other) {
			PinData data;
			other.CircuitProject.PinSet.Table.GetData(other.PinRowId, out data);
			if(this.FindByPinId(data.PinId) != null) {
				data.PinId = Guid.NewGuid();
			}
			data.Name = this.UniqueName(data.Name, this.CircuitProject.LogicalCircuitSet.FindByLogicalCircuitId(data.CircuitId));
			return this.Register(this.Table.Insert(ref data));
		}
	}
}
