using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Project {
		public const double MinZoom = 0.1;
		public const double MaxZoom = 3;

		public const int MinFrequency = 1;
		public const int MaxFrequency = 100;

		public static double CheckZoom(double value) {
			return Math.Max(Project.MinZoom, Math.Min(value, Project.MaxZoom));
		}

		public static int CheckFrequency(int value) {
			return Math.Max(Project.MinFrequency, Math.Min(value, Project.MaxFrequency));
		}
	}

	public partial class ProjectSet {
		public Project Project { get; private set; }

		public void Load(XmlNodeList list) {
			ProjectData.Load(this.Table, list, rowId => this.Create(rowId));
			if(this.Count() != 1) {
				throw new CircuitException(Cause.CorruptedFile, Resources.ErrorProjectCount);
			}
			this.Project = this.First();
		}

		public Project Copy(Project other) {
			Tracer.Assert(this.Count() == 0);
			ProjectData data;
			other.CircuitProject.ProjectSet.Table.GetData(other.ProjectRowId, out data);
			return this.Create(this.Table.Insert(ref data));
		}
	}
}
