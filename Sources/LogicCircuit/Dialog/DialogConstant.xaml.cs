using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogConstant.xaml
	/// </summary>
	public partial class DialogConstant : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private Constant constant;

		public DialogConstant(Constant constant) {
			this.DataContext = this;
			this.constant = constant;
			this.InitializeComponent();
			this.value.Text = constant.ConstantValue.ToString("X", CultureInfo.InvariantCulture);
			this.bitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			this.bitWidth.SelectedItem = this.constant.BitWidth;
			this.side.ItemsSource = PinDescriptor.PinSideRange;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.constant.PinSide);
			this.note.Text = constant.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				int bitWidth = (int)this.bitWidth.SelectedItem;
				int value = Constant.Normalize(int.Parse(this.value.Text.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture), bitWidth);
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				string note = this.note.Text.Trim();

				if(this.constant.BitWidth != bitWidth || this.constant.ConstantValue != value || this.constant.PinSide != pinSide || this.constant.Note != note) {
					this.constant.CircuitProject.InTransaction(() => {
						this.constant.BitWidth = bitWidth;
						this.constant.ConstantValue = value;
						this.constant.PinSide = pinSide;
						this.constant.Note = note;
						this.constant.Pins.First().PinSide = pinSide;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
