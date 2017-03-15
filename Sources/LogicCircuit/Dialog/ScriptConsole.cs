using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogicCircuit {
	public class ScriptConsole : TextBox {
		public Action<string> CommandEnter { get; set; } = text => {};
		public Func<bool> CommandBreak { get; set; } = () => false;

		private int inputStarts = 0;

		private SettingsStringCache historySettings = new SettingsStringCache(Settings.User, "ScriptConsole.History", null);
		private List<string> history;
		private int historyIndex;

		public ScriptConsole() {
			this.IsUndoEnabled = false;
			this.AcceptsReturn = true;
			this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			this.CommandBindings.Add(new CommandBinding(
				ApplicationCommands.Paste,
				new ExecutedRoutedEventHandler((sender, e) => {
					this.Paste();
				}),
				new CanExecuteRoutedEventHandler((sender, e) => {
					if(!this.IsEditAllowed()) {
						e.CanExecute = false;
						e.Handled = true;
					}
				})
			));

			this.CommandBindings.Add(new CommandBinding(
				ApplicationCommands.Cut,
				new ExecutedRoutedEventHandler((sender, e) => {
					this.Cut();
				}),
				new CanExecuteRoutedEventHandler((sender, e) => {
					if(!this.IsEditAllowed()) {
						e.CanExecute = false;
						e.Handled = true;
					}
				})
			));

			this.history = this.LoadHistory();
			this.historyIndex = this.history.Count;
		}

		private List<string> LoadHistory() {
			List<string> list = new List<string>();
			string text = this.historySettings.Value;
			if(!string.IsNullOrEmpty(text)) {
				string[] item = text.Split(new char[] { '\n' }, Settings.User.MaxRecentFileCount, StringSplitOptions.RemoveEmptyEntries);
				if(item != null) {
					HashSet<string> set = new HashSet<string>();
					foreach(string command in item) {
						if(set.Add(command)) {
							list.Add(command);
						}
					}
				}
			}
			return list;
		}

		private void SaveHistory() {
			StringBuilder text = new StringBuilder();
			for(int i = Math.Max(0, this.history.Count - Settings.User.MaxRecentFileCount); i < this.history.Count; i++) {
				text.AppendLine(this.history[i]);
			}
			this.historySettings.Value = text.ToString();
		}

		private void HistoryAdd(string text) {
			if(!string.IsNullOrWhiteSpace(text)) {
				this.history.Remove(text);
				this.history.Add(text);
				this.historyIndex = this.history.Count;
			}
		}

		private void SetCommand(string text) {
			this.Text = this.Text.Substring(0, this.inputStarts) + text;
			this.Select(this.inputStarts, text.Length);
		}

		private void HistoryUp() {
			if(0 < this.historyIndex) {
				this.historyIndex--;
				this.SetCommand(this.history[this.historyIndex]);
			}
		}

		private void HistoryDown() {
			if(this.historyIndex < this.history.Count - 1) {
				this.historyIndex++;
				this.SetCommand(this.history[this.historyIndex]);
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if(e.Property == TextBox.IsUndoEnabledProperty && this.IsUndoEnabled ||
				e.Property == TextBox.AcceptsReturnProperty && !this.AcceptsReturn
			) {
				throw new InvalidOperationException();
			}
		}

		public void Prompt(bool fromNewLine, string promptText) {
			if(fromNewLine) {
				string text = this.Text;
				if(0 < text.Length && text[text.Length - 1] != '\n') {
					this.AppendText("\n");
				}
			}
			this.AppendText(promptText);
			this.ScrollToEnd();
			this.inputStarts = this.Text.Length;
			this.Select(this.Text.Length, 0);
		}

		private bool IsEditAllowed() {
			return this.inputStarts <= this.SelectionStart;
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
			if(!this.IsEditAllowed()) {
				e.Handled = true;
			} else {
				base.OnPreviewTextInput(e);
			}
		}

		//Left/Right Arrow keys:  Move cursor left or right within text
		//Up/Down Arrow keys:  Move cursor to previous/next line, maintaining position within line
		//Ctrl+Left/Right Arrow:  Move to beginning of previous/next word
		//Ctrl+Up Arrow:  Move to start of text
		//Ctrl+Down Arrow:  Move to end of text
		//Shift+Left/Right/Up/Down Arrow:  Move cursor while selecting text
		//Home key:  Move to beginning of current line
		//End key:  Move to end of current line
		//Shift+Home/End:  Select text to beginning/end of current line
		//Page Up/Down:  Move up/down full page
		//Insert key:  Toggle Insert/Overwrite mode
		//Delete key:  Delete character to right of cursor
		//Backspace key:  Delete character to right of cursor
		//Ctrl+A:  Select all text
		//Ctrl+X:  Cut selected text
		//Ctrl+C:  Copy selected text
		//Ctrl+V:  Paste text at current position
		protected override void OnPreviewKeyDown(KeyEventArgs e) {
			try {
				string text;
				switch(e.Key) {
				case Key.Enter:
					text = this.Text.Substring(this.inputStarts);
					this.inputStarts = int.MaxValue;
					base.OnPreviewKeyDown(e);
					this.HistoryAdd(text);
					this.SaveHistory();
					this.AppendText("\n");
					this.Select(this.Text.Length, 0);
					this.ScrollToEnd();
					this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => this.CommandEnter(text)));
					e.Handled = true;
					break;
				case Key.C:
					if(e.KeyboardDevice.Modifiers == ModifierKeys.Control && 0 == this.SelectionLength && this.CommandBreak()) {
						e.Handled = true;
						return;
					}
					base.OnPreviewKeyDown(e);
					break;
				case Key.Up:
					if(this.IsEditAllowed()) {
						this.HistoryUp();
						e.Handled = true;
					} else {
						base.OnPreviewKeyDown(e);
					}
					break;
				case Key.Down:
					if(this.IsEditAllowed()) {
						this.HistoryDown();
						e.Handled = true;
					} else {
						base.OnPreviewKeyDown(e);
					}
					break;
				case Key.Left:
					if(this.SelectionStart < this.inputStarts || this.inputStarts < this.SelectionStart &&
						(e.KeyboardDevice.Modifiers != ModifierKeys.Control || !string.IsNullOrWhiteSpace(this.Text.Substring(this.inputStarts, this.SelectionStart - this.inputStarts)))
					) {
						base.OnPreviewKeyDown(e);
					} else {
						e.Handled = true;
					}
					break;
				case Key.Home:
					if(this.IsEditAllowed()) {
						if(e.KeyboardDevice.Modifiers == ModifierKeys.Shift) {
							this.Select(this.inputStarts, this.SelectionStart - this.inputStarts);
						} else {
							this.Select(this.inputStarts, 0);
						}
						e.Handled = true;
					} else {
						base.OnPreviewKeyDown(e);
					}
					break;
				case Key.Delete:
					if(this.IsEditAllowed()) {
						base.OnPreviewKeyDown(e);
					} else {
						e.Handled = true;
					}
					break;
				case Key.Back:
					if(this.inputStarts < this.SelectionStart) {
						base.OnPreviewKeyDown(e);
					} else {
						e.Handled = true;
					}
					break;
				default:
					base.OnPreviewKeyDown(e);
					break;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
