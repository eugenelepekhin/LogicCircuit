using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace LogicCircuit {
	partial class ReportBuilder {
		public static FlowDocument Build(LogicalCircuit root) {
			using(StringReader textReader = new StringReader(new ReportBuilder(root).TransformText())) {
				using(XmlTextReader xmlReader = new XmlTextReader(textReader)) {
					return XamlReader.Load(xmlReader) as FlowDocument;
				}
			}
		}

		private LogicalCircuit Root;
		private Project Project { get { return this.Root.CircuitProject.ProjectSet.Project; } }
		private List<string> Functions;
		private Dictionary<string, int> Usage;
		private Exception BuildMapException;

		private ReportBuilder(LogicalCircuit root) {
			this.Root = root;
			this.ReportFunctions(root);
		}

		private int CategoryCount {
			get {
				HashSet<string> category = new HashSet<string>();
				foreach(LogicalCircuit circuit in this.Project.CircuitProject.LogicalCircuitSet) {
					if(!string.IsNullOrEmpty(circuit.Category)) {
						category.Add(circuit.Category);
					}
				}
				return category.Count;
			}
		}

		private void ReportFunctions(LogicalCircuit root) {
			try {
				CircuitMap map = new CircuitMap(root);
				CircuitState state = map.Apply(1);
				Dictionary<string, int> func = new Dictionary<string, int>();
				foreach(CircuitFunction f in state.Functions) {
					string name = null;
					if(f is FunctionClock) {
						name = LogicCircuit.Resources.GateClockName;
					} else if(f is FunctionNot) {
						name = LogicCircuit.Resources.GateNotName;
					} else if(f is FunctionTriState) {
						name = LogicCircuit.Resources.GateTriStateName;
					} else if(f is FunctionTriStateGroup) {
						name = LogicCircuit.Resources.TriStateGroupName;
					} else if(f is FunctionAnd) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateAndName, f.ParameterCount);
					} else if(f is FunctionAndNot) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateAndNotName, f.ParameterCount);
					} else if(f is FunctionOr) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateOrName, f.ParameterCount);
					} else if(f is FunctionOrNot) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateOrNotName, f.ParameterCount);
					} else if(f is FunctionXor) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateXorName, f.ParameterCount);
					} else if(f is FunctionXorNot) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateXorNotName, f.ParameterCount);
					} else if(f is FunctionOdd) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateOddName, f.ParameterCount);
					} else if(f is FunctionEven) {
						name = LogicCircuit.Resources.ReportGateName(LogicCircuit.Resources.GateEvenName, f.ParameterCount);
					} else if(f is Function7Segment) {
						name = LogicCircuit.Resources.Gate7SegName;
					} else if(f is FunctionButton) {
						name = LogicCircuit.Resources.NameButton;
					} else if(f is FunctionConstant) {
						name = LogicCircuit.Resources.NameConstant;
					} else if(f is FunctionLed) {
						name = LogicCircuit.Resources.GateLedName;
					} else if(f is FunctionRam) {
						FunctionMemory memory = (FunctionMemory)f;
						name = LogicCircuit.Resources.ReportMemoryName(LogicCircuit.Resources.RAMNotation, memory.AddressBitWidth, memory.DataBitWidth);
					} else if(f is FunctionRom) {
						FunctionMemory memory = (FunctionMemory)f;
						name = LogicCircuit.Resources.ReportMemoryName(LogicCircuit.Resources.ROMNotation, memory.AddressBitWidth, memory.DataBitWidth);
					} else if(f is FunctionProbe) {
						name = LogicCircuit.Resources.GateProbeName;
					} else if(f is FunctionLedMatrix) {
						name = LogicCircuit.Resources.NameLedMatrix;
					} else {
						Tracer.Fail(LogicCircuit.Resources.FailUnknownFunction(f.Name));
					}
					if(func.ContainsKey(name)) {
						func[name]++;
					} else {
						func.Add(name, 1);
					}
				}

				this.Functions = new List<string>(func.Keys);
				this.Functions.Sort(StringComparer.Ordinal);
				this.Usage = func;
			} catch(Exception exception) {
				this.BuildMapException = exception;
			}
		}
	}
}
