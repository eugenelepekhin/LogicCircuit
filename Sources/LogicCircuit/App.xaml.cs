using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		public string FileToOpen { get; private set; }

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			if(e != null && e.Args != null && 0 < e.Args.Length && !string.IsNullOrEmpty(e.Args[0])) {
				this.FileToOpen = e.Args[0];
			}
		}
	}
}
