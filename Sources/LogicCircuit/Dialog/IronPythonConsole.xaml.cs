using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for IronPythonConsole.xaml
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public sealed partial class IronPythonConsole : Window {
		private static IronPythonConsole currentConsole = null;

		internal static void Run(Window parent) {
			if(IronPythonConsole.currentConsole == null) {
				IronPythonConsole console = new IronPythonConsole();
				console.Owner = parent;
				IronPythonConsole.currentConsole = console;
				console.Show();
			} else {
				IronPythonConsole.currentConsole.Focus();
			}
		}

		internal static void Run(Window parent, string script) {
			IronPythonConsole.Run(parent);
			IronPythonConsole console = IronPythonConsole.currentConsole;
			if(console != null) {
				console.writer.WriteLine(script);
				console.Execute(script);
			}
		}

		internal static void Stop() {
			if(IronPythonConsole.currentConsole != null) {
				IronPythonConsole.currentConsole.Close();
			}
		}

		internal static void Clear() {
			IronPythonConsole console = IronPythonConsole.currentConsole;
			if(console != null) {
				console.Dispatcher.Invoke(() => console.console.Clear());
			}
		}

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private ScriptEngine scriptEngine;
		private MemoryStream stdout;
		private LogWriter writer;
		private MemoryStream stdin;
		private LogReader reader;
		private ScriptScope scope;
		private StringBuilder command = new StringBuilder();
		private Thread thread;

		private string suggestionExpr = null;
		private List<string> suggestions = null;
		private int lastSuggestion = 0;

		private IronPythonConsole() {
			this.DataContext = this;
			this.InitializeComponent();

			this.scriptEngine = Python.CreateEngine();
			this.stdout = new MemoryStream();
			this.writer = new LogWriter(this.console);
			this.stdin = new MemoryStream();
			this.reader = new LogReader(this.console);
			this.console.CommandEnter = this.Execute;
			this.console.CommandBreak = this.Abort;
			this.console.CommandSuggestion = this.Suggest;

			this.scriptEngine.Runtime.IO.SetOutput(this.stdout, this.writer);
			this.scriptEngine.Runtime.IO.SetInput(this.stdin, this.reader, Encoding.UTF8);
			this.scope = this.scriptEngine.CreateScope();

			this.scope.ImportModule("clr");
			this.scriptEngine.Execute("import clr", this.scope);
			this.scriptEngine.Execute("import System", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Core\")", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"System.Linq\")", this.scope);
			this.scriptEngine.Execute("clr.ImportExtensions(System.Linq)", this.scope);
			this.scriptEngine.Execute("clr.AddReference(\"LogicCircuit\")", this.scope);
			this.scriptEngine.Execute("from LogicCircuit import *", this.scope);

			this.writer.WriteLine(this.scriptEngine.Setup.DisplayName);
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
			this.stdin.Close();
			this.reader.Close();
		}

		private void Prompt() {
			string text = (0 < this.command.Length) ? "... " : ">>> ";
			this.Dispatcher.BeginInvoke(new Action(() => this.console.Prompt(true, text)), DispatcherPriority.ApplicationIdle);
		}

		private bool CanExecute(string text) {
			CompilerOptions options = this.scriptEngine.GetCompilerOptions(this.scope);
			ScriptCodeParseResult result = this.scriptEngine.CreateScriptSourceFromString(text, SourceCodeKind.InteractiveCode).GetCodeProperties(options);
			return (result == ScriptCodeParseResult.Complete || result == ScriptCodeParseResult.Empty || result == ScriptCodeParseResult.Invalid);
		}

		private void Execute(string text) {
			this.suggestions = null;
			bool isEmpty = string.IsNullOrEmpty(text);
			this.command.AppendLine(text);
			text = this.command.ToString();
			if(isEmpty && !string.IsNullOrEmpty(text) || this.CanExecute(text)) {
				this.command.Length = 0;
				this.Start(text);
			} else {
				this.Prompt();
			}
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
					this.writer.WriteLine("{0}: {1}", exception.GetType().Name, exception.Message);
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

		private bool Abort() {
			this.suggestions = null;
			try {
				Thread t = this.thread;
				if(t != null) {
					t.Abort();
					return true;
				}
				if(0 < this.command.Length) {
					this.command.Length = 0;
					this.Prompt();
					return true;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
			return false;
		}

		private string Suggest(string text) {
			string expr = text;
			for(int i = text.Length; 0 < i; i--) {
				char c = text[i - 1];
				if(!char.IsLetterOrDigit(c) && c != '.' && c != '_') {
					expr = text.Substring(i);
					break;
				}
			}
			if(!string.IsNullOrWhiteSpace(expr)) {
				try {
					int index = expr.LastIndexOf('.');
					string prefix = expr.Substring(0, Math.Max(0, index));
					string sofar = (index < 0) ? expr : expr.Substring(index + 1);
					List<string> list;
					if(this.suggestionExpr != expr || suggestions == null) {
						if(string.IsNullOrWhiteSpace(prefix)) {
							list = this.scope.GetVariableNames().Where(name => name.StartsWith(sofar, StringComparison.Ordinal)).OrderBy(name => name.Length).ToList();
						} else {
							object obj = this.scriptEngine.CreateScriptSourceFromString(prefix, SourceCodeKind.Expression).Execute(this.scope);
							list = this.scriptEngine.Operations.GetMemberNames(obj).Where(name => name.StartsWith(sofar, StringComparison.Ordinal)).ToList();
						}
						this.lastSuggestion = 0;
					} else {
						list = this.suggestions;
						this.lastSuggestion++;
						if(list.Count <= this.lastSuggestion) {
							this.lastSuggestion = 0;
						}
					}
					this.suggestions = list;
					this.suggestionExpr = expr;
					if(list != null && this.lastSuggestion < list.Count) {
						string current = list[this.lastSuggestion];
						return current.Substring(sofar.Length);
					}
				} catch(Exception exception) {
					Tracer.Report("IronPythonConsole.Suggest", exception);
				}
			}
			
			return string.Empty;
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
								bool scroll = (this.textBox.SelectionStart == this.textBox.Text.Length);
								string str;
								while(this.queue.TryDequeue(out str)) {
									this.textBox.AppendText(str);
								}
								if(scroll) {
									this.textBox.ScrollToEnd();
									this.textBox.Select(this.textBox.Text.Length, 0);
								}
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

		private class LogReader : TextReader {
			private readonly ScriptConsole console;
			private StringReader stringReader;

			public LogReader(ScriptConsole console) : base() {
				this.console = console;
			}

			public override string ReadLine() {
				Action<string> oldEnter = this.console.CommandEnter;
				try {
					string line = null;
					using(AutoResetEvent waiter = new AutoResetEvent(false)) {
						this.console.CommandEnter = text => {
							line = text;
							waiter.Set();
						};
						this.console.Dispatcher.Invoke(() => this.console.Prompt(false, string.Empty));
						waiter.WaitOne();
					}
					return line;
				} finally {
					this.console.CommandEnter = oldEnter;
				}
			}

			public override int Read() {
				if(this.stringReader == null) {
					string line = this.ReadLine();
					if(string.IsNullOrEmpty(line)) {
						return -1;
					}
					this.stringReader = new StringReader(line + "\n");
				}
				int i = this.stringReader.Read();
				if(i < 0) {
					this.stringReader.Close();
					this.stringReader = null;
				}
				return i;
			}
		}
	}
}
