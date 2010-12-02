using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LogicCircuit {
	public class CircuitEditor : CircuitViewer, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public Mainframe Mainframe { get; private set; }
		public ProjectManager ProjectManager { get; private set; }

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

		//public CircuitEditor(Mainframe mainframe, ProjectManager projectManager) : base(mainFrame.Diagram, projectManager.CircuitProject) {
		public CircuitEditor(Mainframe mainframe, ProjectManager projectManager) {
			this.Mainframe = mainframe;
			this.ProjectManager = projectManager;
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.Mainframe.NotifyPropertyChanged(this.PropertyChanged, this, propertyName);
		}
	}
}
