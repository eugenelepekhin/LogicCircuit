using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private static string[] availableCultureNames = new string[] {
			"en-US",
			"ru-RU",
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
			if(!App.availableCultureNames.Contains(name)) {
				name = App.availableCultureNames[0];
			}
			return name;
		}
	}
}
