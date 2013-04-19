using System;
using System.Collections.Generic;
using System.Linq;
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

			HashSet<string> set = new HashSet<string>(this.logicalCircuit.CircuitProject.LogicalCircuitSet.Select(c => c.Category));
			set.Add(string.Empty);
			foreach(string s in set.OrderBy(s => s)) {
				this.category.Items.Add(s);
			}

			this.category.Text = this.logicalCircuit.Category;
			if(this.logicalCircuit.ContainsDisplays()) {
				this.isDisplay.IsChecked = logicalCircuit.IsDisplay;
			} else {
				this.isDisplay.IsEnabled = false;
			}
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
				bool isDisplay = this.logicalCircuit.ContainsDisplays() ? this.isDisplay.IsChecked.Value : this.logicalCircuit.IsDisplay;
				string description = this.description.Text.Trim();

				if(this.logicalCircuit.Name != name || this.logicalCircuit.Notation != notation ||
					this.logicalCircuit.Category != category || this.logicalCircuit.IsDisplay != isDisplay || this.logicalCircuit.Description != description
				) {
					this.logicalCircuit.CircuitProject.InTransaction(() => {
						this.logicalCircuit.Rename(name);
						this.logicalCircuit.Notation = notation;
						this.logicalCircuit.Category = category;
						this.logicalCircuit.IsDisplay = isDisplay;
						this.logicalCircuit.Description = description;
						this.logicalCircuit.CircuitProject.CollapsedCategorySet.Purge();
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
