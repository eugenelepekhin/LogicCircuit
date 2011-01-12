﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogPin.xaml
	/// </summary>
	public partial class DialogPin : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private Pin pin;

		public DialogPin(Pin pin) {
			this.DataContext = this;
			this.pin = pin;
			this.InitializeComponent();
			this.type.Text = this.pin.PinType.ToString();
			this.name.Text = this.pin.Name;
			this.notation.Text = this.pin.JamNotation;
			this.note.Text = this.pin.Note;
			this.side.ItemsSource = Enum.GetNames(typeof(PinSide));
			this.side.SelectedItem = this.pin.PinSide.ToString();
			this.inverted.IsChecked = this.pin.Inverted;
			this.bitWidth.ItemsSource = PinDescriptor.BitRange(1);
			this.bitWidth.SelectedItem = this.pin.BitWidth;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string notation = this.notation.Text.Trim();
				string note = this.note.Text.Trim();
				PinSide pinSide = (PinSide)Enum.Parse(typeof(PinSide), this.side.SelectedItem.ToString());
				bool inverted = this.inverted.IsChecked.Value;
				int bitWidth = (int)this.bitWidth.SelectedItem;

				if(this.pin.Name != name || this.pin.JamNotation != notation || this.pin.Note != note ||
					this.pin.PinSide != pinSide || this.pin.Inverted != inverted || this.pin.BitWidth != bitWidth
				) {
					this.pin.CircuitProject.InTransaction(() => {
						this.pin.Rename(name);
						this.pin.JamNotation = notation;
						this.pin.Note = note;
						this.pin.PinSide = pinSide;
						this.pin.Inverted = inverted;
						this.pin.BitWidth = bitWidth;
					});
				} 
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}