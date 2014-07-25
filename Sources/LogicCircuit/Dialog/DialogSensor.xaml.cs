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
			this.sensorType.SelectedItem = SensorDescriptor.SensorTypes.First(t => t.Value == this.sensor.SensorType);
			this.bitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			this.bitWidth.SelectedItem = this.sensor.BitWidth;
			this.side.ItemsSource = PinDescriptor.PinSideRange;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.sensor.PinSide);
			this.notation.Text = this.sensor.Notation;
			this.data.Text = this.sensor.Data;
			this.note.Text = this.sensor.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				SensorType sensorType = ((EnumDescriptor<SensorType>)this.sensorType.SelectedItem).Value;
				int bitWidth = (int)this.bitWidth.SelectedItem;
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				string notation = this.notation.Text.Trim();
				string note = this.note.Text.Trim();
				string data = this.data.Text.Trim();

				if(	this.sensor.SensorType != sensorType ||
					this.sensor.BitWidth != bitWidth ||
					this.sensor.PinSide != pinSide ||
					this.sensor.Notation != notation ||
					this.sensor.Note != note ||
					this.sensor.Data != data
				) {
					this.sensor.CircuitProject.InTransaction(() => {
						this.sensor.SensorType = sensorType;
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
