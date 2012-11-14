using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogOptions.xaml
	/// </summary>
	public partial class DialogOptions : Window {

		private Mainframe mainFrame;

		//The order of items in this array should match order in enum GateShape
		private string[] gateShapeList = new string[] { Properties.Resources.GateShapeRectangular, Properties.Resources.GateShapeShaped };

		public IEnumerable<string> GateShapeList { get { return this.gateShapeList; } }
		public IEnumerable<int> RecentFileRange { get; private set; }
		public CultureInfo CurrentCulture { get; set; }

		public DialogOptions(Mainframe mainFrame) {
			this.mainFrame = mainFrame;
			this.RecentFileRange = PinDescriptor.NumberRange(1, 24);
			this.CurrentCulture = App.CurrentCulture;
			this.DataContext = this;
			this.InitializeComponent();
			this.loadLastFile.IsChecked = Settings.User.LoadLastFileOnStartup;
			this.showGrid.IsChecked = mainFrame.ShowGrid;
			this.gateShape.SelectedIndex = (int)Settings.User.GateShape;
			this.maxRecentFiles.SelectedItem = Settings.User.MaxRecentFileCount;

		}

		private void OkButtonClick(object sender, RoutedEventArgs e) {
			Settings.User.LoadLastFileOnStartup = this.loadLastFile.IsChecked.Value;
			Settings.User.MaxRecentFileCount = (int)this.maxRecentFiles.SelectedItem;
			this.mainFrame.ShowGrid = this.showGrid.IsChecked.Value;
			Settings.User.GateShape = (GateShape)this.gateShape.SelectedIndex;
			if(this.CurrentCulture != App.CurrentCulture) {
				App.CurrentCulture = this.CurrentCulture;
			}
			if(Properties.Resources.Culture != this.CurrentCulture) {
				App.Mainframe.InformationMessage(Properties.Resources.MessageRestartRequared);
			}
			this.DialogResult = true;
			this.Close();
		}
	}
}
