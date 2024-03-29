﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogButton.xaml
	/// </summary>
	public partial class DialogButton : Window {
		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private readonly CircuitButton button;

		public DialogButton(CircuitButton button) {
			this.button = button;
			this.DataContext = this;
			this.InitializeComponent();

			this.name.Text = this.button.Notation;
			this.isToggle.IsChecked = this.button.IsToggle;
			this.side.SelectedItem = PinDescriptor.PinSideDescriptor(this.button.PinSide);
			this.inverted.IsChecked = this.button.Inverted;
			this.keyGesture.Key = button.Key;
			this.keyGesture.ModifierKeys = button.ModifierKeys;
			this.keyGesture.Refresh();
			this.note.Text = this.button.Note ?? string.Empty;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string note = this.note.Text.Trim();
				PinSide pinSide = ((EnumDescriptor<PinSide>)this.side.SelectedItem).Value;
				bool isInverted = this.inverted.IsChecked.HasValue && this.inverted.IsChecked.Value;
				Key key = this.keyGesture.Key;
				ModifierKeys modifier = this.keyGesture.ModifierKeys;
				if(	this.button.Notation != name ||
					this.button.IsToggle != this.isToggle.IsChecked ||
					this.button.PinSide != pinSide ||
					this.button.Inverted != isInverted ||
					this.button.Key != key ||
					this.button.ModifierKeys != modifier ||
					this.button.Note != note
				) {
					this.button.CircuitProject.InTransaction(() => {
						this.button.Notation = name;
						this.button.IsToggle = this.isToggle.IsChecked!.Value;
						this.button.PinSide = pinSide;
						this.button.Inverted = isInverted;
						this.button.Key = key;
						this.button.ModifierKeys = modifier;
						this.button.Note = note;
						this.button.Pins.First().PinSide = pinSide;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
