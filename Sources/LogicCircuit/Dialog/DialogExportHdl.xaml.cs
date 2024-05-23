// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogExportHdl.xaml
	/// </summary>
	public partial class DialogExportHdl : Window {
		public enum HdlExportType {
			N2T,
			N2TFull,
			Verilog,
			VerilogFull,
			Other
		}

		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private readonly LogicalCircuit logicalCircuit;

		public IEnumerable<EnumDescriptor<HdlExportType>> ExportTypes { get; } = new EnumDescriptor<HdlExportType>[] {
			new EnumDescriptor<HdlExportType>(HdlExportType.N2T, Properties.Resources.HdlExportN2T),
			new EnumDescriptor<HdlExportType>(HdlExportType.N2TFull, Properties.Resources.HdlExportN2TandTests),
			new EnumDescriptor<HdlExportType>(HdlExportType.Verilog, Properties.Resources.HdlExportVerilog),
			new EnumDescriptor<HdlExportType>(HdlExportType.VerilogFull, Properties.Resources.HdlExportVerilogFull),
		};

		private readonly SettingsEnumCache<HdlExportType> selectedExportType = new SettingsEnumCache<HdlExportType>(
			Settings.User,
			nameof(DialogExportHdl) + "." + nameof(SelectedExportType),
			HdlExportType.N2T
		);
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
				this.log.Document = new System.Windows.Documents.FlowDocument();

				void logText(Brush? decorator, string text) {
					App.Dispatch(() => {
						Paragraph paragraph = new Paragraph();
						if(decorator != null) {
							Run run = new Run("\u25C9 ") {
								Foreground = decorator
							};
							paragraph.Inlines.Add(run);
						}
						paragraph.Inlines.Add(text);
						this.log.Document.Blocks.Add(paragraph);
					});
				}
				void message(string text) => logText(null, text);
				void error(string text) => logText(Brushes.Red, text);
				void warning(string text) => logText(Brushes.Orange, text);

				HdlExport? hdl = null;
				switch(this.SelectedExportType.Value) {
				case HdlExportType.N2T:
				case HdlExportType.N2TFull:
					hdl = new N2TExport(this.SelectedExportType.Value == HdlExportType.N2TFull, this.CommentPoints, message, error, warning);
					break;
				case HdlExportType.Verilog:
				case HdlExportType.VerilogFull:
					hdl = new VerilogExport(this.SelectedExportType.Value == HdlExportType.VerilogFull, this.CommentPoints, message, error, warning);
					break;
				default:
					throw new InvalidOperationException();
				}
				hdl.ExportCircuit(this.logicalCircuit, this.TargetFolder, this.OnlyCurrent);
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
