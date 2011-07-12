using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogConstant.xaml
	/// </summary>
	public partial class DialogConstant : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private Constant constant;

		public DialogConstant(Constant constant) {
			this.DataContext = this;
			this.constant = constant;
			this.InitializeComponent();
			this.value.Text = constant.Notation;
			this.bitWidth.ItemsSource = PinDescriptor.NumberRange(1);
			this.bitWidth.SelectedItem = this.constant.BitWidth;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				int bitWidth = (int)this.bitWidth.SelectedItem;
				int value = int.Parse(this.value.Text.Trim(), NumberStyles.HexNumber);

				if(this.constant.BitWidth != bitWidth || this.constant.ConstantValue != value) {
					this.constant.CircuitProject.InTransaction(() => {
						this.constant.BitWidth = bitWidth;
						this.constant.ConstantValue = value;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
