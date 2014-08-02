using System;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogSound.xaml
	/// </summary>
	public partial class DialogSound : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Sound sound;

		public DialogSound(Sound sound) {
			this.sound = sound;
			this.DataContext = this;
			this.InitializeComponent();

			this.side.ItemsSource = PinDescriptor.PinSideRange;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.sound.PinSide);
			this.notation.Text = this.sound.Notation;
			this.note.Text = this.sound.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				string notation = this.notation.Text.Trim();
				string note = this.note.Text.Trim();

				if(	this.sound.PinSide != pinSide ||
					this.sound.Notation != notation ||
					this.sound.Note != note
				) {
					this.sound.CircuitProject.InTransaction(() => {
						this.sound.PinSide = pinSide;
						this.sound.Notation = notation;
						this.sound.Note = note;
						this.sound.Pins.First().PinSide = pinSide;
					});
				}

				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
