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

	public sealed partial class ProjectSet : IRecordLoader {
		public Project Project { get; private set; }

		void IRecordLoader.Load(XmlReader reader) {
			if(this.Project != null) {
				throw new CircuitException(Cause.CorruptedFile, Resources.ErrorProjectCount);
			}
			
			this.Project = this.Create(ProjectData.Load(this.Table, reader));
		}

		public Project Copy(Project other) {
			Tracer.Assert(this.Count() == 0);
			ProjectData data;
			other.CircuitProject.ProjectSet.Table.GetData(other.ProjectRowId, out data);
			data.Project = null;
			return this.Create(this.Table.Insert(ref data));
		}
	}
}
