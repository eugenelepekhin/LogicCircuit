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
				if(this.circuitEditor != value) {
					this.circuitEditor = value;
					this.NotifyPropertyChanged("CircuitEditor");
				}
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
			ThreadPool.QueueUserWorkItem(o => {
				try {
					string file = App.CurrentApp.FileToOpen;
					if(string.IsNullOrEmpty(file) || !File.Exists(file)) {
						if(Settings.User.LoadLastFileOnStartup) {
							file = Settings.User.RecentFile();
						}
					}
					if(!string.IsNullOrEmpty(file) && File.Exists(file)) {
						this.Edit(file);
					} else {
						this.Edit(null);
					}
				} catch(Exception exception) {
					Tracer.Report("Mainframe.PostLoaded", exception);
					this.ReportException(exception);
					this.Edit(null);
				}
			});
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			base.OnClosing(e);
			try {
				if(!this.EnsureSaved()) {
					e.Cancel = true;
				} else {
					Settings.User.Save();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.OnClosing", exception);
				this.ReportException(exception);
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

		private void WindowLostFocus(object sender, RoutedEventArgs e) {
			try {
				//this.CircuitEditor.CanvasLostFocus(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameLostFocus", exception);
				this.ReportException(exception);
			}
		}

		private void WindowLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			try {
				//this.CircuitEditor.CanvasLostFocus(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameLostKeyboardFocus", exception);
				this.ReportException(exception);
			}
		}

		private void WindowKeyDown(object sender, KeyEventArgs e) {
			try {
				//this.CircuitEditor.CanvasKeyDown(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameKeyDown", exception);
				this.ReportException(exception);
			}
		}

		private void WindowKeyUp(object sender, KeyEventArgs e) {
			try {
				//this.CircuitEditor.CanvasKeyUp(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameKeyUp", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDragEnter(object sender, DragEventArgs e) {
			try {
				//this.CircuitEditor.CanvasDragOver(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDragEnter", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDragOver(object sender, DragEventArgs e) {
			try {
				//this.CircuitEditor.CanvasDragOver(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDragOver", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDragLeave(object sender, DragEventArgs e) {
			try {
				//this.CircuitEditor.CanvasDragLeave(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDragLeave", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDrop(object sender, DragEventArgs e) {
			try {
				//this.CircuitEditor.CanvasDrop(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDrop", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				//this.CircuitEditor.CanvasMouseDown(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseDown", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseUp(object sender, MouseButtonEventArgs e) {
			try {
				//this.CircuitEditor.CanvasMouseUp(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseUp", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseMove(object sender, MouseEventArgs e) {
			try {
				//this.CircuitEditor.CanvasMouseMove(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseMove", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramLostFocus(object sender, RoutedEventArgs e) {
			try {
				//this.CircuitEditor.CanvasLostFocus(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramLostFocus", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			try {
				//this.CircuitEditor.CanvasLostFocus(sender, e);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramLostKeyboardFocus", exception);
				this.ReportException(exception);
			}
		}

		private void PowerButtonMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				this.CircuitEditor.Power = (this.PowerSwitch.IsChecked.Value == false);
			} catch(Exception exception) {
				Tracer.Report("MainFrame.PowerButtonMouseDown", exception);
				this.ReportException(exception);
			}
		}
	}
}
