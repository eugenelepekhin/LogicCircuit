using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Sensor {
		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Name {
			get { return Properties.Resources.NameSensor; }
			set { throw new NotSupportedException(); }
		}

		public override string ToolTip { get { return Circuit.BuildToolTip(Properties.Resources.ToolTipSensor(this.Notation), this.Note); } }

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.SensorSet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			string skin = SymbolShape.SensorAuto;
			switch(this.SensorType) {
			case LogicCircuit.SensorType.Series:
			case LogicCircuit.SensorType.Loop:
			case LogicCircuit.SensorType.Random:
				break;
			case LogicCircuit.SensorType.Manual:
				skin = SymbolShape.SensorManual;
				break;
			default:
				Tracer.Fail();
				break;
			}
			return symbol.CreateSimpleGlyph(skin, symbol);
		}

		partial void OnSensorChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class SensorSet {
		private Sensor Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, SensorData.SensorIdField.Field)
			};
			Sensor sensor = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePin(sensor);
			return sensor;
		}

		private void CreateDevicePin(Sensor sensor) {
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(sensor, PinType.Output, sensor.BitWidth);
			pin.PinSide = sensor.PinSide;
		}

		public Sensor Create(SensorType sensorType, int bitWidth, PinSide pinSide, string notation) {
			Sensor sensor = this.CreateItem(Guid.NewGuid(), sensorType, bitWidth, pinSide, notation, SensorData.DataField.Field.DefaultValue, SensorData.NoteField.Field.DefaultValue);
			this.CreateDevicePin(sensor);
			return sensor;
		}

		public Sensor Copy(Sensor other) {
			SensorData data;
			other.CircuitProject.SensorSet.Table.GetData(other.SensorRowId, out data);
			if(this.FindBySensorId(data.SensorId) != null) {
				data.SensorId = Guid.NewGuid();
			}
			data.Sensor = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<SensorData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
