using System;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogButton.xaml
	/// </summary>
	public partial class DialogButton : Window {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private CircuitButton button;

		public DialogButton(CircuitButton button) {
			this.button = button;
			this.DataContext = this;
			this.InitializeComponent();

			this.name.Text = this.button.Notation;
			this.isToggle.IsChecked = this.button.IsToggle;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.button.PinSide);
			this.note.Text = this.button.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string note = this.note.Text.Trim();
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				if(this.button.Notation != name || this.button.IsToggle != this.isToggle.IsChecked || this.button.PinSide != pinSide || this.button.Note != note) {
					this.button.CircuitProject.InTransaction(() => {
						this.button.Notation = name;
						this.button.IsToggle = this.isToggle.IsChecked.Value;
						this.button.PinSide = pinSide;
						this.button.Note = note;
						this.button.Pins.First().PinSide = pinSide;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
