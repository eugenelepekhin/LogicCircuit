using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
			this.note.Text = this.button.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string note = this.note.Text.Trim();
				if(this.button.Notation != name || this.button.IsToggle != this.isToggle.IsChecked || this.button.Note != note) {
					this.button.CircuitProject.InTransaction(() => {
						this.button.Notation = name;
						this.button.IsToggle = this.isToggle.IsChecked.Value;
						this.button.Note = note;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
