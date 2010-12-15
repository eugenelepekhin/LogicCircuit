using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

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

		private void WebRequestNavigate(object sender, RequestNavigateEventArgs e) {
			Hyperlink link = sender as Hyperlink;
			if(link != null) {
				Process.Start(link.NavigateUri.AbsoluteUri);
			}
		}
	}
}
