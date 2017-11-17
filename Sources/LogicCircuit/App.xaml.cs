using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.IO;
using CommandLineParser;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private const string SettingsCultureName = "Application.CultureInfo.Name";

		private static readonly string[] availableCultureNames = new string[] {
			"en",
			"ar",
			"de",
			"el",
			"es",
			"fa",
			"fr",
			"he",
			"hu",
			"it",
			"ja",
			"ko",
			"lt",
			"nl",
			"pl",
			"pt-BR",
			"pt-PT",
			"ru",
			"uk",
			"zh-Hans",
		};
		
		public static IEnumerable<CultureInfo> AvailableCultures {
			get { return App.availableCultureNames.Select(s => CultureInfo.GetCultureInfo(s)); }
		}

		private static SettingsStringCache currentCultureName = new SettingsStringCache(Settings.User, App.SettingsCultureName,
			App.DefaultCultureName(), App.availableCultureNames
		);

		public static CultureInfo CurrentCulture {
			get { return CultureInfo.GetCultureInfo(App.currentCultureName.Value); }
			set { App.currentCultureName.Value = App.ValidateCultureName((value ?? CultureInfo.GetCultureInfo(App.DefaultCultureName())).Name); }
		}

		internal static App CurrentApp { get { return (App)App.Current; } }
		internal static Mainframe Mainframe { get; set; }

		internal string FileToOpen { get; private set; }
		internal string ScriptToRun { get; private set; }
		internal string CommandLineErrors { get; private set; }

		protected override void OnStartup(StartupEventArgs e) {
			App.InitLogging();
			App.ValidateSettingsCulture();
			LogicCircuit.Properties.Resources.Culture = App.CurrentCulture;
			Tracer.FullInfo("App", "Starting with culture: {0}", App.CurrentCulture.Name);

			base.OnStartup(e);

			App.InitCommands();

			if(e != null && e.Args != null && 0 < e.Args.Length) {
				bool showHelp = false;
				CommandLine commandLine = new CommandLine()
					.AddFlag("help", "?", "Show this message", false, value => showHelp = value)
					.AddString("run", "r", "<script>", "IronPython script to run on startup", false, file => this.ScriptToRun = file)
				;
				string errors = commandLine.Parse(e.Args, files => this.FileToOpen = files.FirstOrDefault());

				if(!string.IsNullOrEmpty(errors)) {
					Tracer.FullInfo("App", "Errors parsing command line parameters: {0}", errors);
				}
				if(showHelp || errors != null) {
					this.CommandLineErrors = (
						(errors ?? "") +
						"\nLogicCircuit command line parameters:\n" + commandLine.Help() + "<CircuitProject file path> - open the file"
					).Trim();
				}
			}
			Tracer.FullInfo("App", "Application launched with file to open: \"{0}\"", this.FileToOpen);
			Tracer.FullInfo("App", "Application launched with script to run: \"{0}\"", this.ScriptToRun);
			EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent, new RoutedEventHandler(TextBoxGotKeyboardFocus));
			EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseDownEvent, new RoutedEventHandler(TextBoxPreviewMouseDown));

			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(UIElement), new FrameworkPropertyMetadata(10000));
		}

		private static void InitLogging() {
			SettingsBoolCache logging = new SettingsBoolCache(Settings.User, "Tracer.WriteToLogFile", false);
			Tracer.WriteToLogFile = logging.Value;
		}

		private static void ValidateSettingsCulture() {
			SettingsStringCache cultureName = new SettingsStringCache(Settings.User, App.SettingsCultureName, App.DefaultCultureName());
			string name = App.ValidateCultureName(cultureName.Value);
			if(name != cultureName.Value) {
				Tracer.FullInfo("App.ValidateSettingsCulture", "Replacing current settings culture {0} with {1}", cultureName.Value, name);
				App.currentCultureName.Value = name;
			}
		}

		private void TextBoxGotKeyboardFocus(object sender, RoutedEventArgs e) {
			try {
				TextBox textBox = sender as TextBox;
				if(textBox != null && !textBox.AcceptsReturn) {
					textBox.SelectAll();
				}
			} catch(Exception exception) {
				Tracer.Report("App.TextBoxGotKeyboardFocus", exception);
			}
		}

		private void TextBoxPreviewMouseDown(object sender, RoutedEventArgs e) {
			try {
				TextBox textBox = sender as TextBox;
				if(textBox != null && !textBox.AcceptsReturn && !textBox.IsFocused) {
					textBox.Focus();
					textBox.SelectAll();
					e.Handled = true;
				}
			} catch(Exception exception) {
				Tracer.Report("App.TextBoxPreviewMouseDown", exception);
			}
		}

		private static string ValidateCultureName(string cultureName) {
			if(!App.availableCultureNames.Contains(cultureName, StringComparer.OrdinalIgnoreCase)) {
				// Take language part of culture name: first two chars of "en-EN"
				string prefix = cultureName.Substring(0, Math.Min(cultureName.Length, 2));
				cultureName = App.availableCultureNames.FirstOrDefault(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ?? App.availableCultureNames[0];
			}
			return cultureName;
		}

		private static string DefaultCultureName() {
			return App.ValidateCultureName(CultureInfo.CurrentUICulture.Name);
		}

		private static void InitCommands() {
			// Text editor commands
			ApplicationCommands.Cut.Text = LogicCircuit.Properties.Resources.CommandEditCut;
			ApplicationCommands.Copy.Text = LogicCircuit.Properties.Resources.CommandEditCopy;
			ApplicationCommands.Paste.Text = LogicCircuit.Properties.Resources.CommandEditPaste;
			ApplicationCommands.Undo.Text = LogicCircuit.Properties.Resources.CommandEditUndo;
			ApplicationCommands.Redo.Text = LogicCircuit.Properties.Resources.CommandEditRedo;

			EditingCommands.ToggleBold.Text = LogicCircuit.Properties.Resources.CommandEditBold;
			EditingCommands.ToggleItalic.Text = LogicCircuit.Properties.Resources.CommandEditItalic;
			EditingCommands.ToggleUnderline.Text = LogicCircuit.Properties.Resources.CommandEditUnderline;
			EditingCommands.IncreaseFontSize.Text = LogicCircuit.Properties.Resources.CommandEditIncreaseFontSize;
			EditingCommands.DecreaseFontSize.Text = LogicCircuit.Properties.Resources.CommandEditDecreaseFontSize;

			EditingCommands.ToggleBullets.Text = LogicCircuit.Properties.Resources.CommandEditBullets;
			EditingCommands.ToggleNumbering.Text = LogicCircuit.Properties.Resources.CommandEditNumbering;

			EditingCommands.AlignLeft.Text = LogicCircuit.Properties.Resources.CommandEditAlignLeft;
			EditingCommands.AlignCenter.Text = LogicCircuit.Properties.Resources.CommandEditAlignCenter;
			EditingCommands.AlignRight.Text = LogicCircuit.Properties.Resources.CommandEditAlignRight;
			EditingCommands.AlignJustify.Text = LogicCircuit.Properties.Resources.CommandEditAlignJustify;
			EditingCommands.IncreaseIndentation.Text = LogicCircuit.Properties.Resources.CommandEditIncreaseIndentation;
			EditingCommands.DecreaseIndentation.Text = LogicCircuit.Properties.Resources.CommandEditDecreaseIndentation;
		}

		// Scripting support

		public static Editor Editor => App.Mainframe?.Editor;

		public static CircuitTester CreateTester(string circuitName) {
			if(string.IsNullOrEmpty(circuitName)) {
				throw new ArgumentNullException(nameof(circuitName));
			}
			Editor editor = App.Editor;
			if(editor == null) {
				throw new InvalidOperationException("Editor was not created yet");
			}
			LogicalCircuit circuit = editor.CircuitProject.LogicalCircuitSet.FindByName(circuitName);
			if(circuit == null) {
				throw new CircuitException(Cause.UserError, string.Format(CultureInfo.InvariantCulture, "Logical Circuit {0} not found", circuitName));
			}
			if(!CircuitTestSocket.IsTestable(circuit)) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Logical Circuit {0} is not testable. There are no any input or/and output pins on it.", circuitName)
				);
			}
			return new CircuitTester(editor, circuit);
		}

		public static void ClearConsole() {
			IronPythonConsole.Clear();
		}

		public static void Dispatch(Action action) {
			App.Mainframe.Dispatcher.Invoke(action);
		}

		public static void InTransaction(Action action) {
			App.Editor.CircuitProject.InTransaction(action);
		}

		public static void OpenFile(string fileName) {
			if(string.IsNullOrEmpty(fileName)) {
				throw new ArgumentNullException(nameof(fileName));
			}
			if(!Mainframe.IsFilePathValid(fileName) || !File.Exists(fileName)) {
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "File \"{0}\" does not exist", fileName));
			}
			App.Mainframe.Dispatcher.Invoke(() => App.Mainframe.Open(fileName));
		}
	}
}
