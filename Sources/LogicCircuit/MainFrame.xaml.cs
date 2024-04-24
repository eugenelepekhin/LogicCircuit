﻿// Ignore Spelling: Hdl

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for Mainframe.xaml
	/// </summary>
	public sealed partial class Mainframe : Window, INotifyPropertyChanged, IDisposable {

		public event PropertyChangedEventHandler? PropertyChanged;

		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this, double.NaN, double.NaN)); } }

		public SettingsGridLengthCache ProjectWidth { get; private set; }
		public SettingsGridLengthCache DiagramWidth { get; private set; }

		private readonly SettingsBoolCache showGrid = new SettingsBoolCache(Settings.User, "Settings.ShowGrid", true);
		public bool ShowGrid {
			get { return this.showGrid.Value; }
			set {
				this.showGrid.Value = value;
				this.NotifyPropertyChanged(nameof(this.ShowGrid));
			}
		}

		private readonly SettingsBoolCache isDiagramBackgroundWhite = new SettingsBoolCache(Settings.User, "Settings.IsDiagramBackgroundWhite", false);
		public bool IsDiagramBackgroundWhite {
			get => this.isDiagramBackgroundWhite.Value;
			set => this.isDiagramBackgroundWhite.Value = value;
		}
		public Brush DiagramBackground => this.IsDiagramBackgroundWhite ? Brushes.White : (Brush)Application.Current.Resources["DiagramBackground"];

		private Editor? editor;
		public Editor? Editor {
			get { return this.editor; }
			set {
				if(this.editor != value) {
					this.editor = value;
					this.NotifyPropertyChanged(nameof(this.Editor));
				}
			}
		}
		private SettingsIntegerCache autoSaveInterval = new SettingsIntegerCache(Settings.User, "Settings.AutoSaveInterval", 0, 15 * 60, 0);
		public int AutoSaveInterval {
			get { return this.autoSaveInterval.Value; }
			set {
				if(this.autoSaveInterval.Value != value) {
					this.autoSaveInterval.Value = value;
					this.ResetAutoSaveTimer();
				}
			}
		}

		private Timer? autoSaveTimer;
		private LogicalCircuit LogicalCircuit() { return this.Editor!.Project.LogicalCircuit; }

		private string statusText = Properties.Resources.Loading;
		private volatile bool statusChanged;
		public string Status {
			get { return this.statusText; }
			set {
				string text = value ?? string.Empty;
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
								this.NotifyPropertyChanged(nameof(this.Status));
							})
						);
					}
				}
			}
		}

		public Point ScrollOffset {
			get {
				ScrollViewer scrollViewer = this.DiagramScroll;
				return new Point(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
			}
			set {
				ScrollViewer scrollViewer = this.DiagramScroll;
				scrollViewer.ScrollToHorizontalOffset(value.X);
				scrollViewer.ScrollToVerticalOffset(value.Y);
			}
		}
		public LambdaUICommand CommandNew => new LambdaUICommand(Properties.Resources.CommandFileNew, o => this.New(), new KeyGesture(Key.N, ModifierKeys.Control)) {
			IconPath = "Icon/FileNew.xaml"
		};
		public LambdaUICommand CommandOpen => new LambdaUICommand(Properties.Resources.CommandFileOpen, o => this.Open(), new KeyGesture(Key.O, ModifierKeys.Control)) {
			IconPath = "Icon/FileOpen.xaml"
		};
		public LambdaUICommand CommandOpenRecent { get; private set; }
		public LambdaUICommand CommandSave => new LambdaUICommand(Properties.Resources.CommandFileSave, o => this.Save(), new KeyGesture(Key.S, ModifierKeys.Control)) {
			IconPath = "Icon/FileSave.xaml"
		};
		public LambdaUICommand CommandSaveAs => new LambdaUICommand(Properties.Resources.CommandFileSaveAs, o => this.SaveAs()) {
			IconPath = "Icon/FileSaveAs.xaml"
		};
		public LambdaUICommand CommandImport => new LambdaUICommand(Properties.Resources.CommandFileFileImport, o => this.Editor != null && this.Editor.InEditMode, o => this.Import()) {
			IconPath = "Icon/FileImport.xaml"
		};
		public LambdaUICommand CommandExportImage => new LambdaUICommand(Properties.Resources.CommandFileExportImage,
			o => this.Editor != null && !this.LogicalCircuit().IsEmpty(),
			o => this.ShowDialog(new DialogExportImage(this.Editor!))
		) {
			IconPath = "Icon/FileExportImage.xaml"
		};
		public LambdaUICommand CommandExportHdl => new LambdaUICommand(Properties.Resources.CommandFileExportHdl,
			o => this.Editor != null && !this.LogicalCircuit().IsEmpty(),
			o => this.ShowDialog(new DialogExportHdl(this.Editor!))
		);
		public LambdaUICommand CommandClose => new LambdaUICommand(Properties.Resources.CommandFileClose, o => this.Close(), new KeyGesture(Key.F4, ModifierKeys.Alt)) {
			IconPath = "Icon/FileClose.xaml"
		};
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public LambdaUICommand CommandHelp => new LambdaUICommand(
			Properties.Resources.CommandHelpView,
			o => Process.Start(new ProcessStartInfo(Properties.Resources.HelpContent) { UseShellExecute = true }),
			new KeyGesture(Key.F1)
		) {
			IconPath = "Icon/F1Help.xaml"
		};
		public LambdaUICommand CommandAbout => new LambdaUICommand(Properties.Resources.CommandHelpAbout, o => this.ShowDialog(new DialogAbout()));
		public LambdaUICommand CommandOptions => new LambdaUICommand(Properties.Resources.CommandToolsOptions, o => this.Editor != null && this.Editor.InEditMode, o => {
			bool? result = this.ShowDialog(new DialogOptions(this));
			if(result.HasValue && result.Value) {
				this.NotifyPropertyChanged(nameof(this.Editor));
				this.NotifyPropertyChanged(nameof(this.DiagramBackground));
				this.Editor!.FullRefresh();
			}
		}) {
			IconPath = "Icon/ToolsOptions.xaml"
		};
		public LambdaUICommand CommandIronPython => new LambdaUICommand(Properties.Resources.CommandToolsIronPython, o => this.Editor != null, o => IronPythonConsole.Run(this)) {
			IconPath = "Icon/PythonConsole.xaml"
		};

		public Mainframe() {
			App.Mainframe = this;
			this.ProjectWidth = new SettingsGridLengthCache(Settings.User, "Mainframe.ProjectWidth", "0.25*");
			this.DiagramWidth = new SettingsGridLengthCache(Settings.User, "Mainframe.DiagramWidth", "0.75*");

			// Create this command here as it used multiple times not like all other commands only onces when menu is created.
			this.CommandOpenRecent = new LambdaUICommand(Properties.Resources.CommandFileOpenRecent, file => this.OpenRecent((file as string)!));

			this.DataContext = this;
			this.InitializeComponent();

			this.autoSaveTimer = new Timer(o => this.Editor?.AutoSave(), null, Timeout.Infinite, Timeout.Infinite);

			Thread thread = new Thread(new ThreadStart(() => {
				try {
					string? file = App.CurrentApp.FileToOpen;
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
					if(!string.IsNullOrEmpty(App.CurrentApp.CommandLineErrors)) {
						this.ShowErrorMessage(App.CurrentApp.CommandLineErrors, null);
					} else if(!string.IsNullOrEmpty(App.CurrentApp.ScriptToRun)) {
						this.Dispatcher.BeginInvoke(new Action(() => IronPythonConsole.Run(this, App.CurrentApp.ScriptToRun)));
					} else {
						// Reuse this thread to check if there are any translations requests are pending
						DialogAbout.CheckTranslationRequests(this.Dispatcher);
					}
				} catch(Exception exception) {
					Tracer.Report("Mainframe.PostLoaded", exception);
					this.ReportException(exception);
					this.Edit(null);
				}
			}));
			//TextNote validator will instantiate FlowDocument that in some cases required to happened only on STA.
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Name = "ProjectLoader";
			thread.Priority = ThreadPriority.AboveNormal;
			thread.Start();

			// Check for new available version
			Thread versionThread = new Thread(new ThreadStart(() => DialogAbout.CheckVersion(this.Dispatcher))) {
				IsBackground = true,
				Name = "CheckVersion",
				Priority = ThreadPriority.BelowNormal
			};
			versionThread.Start();

			#if DEBUG && false
				this.Loaded += (object sender, RoutedEventArgs e) => {
					Menu menu = (Menu)((Grid)this.Content).Children[0];
					string text = "Test";
					menu.Items.Add(new MenuItem() { Header = text, Command = new LambdaUICommand(text, o => {
						DialogMessage.Show(this,
							"hello",
							"world <Hyperlink NavigateUri=\"http://www.rbc.ru\">link 1</Hyperlink> or <Hyperlink NavigateUri=\"http://www.lenta.ru\">link 2</Hyperlink> hello <Hyperlink NavigateUri=\"http://www.cnn.com\">and link 3</Hyperlink> end",
							"details",
							MessageBoxImage.Error,
							MessageBoxButton.OKCancel
						);
					})});
				};
			#endif
		}

		public void Dispose() {
			this.autoSaveTimer?.Dispose();
			this.autoSaveTimer = null;
		}

		internal void ResetAutoSaveTimer() {
			int interval = this.AutoSaveInterval;
			interval = (interval == 0) ? Timeout.Infinite : interval * 1000;
			this.autoSaveTimer!.Change(interval, interval);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			base.OnClosing(e);
			try {
				if(!this.EnsureSaved()) {
					e.Cancel = true;
				} else {
					IronPythonConsole.Stop();
					Settings.User.Save();
					this.autoSaveTimer?.Dispose();
					this.autoSaveTimer = null;
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.OnClosing", exception);
				this.ReportException(exception);
			}
		}

		internal void NotifyPropertyChanged(PropertyChangedEventHandler? handler, object sender, string propertyName) {
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

		private void ShowMessage(string message, string? details, MessageBoxImage messageBoxImage) {
			if(!this.Dispatcher.CheckAccess()) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					new Action<string, string, MessageBoxImage>(this.ShowMessage), message, details, messageBoxImage
				);
			} else {
				DialogMessage.Show(this,
					Properties.Resources.MainFrameCaption(null), message, details, messageBoxImage, MessageBoxButton.OK
				);
				if(this.Editor != null && this.Editor.Power && messageBoxImage == MessageBoxImage.Error) {
					this.Editor.Power = false;
				}
			}
		}

		private void ShowErrorMessage(string message, string? details) {
			this.ShowMessage(message, details, MessageBoxImage.Error);
		}

		public void ErrorMessage(string message, Exception exception) {
			this.ShowErrorMessage(message, exception.ToString());
		}

		public void ErrorMessage(string message) {
			this.ShowErrorMessage(message, null);
		}

		public void InformationMessage(string message) {
			this.ShowMessage(message, null, MessageBoxImage.Information);
		}

		public void ReportException(Exception exception) {
			if(exception is CircuitException circuitException && circuitException.Cause == Cause.UserError) {
				this.ShowErrorMessage(exception.Message, null);
			} else {
				this.ShowErrorMessage(exception.Message, exception.ToString());
			}
		}

		public bool? ShowDialog(Window window) {
			window.Owner = this;
			return window.ShowDialog();
		}

		private void WindowKeyDown(object sender, KeyEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramKeyDown(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameKeyDown", exception);
				this.ReportException(exception);
			}
		}

		private void WindowKeyUp(object sender, KeyEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramKeyUp(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.MainFrameKeyUp", exception);
				this.ReportException(exception);
			}
		}

		private void DescriptorMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				if(e.ChangedButton == MouseButton.Left && this.Editor != null) {
					this.Editor.DescriptorMouseDown((FrameworkElement)sender, e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DescriptorMouseDown", exception);
				this.ReportException(exception);
			}
		}

		private void DescriptorMouseUp(object sender, MouseButtonEventArgs e) {
			try {
				if(e.ChangedButton == MouseButton.Left && this.Editor != null) {
					this.Editor.DescriptorMouseUp();
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DescriptorMouseUp", exception);
				this.ReportException(exception);
			}
		}

		private void DescriptorMouseMove(object sender, MouseEventArgs e) {
			try {
				if(e.LeftButton == MouseButtonState.Pressed && this.Editor != null) {
					this.Editor.DescriptorMouseMove((FrameworkElement)sender, e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DescriptorMouseMove", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDragOver(object sender, DragEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramDragOver(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDragOver", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramDrop(object sender, DragEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramDrop(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramDrop", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				Tracer.Assert(sender == this.Diagram);
				if(e.ChangedButton == MouseButton.Left && this.Editor != null) {
					this.Editor.DiagramMouseDown(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseDown", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseUp(object sender, MouseButtonEventArgs e) {
			try {
				Tracer.Assert(sender == this.Diagram);
				if(e.ChangedButton == MouseButton.Left && this.Editor != null) {
					this.Editor.DiagramMouseUp(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseUp", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseMove(object sender, MouseEventArgs e) {
			try {
				Tracer.Assert(sender == this.Diagram);
				if(e.LeftButton == MouseButtonState.Pressed && this.Editor != null) {
					this.Editor.DiagramMouseMove(e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseMove", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramMouseWheel(object sender, MouseWheelEventArgs e) {
			try {
				Tracer.Assert(sender == this.Diagram);
				if(this.Editor != null) {
					this.Editor.DiagramMouseWheel(this.DiagramScroll, e);
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramMouseMove", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramLostFocus(object sender, RoutedEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramLostFocus();
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramLostFocus", exception);
				this.ReportException(exception);
			}
		}

		private void DiagramLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.DiagramLostFocus();
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.DiagramLostKeyboardFocus", exception);
				this.ReportException(exception);
			}
		}

		private void PowerButtonMouseDown(object sender, MouseButtonEventArgs e) {
			try {
				Editor? current = this.Editor;
				if(e.ChangedButton == MouseButton.Left && current != null) {
					current.Power = !current.Power;
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.PowerButtonMouseDown", exception);
				this.ReportException(exception);
			}
		}

		private static TreeViewItem? Container(TreeView treeView, CircuitMap map) {
			Tracer.Assert(map);
			if(map.Parent == null) {
				return (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(map);
			} else {
				TreeViewItem? parent = Mainframe.Container(treeView, map.Parent);
				if(parent != null) {
					parent.IsExpanded = true;
					return (TreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(map);
				} else {
					return null;
				}
			}
		}

		private void RunningMapDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.Power && e.ChangedButton == MouseButton.Left) {
					if(sender is TreeView treeView && treeView.SelectedItem != null) {
						if(treeView.SelectedItem is CircuitMap map) {
							this.Editor.OpenLogicalCircuit(map);
							TreeViewItem? item = Mainframe.Container(treeView, map);
							if(item != null) {
								item.IsExpanded = true;
							}
							e.Handled = true;
						}
					}
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.RunningMapDoubleClick", exception);
				this.ReportException(exception);
			}
		}

		private void RunningMapTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			try {
				if(sender == e.OriginalSource && e.NewValue != null && !object.ReferenceEquals(e.OldValue, e.NewValue)) {
					if(e.NewValue is CircuitMap map) {
						TreeViewItem? item = Mainframe.Container((TreeView)sender, map);
						if(item != null) {
							item.IsExpanded = true;
							item.BringIntoView();
						}
					}
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.RunningMapTreeViewSelectedItemChanged", exception);
				this.ReportException(exception);
			}
		}
	}
}
