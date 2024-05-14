// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LogicCircuit {
	internal abstract class HdlExport {
		internal const int MaxTestableInputBits = 16;

		private readonly struct JamBit : IEquatable<JamBit> {
			public Jam Jam { get; }
			public int Bit { get; }
			public JamBit(Jam jam, int bit) {
				this.Jam = jam;
				this.Bit = bit;
			}

			public bool Equals(JamBit other) => this.Jam == other.Jam && this.Bit == other.Bit;
			public override bool Equals(object? obj) => obj is JamBit && this.Equals((JamBit)obj);
			public override int GetHashCode() => HashCode.Combine(this.Jam, this.Bit);
		}

		public bool CommentPoints { get; }

		private readonly Action<string> logMessage;
		private readonly Action<string> logError;
		private int ErrorCount { get; set; }

		protected HdlExport(bool commentPoints, Action<string> logMessage, Action<string> logError) {
			this.CommentPoints = commentPoints;
			this.logMessage = logMessage;
			this.logError = logError;
		}

		protected abstract HdlTransformation? CreateTransformation(string name, IList<HdlSymbol> inputPins, IList<HdlSymbol> outputPins, IList<HdlSymbol> parts);
		protected virtual string FileName(LogicalCircuit circuit) => circuit.Name + ".hdl";
		public abstract bool CanExport(Circuit circuit);
		public abstract bool IsValid(string name);

		protected virtual bool PostExport(LogicalCircuit logicalCircuit, string folder) {
			this.Message(Properties.Resources.MessageHdlExportResut(logicalCircuit.Name, this.ErrorCount));
			return this.ErrorCount == 0;
		}

		public virtual string Name(HdlSymbol symbol) {
			Circuit circuit = symbol.CircuitSymbol.Circuit;
			if(circuit is Gate gate) {
				// Gate names are translated so, use their English names.
				switch(gate.GateType) {
				case GateType.Clock:return "Clock";
				case GateType.Not:	return "Not";
				case GateType.Or:	return gate.InvertedOutput ? "NOr" : "Or";
				case GateType.And:	return gate.InvertedOutput ? "NAnd" : "And";
				case GateType.Xor:	return gate.InvertedOutput ? "XNor" : "Xor";
				case GateType.Led:	return "LED";
				case GateType.Probe:return "Probe";
				case GateType.TriState1:
				case GateType.TriState2: return "Tri-state";
				}
			}
			return circuit.Name.Trim();
		}
		public virtual string Name(Jam jam) => jam.Pin.Name.Trim();

		protected void Message(string text) => this.logMessage(text);

		protected void Error(string text) {
			this.ErrorCount++;
			this.logError(text);
		}

		public bool ExportCircuit(LogicalCircuit circuit, string folder, bool onlyOne) {
			CircuitMap map = new CircuitMap(circuit);
			ConnectionSet connectionSet = map.ConnectionSet();

			bool export(LogicalCircuit logicalCircuit) {
				string file = this.FileName(logicalCircuit);
				this.Message(Properties.Resources.MessageExportingHdl(logicalCircuit.Name, file));
				Dictionary<CircuitSymbol, HdlSymbol> symbolMap = this.Collect(logicalCircuit, connectionSet);
				if(0 < this.ErrorCount) return false;
				string? hdl = this.ExportCircuit(logicalCircuit, symbolMap);
				if(!string.IsNullOrEmpty(hdl)) {
					string path = Path.Combine(folder, file);
					File.WriteAllText(path, hdl);
					return this.PostExport(logicalCircuit, folder);
				} else {
					this.Message(Properties.Resources.ErrorExportHdlFile(file));
				}
				return false;
			}

			bool walk(CircuitMap circuitMap, HashSet<LogicalCircuit> exported) {
				if(exported.Add(circuitMap.Circuit)) {
					if(export(circuitMap.Circuit)) {
						foreach(CircuitMap child in circuitMap.Children) {
							if(!walk(child, exported)) {
								return false;
							}
						}
						return true;
					}
					return false;
				}
				return true;
			}

			if(onlyOne) {
				return export(circuit);
			} else {
				return walk(map, new HashSet<LogicalCircuit>());
			}
		}

		private Dictionary<CircuitSymbol, HdlSymbol> Collect(LogicalCircuit circuit, ConnectionSet connectionSet) {
			bool consider(CircuitSymbol symbol, bool showError) {
				Circuit circuit = symbol.Circuit;
				if(!this.CanExport(circuit)) {
					if(showError) {
						this.Error(Properties.Resources.ErrorHdlCircuitUnsupported(circuit.Name, symbol.Point));
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
								HashSet<JamBit> visited = new HashSet<JamBit>();
								void Propagate(Jam enterJam, int enterBit) {
									if(visited.Add(new JamBit(enterJam, enterBit))) {
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

			HdlTransformation? transformation = this.CreateTransformation(circuit.Name, inputPins, outputPins, parts);
			if(transformation == null || !this.Validate(transformation)) {
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

			if(!this.IsValid(transformation.Name)) {
				this.Error(Properties.Resources.ErrorHdlCircuitName(transformation.Name));
				valid = false;
			}

			List<Jam> jams = new List<Jam>();
			foreach(HdlSymbol inputPin in transformation.InputPins) {
				string name = inputPin.Name;
				if(!this.IsValid(name)) {
					this.Error(Properties.Resources.ErrorHdlInputPinName(name));
					valid = false;
				}
				jams.AddRange(inputPin.CircuitSymbol.Jams());
				foreach(HdlConnection connection in inputPin.HdlConnections()) {
					if(connection.ConnectsInputWithOutput()) {
						this.Error(Properties.Resources.ErrorHdlPinDirectConnect(connection.InHdlSymbol.Name, connection.OutHdlSymbol.Name));
						valid = false;
					}
				}
			}
			foreach(HdlSymbol outputPin in transformation.OutputPins) {
				string name = outputPin.Name;
				if(!this.IsValid(name)) {
					this.Error(Properties.Resources.ErrorHdlOutputPinName(name));
					valid = false;
				}
				jams.AddRange(outputPin.CircuitSymbol.Jams());
			}
			foreach(HdlSymbol symbol in transformation.Parts.Where(p => !p.AutoGenerated)) {
				string name = symbol.Name;
				if(!this.IsValid(name)) {
					this.Error(Properties.Resources.ErrorHdlPartName(name));
					valid = false;
				}
				jams.AddRange(symbol.CircuitSymbol.Jams());
			}
			Dictionary<GridPoint, Jam> points = new Dictionary<GridPoint, Jam>();
			foreach(Jam jam in jams) {
				if(!points.TryAdd(jam.AbsolutePoint, jam)) {
					Jam other = points[jam.AbsolutePoint];
					this.Error(Properties.Resources.ErrorHdlJamsColocated(
						jam.Pin.Name, jam.CircuitSymbol.Circuit.Name, jam.CircuitSymbol.Point, other.Pin.Name, other.CircuitSymbol.Circuit.Name, other.CircuitSymbol.Point
					));
					valid = false;
				}
			}
			return valid;
		}
	}
}
