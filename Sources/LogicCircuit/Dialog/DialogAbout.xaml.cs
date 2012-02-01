using System;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogAbout.xaml
	/// </summary>
	public partial class DialogAbout : Window {

		public string Version { get; set; }

		public DialogAbout() {
			this.Version = this.GetType().Assembly.GetName().Version.ToString();
			this.DataContext = this;
			this.InitializeComponent();
		}
	}
}
