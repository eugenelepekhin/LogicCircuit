using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogCircuit.xaml
	/// </summary>
	public partial class DialogCircuit : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private LogicalCircuit logicalCircuit;

		public DialogCircuit(LogicalCircuit logicalCircuit) {
			this.DataContext = this;
			this.InitializeComponent();
			this.logicalCircuit = logicalCircuit;
			this.name.Text = this.logicalCircuit.Name;
			this.notation.Text = this.logicalCircuit.Notation;
			HashSet<string> list = new HashSet<string>();
			foreach(LogicalCircuit circuit in this.logicalCircuit.CircuitProject.LogicalCircuitSet) {
				if(list.Add(circuit.Category)) {
					this.category.Items.Add(circuit.Category);
				}
			}
			if(list.Add(string.Empty)) {
				this.category.Items.Add(string.Empty);
			}
			this.category.Text = this.logicalCircuit.Category;
			this.description.Text = this.logicalCircuit.Description;
			this.Loaded += new RoutedEventHandler(this.DialogCircuitLoaded);
		}

		private void DialogCircuitLoaded(object sender, RoutedEventArgs e) {
			ControlTemplate template = this.category.Template;
			if(template != null) {
				TextBox textBox = template.FindName("PART_EditableTextBox", this.category) as TextBox;
				if(textBox != null) {
					SpellCheck spellCheck = textBox.SpellCheck;
					if(spellCheck != null) {
						spellCheck.IsEnabled = true;
					}
				}
			}
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				string name = this.name.Text.Trim();
				string notation = this.notation.Text.Trim();
				string category = this.category.Text.Trim();
				category = category.Substring(0, Math.Min(category.Length, 64)).Trim();
				string description = this.description.Text.Trim();

				if(this.logicalCircuit.Name != name || this.logicalCircuit.Notation != notation ||
					this.logicalCircuit.Category != category || this.logicalCircuit.Description != description
				) {
					this.logicalCircuit.CircuitProject.InTransaction(() => {
						this.logicalCircuit.Rename(name);
						this.logicalCircuit.Notation = notation;
						this.logicalCircuit.Category = category;
						this.logicalCircuit.Description = description;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
