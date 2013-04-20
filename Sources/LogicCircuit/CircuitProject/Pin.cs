using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Pin {
		public LogicalCircuit LogicalCircuit { get { return (LogicalCircuit)this.Circuit; } }

		public override string Notation {
			get {
				switch(this.PinType) {
				case LogicCircuit.PinType.Input: return Properties.Resources.TitlePinInput(this.Name);
				case LogicCircuit.PinType.Output: return Properties.Resources.TitlePinOutput(this.Name);
				default:
					Tracer.Fail();
					return string.Empty;
				}
			}
			set { throw new InvalidOperationException(); }
		}

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.PinSet.Copy(this, target);
		}

		public void Rename(string name) {
			if(PinData.NameField.Field.Compare(this.Name, name) != 0) {
				this.Name = this.CircuitProject.PinSet.UniqueName(name, this.LogicalCircuit);
			}
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateSimpleGlyph(SymbolShape.Pin, symbol);
		}

		partial void OnPinChanged() {
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class PinSet : NamedItemSet {
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

		public Pin Copy(Pin other, Circuit target) {
			PinData data;
			other.CircuitProject.PinSet.Table.GetData(other.PinRowId, out data);
			if(this.FindByPinId(data.PinId) != null) {
				data.PinId = Guid.NewGuid();
			}
			data.CircuitId = target.CircuitId;
			data.Name = this.UniqueName(data.Name, this.CircuitProject.LogicalCircuitSet.FindByLogicalCircuitId(data.CircuitId));
			data.Pin = null;
			return this.Register(this.Table.Insert(ref data));
		}

		partial void NotifyPinSetChanged(TableChange<PinData> change) {
			PinData.CircuitIdField field = PinData.CircuitIdField.Field;
			LogicalCircuit logicalCircuit = this.CircuitProject.LogicalCircuitSet.FindByLogicalCircuitId(
				(change.Action == SnapTableAction.Delete) ? change.GetOldField(field) : change.GetNewField(field)
			);
			Tracer.Assert(change.Action == SnapTableAction.Delete || logicalCircuit != null);
			if(logicalCircuit != null) {
				logicalCircuit.ResetPins();
			}
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<PinData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
