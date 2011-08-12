using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogReport.xaml
	/// </summary>
	public partial class DialogReport : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public FlowDocument Document { get; private set; }

		public DialogReport(LogicalCircuit root) {
			this.Document = this.BuildReport(root);
			this.DataContext = this;
			this.InitializeComponent();
		}

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Key == System.Windows.Input.Key.Escape) {
				this.Close();
			}
		}

		private FlowDocument BuildReport(LogicalCircuit root) {
			StringBuilder text = new StringBuilder();
			this.Report(text, root);
			using(StringReader textReader = new StringReader(text.ToString())) {
				using(XmlTextReader xmlReader = new XmlTextReader(textReader)) {
					return XamlReader.Load(xmlReader) as FlowDocument;
				}
			}
		}

		private void Report(StringBuilder text, LogicalCircuit root) {
			text.Append("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
			this.ReportProject(text, root.CircuitProject.ProjectSet.Project);
			this.ReportFunctions(text, root);
			text.AppendFormat("</FlowDocument>");
		}

		private void ReportProject(StringBuilder text, Project project) {
			HashSet<string> category = new HashSet<string>();
			foreach(LogicalCircuit circuit in project.CircuitProject.LogicalCircuitSet) {
				if(!string.IsNullOrEmpty(circuit.Category)) {
					category.Add(circuit.Category);
				}
			}
			text.AppendFormat(LogicCircuit.Resources.ReportProject,
				project.Name, project.Description,
				project.CircuitProject.LogicalCircuitSet.Count(), category.Count,
				project.CircuitProject.CircuitSymbolSet.Count(), project.CircuitProject.WireSet.Count()
			);
		}

		private void ReportFunctions(StringBuilder text, LogicalCircuit root) {
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
					} else {
						Tracer.Fail(LogicCircuit.Resources.FailUnknownFunction(f.Name));
					}
					if(func.ContainsKey(name)) {
						func[name]++;
					} else {
						func.Add(name, 1);
					}
				}
				List<string> keys = new List<string>(func.Keys);
				keys.Sort(StringComparer.Ordinal);
				text.AppendFormat(LogicCircuit.Resources.ReportFunction1, root.Name);
				bool white = true;
				int total = 0;
				foreach(string name in keys) {
					text.AppendFormat(LogicCircuit.Resources.ReportFunction2, white ? "White" : "WhiteSmoke", name, func[name]);
					white = !white;
					total += func[name];
				}
				text.AppendFormat(LogicCircuit.Resources.ReportFunction3, total);
			} catch(Exception exception) {
				text.AppendFormat(LogicCircuit.Resources.ReportFunction0, exception.Message, exception.ToString());
			}
		}
	}
}
