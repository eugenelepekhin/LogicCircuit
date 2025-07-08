﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using LogicCircuit.DataPersistent;
using Microsoft.Win32;

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

		public static bool IsFilePathValid(string? path) {
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

		// This is not bulletproof as file might be created between File.Exist and creating the file.
		// However, seems like good enough for circuit saving.
		public static string TempFile(string originalFile) {
			string file;
			do {
				file = Path.Combine(Path.GetDirectoryName(originalFile)!, DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture));
			} while(File.Exists(file));
			return file;
		}

		public static string BackupFile(string originalFile) {
			return Path.ChangeExtension(originalFile, ".backup");
		}

		internal static string? AutoSaveFile(string? file) {
			if(Mainframe.IsFilePathValid(file) && Path.HasExtension(file)) {
				string extension = Path.GetExtension(file);
				if(!string.IsNullOrEmpty(extension) && 2 < extension.Length) {
					return string.Concat(file.AsSpan(0, file.Length - 2), "~$");
				}
			}
			return null;
		}

		internal static void Hide(string file) {
			if(Mainframe.IsFileExists(file)) {
				File.SetAttributes(file, FileAttributes.Hidden | File.GetAttributes(file));
			}
		}

		public static bool IsFileExists(string file) {
			return Mainframe.IsFilePathValid(file) && File.Exists(file);
		}

		internal static void DeleteFile(string file) {
			if(Mainframe.IsFileExists(file)) {
				try {
					File.Delete(file);
				} catch(Exception exception) {
					Tracer.Report("Mainframe.DeleteFile", exception);
				}
			}
		}

		internal static void DeleteAutoSaveFile(string? file) {
			if(!string.IsNullOrEmpty(file)) {
				Mainframe.DeleteFile(Mainframe.AutoSaveFile(file)!);
			}
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
					this.ResetAutoSaveTimer();
					Mainframe.DeleteAutoSaveFile(this.Editor?.File);
					break;
				case MessageBoxResult.Cancel:
					this.Status = Properties.Resources.OperationCanceled;
					return false;
				}
			}
			return true;
		}

		private void Edit(string? file) {
			if(this.Editor != null) {
				this.Editor.Power = false;
			}
			this.ResetAutoSaveTimer();
			Editor circuitEditor;
			try {
				circuitEditor = new Editor(this, file);
			} catch(SnapStoreException snapStoreException) {
				Tracer.Report("Mainframe.Edit", snapStoreException);
				throw new CircuitException(Cause.CorruptedFile, snapStoreException, Properties.Resources.ErrorFileCorrupted(file ?? string.Empty));
			}
			if(circuitEditor.File != null) {
				Settings.User.AddRecentFile(circuitEditor.File);
			}
			this.Dispatcher.BeginInvoke(new Action(() => this.ScrollOffset = new Point(0, 0)), System.Windows.Threading.DispatcherPriority.Normal);
			this.Editor = circuitEditor;
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
				string? file = Settings.User.RecentFile();
				if(Mainframe.IsFilePathValid(file)) {
					dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(file!));
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

		internal void Open(string file) {
			if(this.Editor == null || this.EnsureSaved()) {
				this.Edit(file);
			}
		}

		private void Save(string file) {
			this.Editor!.Save(file);
			Settings.User.AddRecentFile(file);
			this.Status = Properties.Resources.FileSaved(file);
		}

		private void SaveAs() {
			string? file = this.Editor!.File;
			if(!Mainframe.IsFilePathValid(file)) {
				file = Settings.User.RecentFile();
				string dir;
				if(Mainframe.IsFilePathValid(file)) {
					dir = Path.GetDirectoryName(file)!;
				} else {
					dir = Mainframe.DefaultProjectFolder();
				}
				file = Path.Combine(dir, this.Editor.Project.Name + Mainframe.FileExtention);
			}
			Debug.Assert(file != null);
			SaveFileDialog dialog = new SaveFileDialog {
				InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(file)),
				FileName = Path.GetFileName(file),
				Filter = Mainframe.FileFilter,
				DefaultExt = Mainframe.FileExtention
			};
			bool? result = dialog.ShowDialog(this);
			if(result.HasValue && result.Value) {
				this.Save(dialog.FileName);
			}
		}

		private void Save() {
			string? file = this.Editor!.File;
			if(Mainframe.IsFilePathValid(file)) {
				this.Save(file!);
			} else {
				this.SaveAs();
			}
		}

		private void Import() {
			if(this.Editor != null && this.Editor.InEditMode) {
				string dir = Mainframe.DefaultProjectFolder();
				string? recent = Settings.User.RecentFile();
				if(Mainframe.IsFilePathValid(recent)) {
					dir = Path.GetDirectoryName(recent)!;
				}
				SettingsStringCache location = new SettingsStringCache(Settings.User, "ImportFile.Folder", dir);
				OpenFileDialog dialog = new OpenFileDialog {
					Filter = Mainframe.FileFilter,
					DefaultExt = Mainframe.FileExtention,
					InitialDirectory = Mainframe.IsDirectoryPathValid(location.Value) ? location.Value : Mainframe.DefaultProjectFolder()
				};
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					string file = dialog.FileName;
					location.Value = Path.GetDirectoryName(file)!;
					this.Editor.Import(file);
				}
			}
		}
	}
}
