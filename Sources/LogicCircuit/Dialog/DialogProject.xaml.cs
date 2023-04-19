using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogProject.xaml
	/// </summary>
	public partial class DialogProject : Window {

		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private readonly Project project;

		public DialogProject(Project project) {
			this.DataContext = this;
			this.project = project;
			this.InitializeComponent();
			this.name.Text = project.Name;
			this.description.Text = project.Note;
			List<CurcuitInfo> curcuits = this.Circuits();
			CurcuitInfo current = curcuits.First(i => i.Circuit == this.project.StartupCircuit);
			this.startup.ItemsSource = curcuits;
			this.startup.SelectedItem = current;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string description = this.description.Text.Trim();
				CurcuitInfo info = (CurcuitInfo)this.startup.SelectedItem;

				if(this.project.Name != name || this.project.Note != description || this.project.StartupCircuit != info.Circuit) {
					this.project.CircuitProject.InTransaction(() => {
						this.project.Name = name;
						this.project.Note = description;
						this.project.SetStartup(info.Circuit);
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private List<CurcuitInfo> Circuits() {
			IEnumerable<CurcuitInfo> one(LogicalCircuit? value) { yield return new CurcuitInfo(value); }
			return one(null).Union(this.project.CircuitProject.LogicalCircuitSet.OrderBy(c => c.Name).Select(c => new CurcuitInfo(c))).ToList();
		}

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class CurcuitInfo {
			public LogicalCircuit? Circuit { get; }
			public string Name => this.Circuit?.Name ?? Properties.Resources.TitleCurrent;

			public CurcuitInfo(LogicalCircuit? circuit) {
				this.Circuit = circuit;
			}
		}
	}
}
