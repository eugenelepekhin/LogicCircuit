using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace LogicCircuit {
	public class CircuitDescriptorList : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private static List<ICircuitDescriptor> primitiveList;

		private readonly CircuitProject circuitProject;
		private readonly Dictionary<LogicalCircuit, LogicalCircuitDescriptor> logicalCircuitDescriptors = new Dictionary<LogicalCircuit, LogicalCircuitDescriptor>();
		private LogicalCircuitDescriptor current;

		public CircuitDescriptorList(CircuitProject circuitProject) : base() {
			this.circuitProject = circuitProject;
			this.circuitProject.LogicalCircuitSet.LogicalCircuitSetChanged += new EventHandler(this.LogicalCircuitSetChanged);
			this.circuitProject.ProjectSet.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
			if(CircuitDescriptorList.primitiveList == null) {
				CircuitDescriptorList.InitPrimitive();
			}
		}

		public void Refresh() {
			CircuitDescriptorList.InitPrimitive();
			this.NotifyPropertyChanged();
		}

		public IEnumerable<ICircuitDescriptor> CircuitDescriptors {
			get {
				this.current = null;
				this.logicalCircuitDescriptors.Clear();
				foreach(LogicalCircuit circuit in this.circuitProject.LogicalCircuitSet) {
					LogicalCircuitDescriptor descriptor = new LogicalCircuitDescriptor(circuit);
					if(descriptor.IsCurrent) {
						Tracer.Assert(this.current == null);
						this.current = descriptor;
					}
					this.logicalCircuitDescriptors.Add(circuit, descriptor);
				}
				List<ICircuitDescriptor> list = new List<ICircuitDescriptor>(this.logicalCircuitDescriptors.Values);
				list.Sort(CircuitDescriptorComparer.Comparer);
				foreach(ICircuitDescriptor d in list) {
					yield return d;
				}
				foreach(ICircuitDescriptor d in CircuitDescriptorList.primitiveList) {
					yield return d;
				}
			}
		}

		public void UpdateGlyph(LogicalCircuit logicalCircuit) {
			LogicalCircuitDescriptor descriptor;
			if(this.logicalCircuitDescriptors.TryGetValue(logicalCircuit, out descriptor)) {
				descriptor.ResetGlyph();
			}
		}

		private void LogicalCircuitSetChanged(object sender, EventArgs e) {
			this.NotifyPropertyChanged();
		}

		private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == "LogicalCircuit") {
				if(this.current != null) {
					this.current.NotifyCurrentChanged();
				}
				if(!this.logicalCircuitDescriptors.TryGetValue(this.circuitProject.ProjectSet.Project.LogicalCircuit, out this.current)) {
					this.current = null;
				}
				if(this.current != null) {
					this.current.NotifyCurrentChanged();
				}
			}
		}

		private void NotifyPropertyChanged() {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs("CircuitDescriptors"));
			}
		}

		private static void InitPrimitive() {
			CircuitProject project = CircuitProject.Create(null);
			project.StartTransaction();
			List<ICircuitDescriptor> list = new List<ICircuitDescriptor>();

			list.Add(new PinDescriptor(project, PinType.Input));
			list.Add(new PinDescriptor(project, PinType.Output));
			list.Add(new ButtonDescriptor(project));
			list.Add(new ConstantDescriptor(project));
			list.Add(new SplitterDescriptor(project));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Clock, 0, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Led, 1, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Led, 8, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Probe, 1, false)));

			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Not, 1, true)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.TriState, 2, false)));

			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.And, 2, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.And, 2, true)));

			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Or, 2, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Or, 2, true)));

			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Xor, 2, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Xor, 2, true)));

			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Even, 2, false)));
			list.Add(new GateDescriptor(project.GateSet.Gate(GateType.Odd, 2, false)));

			list.Add(new MemoryDescriptor(project, false));
			list.Add(new MemoryDescriptor(project, true));

			CircuitDescriptorList.primitiveList = list;
		}
	}
}
