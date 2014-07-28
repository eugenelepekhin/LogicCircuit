using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogSensor.xaml
	/// </summary>
	public partial class DialogSensor : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Sensor sensor;

		public DialogSensor(Sensor sensor) {
			this.sensor = sensor;
			this.DataContext = this;
			this.InitializeComponent();

			this.sensorType.ItemsSource = SensorDescriptor.SensorTypes;
			SensorType type = this.sensor.SensorType;
			this.loop.IsChecked = (type != SensorType.Series);
			if(type == SensorType.Loop) {
				type = SensorType.Series;
			}
			this.sensorType.SelectedItem = SensorDescriptor.SensorTypes.First(t => t.Value == type);
			this.bitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			this.bitWidth.SelectedItem = this.sensor.BitWidth;
			this.side.ItemsSource = PinDescriptor.PinSideRange;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.sensor.PinSide);
			this.notation.Text = this.sensor.Notation;
			this.data.Text = (type == SensorType.Series) ? this.sensor.Data : Sensor.DefaultSeriesData;
			this.note.Text = this.sensor.Note;
			int min = Sensor.DefaultRandomMinInterval;
			int max = Sensor.DefaultRandomMaxInterval;
			if(type == SensorType.Random) {
				IList<SensorPoint> list = Sensor.ParseSeries(this.sensor.Data, 32);
				if(0 < list.Count) {
					min = list[0].Tick;
					max = list[0].Value;
				}
			}
			this.minTicks.Text = min.ToString(Properties.Resources.Culture);
			this.maxTicks.Text = max.ToString(Properties.Resources.Culture);
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				SensorType type = ((EnumDescriptor<SensorType>)this.sensorType.SelectedItem).Value;
				int bitWidth = (int)this.bitWidth.SelectedItem;
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				string notation = this.notation.Text.Trim();
				string note = this.note.Text.Trim();
				string data = this.data.Text.Trim();
				string minText = this.minTicks.Text.Trim();
				string maxText = this.maxTicks.Text.Trim();
				if(type == SensorType.Series && this.loop.IsChecked.HasValue && this.loop.IsChecked.Value) {
					type = SensorType.Loop;
				} else if(type == SensorType.Random) {
					int min, max;
					if(	int.TryParse(minText, NumberStyles.Integer, Properties.Resources.Culture, out min) &&
						int.TryParse(maxText, NumberStyles.Integer, Properties.Resources.Culture, out max)
					) {
						data = Sensor.SaveSeries(new List<SensorPoint>() { new SensorPoint(min, max) });
					} else {
						data = Sensor.DefaultRandomData;
					}
				} else if(type == SensorType.Manual) {
					data = string.Empty;
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
