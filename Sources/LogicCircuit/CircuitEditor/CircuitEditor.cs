using System;
using System.ComponentModel;

namespace LogicCircuit {
	public partial class CircuitEditor : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public Mainframe Mainframe { get; private set; }
		public string File { get; private set; }
		public CircuitProject CircuitProject { get; private set; }
		private int savedVersion;
		public CircuitDescriptorList CircuitDescriptorList { get; private set; }

		public bool HasChanges { get { return this.savedVersion != this.CircuitProject.Version; } }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }
		public string Caption { get { return Resources.MainFrameCaption(this.File); } }

		// TODO: implement it correctly
		private bool power = false;
		public bool Power {
			get { return this.power; }
			set {
				if(this.power != value) {
					this.power = value;
					this.NotifyPropertyChanged("Power");
				}
			}
		}

		public CircuitEditor(Mainframe mainframe) {
			this.Init(mainframe, null);
		}

		public CircuitEditor(Mainframe mainframe, string file) {
			this.Init(mainframe, file);
		}

		private void Init(Mainframe mainframe, string file) {
			this.Mainframe = mainframe;
			this.File = file;
			if(this.File == null) {
				this.CircuitProject = CircuitProject.Create();
			} else {
				this.CircuitProject = CircuitProject.Load(this.File);
			}
			this.savedVersion = this.CircuitProject.Version;
			this.CircuitDescriptorList = new CircuitDescriptorList(this.CircuitProject);
		}

		public void Save(string file) {
			this.CircuitProject.Save(file);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.NotifyPropertyChanged("File");
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.Mainframe.NotifyPropertyChanged(this.PropertyChanged, this, propertyName);
		}

		public double Zoom {
			get { return this.CircuitProject.ProjectSet.Project.Zoom; }
			set {
				if(this.Zoom != value) {
					try {
						this.CircuitProject.InTransaction(() => this.CircuitProject.ProjectSet.Project.Zoom = value);
						this.NotifyPropertyChanged("Zoom");
					} catch(Exception exception) {
						this.Mainframe.ReportException(exception);
					}
				}
			}
		}

		public int Frequency {
			get; set;
		}

		public bool IsMaximumSpeed {
			get; set;
		}
	}
}
