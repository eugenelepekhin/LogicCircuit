using System;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogProbe.xaml
	/// </summary>
	public partial class DialogProbe : Window {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private CircuitProbe probe;

		public DialogProbe(CircuitProbe probe) {
			this.probe = probe;
			this.DataContext = this;
			this.InitializeComponent();

			this.name.Text = this.probe.DisplayName;
			this.description.Text = this.probe.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string note = this.description.Text.Trim();
				if(this.probe.DisplayName != name || this.probe.Note != note) {
					this.probe.CircuitProject.InTransaction(() => {
						this.probe.Rename(name);
						this.probe.Note = note;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
