// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogicCircuit {
	public enum HdlExportType {
		N2T,
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
					file += ".hdl";
					break;
				}
				this.Message(Properties.Resources.MessageExportingHdl(logicalCircuit.Name, file));
				string hdl = this.ExportCircuit(logicalCircuit, connectionSet, splitters);
				if(!string.IsNullOrEmpty(hdl)) {
					file = Path.Combine(folder, file);
					File.WriteAllText(file, hdl);
					return true;
				} else {
					this.Message(Properties.Resources.ErrorExportHdlFile(file));
				}
				return false;
			}

			bool walk(CircuitMap circuitMap) {
				if(export(circuitMap.Circuit)) {
					foreach(CircuitMap child in circuitMap.Children) {
						if(!walk(child)) {
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
				return walk(map);
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
	}
}
