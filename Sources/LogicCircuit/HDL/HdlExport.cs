// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace LogicCircuit {
	public enum HdlExportType {
		N2T,
		N2TFull,
		Other
	}

	public class HdlExport {
		private readonly Action<string> logMessage;
		private readonly Action<string> logError;
		public int ErrorCount { get; private set; }

		private readonly Dictionary<CircuitSymbol, HdlSymbol> map = new Dictionary<CircuitSymbol, HdlSymbol>();
		private readonly List<HdlSymbol> list = new List<HdlSymbol>();
		public IEnumerable<HdlSymbol> HdlSymbols => this.list;
		public IEnumerable<HdlSymbol> InputPins => this.list.Where(symbol => symbol.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input).OrderBy(s => (Pin)s.CircuitSymbol.Circuit, PinComparer.Comparer);
		public IEnumerable<HdlSymbol> OutputPins => this.list.Where(symbol => symbol.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Output).OrderBy(s => (Pin)s.CircuitSymbol.Circuit, PinComparer.Comparer);
		public IEnumerable<HdlSymbol> Parts => this.list.Where(symbol => !(symbol.CircuitSymbol.Circuit is Pin));

		public HdlExportType HdlExportType { get; }

		public HdlExport(HdlExportType hdlExportType, Action<string> logMessage, Action<string> logError) {
			this.logMessage = logMessage;
			this.logError = logError;
			this.HdlExportType = hdlExportType;
		}

		public void Message(string text) => this.logMessage(text);
		private void DispatchMessage(string message) => App.Dispatch(() => this.Message(message));

		public void Error(string text) {
			this.ErrorCount++;
			this.logError(text);
		}

		public bool ExportCircuit(LogicalCircuit circuit, string folder, bool onlyOne) {
			CircuitMap map = new CircuitMap(circuit);
			ConnectionSet connectionSet = map.ConnectionSet();
			Dictionary<string, HdlSymbol> splitters = new Dictionary<string, HdlSymbol>();

			bool export(LogicalCircuit logicalCircuit) {
				string file = logicalCircuit.Name;
				switch(this.HdlExportType) {
				case HdlExportType.N2T:
				case HdlExportType.N2TFull:
					file += ".hdl";
					break;
				}
				this.Message(Properties.Resources.MessageExportingHdl(logicalCircuit.Name, file));
				string hdl = this.ExportCircuit(logicalCircuit, connectionSet, splitters);
				if(!string.IsNullOrEmpty(hdl)) {
					file = Path.Combine(folder, file);
					File.WriteAllText(file, hdl);
					if(this.HdlExportType == HdlExportType.N2TFull) {
						this.ExportN2TTest(logicalCircuit, folder);
					}
					return true;
				} else {
					this.Message(Properties.Resources.ErrorExportHdlFile(file));
				}
				return false;
			}

			bool walk(CircuitMap circuitMap, HashSet<LogicalCircuit> exported) {
				if(exported.Add(circuitMap.Circuit) && export(circuitMap.Circuit)) {
					foreach(CircuitMap child in circuitMap.Children) {
						if(!walk(child, exported)) {
							return false;
						}
					}
					return true;
				}
				return false;
			}

			if(onlyOne) {
				return export(circuit);
			} else {
				return walk(map, new HashSet<LogicalCircuit>());
			}
		}

		private string ExportCircuit(LogicalCircuit circuit, ConnectionSet connectionSet, Dictionary<string, HdlSymbol> splitters) {
			this.map.Clear();
			this.list.Clear();
			foreach(CircuitSymbol symbol in circuit.CircuitSymbols()) {
				HdlSymbol hdlSymbol = new HdlSymbol(this, symbol);
				this.map.Add(symbol, hdlSymbol);
				this.list.Add(hdlSymbol);
			}
			foreach(HdlSymbol symbol in this.list) {
				foreach(Jam output in symbol.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Output || (j.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input))) {
					foreach(Connection connection in connectionSet.SelectByOutput(output)) {
						symbol.Add(connection);
						HdlSymbol other = this.map[(CircuitSymbol)connection.InJam.CircuitSymbol];
						other.Add(connection);
					}
				}
			}
			foreach(HdlSymbol symbol in this.list) {
				symbol.SortConnections();
				if(symbol.CircuitSymbol.Circuit is Splitter splitter) {
					splitters.Add(symbol.Name, symbol);
				}
			}
			this.SortSymbols();

			T4Transformation? transformation = null;
			switch(this.HdlExportType) {
			case HdlExportType.N2T:
			case HdlExportType.N2TFull:
				transformation = new N2THdl(this, circuit, circuit.Name);
				break;
			default:
				throw new InvalidOperationException();
			}
			return transformation.TransformText();
		}

		private void SortSymbols() {
			this.list.ForEach(symbol => symbol.Order = 0);
			foreach(HdlSymbol symbol in this.list.Where(s => s.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input)) {
				this.Sort(new HashSet<HdlSymbol>(), symbol, 1);
			}
			this.list.Sort((x, y) => x.Order - y.Order);
		}

		private void Sort(HashSet<HdlSymbol> ignore, HdlSymbol symbol, int order) {
			symbol.Order = Math.Max(symbol.Order, order);
			foreach(Connection connection in symbol.Connections.Where(con => con.OutJam.CircuitSymbol == symbol.CircuitSymbol)) {
				HdlSymbol other = this.map[(CircuitSymbol)connection.InJam.CircuitSymbol];
				if(ignore.Add(other)) {
					this.Sort(ignore, other, symbol.Order + 1);
				}
			}
		}

		private void ExportN2TTest(LogicalCircuit circuit, string folder) {
			CircuitTestSocket socket = new CircuitTestSocket(circuit);
			
			void reportProgress(double progress) => this.DispatchMessage($"Building truth table for {circuit.Name} {progress:f1}% done");

			ThreadPool.QueueUserWorkItem(o => {
				try {
					bool isTrancated = false;
					IList<TruthState>? table = socket.BuildTruthTable(reportProgress, () => true, null, DialogTruthTable.MaxRows, out isTrancated);
					if (table == null || isTrancated) {
						this.DispatchMessage("Failed to build truth table.");
					} else {
						this.ExportN2TTest(
							circuit.Name,
							socket.Inputs.Select(i => i.Pin.Name).ToList(),
							socket.Outputs.Select(o => o.Pin.Name).ToList(),
							folder,
							table
						);
					}
				} catch(Exception exception) {
					App.Mainframe.ReportException(exception);
				}
			});
		}

		private void ExportN2TTest(
			string circuitName, List<string> inputs, List<string> outputs, string folder, IList<TruthState> table
		) {
			string formatExpect(string text) {
				if(3 <= text.Length) {
					text = text.Substring(0, 3);
					return text;
				}
				int trail = 0;
				int lead = 0;
				switch(text.Length) {
				case 1:
					lead = 2;
					trail = 1;
					break;
				case 2:
					lead = 0;
					trail = 1;
					break;
				}
				string format = string.Format(CultureInfo.InvariantCulture, "{{0,{0}}}", lead);
				string result = string.Format(CultureInfo.InvariantCulture, format, text) + new string(' ', trail);
				return result;
			}
			StringBuilder expect = new StringBuilder();
			StringBuilder script = new StringBuilder();

			script.AppendLine(CultureInfo.InvariantCulture, $"load {circuitName + ".hdl"},");
			script.AppendLine(CultureInfo.InvariantCulture, $"output-file {circuitName + ".out"},");
			script.AppendLine(CultureInfo.InvariantCulture, $"compare-to {circuitName + ".cmp"},");
			script.Append("output-list");

			foreach(string field in inputs.Concat(outputs)) {
				script.Append(CultureInfo.InvariantCulture, $" {field}");
				expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(field)}");
			}
			script.AppendLine(";");

			script.AppendLine();
			expect.AppendLine("|");

			foreach(TruthState state in table) {
				int index = 0;
				foreach(string input in inputs) {
					script.AppendLine(CultureInfo.InvariantCulture, $"set {input} {state.Input[index]},");
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state.Input[index].ToString(CultureInfo.InvariantCulture))}");
					index++;
				}
				script.AppendLine("eval,");
				script.AppendLine("output;");
				script.AppendLine();
				index = 0;
				foreach(string output in outputs) {
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state[index].ToString(CultureInfo.InvariantCulture))}");
					index++;
				}
				expect.AppendLine("|");
			}

			string testFile = Path.Combine(folder, circuitName + ".tst");
			File.WriteAllText(testFile, script.ToString());
			this.DispatchMessage($"Saving test file {testFile}");

			string cmpFile = Path.Combine(folder, circuitName + ".cmp");
			File.WriteAllText(cmpFile, expect.ToString());
			this.DispatchMessage($"Saving .cmp file {cmpFile}");
		}
	}
}
