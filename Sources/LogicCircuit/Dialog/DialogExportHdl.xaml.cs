// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogExportHdl.xaml
	/// </summary>
	public partial class DialogExportHdl : Window {
		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private readonly LogicalCircuit logicalCircuit;

		public IEnumerable<EnumDescriptor<HdlExportType>> ExportTypes { get; } = new EnumDescriptor<HdlExportType>[] {
			new EnumDescriptor<HdlExportType>(HdlExportType.N2T, "From Nand to Tetris HDL"),
			new EnumDescriptor<HdlExportType>(HdlExportType.N2TFull, "From Nand to Tetris HDL & test files"),
		};

		private readonly SettingsEnumCache<HdlExportType> selectedExportType = new SettingsEnumCache<HdlExportType>(Settings.User, nameof(DialogExportHdl) + "." + nameof(SelectedExportType), HdlExportType.N2T);
		public EnumDescriptor<HdlExportType> SelectedExportType {
			get => this.ExportTypes.First(item => item.Value == this.selectedExportType.Value);
			set => this.selectedExportType.Value = value.Value;
		}

		private readonly SettingsStringCache targetFolder = new SettingsStringCache(Settings.User, nameof(DialogExportHdl) + "." + nameof(TargetFolder), Mainframe.DefaultProjectFolder());
		public string TargetFolder {
			get => this.targetFolder.Value;
			set => this.targetFolder.Value = value;
		}

		private readonly SettingsBoolCache onlyCurrent = new SettingsBoolCache(Settings.User, nameof(DialogExportHdl) + "." + nameof(OnlyCurrent), true);
		public bool OnlyCurrent {
			get => this.onlyCurrent.Value;
			set => this.onlyCurrent.Value = value;
		}

		private readonly SettingsBoolCache commentPoints = new SettingsBoolCache(Settings.User, nameof(DialogExportHdl) + "." + nameof(CommentPoints), true);
		public bool CommentPoints {
			get => this.commentPoints.Value;
			set => this.commentPoints.Value = value;
		}

		public DialogExportHdl(Editor editor) {
			this.logicalCircuit = editor.Project.LogicalCircuit; 
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void ButtonExportClick(object sender, RoutedEventArgs e) {
			try {
				e.Handled = true;
				this.log.Clear();
				void message(string text) {
					this.log.Text += text;
					this.log.Text += "\n";
				}
				HdlExport hdl = new HdlExport(this.SelectedExportType.Value, this.CommentPoints, message, message);
				hdl.ExportCircuit(this.logicalCircuit, this.TargetFolder, this.OnlyCurrent);
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
