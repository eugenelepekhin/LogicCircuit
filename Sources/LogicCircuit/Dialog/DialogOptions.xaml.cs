using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogOptions.xaml
	/// </summary>
	public partial class DialogOptions : Window {

		private Mainframe mainframe;

		//The order of items in this array should match order in enum GateShape
		private EnumDescriptor<GateShape>[] gateShapeList = new EnumDescriptor<GateShape>[] {
			new EnumDescriptor<GateShape>(GateShape.Rectangular, Properties.Resources.GateShapeRectangular),
			new EnumDescriptor<GateShape>(GateShape.Shaped, Properties.Resources.GateShapeShaped)
		};

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<EnumDescriptor<GateShape>> GateShapeList { get { return this.gateShapeList; } }
		public IEnumerable<int> RecentFileRange { get; private set; }
		public CultureInfo CurrentCulture { get; set; }

		public DialogOptions(Mainframe mainframe) {
			this.mainframe = mainframe;
			this.RecentFileRange = PinDescriptor.NumberRange(1, 24);
			this.CurrentCulture = App.CurrentCulture;
			this.DataContext = this;
			this.InitializeComponent();
			this.loadLastFile.IsChecked = Settings.User.LoadLastFileOnStartup;
			this.showGrid.IsChecked = this.mainframe.ShowGrid;
			this.gateShape.SelectedItem = this.GateShapeList.First(d => d.Value == Settings.User.GateShape);
			this.maxRecentFiles.SelectedItem = Settings.User.MaxRecentFileCount;

		}

		private void OkButtonClick(object sender, RoutedEventArgs e) {
			Settings.User.LoadLastFileOnStartup = this.loadLastFile.IsChecked.Value;
			Settings.User.MaxRecentFileCount = (int)this.maxRecentFiles.SelectedItem;
			this.mainframe.ShowGrid = this.showGrid.IsChecked.Value;
			Settings.User.GateShape = ((EnumDescriptor<GateShape>)this.gateShape.SelectedItem).Value;
			App.CurrentCulture = this.CurrentCulture;
			
			if(Properties.Resources.Culture != this.CurrentCulture) {
				// Show message in both languages old and new.
				CultureInfo old = Properties.Resources.Culture;
				string oldMessage = Properties.Resources.MessageRestartRequared;
				Properties.Resources.Culture = this.CurrentCulture;
				string newMessage = Properties.Resources.MessageRestartRequared;
				Properties.Resources.Culture = old;
				if(oldMessage != newMessage) {
					App.Mainframe.InformationMessage(oldMessage + "\n\n" + newMessage);
				} else {
					App.Mainframe.InformationMessage(oldMessage);
				}
			}
			this.DialogResult = true;
			this.Close();
		}
	}
}
