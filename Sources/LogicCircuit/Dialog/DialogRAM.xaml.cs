using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogRAM.xaml
	/// </summary>
	public partial class DialogRAM : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Memory memory;

		public DialogRAM(Memory memory) {
			Tracer.Assert(memory.Writable);
			this.memory = memory;
			this.DataContext = this;
			this.InitializeComponent();

			this.addressBitWidth.ItemsSource = PinDescriptor.AddressBitRange();
			this.dataBitWidth.ItemsSource = PinDescriptor.BitRange(1);
			this.writeOn.ItemsSource = new string[] { LogicCircuit.Resources.WriteOn0, LogicCircuit.Resources.WriteOn1 };
			this.addressBitWidth.SelectedItem = this.memory.AddressBitWidth;
			this.dataBitWidth.SelectedItem = this.memory.DataBitWidth;
			this.writeOn.SelectedIndex = this.memory.WriteOn1 ? 1 : 0;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				int addressBitWidth = (int)this.addressBitWidth.SelectedItem;
				int dataBitWidth = (int)this.dataBitWidth.SelectedItem;
				bool writeOn1 = (this.writeOn.SelectedIndex == 0) ? false : true;

				if(this.memory.AddressBitWidth != addressBitWidth || this.memory.DataBitWidth != dataBitWidth || this.memory.WriteOn1 != writeOn1) {
					this.memory.CircuitProject.InTransaction(() => {
						this.memory.AddressBitWidth = addressBitWidth;
						this.memory.DataBitWidth = dataBitWidth;
						this.memory.WriteOn1 = writeOn1;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
