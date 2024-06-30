// Ignore Spelling: Hdl

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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

		private struct MessageData {
			public Brush? brush;
			public string text;
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

		public static readonly DependencyProperty RunningProperty = DependencyProperty.Register(nameof(Running), typeof(bool), typeof(DialogExportHdl));
		public bool Running {
			get => (bool)this.GetValue(DialogExportHdl.RunningProperty);
			set => this.SetValue(DialogExportHdl.RunningProperty, value);
		}

		private readonly ConcurrentQueue<MessageData> messages = new ConcurrentQueue<MessageData>();
		private int pumping;
		private bool continueExport;

		public DialogExportHdl(Editor editor) {
			this.logicalCircuit = editor.Project.LogicalCircuit; 
			this.DataContext = this;
			this.InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			e.Cancel = this.Running;
		}

		private void OnFinished() {
			if(!this.continueExport) {
				this.Error(Properties.Resources.ErrorHdlExportAborted);
			}
			this.ShowMessages();
			App.Dispatch(() => this.Running = false);
		}

		private void LogText(Brush? decorator, string text) {
			this.messages.Enqueue(new MessageData() { brush = decorator, text = text });
			this.ShowMessages();
		}

		private void ShowMessages() {
			App.Dispatch(() => {
				if(0 == Interlocked.CompareExchange(ref this.pumping, 1, 0)) {
					while(this.messages.TryDequeue(out MessageData messageData)) {
						Paragraph paragraph = new Paragraph();
						if(messageData.brush != null) {
							Run run = new Run("\u25C9 ") {
								Foreground = messageData.brush
							};
							paragraph.Inlines.Add(run);
						}
						paragraph.Inlines.Add(messageData.text);
						this.log.Document.Blocks.Add(paragraph);
						this.log.ScrollToEnd();
					}
					this.pumping = 0;
				}
			});
		}

		private void Message(string text) => this.LogText(null, text);
		private void Error(string text) => this.LogText(Brushes.Red, text);
		private void Warning(string text) => this.LogText(Brushes.Orange, text);

		private void ButtonExportClick(object sender, RoutedEventArgs e) {
			try {
				this.Running = true;
				e.Handled = true;
				this.log.Document = new System.Windows.Documents.FlowDocument();

				bool exportTests = false;
				HdlExport? hdl = null;
				switch(this.SelectedExportType.Value) {
				case HdlExportType.N2TFull:
					exportTests = true;
					goto case HdlExportType.N2T;
				case HdlExportType.N2T:
					hdl = new N2TExport(exportTests, this.CommentPoints, this.Message, this.Error, this.Warning);
					break;
				case HdlExportType.VerilogFull:
					exportTests = true;
					goto case HdlExportType.Verilog;
				case HdlExportType.Verilog:
					hdl = new VerilogExport(exportTests, this.CommentPoints, this.Message, this.Error, this.Warning);
					break;
				default:
					throw new InvalidOperationException();
				}
				this.continueExport = true;
				hdl.ExportCircuit(this.logicalCircuit, this.TargetFolder, this.OnlyCurrent, true, i => this.continueExport,  this.OnFinished);
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
				this.Running = false;
			}
		}

		private void ButtonStopClick(object sender, RoutedEventArgs e) {
			this.continueExport = false;
		}
	}
}
