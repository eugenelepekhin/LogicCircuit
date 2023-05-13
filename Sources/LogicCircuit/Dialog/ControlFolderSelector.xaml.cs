using System;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for ControlFolderSelector.xaml
	/// </summary>
	public partial class ControlFolderSelector : UserControl {
		public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register(nameof(SelectedFolder), typeof(string), typeof(ControlFolderSelector));
		public string SelectedFolder {
			get => (string)this.GetValue(ControlFolderSelector.SelectedFolderProperty);
			set => this.SetValue(ControlFolderSelector.SelectedFolderProperty, value);
		}

		public LambdaUICommand CommandSelect { get; }

		public ControlFolderSelector() {
			this.CommandSelect = new LambdaUICommand(Properties.Resources.CommandEllipsis, o => this.Select());
			this.InitializeComponent();
		}

		protected override void OnGotFocus(RoutedEventArgs e) {
			this.textBoxSelectedFolder.Focus();
		}

		private void Select() {
			try {
				DialogSelectFolder dialog = new DialogSelectFolder();
				dialog.FileName = this.SelectedFolder;
				if(dialog.ShowDialog(Window.GetWindow(this))) {
					this.SelectedFolder = dialog.FileName;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
