using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Sensor {
		public static IList<SensorPoint> ParseSeries(string data, int bitWidth) {
			Tracer.Assert(data != null);
			List<SensorPoint> list = new List<SensorPoint>();
			int lastTick = -1;
			foreach(string item in data.Split(' ')) {
				if(!string.IsNullOrWhiteSpace(item)) {
					int tick, value;
					string[] parts = item.Split(':');
					if(	parts == null || parts.Length != 2 ||
						!int.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out tick) ||
						!int.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value) ||
						tick <= lastTick
					) {
						throw new ArgumentException();
					}
					lastTick = tick;
					list.Add(new SensorPoint(tick, Constant.Normalize(value, bitWidth)));
				}
			}
			return list;
		}

		public static string SaveSeries(IList<SensorPoint> list) {
			StringBuilder text = new StringBuilder();
			foreach(SensorPoint point in list) {
				if(0 < text.Length) {
					text.Append(" ");
				}
				text.AppendFormat(CultureInfo.InvariantCulture, "{0:X}:{1:X}", point.Tick, point.Value);
			}
			return text.ToString();
		}

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

		public static string UnknownValue { get { return Properties.Resources.CircuitProbeNotation; } } // Show ? on power off.

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.SensorSet.Copy(this);
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			return base.CircuitSymbolWidth(4);
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			return base.CircuitSymbolHeight(3);
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
			return symbol.CreateSensorGlyph(skin);
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
