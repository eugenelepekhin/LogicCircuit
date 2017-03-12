using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
		private StringBuilder command = new StringBuilder();
		private Thread thread;

		private IronPythonConsole(Mainframe mainframe) {
			this.DataContext = this;
			this.InitializeComponent();

			this.scriptEngine = Python.CreateEngine();
			this.stdout = new MemoryStream();
			this.writer = new LogWriter(this.console);
			this.console.CommandEnter = this.Execute;
			this.console.CommandBreak = this.Abort;

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

		protected override void OnClosing(CancelEventArgs e) {
			this.Abort();
			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			IronPythonConsole.currentConsole = null;
			this.stdout.Close();
			this.writer.Close();
		}

		private void Prompt() {
			string text = (0 < this.command.Length) ? "... " : ">>> ";
			this.Dispatcher.BeginInvoke(new Action(() => this.console.Prompt(text)), DispatcherPriority.ApplicationIdle);
		}

		private static bool IsMultiline(string text) {
			string end = text.Trim();
			return end.EndsWith(":", StringComparison.Ordinal) || end.EndsWith(@"\", StringComparison.Ordinal);
		}

		private void Execute(string text) {
			if(0 < text.Length && (0 < this.command.Length || IronPythonConsole.IsMultiline(text))) {
				this.command.AppendLine(text);
			} else {
				if(string.IsNullOrWhiteSpace(text) && 0 < this.command.Length) {
					text = this.command.ToString();
					this.command.Length = 0;
				}
				if(!string.IsNullOrWhiteSpace(text)) {
					this.Start(text);
					return;
				}
			}
			this.Prompt();
		}

		private void Start(string text) {
			Action run = () => {
				try {
					dynamic result = this.scriptEngine.Execute(text, this.scope);
					if(result != null) {
						this.writer.WriteLine(result.ToString());
					}
				} catch(ThreadAbortException) {
					this.writer.WriteLine("Script terminated by user");
				} catch(Exception exception) {
					this.writer.WriteLine(exception.Message);
				} finally {
					this.Prompt();
					this.thread = null;
				}
			};
			Tracer.Assert(this.thread == null);
			this.thread = new Thread(new ThreadStart(run));
			this.thread.SetApartmentState(ApartmentState.STA);
			this.thread.Name = "IronPython Thread";
			this.thread.IsBackground = true;
			this.thread.Priority = ThreadPriority.Normal;
			this.thread.Start();
		}

		private void Abort() {
			try {
				Thread t = this.thread;
				if(t != null) {
					t.Abort();
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private class LogWriter : TextWriter {
			private readonly TextBox textBox;
			private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

			public override Encoding Encoding => Encoding.Unicode;

			public LogWriter(TextBox textBox) : base(CultureInfo.InvariantCulture) {
				this.textBox = textBox;
			}

			private void Append(string text) {
				text = text.Replace("\r", "");
				if(!string.IsNullOrEmpty(text)) {
					bool invoke = this.queue.IsEmpty;
					this.queue.Enqueue(text);
					if(invoke) {
						this.textBox.Dispatcher.Invoke(
							new Action(() => {
								string str;
								while(this.queue.TryDequeue(out str)) {
									this.textBox.AppendText(str);
								}
								this.textBox.ScrollToEnd();
								this.textBox.Select(this.textBox.Text.Length, 0);
							})
						);
					}
				}
			}

			public override void Write(char value) {
				this.Append(new string(value, 1));
			}

			public override void Write(char[] buffer, int index, int count) {
				this.Append(new string(buffer, index, count));
			}

			public override void Write(string value) {
				this.Append(value);
			}
		}
	}
}
