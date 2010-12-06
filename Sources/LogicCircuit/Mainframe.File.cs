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
			if(this.Editor.HasChanges) {
				MessageBoxResult result = DialogMessage.Show(this, this.Title,
					LogicCircuit.Resources.MessageSaveFile(this.Editor.CircuitProject.ProjectSet.Project.Name), null,
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

		private void Edit(string file) {
			Editor editor = new Editor(this, file);
			if(this.Editor != null) {
				this.Editor.Power = false;
			}
			if(editor.File != null) {
				Settings.User.AddRecentFile(editor.File);
			}
			this.Editor = editor;
			this.Editor.Refresh();
			this.Status = LogicCircuit.Resources.Ready;
		}

		private void New() {
			if(this.Editor == null || this.EnsureSaved()) {
				this.Edit(null);
			}
		}

		private void Open() {
			if(this.Editor == null || this.EnsureSaved()) {
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
					this.Edit(file);
				}
			}
		}

		private void SaveAs() {
			string file = this.Editor.File;
			if(!Mainframe.IsFilePathValid(file)) {
				file = Settings.User.RecentFile();
				string dir;
				if(Mainframe.IsFilePathValid(file)) {
					dir = Path.GetDirectoryName(file);
				} else {
					dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				}
				file = Path.Combine(dir, this.Editor.CircuitProject.ProjectSet.Project.Name + Mainframe.FileExtention);
			}
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.FileName = file;
			dialog.Filter = Mainframe.FileFilter;
			dialog.DefaultExt = Mainframe.FileExtention;
			bool? result = dialog.ShowDialog(this);
			if(result.HasValue && result.Value) {
				file = dialog.FileName;
				this.Editor.Save(file);
				Settings.User.AddRecentFile(file);
				this.Status = LogicCircuit.Resources.FileSaved(file);
			}
		}

		private void Save() {
			string file = this.Editor.File;
			if(Mainframe.IsFilePathValid(file)) {
				this.Editor.Save(file);
				Settings.User.AddRecentFile(file);
				this.Status = LogicCircuit.Resources.FileSaved(file);
			} else {
				this.SaveAs();
			}
		}
	}
}
