using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogSensor.xaml
	/// </summary>
	public partial class DialogSensor : Window, IDataErrorInfo {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Sensor sensor;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<EnumDescriptor<SensorType>> SensorTypes { get; private set; }
		public EnumDescriptor<SensorType> SelectedSensorType { get; set; }

		public bool IsLoop { get; set; }

		public string SeriesData { get; set; }
		public string RandomMin { get; set; }
		public string RandomMax { get; set; }
		public string ManualInitialValue { get; set; }

		public string Error { get { return null; } }

		private int parsedMin = -1, parsedMax = -1;
		private string dataError = null;

		public string this[string columnName] {
			get {
				int old;
				string error = null;
				switch(columnName) {
				case "SeriesData":
					this.dataError = Sensor.ParseError(this.SeriesData);
					error = this.dataError;
					break;
				case "RandomMin":
					old = this.parsedMin;
					if(!int.TryParse(this.RandomMin.Trim(), NumberStyles.None, Properties.Resources.Culture, out this.parsedMin) || this.parsedMin < 1) {
						this.parsedMin = -1;
						error = Properties.Resources.ErrorBadPositiveNumber;
						break;
					}
					if(0 < this.parsedMax && this.parsedMax < this.parsedMin) {
						error = Properties.Resources.ErrorBadInterval;
						break;
					} else if(this.parsedMin < old) {
						this.maxTicks.GetBindingExpression(TextBox.TextProperty).UpdateSource();
					}
					break;
				case "RandomMax":
					old = this.parsedMax;
					if(!int.TryParse(this.RandomMax.Trim(), NumberStyles.None, Properties.Resources.Culture, out this.parsedMax)) {
						this.parsedMax = -1;
						error = Properties.Resources.ErrorBadPositiveNumber;
						break;
					}
					if(0 < this.parsedMin && this.parsedMax < this.parsedMin) {
						error = Properties.Resources.ErrorBadInterval;
						break;
					} else if(old < this.parsedMax) {
						this.minTicks.GetBindingExpression(TextBox.TextProperty).UpdateSource();
					}
					break;
				case "ManualInitialValue":
					if(!int.TryParse(this.ManualInitialValue.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out old)) {
						error = Properties.Resources.ErrorBadHexNumber;
					}
					break;
				}
				if(this.buttonOk != null) {
					switch(this.SelectedSensorType.Value) {
					case SensorType.Series:
						this.buttonOk.IsEnabled = string.IsNullOrWhiteSpace(this.dataError);
						break;
					case SensorType.Random:
						this.buttonOk.IsEnabled = (0 < this.parsedMin && this.parsedMin <= this.parsedMax);
						break;
					case SensorType.Manual:
						this.buttonOk.IsEnabled = string.IsNullOrEmpty(error);
						break;
					default:
						this.buttonOk.IsEnabled = true;
						break;
					}
				}
				return error;
			}
		}

		public DialogSensor(Sensor sensor) {
			this.sensor = sensor;

			this.SensorTypes = SensorDescriptor.SensorTypes;
			SensorType type = this.sensor.SensorType;
			this.IsLoop = (type != SensorType.Series);
			if(type == SensorType.Loop) {
				type = SensorType.Series;
			}
			this.SelectedSensorType = this.SensorTypes.First(t => t.Value == type);

			this.SeriesData = (type == SensorType.Series) ? this.sensor.Data : Sensor.DefaultSeriesData;

			int min = Sensor.DefaultRandomMinInterval;
			int max = Sensor.DefaultRandomMaxInterval;
			if(type == SensorType.Random) {
				SensorPoint point;
				if(Sensor.TryParsePoint(this.sensor.Data, 32, out point)) {
					min = point.Tick;
					max = point.Value;
				}
			}
			this.RandomMin = min.ToString(Properties.Resources.Culture);
			this.RandomMax = max.ToString(Properties.Resources.Culture);

			if(type == SensorType.Manual) {
				string text = this.sensor.Data;
				int value;
				if(string.IsNullOrEmpty(text) || !int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
					value = 0;
				}
				this.ManualInitialValue = Constant.Normalize(value, this.sensor.BitWidth).ToString("X", CultureInfo.InvariantCulture);
			} else {
				this.ManualInitialValue = "0";
			}

			this.DataContext = this;
			this.InitializeComponent();

			this.bitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			this.bitWidth.SelectedItem = this.sensor.BitWidth;
			this.side.ItemsSource = PinDescriptor.PinSideRange;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.sensor.PinSide);
			this.notation.Text = this.sensor.Notation;
			this.note.Text = this.sensor.Note;

		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			// All the logic of this method assumes that validation prevents this been called if there is an incorrect user input.
			try {
				int bitWidth = (int)this.bitWidth.SelectedItem;
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				string notation = this.notation.Text.Trim();
				string note = this.note.Text.Trim();
				string data = this.SeriesData.Trim();
				string minText = this.RandomMin.Trim();
				string maxText = this.RandomMax.Trim();
				string initial = this.ManualInitialValue.Trim();

				SensorType type = this.SelectedSensorType.Value;
				if(type == SensorType.Series && string.IsNullOrWhiteSpace(data)) {
					data = Sensor.DefaultSeriesData;
				}
				if(type == SensorType.Series && this.IsLoop) {
					type = SensorType.Loop;
				} else if(type == SensorType.Random) {
					int min, max;
					if(	int.TryParse(minText, NumberStyles.Integer, Properties.Resources.Culture, out min) &&
						int.TryParse(maxText, NumberStyles.Integer, Properties.Resources.Culture, out max) &&
						0 < min && min <= max
					) {
						data = Sensor.SaveSeries(new List<SensorPoint>() { new SensorPoint(min, max) });
					} else {
						data = Sensor.DefaultRandomData;
					}
				} else if(type == SensorType.Manual) {
					int value;
					if(!int.TryParse(initial, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
						value = 0;
					}
					data = Constant.Normalize(value, bitWidth).ToString("X", CultureInfo.InvariantCulture);
				}

				if(	this.sensor.SensorType != type ||
					this.sensor.BitWidth != bitWidth ||
					this.sensor.PinSide != pinSide ||
					this.sensor.Notation != notation ||
					this.sensor.Note != note ||
					this.sensor.Data != data
				) {
					this.sensor.CircuitProject.InTransaction(() => {
						this.sensor.SensorType = type;
						this.sensor.BitWidth = bitWidth;
						this.sensor.PinSide = pinSide;
						this.sensor.Notation = notation;
						this.sensor.Note = note;
						this.sensor.Data = data;
						this.sensor.Pins.First().PinSide = pinSide;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
