using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogFind.xaml
	/// </summary>
	public partial class DialogFind : Window {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private SettingsStringCache searchFilter;
		public string SearchFilter {
			get { return this.searchFilter.Value; }
			set { this.searchFilter.Value = value; }
		}

		private Editor editor;
		private Dictionary<LogicalCircuit, List<Symbol>> searchMap = new Dictionary<LogicalCircuit, List<Symbol>>();
		
		public DialogFind(Editor editor) {
			this.searchFilter = new SettingsStringCache(Settings.Session, "DialogFind.filter", string.Empty);
			this.editor = editor;
			this.DataContext = this;
			this.InitializeComponent();
			this.PreviewKeyUp += DialogFindPreviewKeyUp;
		}

		private void DialogFindPreviewKeyUp(object sender, KeyEventArgs e) {
			if(e.Key == Key.Escape) {
				this.Close();
			}
		}

		private void Add(Symbol symbol) {
			List<Symbol> list;
			if(!this.searchMap.TryGetValue(symbol.LogicalCircuit, out list)) {
				list = new List<Symbol>();
				this.searchMap.Add(symbol.LogicalCircuit, list);
			}
			list.Add(symbol);
		}

		private void ButtonSearchClick(object sender, RoutedEventArgs e) {
			try {
				string text = this.SearchFilter;
				if(!string.IsNullOrWhiteSpace(text)) {
					this.searchMap.Clear();
					this.resultList.ItemsSource = null;
					Regex regex = new Regex(text.Trim(), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

					foreach(CircuitSymbol symbol in this.editor.CircuitProject.CircuitSymbolSet) {
						if(symbol.Circuit.Match(regex)) {
							this.Add(symbol);
						}
					}
					foreach(TextNote symbol in this.editor.CircuitProject.TextNoteSet) {
						if(symbol.Match(regex)) {
							this.Add(symbol);
						}
					}
					foreach(LogicalCircuit circuit in this.editor.CircuitProject.LogicalCircuitSet) {
						if(circuit.Match(regex)) {
							if(!this.searchMap.ContainsKey(circuit)) {
								this.searchMap.Add(circuit, null);
							}
						}
					}

					this.resultList.ItemsSource = this.searchMap.Keys;
					if(0 < this.searchMap.Count) {
						this.resultList.Focus();
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void resultListMouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				ListBox listBox = sender as ListBox;
				if(listBox != null) {
					LogicalCircuit selected = listBox.SelectedItem as LogicalCircuit;
					if(selected != null) {
						this.editor.OpenLogicalCircuit(selected);
						List<Symbol> list;
						if(this.searchMap.TryGetValue(selected, out list) && list != null && 0 < list.Count) {
							this.editor.Select(list);
						}
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void resultListPreviewKeyUp(object sender, KeyEventArgs e) {
			if(e.Key == Key.Space) {
				this.resultListMouseDoubleClick(sender, null);
			}
		}
	}
}
