using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogUsage.xaml
	/// </summary>
	public partial class DialogUsage : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		public LogicalCircuit LogicalCircuit { get; private set; }
		public IEnumerable<LogicalCircuit> Usage { get; private set; }

		public DialogUsage(LogicalCircuit logicalCircuit) {
			this.LogicalCircuit = logicalCircuit;
			this.Usage = new HashSet<LogicalCircuit>(this.LogicalCircuit.CircuitProject.CircuitSymbolSet.SelectByCircuit(this.LogicalCircuit).Select(s => s.LogicalCircuit)).ToList();
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ListBox listBox = sender as ListBox;
			if(listBox != null) {
				LogicalCircuit selected = listBox.SelectedItem as LogicalCircuit;
				if(selected != null) {
					Mainframe mainframe = (Mainframe)this.Owner;
					Editor editor = mainframe.Editor;
					editor.OpenLogicalCircuit(selected);
					editor.Select(editor.CircuitProject.CircuitSymbolSet.SelectByCircuit(this.LogicalCircuit).Where(s => s.LogicalCircuit == selected));
				}
			}
		}
	}
}
