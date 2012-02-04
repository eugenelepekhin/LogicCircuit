using System;
using System.Windows;
using System.Windows.Documents;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogReport.xaml
	/// </summary>
	public partial class DialogReport : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public FlowDocument Document { get; private set; }

		public DialogReport(LogicalCircuit root) {
			this.Document = ReportBuilder.Build(root);
			this.DataContext = this;
			this.InitializeComponent();
		}

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Key == System.Windows.Input.Key.Escape) {
				this.Close();
			}
		}
	}
}
