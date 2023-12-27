using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace LogicCircuit {
	partial class ReportBuilder {
		public static FlowDocument Build(LogicalCircuit root) {
			using(StringReader textReader = new StringReader(new ReportBuilder(root).TransformText())) {
				using(XmlReader xmlReader = XmlHelper.CreateReader(textReader)) {
					return (FlowDocument)XamlReader.Load(xmlReader);
				}
			}
		}

		private readonly LogicalCircuit Root;
		private Project Project { get { return this.Root.CircuitProject.ProjectSet.Project; } }
		private List<string> Functions;
		private Dictionary<string, int> Usage;
		private Exception? BuildMapException;

		private ReportBuilder(LogicalCircuit root) {
			this.Root = root;
			this.ReportFunctions(root);
			Debug.Assert(this.Functions != null && this.Usage != null);
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

		[SuppressMessage("Performance", "CA1854:Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method")]
		[SuppressMessage("Performance", "CA1864:Prefer the 'IDictionary.TryAdd(TKey, TValue)' method")]
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
