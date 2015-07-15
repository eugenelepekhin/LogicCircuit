using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogMemory.xaml
	/// </summary>
	public partial class DialogMemory : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public IFunctionMemory FunctionMemory { get; private set; }

		public DialogMemory(IFunctionMemory functionMemory) {
			this.FunctionMemory = functionMemory;
			this.DataContext = this;
			this.InitializeComponent();
		}
	}
}
