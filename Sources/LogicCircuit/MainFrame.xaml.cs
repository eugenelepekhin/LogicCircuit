using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for MainFrame.xaml
	/// </summary>
	public partial class MainFrame : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public MainFrame() {
			this.InitializeComponent();
		}
	}
}
