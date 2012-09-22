using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	partial class Mainframe {

		private const string FileExtention = ".CircuitProject";
		private static readonly string FileFilter = Properties.Resources.FileFilter(Mainframe.FileExtention);

		public static string DefaultProjectFolder() {
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
		public static string DefaultPictureFolder() {
			return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
		}

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
			if(this.Editor != null && this.Editor.HasChanges) {
				MessageBoxResult result = DialogMessage.Show(this, this.Title,
					Properties.Resources.MessageSaveFile(this.Editor.Project.Name), null,
					MessageBoxImage.Question, MessageBoxButton.YesNoCancel
				);
				switch(result) {
				case MessageBoxResult.Yes:
					this.Save();
					break;
				case MessageBoxResult.No:
					break;
				case MessageBoxResult.Cancel:
					this.Status = Properties.Resources.OperationCanceled;
					return false;
				}
			}
			return true;
		}

		private void Edit(string file) {
			Editor editor;
			try {
				editor = new Editor(this, file);
			} catch(SnapStoreException snapStoreException) {
				Tracer.Report("Mainframe.Edit", snapStoreException);
				throw new CircuitException(Cause.CorruptedFile, snapStoreException, Properties.Resources.ErrorFileCorrupted(file));
			}
			if(this.Editor != null) {
				this.Editor.Power = false;
			}
			if(editor.File != null) {
				Settings.User.AddRecentFile(editor.File);
			}
			this.Dispatcher.BeginInvoke(new Action(() => this.ScrollOffset = new Point(0, 0)), System.Windows.Threading.DispatcherPriority.Normal);
			this.Editor = editor;
			this.Status = Properties.Resources.Ready;
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
				} else {
					dialog.InitialDirectory = Mainframe.DefaultProjectFolder();
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

		private void OpenRecent(string file) {
			if(Mainframe.IsFilePathValid(file) && File.Exists(file)) {
				if(this.Editor == null || this.EnsureSaved()) {
					this.Edit(file);
				}
			} else if(file != null) {
				MessageBoxResult result = DialogMessage.Show(this, this.Title,
					Properties.Resources.MessageInvalidRecentFile(file), null,
					MessageBoxImage.Question, MessageBoxButton.YesNo
				);
				if(result == MessageBoxResult.Yes) {
					Settings.User.DeleteRecentFile(file);
				}
			}
		}

		private void Save(string file) {
			this.Editor.Save(file);
			Settings.User.AddRecentFile(file);
			this.Status = Properties.Resources.FileSaved(file);
		}

		private void SaveAs() {
			string file = this.Editor.File;
			if(!Mainframe.IsFilePathValid(file)) {
				file = Settings.User.RecentFile();
				string dir;
				if(Mainframe.IsFilePathValid(file)) {
					dir = Path.GetDirectoryName(file);
				} else {
					dir = Mainframe.DefaultProjectFolder();
				}
				file = Path.Combine(dir, this.Editor.Project.Name + Mainframe.FileExtention);
			}
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(file));
			dialog.FileName = Path.GetFileName(file);
			dialog.Filter = Mainframe.FileFilter;
			dialog.DefaultExt = Mainframe.FileExtention;
			bool? result = dialog.ShowDialog(this);
			if(result.HasValue && result.Value) {
				this.Save(dialog.FileName);
			}
		}

		private void Save() {
			string file = this.Editor.File;
			if(Mainframe.IsFilePathValid(file)) {
				this.Save(file);
			} else {
				this.SaveAs();
			}
		}

		private void Import() {
			if(this.Editor != null && this.Editor.InEditMode) {
				string dir = Mainframe.DefaultProjectFolder();
				string recent = Settings.User.RecentFile();
				if(Mainframe.IsFilePathValid(recent)) {
					dir = Path.GetDirectoryName(recent);
				}
				SettingsStringCache location = new SettingsStringCache(Settings.User, "ImportFile.Folder", dir);
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = Mainframe.FileFilter;
				dialog.DefaultExt = Mainframe.FileExtention;
				dialog.InitialDirectory = Mainframe.IsDirectoryPathValid(location.Value) ? location.Value : Mainframe.DefaultProjectFolder();
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					string file = dialog.FileName;
					location.Value = Path.GetDirectoryName(file);
					this.Editor.Import(file);
				}
			}
		}
	}
}
