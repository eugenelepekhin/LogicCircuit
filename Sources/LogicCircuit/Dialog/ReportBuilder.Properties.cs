using System;
using System.Collections.Generic;
using System.IO;
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
					string name = f.ReportName;
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
