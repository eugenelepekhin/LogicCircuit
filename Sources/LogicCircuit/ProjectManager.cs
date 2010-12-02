using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace LogicCircuit {
	public class ProjectManager : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private int savedVersion = 0;
		public string File { get; private set; }
		public CircuitProject CircuitProject { get; private set; }

		public ProjectManager() {
			this.CreateProject();
		}

		public bool HasChanges { get { return this.savedVersion != this.CircuitProject.Version; } }

		public void CreateProject() {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(ProjectManager.ChangeGuid(ProjectManager.ChangeGuid(Schema.Empty, "ProjectId"), "LogicalCircuitId"));
			this.CircuitProject = ProjectManager.Load(xml);
			this.File = null;
			this.savedVersion = this.CircuitProject.Version;

			this.NotifyPropertyChanged("File");
			this.NotifyPropertyChanged("CircuitProject");
		}

		public void LoadProject(string file) {
			XmlDocument xml = new XmlDocument();
			xml.Load(file);
			this.CircuitProject = ProjectManager.Load(xml);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;

			this.NotifyPropertyChanged("File");
			this.NotifyPropertyChanged("CircuitProject");
		}

		public void SaveProject(string file) {
			XmlDocument xml = this.CircuitProject.Save();
			XmlHelper.Save(xml, file);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.NotifyPropertyChanged("File");
		}

		private void NotifyPropertyChanged(string name) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private static string ChangeGuid(string text, string nodeName) {
			string s = Regex.Replace(text,
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>\{{?[0-9a-fA-F]{{8}}-([0-9a-fA-F]{{4}}-){{3}}[0-9a-fA-F]{{12}}\}}?</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName
				),
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>{2}</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName, Guid.NewGuid()
				),
				RegexOptions.CultureInvariant | RegexOptions.Singleline
			);
			return s;
		}

		private static CircuitProject Load(XmlDocument xml) {
			CircuitProject project = new CircuitProject();
			project.InTransaction(() => project.Load(xml));
			return project;
		}
	}
}
