using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for Mainframe.xaml
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class Mainframe : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this, double.NaN, double.NaN)); } }

		public SettingsGridLengthCache ProjectWidth { get; private set; }
		public SettingsGridLengthCache DiagramWidth { get; private set; }

		private SettingsBoolCache showGrid = new SettingsBoolCache(Settings.User, "Settings.ShowGrid", true);
		public bool ShowGrid {
			get { return this.showGrid.Value; }
			set {
				this.showGrid.Value = value;
				this.NotifyPropertyChanged("ShowGrid");
			}
		}

		private Editor editor;
		public Editor Editor {
			get { return this.editor; }
			set {
				if(this.editor != value) {
					this.editor = value;
					this.NotifyPropertyChanged("Editor");
				}
			}
		}
		private LogicalCircuit LogicalCircuit() { return this.Editor.Project.LogicalCircuit; }

		private string statusText = Properties.Resources.Loading;
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
		public LambdaUICommand CommandNew => new LambdaUICommand(Properties.Resources.CommandFileNew, o => this.New(), new KeyGesture(Key.N, ModifierKeys.Control));
		public LambdaUICommand CommandOpen => new LambdaUICommand(Properties.Resources.CommandFileOpen, o => this.Open(), new KeyGesture(Key.O, ModifierKeys.Control));
		public LambdaUICommand CommandOpenRecent { get; private set; }
		public LambdaUICommand CommandSave => new LambdaUICommand(Properties.Resources.CommandFileSave, o => this.Save(), new KeyGesture(Key.S, ModifierKeys.Control));
		public LambdaUICommand CommandSaveAs => new LambdaUICommand(Properties.Resources.CommandFileSaveAs, o => this.SaveAs());
		public LambdaUICommand CommandImport => new LambdaUICommand(Properties.Resources.CommandFileFileImport, o => this.Editor != null && this.Editor.InEditMode, o => this.Import());
		public LambdaUICommand CommandExportImage => new LambdaUICommand(Properties.Resources.CommandFileExportImage,
			o => this.Editor != null && !this.LogicalCircuit().IsEmpty(),
			o => this.ShowDialog(new DialogExportImage(this.Editor))
		);
		public LambdaUICommand CommandClose => new LambdaUICommand(Properties.Resources.CommandFileClose, o => this.Close(), new KeyGesture(Key.F4, ModifierKeys.Alt));
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public LambdaUICommand CommandHelp => new LambdaUICommand(Properties.Resources.CommandHelpView, o => Process.Start(Properties.Resources.HelpContent), new KeyGesture(Key.F1));
		public LambdaUICommand CommandAbout => new LambdaUICommand(Properties.Resources.CommandHelpAbout, o => this.ShowDialog(new DialogAbout()));
		public LambdaUICommand CommandOptions => new LambdaUICommand(Properties.Resources.CommandToolsOptions, o => this.Editor != null && this.Editor.InEditMode, o => {
			bool? result = this.ShowDialog(new DialogOptions(this));
			if(result.HasValue && result.Value) {
				this.NotifyPropertyChanged("Editor");
				this.Editor.FullRefresh();
			}
		});
		public LambdaUICommand CommandIronPython => new LambdaUICommand(Properties.Resources.CommandToolsIronPython, o => this.Editor != null, o => ScriptConsole.Run(this));

		public Mainframe() {
			App.Mainframe = this;
			this.ProjectWidth = new SettingsGridLengthCache(Settings.User, "Mainframe.ProjectWidth", "0.25*");
			this.DiagramWidth = new SettingsGridLengthCache(Settings.User, "Mainframe.DiagramWidth", "0.75*");

			// Create this command here as it used multiple times not like all other commands only onces when menu is created.
			this.CommandOpenRecent = new LambdaUICommand(Properties.Resources.CommandFileOpenRecent, file => this.OpenRecent(file as string));

			this.DataContext = this;
			this.InitializeComponent();

			Thread thread = new Thread(new ThreadStart(() => {
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
					// Reuse this thread to check if there are any translations requests are pending
					DialogAbout.CheckTranslationRequests(this.Dispatcher);
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
			Thread versionThread = new Thread(new ThreadStart(() => DialogAbout.CheckVersion(this.Dispatcher)));
			versionThread.IsBackground = true;
			versionThread.Name = "CheckVersion";
			versionThread.Priority = ThreadPriority.BelowNormal;
			versionThread.Start();

			#if DEBUG && false
				this.Loaded += (object sender, RoutedEventArgs e) => {
					Menu menu = (Menu)((Grid)this.Content).Children[0];
					string text = "Test";
					menu.Items.Add(new MenuItem() { Header = text, Command = new LambdaUICommand(text, o => {
						DialogMessage.Show(this, "hello", "world", "details", MessageBoxImage.Error, MessageBoxButton.OKCancel);
					})});
				};
			#endif
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			base.OnClosing(e);
			try {
				if(!this.EnsureSaved()) {
					e.Cancel = true;
				} else {
					ScriptConsole.Stop();
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

		private void ShowMessage(string message, string details, MessageBoxImage messageBoxImage) {
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

		private void ShowErrorMessage(string message, string details) {
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
			CircuitException circuitException = exception as CircuitException;
			if(circuitException != null && circuitException.Cause == Cause.UserError) {
				this.ShowErrorMessage(exception.Message, null);
			} else {
				this.ShowErrorMessage(exception.Message, exception.ToString());
			}
		}

		public bool? ShowDialog(Window window) {
			window.Owner = this;
			return window.ShowDialog();
		}

		public CircuitTester CreateCircuitTester(string circuitName) {
			if(string.IsNullOrEmpty(circuitName)) {
				throw new ArgumentNullException(nameof(circuitName));
			}
			Editor e = this.Editor;
			if(e == null) {
				throw new InvalidOperationException("Editor was not created yet");
			}
			LogicalCircuit circuit = e.CircuitProject.LogicalCircuitSet.FindByName(circuitName);
			if(circuit == null) {
				throw new CircuitException(Cause.UserError, string.Format(CultureInfo.InvariantCulture, "Logical Circuit {0} not found", circuitName));
			}
			if(!CircuitTestSocket.IsTestable(circuit)) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Logical Circuit {0} is not testable. There are no any input or/and output pins on it.", circuitName)
				);
			}
			return new CircuitTester(e, circuit);
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
				Editor current = this.Editor;
				if(e.ChangedButton == MouseButton.Left && current != null) {
					current.Power = !current.Power;
				}
			} catch(Exception exception) {
				Tracer.Report("MainFrame.PowerButtonMouseDown", exception);
				this.ReportException(exception);
			}
		}

		private TreeViewItem Container(TreeView treeView, CircuitMap map) {
			Tracer.Assert(map != null);
			if(map.Parent == null) {
				return (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(map);
			} else {
				TreeViewItem parent = this.Container(treeView, map.Parent);
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
					TreeView treeView = sender as TreeView;
					if(treeView != null && treeView.SelectedItem != null) {
						CircuitMap map = treeView.SelectedItem as CircuitMap;
						if(map != null) {
							this.Editor.OpenLogicalCircuit(map);
							TreeViewItem item = this.Container(treeView, map);
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
					CircuitMap map = e.NewValue as CircuitMap;
					if(map != null) {
						TreeViewItem item = this.Container((TreeView)sender, map);
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
