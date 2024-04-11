// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogicCircuit {
	public enum HdlExportType {
		N2T,
		N2TFull,
		Other
	}

	public class HdlExport {
		public HdlExportType HdlExportType { get; }
		public bool CommentPoints { get; }
		public bool IsNand2Tetris => this.HdlExportType == HdlExportType.N2T || this.HdlExportType == HdlExportType.N2TFull;

		private readonly Action<string> logMessage;
		private readonly Action<string> logError;
		public int ErrorCount { get; private set; }

		public HdlExport(HdlExportType hdlExportType, bool commentPoints, Action<string> logMessage, Action<string> logError) {
			this.HdlExportType = hdlExportType;
			this.CommentPoints = commentPoints;
			this.logMessage = logMessage;
			this.logError = logError;
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

			bool export(LogicalCircuit logicalCircuit) {
				string file = logicalCircuit.Name;
				switch(this.HdlExportType) {
				case HdlExportType.N2T:
				case HdlExportType.N2TFull:
					file += ".hdl";
					break;
				}
				this.Message(Properties.Resources.MessageExportingHdl(logicalCircuit.Name, file));
				Dictionary<CircuitSymbol, HdlSymbol> symbolMap = this.Collect(circuit, connectionSet);
				if(0 < this.ErrorCount) return false;
				string? hdl = this.ExportCircuit(logicalCircuit, symbolMap);
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

		private Dictionary<CircuitSymbol, HdlSymbol> Collect(LogicalCircuit circuit, ConnectionSet connectionSet) {
			bool consider(CircuitSymbol symbol, bool showWarning) {
				Circuit circuit = symbol.Circuit;
				if(	circuit is CircuitButton ||
					circuit is Sensor ||
					circuit is Gate gate && (gate.GateType == GateType.Clock || gate.GateType == GateType.Led || gate.GateType == GateType.TriState1 || gate.GateType == GateType.TriState2) ||
					circuit is LedMatrix ||
					circuit is Sound

				) {
					if(showWarning) {
						this.Error($"Unsupported circuit {circuit.Name}{symbol.Point}");
					}
					return false;
				}
				return (circuit is not Splitter) && (circuit is not CircuitProbe);
			}
			Dictionary<CircuitSymbol, HdlSymbol> symbolMap = new Dictionary<CircuitSymbol, HdlSymbol>();
			foreach(CircuitSymbol symbol in circuit.CircuitSymbols().Where(s => consider(s, true))) {
				HdlSymbol hdlSymbol = new HdlSymbol(this, symbol);
				symbolMap.Add(symbol, hdlSymbol);
			}
			foreach(HdlSymbol symbol in symbolMap.Values) {
				foreach(Jam output in symbol.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Output || (j.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input))) {
					foreach(Connection connection in connectionSet.SelectByOutput(output)) {
						Debug.Assert(connection.OutJam == output);
						if(connection.InJam.CircuitSymbol.Circuit is Splitter) {
							int width = output.Pin.BitWidth;
							for(int i = 0; i < width; i++) {
								void Propagate(Jam enterJam, int enterBit) {
									Splitter.Pass(enterJam, enterBit, out Jam exitJam, out int exitBit);
									foreach(Connection splitted in connectionSet.SelectByOutput(exitJam)) {
										Debug.Assert(splitted.OutJam == exitJam);
										if(splitted.InJam.CircuitSymbol.Circuit is Splitter) {
											Propagate(splitted.InJam, exitBit);
										} else if(consider((CircuitSymbol)splitted.InJam.CircuitSymbol, false)) {
											if(symbolMap.TryGetValue((CircuitSymbol)splitted.InJam.CircuitSymbol, out HdlSymbol? other)) {
												HdlConnection.Create(symbol, connection.OutJam, i, other, splitted.InJam, exitBit);
											}
										}
									}
								}
								Propagate(connection.InJam, i);
							}
						} else if(consider((CircuitSymbol)connection.InJam.CircuitSymbol, false)) {
							if(symbolMap.TryGetValue((CircuitSymbol)connection.InJam.CircuitSymbol, out HdlSymbol? other)) {
								HdlConnection.Create(symbol, other, connection);
							}
						}
					}
				}
			}
			return symbolMap;
		}

		private string? ExportCircuit(LogicalCircuit circuit, Dictionary<CircuitSymbol, HdlSymbol> symbolMap) {
			HdlExport.OrderSymbols(symbolMap);

			List<HdlSymbol> inputPins = symbolMap.Values.Where(
				symbol => symbol.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input
			).OrderBy(s => (Pin)s.CircuitSymbol.Circuit, PinComparer.Comparer).ToList();

			List<HdlSymbol> outputPins = symbolMap.Values.Where(
				symbol => symbol.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Output
			).OrderBy(s => (Pin)s.CircuitSymbol.Circuit, PinComparer.Comparer).ToList();

			List<HdlSymbol> parts = symbolMap.Values.Where(symbol => {
				Circuit circuit = symbol.CircuitSymbol.Circuit;
				return (circuit is not Pin) && (circuit is not Constant);
			}).ToList();
			CircuitSymbolComparer comparer = new CircuitSymbolComparer(true);
			parts.Sort((x, y) => {
				int order = x.Order - y.Order;
				return (order == 0) ? comparer.Compare(x.CircuitSymbol, y.CircuitSymbol) : order;
			});

			parts.ForEach(symbol => symbol.SortConnections());

			HdlTransformation? transformation = null;
			switch(this.HdlExportType) {
			case HdlExportType.N2T:
			case HdlExportType.N2TFull:
				transformation = new N2THdl(circuit.Name, inputPins, outputPins, parts);
				break;
			default:
				throw new InvalidOperationException();
			}
			if(!this.Validate(transformation) || !transformation.Validate(this)) {
				return null;
			}
			transformation.CommentPoints = this.CommentPoints;
			return transformation.TransformText();
		}

		private static void OrderSymbols(Dictionary<CircuitSymbol, HdlSymbol> symbolMap) {
			void Order(HashSet<HdlSymbol> consider, HdlSymbol symbol, int order) {
				symbol.Order = Math.Max(symbol.Order, order);
				foreach(HdlConnection connection in symbol.HdlConnections().Where(con => con.OutJam.CircuitSymbol == symbol.CircuitSymbol)) {
					HdlSymbol other = symbolMap[(CircuitSymbol)connection.InJam.CircuitSymbol];
					if(consider.Add(other)) {
						Order(consider, other, symbol.Order + 1);
					}
				}
			}

			foreach(HdlSymbol symbol in symbolMap.Values.Where(s => s.CircuitSymbol.Circuit is Pin pin && pin.PinType == PinType.Input)) {
				Order(new HashSet<HdlSymbol>(), symbol, 1);
			}
		}

		private bool Validate(HdlTransformation transformation) {
			bool valid = true;
			Regex identifier = new Regex(@"^[a-zA-Z_][a-zA-Z_0-9]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
			List<Jam> jams = new List<Jam>();
			if(!identifier.IsMatch(transformation.Name)) {
				this.Error($"Invalid name of the circuit {transformation.Name}");
				valid = false;
			}
			foreach(HdlSymbol inputPin in transformation.InputPins) {
				string name = inputPin.Name;
				if(!identifier.IsMatch(name)) {
					this.Error($"Invalid name of input pin: {name}");
					valid = false;
				}
				jams.AddRange(inputPin.CircuitSymbol.Jams());
				foreach(HdlConnection connection in inputPin.HdlConnections()) {
					if(connection.ConnectsInputWithOutput()) {
						this.Error($"Pin {connection.InHdlSymbol.Name} connected directly to pin {connection.OutHdlSymbol.Name}");
						valid = false;
					}
				}
			}
			foreach(HdlSymbol outputPin in transformation.OutputPins) {
				string name = outputPin.Name;
				if(!identifier.IsMatch(name)) {
					this.Error($"Invalid name of output pin: {name}");
					valid = false;
				}
				jams.AddRange(outputPin.CircuitSymbol.Jams());
			}
			foreach(HdlSymbol symbol in transformation.Parts) {
				string name = symbol.Name;
				if(!identifier.IsMatch(name)) {
					this.Error($"Invalid name of part: {name}");
					valid = false;
				}
				jams.AddRange(symbol.CircuitSymbol.Jams());
			}
			Dictionary<GridPoint, Jam> points = new Dictionary<GridPoint, Jam>();
			foreach(Jam jam in jams) {
				if(!points.TryAdd(jam.AbsolutePoint, jam)) {
					Jam other = points[jam.AbsolutePoint];
					this.Error($"Jam {jam.Pin.Name} on {jam.CircuitSymbol.Circuit.Name}{jam.CircuitSymbol.Point} collocated with jam {other.Pin.Name} on {other.CircuitSymbol.Circuit.Name}{other.CircuitSymbol.Point}");
					valid = false;
				}
			}
			return valid;
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
				script.Append(CultureInfo.InvariantCulture, $" {field}%X1.1.1");
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
