using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private static readonly string[] availableCultureNames = new string[] {
			"en-US",
			"ar",
			"es",
			"fr",
			"it-IT",
			"ru-RU",
			"uk-UA",
		};
		
		public static IEnumerable<CultureInfo> AvailableCultures {
			get { return App.availableCultureNames.Select(s => CultureInfo.GetCultureInfo(s)); }
		}

		private static SettingsStringCache currentCultureName = new SettingsStringCache(Settings.User, "Application.CultureInfo.Name",
			App.DefaultCultureName(), App.availableCultureNames
		);

		public static CultureInfo CurrentCulture {
			get { return CultureInfo.GetCultureInfo(App.currentCultureName.Value); }
			set { App.currentCultureName.Value = (value ?? CultureInfo.GetCultureInfo(App.DefaultCultureName())).Name; }
		}

		public static App CurrentApp { get { return (App)App.Current; } }
		public static Mainframe Mainframe { get; set; }

		public string FileToOpen { get; private set; }

		protected override void OnStartup(StartupEventArgs e) {
			LogicCircuit.Properties.Resources.Culture = App.CurrentCulture;

			base.OnStartup(e);

			App.InitCommands();

			if(e != null && e.Args != null && 0 < e.Args.Length && !string.IsNullOrEmpty(e.Args[0])) {
				this.FileToOpen = e.Args[0];
			}
			EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent, new RoutedEventHandler(TextBoxGotKeyboardFocus));
			EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseDownEvent, new RoutedEventHandler(TextBoxPreviewMouseDown));
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

		private static string DefaultCultureName() {
			string name = CultureInfo.CurrentUICulture.Name;
			if(!App.availableCultureNames.Contains(name, StringComparer.OrdinalIgnoreCase)) {
				// Take language part of culture name: first two chars of "en-EN"
				string prefix = name.Substring(0, Math.Min(name.Length, 2));
				Tracer.Assert(prefix.Length == 2);
				name = App.availableCultureNames.FirstOrDefault(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ?? App.availableCultureNames[0];
			}
			return name;
		}

		private static void InitCommands() {
			ApplicationCommands.New.Text = LogicCircuit.Properties.Resources.CommandFileNew;
			ApplicationCommands.Open.Text = LogicCircuit.Properties.Resources.CommandFileOpen;
			ApplicationCommands.Save.Text = LogicCircuit.Properties.Resources.CommandFileSave;
			ApplicationCommands.SaveAs.Text = LogicCircuit.Properties.Resources.CommandFileSaveAs;
			ApplicationCommands.Close.Text = LogicCircuit.Properties.Resources.CommandFileClose;

			ApplicationCommands.Undo.Text = LogicCircuit.Properties.Resources.CommandEditUndo;
			ApplicationCommands.Redo.Text = LogicCircuit.Properties.Resources.CommandEditRedo;
			ApplicationCommands.Cut.Text = LogicCircuit.Properties.Resources.CommandEditCut;
			ApplicationCommands.Copy.Text = LogicCircuit.Properties.Resources.CommandEditCopy;
			ApplicationCommands.Paste.Text = LogicCircuit.Properties.Resources.CommandEditPaste;
			ApplicationCommands.Delete.Text = LogicCircuit.Properties.Resources.CommandEditDelete;
			ApplicationCommands.SelectAll.Text = LogicCircuit.Properties.Resources.CommandEditSelectAll;

			ApplicationCommands.Help.Text = LogicCircuit.Properties.Resources.CommandHelpView;

			// Text editor commands
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
	}
}
