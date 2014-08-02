using System;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogProject.xaml
	/// </summary>
	public partial class DialogProject : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Project project;

		public DialogProject(Project project) {
			this.DataContext = this;
			this.project = project;
			this.InitializeComponent();
			this.name.Text = project.Name;
			this.description.Text = project.Description;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string description = this.description.Text.Trim();

				if(this.project.Name != name || this.project.Description != description) {
					this.project.CircuitProject.InTransaction(() => {
						this.project.Name = name;
						this.project.Description = description;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
