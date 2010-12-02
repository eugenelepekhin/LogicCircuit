using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for Mainframe.xaml
	/// </summary>
	public partial class Mainframe : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private SettingsBoolCache showGrid = new SettingsBoolCache(Settings.User, "Settings.ShowGrid", true);
		public bool ShowGrid {
			get { return this.showGrid.Value; }
			set {
				this.showGrid.Value = value;
				this.NotifyPropertyChanged("ShowGrid");
			}
		}

		private CircuitEditor circuitEditor;
		public CircuitEditor CircuitEditor {
			get { return this.circuitEditor; }
			set {
				this.circuitEditor = value;
				this.NotifyPropertyChanged("CircuitEditor");
			}
		}

		private string statusText = LogicCircuit.Resources.Loading;
		private volatile bool statusChanged = false;
		public string Status {
			get { return this.statusText; }
			set {
				string text = (value == null) ? string.Empty : value;
				if(0 < text.Length) {
					text = Regex.Replace(text.Trim(), @"\s+", " ");
				}
				if(this.statusText != text) {
					this.statusText = text;
					if(!this.statusChanged) {
						this.statusChanged = true;
						this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
							new Action(() => {
								this.statusChanged = false;
								this.NotifyPropertyChanged("Status");
							})
						);
					}
				}
			}
		}

		public Mainframe() {
			this.DataContext = this;
			this.InitializeComponent();
			this.Loaded += new RoutedEventHandler(this.MainFrameLoaded);
		}

		private void MainFrameLoaded(object sender, RoutedEventArgs e) {
			this.Dispatcher.BeginInvoke(new Action(this.PostLoaded), DispatcherPriority.Normal);
		}

		private void PostLoaded() {
			try {
				string file = App.CurrentApp.FileToOpen;
				if(string.IsNullOrEmpty(file) || !File.Exists(file)) {
					if(Settings.User.LoadLastFileOnStartup) {
						file = Settings.User.RecentFile();
					}
				}
				if(!string.IsNullOrEmpty(file) && File.Exists(file)) {
					this.Load(file);
				} else {
					this.New();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.PostLoaded", exception);
				this.ReportException(exception);
				this.New();
			}
		}

		public void NotifyPropertyChanged(PropertyChangedEventHandler handler, object sender, string propertyName) {
			if(handler != null) {
				if(this.Dispatcher.Thread != Thread.CurrentThread) {
					this.Dispatcher.BeginInvoke(new Action<PropertyChangedEventHandler, object, string>(this.NotifyPropertyChanged),
						DispatcherPriority.Normal, handler, sender, propertyName
					);
				} else {
					handler(sender, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.NotifyPropertyChanged(this.PropertyChanged, this, propertyName);
		}

		private void ShowErrorMessage(string message, string details) {
			if(!this.Dispatcher.CheckAccess()) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					new Action<string, string>(this.ShowErrorMessage), message, details
				);
			} else {
				DialogMessage.Show(this,
					LogicCircuit.Resources.MainFrameCaption(null), message, details, MessageBoxImage.Error, MessageBoxButton.OK
				);
				this.CircuitEditor.Power = false;
			}
		}

		public void ReportException(Exception exception) {
			CircuitException circuitException = exception as CircuitException;
			if(circuitException != null && circuitException.Cause == Cause.UserError) {
				this.ShowErrorMessage(exception.Message, null);
			} else {
				this.ShowErrorMessage(exception.Message, exception.ToString());
			}
		}
	}
}
