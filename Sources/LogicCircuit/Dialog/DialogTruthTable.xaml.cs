using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogTruthTable.xaml
	/// </summary>
	public partial class DialogTruthTable : Window {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public LogicalCircuit LogicalCircuit { get; private set; }

		public DialogTruthTable(LogicalCircuit logicalCircuit) {
			Tracer.Assert(CircuitTestSocket.IsTestable(logicalCircuit));
			this.LogicalCircuit  = logicalCircuit;
			this.DataContext = this;
			this.InitializeComponent();
		}
	}
}
