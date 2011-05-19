using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogImport.xaml
	/// </summary>
	public partial class DialogImport : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public string FileName { get; private set; }
		public IEnumerable<CircuitInfo> List { get; private set; }
		public IEnumerable<LogicalCircuit> ImportList {
			get { return (this.DialogResult.HasValue && this.DialogResult.Value) ? this.List.Where(i => i.Import).Select(i => i.Circuit) : Enumerable.Empty<LogicalCircuit>(); }
		}

		public DialogImport(string file, CircuitProject target) {
			this.FileName = file;
			this.DataContext = this;
			this.InitializeComponent();
			Thread thread = new Thread(new ThreadStart(() => {
				try {
					CircuitProject import = CircuitProject.Create(file);
					List<CircuitInfo> list = new List<CircuitInfo>();
					foreach(LogicalCircuit circuit in import.LogicalCircuitSet) {
						list.Add(new CircuitInfo(circuit, target.LogicalCircuitSet.FindByLogicalCircuitId(circuit.LogicalCircuitId) == null));
					}
					list.Sort(CircuitDescriptorComparer.Comparer);
					this.List = list;
					this.NotifyPropertyChanged("List");
				} catch(Exception exception) {
					Tracer.Report("DialogImport.Load", exception);
					App.Mainframe.ReportException(exception);
					this.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
				}
			}));
			//TextNote validator will instantiate FlowDocument that in some cases required to happened only on STA.
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Name = "ImportLoader";
			thread.Priority = ThreadPriority.AboveNormal;
			thread.Start();
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void ButtonCheckAllClick(object sender, RoutedEventArgs e) {
			try {
				if(this.List != null) {
					foreach(CircuitInfo info in this.List) {
						info.SetImport(info.CanImport);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("DialogImport.ButtonCheckAllClick", exception);
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonUncheckAllClick(object sender, RoutedEventArgs e) {
			try {
				if(this.List != null) {
					foreach(CircuitInfo info in this.List) {
						info.SetImport(false);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("DialogImport.ButtonCheckAllClick", exception);
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				this.DialogResult = true;
			} catch(Exception exception) {
				Tracer.Report("DialogImport.ButtonOkClick", exception);
				App.Mainframe.ReportException(exception);
			}
		}

		public class CircuitInfo : LogicalCircuitDescriptor {
			public bool Import { get; set; }
			public bool CanImport { get; private set; }

			public CircuitInfo(LogicalCircuit circuit, bool canImport) : base(circuit) {
				this.CanImport = canImport;
			}

			public void SetImport(bool value) {
				this.Import = value;
				this.NotifyPropertyChanged("Import");
			}
		}
	}
}
