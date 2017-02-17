using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for ScriptConsole.xaml
	/// </summary>
	public partial class ScriptConsole : Window {
		private static ScriptConsole currentConsole = null;

		public static void Run(Mainframe mainframe) {
			if(ScriptConsole.currentConsole == null) {
				ScriptConsole console = new ScriptConsole(mainframe);
				console.Owner = mainframe;
				ScriptConsole.currentConsole = console;
				console.Show();
			} else {
				ScriptConsole.currentConsole.Focus();
			}
		}

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private ScriptEngine scriptEngine;
		private MemoryStream stdout;
		private LogWriter writer;
		private ScriptScope scope;
		private List<string> history;
		private int historyIndex;

		private ScriptConsole(Mainframe mainframe) {
			this.DataContext = this;
			this.InitializeComponent();

			this.scriptEngine = Python.CreateEngine();
			this.stdout = new MemoryStream();
			this.writer = new LogWriter(this.textBoxLog);

			this.scriptEngine.Runtime.IO.SetOutput(this.stdout, this.writer);
			this.scope = this.scriptEngine.CreateScope();
			this.scope.SetVariable("mainframe", mainframe);

			this.history = new List<string>();

			this.scope.ImportModule("clr");
			this.scriptEngine.Execute("import clr", this.scope);
			this.scriptEngine.Execute("import System", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Core\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Linq\")", this.scope);
			this.scriptEngine.Execute("clr.ImportExtensions(System.Linq)", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"LogicCircuit\")", this.scope);
			this.scriptEngine.Execute("from LogicCircuit import *", this.scope);

			this.writer.WriteLine("IronPython");
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			ScriptConsole.currentConsole = null;
			this.stdout.Close();
			this.writer.Close();
		}

		private void Execute() {
			string text = this.textBoxCommand.Text.Trim();
			//string text = "mainframe.InformationMessage('hello, world! message')";
			//string text = "print 'hello, world! print'";
			if(!string.IsNullOrEmpty(text)) {
				this.textBoxCommand.Clear();
				this.HistoryAdd(text);
				this.writer.WriteLine("> " + text);
				try {
					dynamic result = this.scriptEngine.Execute(text, this.scope);
					if(result != null) {
						this.writer.WriteLine(result.ToString());
					}
				} catch(Exception exception) {
					this.writer.WriteLine(exception.Message);
				}
				this.textBoxLog.ScrollToEnd();
			}
		}

		private void HistoryAdd(string text) {
			this.history.Remove(text);
			this.history.Add(text);
			this.historyIndex = this.history.Count;
		}

		private void HistoryRemove(string text) {
			this.history.Remove(text);
			this.historyIndex = this.history.Count;
		}

		private void HistoryUp() {
			if(0 < this.historyIndex) {
				this.historyIndex--;
				this.textBoxCommand.Text = this.history[this.historyIndex];
				this.textBoxCommand.SelectAll();
			}
		}

		private void HistoryDown() {
			if(this.historyIndex < this.history.Count - 1) {
				this.historyIndex++;
				this.textBoxCommand.Text = this.history[this.historyIndex];
				this.textBoxCommand.SelectAll();
			}
		}

		private void textBoxCommandKeyDown(object sender, KeyEventArgs e) {
			switch(e.Key) {
			case Key.Enter:
				this.Execute();
				e.Handled = true;
				break;
			case Key.Up:
				this.HistoryUp();
				e.Handled = true;
				break;
			case Key.Down:
				this.HistoryDown();
				e.Handled = true;
				break;
			case Key.PageUp:
			case Key.PageDown:
				break;
			}
		}

		private class LogWriter : TextWriter {
			private TextBox textBox;

			public override Encoding Encoding => Encoding.Unicode;

			public LogWriter(TextBox textBox) : base(CultureInfo.InvariantCulture) {
				this.textBox = textBox;
			}

			public override void Write(char value) {
				this.textBox.AppendText(new string(value, 1));
			}

			public override void Write(char[] buffer, int index, int count) {
				this.textBox.AppendText(new string(buffer, index, count));
			}

			public override void Write(string value) {
				if(!string.IsNullOrEmpty(value)) {
					this.textBox.AppendText(value.Replace("\r", ""));
				}
			}
		}
	}
}
