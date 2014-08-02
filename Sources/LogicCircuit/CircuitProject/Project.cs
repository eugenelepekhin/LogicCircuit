using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;

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

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class ProjectSet {
		public Project Project { get; private set; }

		public Project Copy(Project other) {
			Tracer.Assert(this.Count() == 0);
			ProjectData data;
			other.CircuitProject.ProjectSet.Table.GetData(other.ProjectRowId, out data);
			data.Project = null;
			return this.Create(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<ProjectData>(nameTable, this.Table, rowId => {
				if(this.Project != null) {
					throw new CircuitException(Cause.CorruptedFile, Properties.Resources.ErrorProjectCount);
				}

				this.Project = this.Create(rowId);
			});
		}
	}
}
