﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows;
using DataPersistent;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogImport.xaml
	/// </summary>
	public partial class DialogImport : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler? PropertyChanged;

		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public string FileName { get; }
		public IEnumerable<CircuitInfo>? List { get; private set; }
		public IEnumerable<LogicalCircuit> ImportList =>
			(this.DialogResult.HasValue && this.DialogResult.Value && this.List != null)
			? this.List.Where(i => i.Import).Select(i => i.Circuit)
			: Enumerable.Empty<LogicalCircuit>()
		;

		public DialogImport(string file, CircuitProject target) {
			this.FileName = file;
			this.DataContext = this;
			this.InitializeComponent();
			Mainframe mainframe = App.Mainframe;
			Thread thread = new Thread(new ThreadStart(() => {
				try {
					CircuitProject import = CircuitProject.Create(file);
					List<CircuitInfo> list = new List<CircuitInfo>();
					foreach(LogicalCircuit circuit in import.LogicalCircuitSet) {
						list.Add(new CircuitInfo(circuit, target.LogicalCircuitSet.FindByLogicalCircuitId(circuit.LogicalCircuitId) == null));
					}
					list.Sort(CircuitDescriptorComparer.Comparer);
					this.List = list;
					this.NotifyPropertyChanged(nameof(this.List));
				} catch(SnapStoreException snapStoreException) {
					Tracer.Report("DialogImport.Load", snapStoreException);
					mainframe.ErrorMessage(Properties.Resources.ErrorFileCorrupted(file), snapStoreException);
					mainframe.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
				} catch(Exception exception) {
					Tracer.Report("DialogImport.Load", exception);
					mainframe.ReportException(exception);
					mainframe.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
				}
			}));
			//TextNote validation will instantiate FlowDocument that in some cases required to happened only on STA.
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Name = "ImportLoader";
			thread.Priority = ThreadPriority.AboveNormal;
			thread.Start();
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
				if(this.List != null) {
					this.DialogResult = true;
				}
			} catch(Exception exception) {
				Tracer.Report("DialogImport.ButtonOkClick", exception);
				App.Mainframe.ReportException(exception);
			}
		}

		public sealed class CircuitInfo : LogicalCircuitDescriptor {
			public bool Import { get; set; }
			public bool CanImport { get; }

			public CircuitInfo(LogicalCircuit circuit, bool canImport) : base(circuit, s => false) {
				this.CanImport = canImport;
				// Expand all categories for a better UX
				this.CategoryExpanded = true;
			}

			public void SetImport(bool value) {
				this.Import = value;
				this.NotifyPropertyChanged(nameof(this.Import));
			}

			// There is no need to persist this to the project so override the property.
			public override bool CategoryExpanded { get; set; }
		}
	}
}
