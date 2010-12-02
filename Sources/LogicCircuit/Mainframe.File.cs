using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace LogicCircuit {
	partial class Mainframe {

		private const string FileExtention = ".CircuitProject";
		private static readonly string FileFilter = LogicCircuit.Resources.FileFilter(Mainframe.FileExtention);

		public static bool IsFilePathValid(string path) {
			if(path != null && path.Length > 0) {
				try {
					if(Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(path)))) {
						return true;
					}
				} catch(Exception exception) {
					Tracer.Report("Mainframe.IsPathValid", exception);
					App.Mainframe.ReportException(exception);
				}
			}
			return false;
		}

		public static bool IsDirectoryPathValid(string path) {
			if(path != null && path.Length > 0) {
				try {
					if(Directory.Exists(Path.GetFullPath(path))) {
						return true;
					}
				} catch(Exception exception) {
					Tracer.Report("Mainframe.IsPathValid", exception);
					App.Mainframe.ReportException(exception);
				}
			}
			return false;
		}

		private bool EnsureSaved() {
			if(this.CircuitEditor.ProjectManager.HasChanges) {
				MessageBoxResult result = DialogMessage.Show(this, this.Title,
					LogicCircuit.Resources.MessageSaveFile(this.CircuitEditor.ProjectManager.CircuitProject.ProjectSet.Project.Name), null,
					MessageBoxImage.Question, MessageBoxButton.YesNoCancel
				);
				switch(result) {
				case MessageBoxResult.Yes:
					this.Save();
					break;
				case MessageBoxResult.No:
					break;
				case MessageBoxResult.Cancel:
					this.Status = LogicCircuit.Resources.OperationCanceled;
					return false;
				}
			}
			return true;
		}

		private void New() {
			if(this.CircuitEditor != null) {
				this.CircuitEditor.Power = false;
			}
			if(this.CircuitEditor == null || this.EnsureSaved()) {
				this.CircuitEditor = new CircuitEditor(this, new ProjectManager());
				this.Status = LogicCircuit.Resources.Ready;
			}
		}

		private void Load(string file) {
			if(this.CircuitEditor != null) {
				this.CircuitEditor.Power = false;
			}
			// Do not check for saved file here as it happened in open
			if(Mainframe.IsFilePathValid(file) && File.Exists(file)) {
				ProjectManager projectManager = new ProjectManager();
				projectManager.LoadProject(file);
				this.CircuitEditor = new CircuitEditor(this, projectManager);
				Settings.User.AddRecentFile(file);
				this.Status = LogicCircuit.Resources.Ready;
			}
		}

		private void Open() {
			if(this.CircuitEditor == null || this.EnsureSaved()) {
				OpenFileDialog dialog = new OpenFileDialog();
				string file = Settings.User.RecentFile();
				if(Mainframe.IsFilePathValid(file)) {
					dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(file));
				}
				dialog.Filter = Mainframe.FileFilter;
				dialog.DefaultExt = Mainframe.FileExtention;
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					file = dialog.FileName;
					this.Load(file);
				}
			}
		}

		private void SaveAs() {
			string file = this.CircuitEditor.ProjectManager.File;
			if(!Mainframe.IsFilePathValid(file)) {
				file = Settings.User.RecentFile();
				string dir;
				if(Mainframe.IsFilePathValid(file)) {
					dir = Path.GetDirectoryName(file);
				} else {
					dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				}
				file = Path.Combine(dir, this.CircuitEditor.ProjectManager.CircuitProject.ProjectSet.Project.Name + Mainframe.FileExtention);
			}
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.FileName = file;
			dialog.Filter = Mainframe.FileFilter;
			dialog.DefaultExt = Mainframe.FileExtention;
			bool? result = dialog.ShowDialog(this);
			if(result.HasValue && result.Value) {
				file = dialog.FileName;
				this.CircuitEditor.ProjectManager.SaveProject(file);
				Settings.User.AddRecentFile(file);
				this.Status = LogicCircuit.Resources.FileSaved(file);
				this.NotifyPropertyChanged("Caption");
			}
		}

		private void Save() {
			string file = this.CircuitEditor.ProjectManager.File;
			if(Mainframe.IsFilePathValid(file)) {
				this.CircuitEditor.ProjectManager.SaveProject(file);
				Settings.User.AddRecentFile(file);
				this.Status = LogicCircuit.Resources.FileSaved(file);
			} else {
				this.SaveAs();
			}
		}
	}
}
