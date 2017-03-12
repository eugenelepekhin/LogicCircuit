using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for IronPythonConsole.xaml
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public sealed partial class IronPythonConsole : Window {
		private static IronPythonConsole currentConsole = null;

		public static void Run(Mainframe mainframe) {
			if(IronPythonConsole.currentConsole == null) {
				IronPythonConsole console = new IronPythonConsole(mainframe);
				console.Owner = mainframe;
				IronPythonConsole.currentConsole = console;
				console.Show();
			} else {
				IronPythonConsole.currentConsole.Focus();
			}
		}

		public static void Stop() {
			if(IronPythonConsole.currentConsole != null) {
				IronPythonConsole.currentConsole.Close();
			}
		}

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private ScriptEngine scriptEngine;
		private MemoryStream stdout;
		private LogWriter writer;
		private ScriptScope scope;

		private IronPythonConsole(Mainframe mainframe) {
			this.DataContext = this;
			this.InitializeComponent();

			this.scriptEngine = Python.CreateEngine();
			this.stdout = new MemoryStream();
			this.writer = new LogWriter(this.console);
			this.console.CommandEnter = this.Execute;

			this.scriptEngine.Runtime.IO.SetOutput(this.stdout, this.writer);
			this.scope = this.scriptEngine.CreateScope();
			this.scope.SetVariable("mainframe", mainframe);

			this.scope.ImportModule("clr");
			this.scriptEngine.Execute("import clr", this.scope);
			this.scriptEngine.Execute("import System", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Core\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Linq\")", this.scope);
			this.scriptEngine.Execute("clr.ImportExtensions(System.Linq)", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"LogicCircuit\")", this.scope);
			this.scriptEngine.Execute("from LogicCircuit import *", this.scope);

			this.writer.WriteLine("IronPython " + this.scriptEngine.LanguageVersion.ToString());
			this.Prompt();
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			IronPythonConsole.currentConsole = null;
			this.stdout.Close();
			this.writer.Close();
		}

		private void Prompt() {
			this.console.Prompt(">>>");
		}

		private void Execute(string text) {
			if(!string.IsNullOrWhiteSpace(text)) {
				try {
					dynamic result = this.scriptEngine.Execute(text, this.scope);
					if(result != null) {
						this.writer.WriteLine(result.ToString());
					}
				} catch(Exception exception) {
					this.writer.WriteLine(exception.Message);
				}
				this.Prompt();
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
