using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LogicCircuit {
	public partial class CircuitMap : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public LogicalCircuit Circuit { get; private set; }
		public CircuitSymbol CircuitSymbol { get; private set; }
		public CircuitMap Parent { get; private set; }

		private Dictionary<CircuitSymbol, CircuitMap> children;
		private HashSet<IFunctionVisual> displays;
		private Dictionary<CircuitSymbol, CircuitFunction> inputs;
		private Dictionary<CircuitSymbol, FunctionMemory> memories;

		private CircuitMap visible;
		public CircuitMap Visible {
			get {
				CircuitMap root = this.Root;
				return root.visible ?? root;
			}
			set {
				Tracer.Assert(value != null);
				CircuitMap old = this.Visible;
				if(old != value) {
					this.Root.visible = value;
					old.NotifyIsCurrentChanged();
					value.NotifyIsCurrentChanged();
				}
			}
		}

		public bool IsCurrent { get { return this == this.Visible; } }

		public CircuitMap(LogicalCircuit circuit) {
			this.Circuit = circuit;
			this.CircuitSymbol = null;
			this.Parent = null;
			this.Expand();
		}

		private CircuitMap(CircuitMap parent, CircuitSymbol circuitSymbol) {
			this.Circuit = (LogicalCircuit)circuitSymbol.Circuit;
			this.CircuitSymbol = circuitSymbol;
			this.Parent = parent;
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void NotifyIsCurrentChanged() {
			this.NotifyPropertyChanged("IsCurrent");
		}

		private void Path(StringBuilder text) {
			if(this.Parent != null) {
				this.Parent.Path(text);
			}
			text.Append(Resources.CircuitMapPathSeparator);
			text.Append(this.Circuit.Name);
			if(this.CircuitSymbol != null) {
				text.Append(this.CircuitSymbol.Point.ToString());
			}
		}

		private void Path(StringBuilder text, CircuitSymbol circuitSymbol) {
			Tracer.Assert(circuitSymbol.LogicalCircuit == this.Circuit);
			this.Path(text);
			text.Append(Resources.CircuitMapPathSeparator);
			text.Append(circuitSymbol.Circuit.Name);
			text.Append(circuitSymbol.Point.ToString());
		}

		public string Path() {
			StringBuilder text = new StringBuilder();
			this.Path(text);
			return text.ToString();
		}

		private string Path(CircuitSymbol circuitSymbol) {
			StringBuilder text = new StringBuilder();
			this.Path(text, circuitSymbol);
			return text.ToString();
		}

		private void Expand() {
			foreach(CircuitSymbol symbol in this.Circuit.CircuitSymbols()) {
				LogicalCircuit lc = symbol.Circuit as LogicalCircuit;
				if(lc != null) {
					if(this.HasLoop(lc)) {
						throw new CircuitException(Cause.UserError,
							Resources.ErrorLoopInCircuit(lc.Name, this.Circuit.Name)
						);
					}
					CircuitMap child = new CircuitMap(this, symbol);
					if(this.children == null) {
						this.children = new Dictionary<CircuitSymbol, CircuitMap>();
					}
					this.children.Add(symbol, child);
					child.Expand();
				}
			}
		}

		private void ConnectMap(ConnectionSet connectionSet) {
			if(!connectionSet.IsConnected(this.Circuit)) {
				if(this.children != null) {
					foreach(CircuitMap child in this.children.Values) {
						child.ConnectMap(connectionSet);
					}
				}
				this.Connect(connectionSet);
				connectionSet.MarkConnected(this.Circuit);
			}
		}

		private void Connect(ConnectionSet connectionSet) {
			Tracer.Assert(!connectionSet.IsConnected(this.Circuit));
			Dictionary<GridPoint, List<Jam>> inJamMap = new Dictionary<GridPoint, List<Jam>>();
			foreach(CircuitSymbol symbol in this.Circuit.CircuitSymbols()) {
				foreach(Jam jam in symbol.Jams()) {
					if(jam.Pin.PinType != PinType.Output) {
						GridPoint p = jam.AbsolutePoint;
						List<Jam> list;
						if(!inJamMap.TryGetValue(p, out list)) {
							list = new List<Jam>();
							inJamMap.Add(p, list);
						}
						list.Add(jam);
					}
				}
			}
			ConductorMap conductorMap = this.Circuit.ConductorMap();
			foreach(CircuitSymbol symbol in this.Circuit.CircuitSymbols()) {
				foreach(Jam outJam in symbol.Jams()) {
					if(outJam.Pin.PinType != PinType.Input) {
						Conductor conductor;
						if(conductorMap.TryGetValue(outJam.AbsolutePoint, out conductor)) {
							foreach(GridPoint point in conductor.Points) {
								List<Jam> list;
								if(inJamMap.TryGetValue(point, out list)) {
									foreach(Jam inJam in list) {
										if(inJam != outJam) {
											if(inJam.Pin.BitWidth != outJam.Pin.BitWidth) {
												Gate gate = inJam.CircuitSymbol.Circuit as Gate;
												if(gate == null || gate.GateType != GateType.Probe) {
													throw new CircuitException(Cause.UserError,
														Resources.ErrorJamBitWidthDifferent(
															inJam.CircuitSymbol.Circuit.Name, inJam.CircuitSymbol.Point,
															outJam.CircuitSymbol.Circuit.Name, outJam.CircuitSymbol.Point,
															this.Circuit.Name
														)
													);
												}
											}
											connectionSet.Connect(inJam, outJam);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public CircuitState Apply(int probeCapacity) {
			Tracer.Assert(this.Circuit != null && this.CircuitSymbol == null && this.Parent == null, "This method should be called on root only");

			ConnectionSet connectionSet = new ConnectionSet();
			this.ConnectMap(connectionSet);
			
			// Flatten the circuit
			SymbolMapList list = new SymbolMapList();
			this.Collect(list);
			CircuitMap.Connect(connectionSet, list);

			// Remove not used results
			CircuitMap.CleanUp(list);

			//TODO: optimize the list. What if sort it in such way that state will be allocated with a better locality, so it will be less cache misses?

			CircuitState circuitState = new CircuitState(3);
			// Allocate states for each result
			foreach(SymbolMap symbolMap in list.SymbolMaps) {
				foreach(Result result in symbolMap.Results) {
					result.Allocate(circuitState);
				}
			}

			// Generate functions
			foreach(SymbolMap symbolMap in list.SymbolMaps) {
				this.Apply(circuitState, symbolMap, probeCapacity);
			}
			circuitState.EndDefinition();

			return circuitState;
		}

		private void Collect(SymbolMapList list) {
			foreach(CircuitSymbol symbol in this.Circuit.CircuitSymbols()) {
				if(CircuitMap.IsPrimitive(symbol.Circuit)) {
					list.AddSymbol(this, symbol);
					foreach(Jam jam in symbol.Jams()) {
						if(jam.Pin.PinType == PinType.Output) {
							for(int i = 0; i < jam.Pin.BitWidth; i++) {
								list.AddResult(this, jam, i);
							}
						}
					}
				} else {
					if(symbol.Circuit is LogicalCircuit) {
						this.children[symbol].Collect(list);
					} else {
						Tracer.Assert(symbol.Circuit is Pin || symbol.Circuit is Splitter);
					}
				}
			}
		}

		private static void Connect(ConnectionSet connectionSet, SymbolMapList list) {
			foreach(SymbolMap symbolMap in list.SymbolMaps) {
				foreach(Result result in symbolMap.Results) {
					result.CircuitMap.Connect(connectionSet, list, result, result.Jam, result.BitNumber);
				}
			}
		}

		private void Connect(ConnectionSet connectionSet, SymbolMapList list, Result result, Jam jam, int bitNumber) {
			Tracer.Assert(bitNumber < jam.Pin.BitWidth);
			foreach(Connection con in connectionSet.SelectByOutput(jam)) {
				Tracer.Assert(con.InJam.CircuitSymbol.LogicalCircuit == this.Circuit);
				Tracer.Assert(con.OutJam.CircuitSymbol.LogicalCircuit == this.Circuit);
				Circuit circuit = con.InJam.CircuitSymbol.Circuit;
				Pin pin;
				if(CircuitMap.IsPrimitive(circuit)) {
					list.AddParameter(result, this, con.InJam, bitNumber);
				} else if((pin = (circuit as Pin)) != null) {
					if(this.Parent != null) {
						this.Parent.Connect(connectionSet, list, result, this.CircuitSymbol.Jam(pin), bitNumber);
					}
				} else if(circuit is LogicalCircuit) {
					this.children[(CircuitSymbol)con.InJam.CircuitSymbol].Connect(connectionSet, list, result, con.InJam.InnerJam, bitNumber);
				} else {
					Splitter splitter = circuit as Splitter;
					if(splitter != null) {
						int splitterPinCount = splitter.PinCount;
						int splitterBitWidth = splitter.BitWidth;
						List<Jam> jams = con.InJam.CircuitSymbol.Jams().ToList();
						// Sort jams in order of their device pins. Assuming first one will be the wide pin and the rest are thin ones,
						// starting from lower bits to higher. This implies that creating of the pins should happened in that order.
						jams.Sort(JamComparer.Comparer);

						Tracer.Assert(1 < splitterPinCount && splitterPinCount <= splitterBitWidth);
						Tracer.Assert(jams.Count == splitterPinCount + 1 && jams[0].Pin.BitWidth == splitterBitWidth);
						{	int sum = 0;
							for(int i = 1; i < jams.Count; sum += jams[i++].Pin.BitWidth);
							Tracer.Assert(jams[0].Pin.BitWidth == sum);
						}

						if(con.InJam == jams[0]) { //wide jam. so find thin one, this bit will be redirected to
							Tracer.Assert(0 <= bitNumber && bitNumber < splitterBitWidth);
							int width = 0;
							for(int i = 1; i < jams.Count; i++) {
								if(bitNumber < width + jams[i].Pin.BitWidth) {
									this.Connect(connectionSet, list, result, jams[i], bitNumber - width);
									break;
								}
								width += jams[i].Pin.BitWidth;
							}
							Tracer.Assert(width < splitterBitWidth); // check if the thin pin was found
						} else { // thin jam. find position of this bit in wide pin
							int width = 0;
							for(int i = 1; i < jams.Count; i++) {
								if(jams[i] == con.InJam) {
									this.Connect(connectionSet, list, result, jams[0], width + bitNumber);
									break;
								}
								width += jams[i].Pin.BitWidth;
							}
							Tracer.Assert(width < splitterBitWidth); // check if the thin pin was found
						}
					} else {
						Tracer.Fail();
					}
				}
			}
		}

		private static void CleanUp(SymbolMapList list) {
			HashSet<SymbolMap> unconnected = new HashSet<SymbolMap>();
			do {
				unconnected.Clear();
				foreach(SymbolMap symbolMap in list.SymbolMaps) {
					if(symbolMap.HasResults) {
						bool connected = false;
						foreach(Result result in symbolMap.Results) {
							if(0 < result.Parameters.Count) {
								connected = true;
								break;
							}
						}
						if(!connected) {
							foreach(Parameter parameter in symbolMap.Parameters) {
								parameter.Result.Parameters.Remove(parameter);
							}
							unconnected.Add(symbolMap);
						}
					}
				}
				foreach(SymbolMap map in unconnected) {
					list.Remove(map);
				}
			} while(0 < unconnected.Count);
		}

		public CircuitMap Child(CircuitSymbol symbol) {
			CircuitMap map;
			if(this.children != null && this.children.TryGetValue(symbol, out map)) {
				return map;
			}
			return null;
		}

		// This is not very effective algorithm
		public FunctionProbe FunctionProbe(CircuitSymbol symbol) {
			if(this.displays != null) {
				foreach(IFunctionVisual visual in this.displays) {
					FunctionProbe probe = visual as FunctionProbe;
					if(probe != null && probe.CircuitSymbol == symbol) {
						return probe;
					}
				}
			}
			return null;
		}

		public FunctionMemory FunctionMemory(CircuitSymbol symbol) {
			if(this.memories != null) {
				FunctionMemory memory;
				if(this.memories.TryGetValue(symbol, out memory)) {
					return memory;
				}
			}
			return null;
		}

		public bool IsVisible(IFunctionVisual function) {
			return this.displays != null && this.displays.Contains(function);
		}

		public void TurnOn() {
			if(this.displays != null) {
				foreach(IFunctionVisual func in this.displays) {
					func.TurnOn();
				}
			}
			if(this.inputs != null) {
				foreach(CircuitFunction func in this.inputs.Values) {
					IFunctionVisual visual = func as IFunctionVisual;
					if(visual != null) {
						visual.TurnOn();
					}
				}
			}
			foreach(CircuitMap map in this.Children) {
				map.TurnOn();
			}
		}

		public void TurnOff() {
			if(this.displays != null) {
				foreach(IFunctionVisual func in this.displays) {
					func.TurnOff();
				}
			}
			if(this.inputs != null) {
				foreach(CircuitFunction func in this.inputs.Values) {
					IFunctionVisual visual = func as IFunctionVisual;
					if(visual != null) {
						visual.TurnOff();
					}
				}
			}
			foreach(CircuitMap map in this.Children) {
				map.TurnOff();
			}
		}

		public void Redraw() {
			if(this.displays != null) {
				foreach(IFunctionVisual func in this.displays) {
					func.Redraw();
				}
			}
		}

		public CircuitFunction Input(CircuitSymbol symbol) {
			if(this.inputs != null) {
				CircuitFunction function;
				if(this.inputs.TryGetValue(symbol,  out function)) {
					return function;
				}
			}
			return null;
		}

		public IEnumerable<CircuitMap> Children {
			get {
				if(this.children == null) {
					return Enumerable.Empty<CircuitMap>();
				}
				return this.children.Values.OrderBy(map => map.CircuitSymbol.Y * Symbol.LogicalCircuitGridWidth + map.CircuitSymbol.X);
			}
		}

		public CircuitMap Root {
			get {
				CircuitMap map = this;
				while(map.Parent != null) {
					map = map.Parent;
				}
				return map;
			}
		}

		public bool IsRoot {
			get { return this.Parent == null; }
		}

		private static bool IsPrimitive(Circuit circuit) {
			return circuit is Gate || circuit is CircuitButton || circuit is Constant || circuit is Memory;
		}

		private bool HasLoop(LogicalCircuit circuit) {
			CircuitMap map = this;
			while(map != null) {
				if(map.Circuit == circuit) {
					return true;
				}
				map = map.Parent;
			}
			return false;
		}

		public FrameworkElement CircuitGlyph {
			get { return new LogicalCircuitDescriptor(this.Circuit, s => false).CircuitGlyph.Glyph; }
		}

		private void Apply(CircuitState circuitState, SymbolMap symbolMap, int probeCapacity) {
			if(symbolMap.CircuitSymbol.Circuit is Gate) {
				Gate gate = (Gate)symbolMap.CircuitSymbol.Circuit;
				switch(gate.GateType) {
				case GateType.Clock:
					CircuitMap.DefineClock(circuitState, symbolMap);
					break;
				case GateType.Not:
				case GateType.Or:
				case GateType.And:
				case GateType.Xor:
				case GateType.Odd:
				case GateType.Even:
					CircuitMap.DefineGate(gate, circuitState, symbolMap);
					break;
				case GateType.Led:
					this.DefineLed(circuitState, symbolMap);
					break;
				case GateType.Probe:
					CircuitMap.DefineProbe(circuitState, symbolMap, probeCapacity);
					break;
				case GateType.TriState:
					CircuitMap.DefineTriState(circuitState, symbolMap);
					break;
				case GateType.Nop:
				default:
					Tracer.Fail();
					break;
				}
			} else if(symbolMap.CircuitSymbol.Circuit is CircuitButton) {
				CircuitMap.DefineButton(circuitState, symbolMap);
			} else if(symbolMap.CircuitSymbol.Circuit is Constant) {
				CircuitMap.DefineConstant(circuitState, symbolMap);
			} else if(symbolMap.CircuitSymbol.Circuit is Memory) {
				Memory memory = (Memory)symbolMap.CircuitSymbol.Circuit;
				if(memory.Writable) {
					CircuitMap.DefineRam(circuitState, symbolMap);
				} else {
					CircuitMap.DefineRom(circuitState, symbolMap);
				}
			} else {
				Tracer.Fail();
			}
		}

		private static int SingleResult(IEnumerable<Result> results) {
			int index = 0;
			foreach(Result result in results) {
				if(index == 0) {
					index = result.StateIndex;
					Tracer.Assert(0 < index);
				} else {
					Tracer.Fail();
				}
			}
			return index;
		}

		private static int SingleResult(SymbolMap symbolMap) {
			int index = CircuitMap.SingleResult(symbolMap.Results);
			Tracer.Assert(0 < index);
			return index;
		}

		private static int[] Results(List<Result> results) {
			int[] map = new int[results.Count];
			for(int i = 0; i < map.Length; i++) {
				map[i] = results[i].StateIndex;
			}
			return map;
		}

		private static int[] Parameters(List<Parameter> parameters) {
			int[] map = new int[parameters.Count];
			for(int i = 0; i < map.Length; i++) {
				map[i] = parameters[i].Result.StateIndex;
			}
			return map;
		}

		private static CircuitFunction DefineClock(CircuitState circuitState, SymbolMap symbolMap) {
			int index = CircuitMap.SingleResult(symbolMap);
			Tracer.Assert(0 < index);
			return new FunctionClock(circuitState, index);
		}

		private static CircuitFunction DefineGate(Gate gate, CircuitState circuitState, SymbolMap symbolMap) {
			int result = CircuitMap.SingleResult(symbolMap);
			int[] parameter = null;
			List<int> mapList = new List<int>();
			foreach(Parameter p in symbolMap.Parameters) {
				int index = p.Result.StateIndex;
				if(0 < index) {
					mapList.Add(index);
				}
			}
			if(0 < mapList.Count) {
				mapList.Sort();
				parameter = mapList.ToArray();
			}
			if(parameter != null) {
				if(gate.InvertedOutput) {
					switch(gate.GateType) {
					case GateType.Not:	return new FunctionNot(circuitState, parameter[0], result);
					case GateType.Or:	return new FunctionOrNot(circuitState, parameter, result);
					case GateType.And:	return new FunctionAndNot(circuitState, parameter, result);
					case GateType.Xor:	return new FunctionXorNot(circuitState, parameter, result);
					}
				} else {
					switch(gate.GateType) {
					case GateType.Or:	return new FunctionOr(circuitState, parameter, result);
					case GateType.And:	return new FunctionAnd(circuitState, parameter, result);
					case GateType.Xor:	return new FunctionXor(circuitState, parameter, result);
					case GateType.Odd:	return new FunctionOdd(circuitState, parameter, result);
					case GateType.Even:	return new FunctionEven(circuitState, parameter, result);
					}
				}
				Tracer.Fail();
			}
			return null;
		}

		private CircuitFunction DefineLed(CircuitState circuitState, SymbolMap symbolMap) {
			//The jams have special meaning here, so lets go from them
			List<Jam> jam = symbolMap.CircuitSymbol.Jams().ToList();
			Tracer.Assert(jam != null && (jam.Count == 1 || jam.Count == 8) &&
				jam.TrueForAll(j => j != null && j.Pin.PinType == PinType.Input)
			);
			CircuitFunction function = null;
			if(jam.Count == 1) {
				Parameter parameter = symbolMap.Parameter(jam[0], 0);
				if(parameter != null) {
					function = new FunctionLed(circuitState, symbolMap.CircuitSymbol, parameter.Result.StateIndex);
				} else {
					Tracer.FullInfo(this.GetType().Name, "{0} on {1}{2} is not connected",
						symbolMap.CircuitSymbol.Circuit, symbolMap.CircuitSymbol.LogicalCircuit, symbolMap.CircuitSymbol.Point
					);
				}
			} else {
				jam.Sort(JamComparer.Comparer);
				int[] param = new int[8];
				bool connected = false;
				for(int i = 0; i < jam.Count; i++) {
					Parameter parameter = symbolMap.Parameter(jam[i], 0);
					if(parameter != null) {
						param[i] = parameter.Result.StateIndex;
						connected = true;
					} else {
						param[i] = 0;
						Tracer.FullInfo(this.GetType().Name, "{0} on {1}{2} is not connected",
							symbolMap.CircuitSymbol.Circuit, symbolMap.CircuitSymbol.LogicalCircuit, symbolMap.CircuitSymbol.Point
						);
					}
				}
				if(connected) {
					function = new Function7Segment(circuitState, symbolMap.CircuitSymbol, param);
				}
			}
			if(function != null) {
				if(symbolMap.CircuitMap.displays == null) {
					symbolMap.CircuitMap.displays = new HashSet<IFunctionVisual>();
				}
				symbolMap.CircuitMap.displays.Add((IFunctionVisual)function);
			}
			return function;
		}

		private static CircuitFunction DefineProbe(CircuitState circuitState, SymbolMap symbolMap, int capacity) {
			List<Parameter> list = new List<Parameter>(symbolMap.Parameters);
			list.Sort(ParameterComparer.BitOrderComparer);
			int[] parameters = new int[list.Count];
			for(int i = 0; i < parameters.Length; i++) {
				parameters[i] = list[i].Result.StateIndex;
			}
			if(parameters != null && 0 < parameters.Length) {
				FunctionProbe probe = new FunctionProbe(symbolMap.CircuitSymbol, circuitState, parameters, capacity);
				probe.Label = symbolMap.CircuitMap.Path(symbolMap.CircuitSymbol);

				if(symbolMap.CircuitMap.displays == null) {
					symbolMap.CircuitMap.displays = new HashSet<IFunctionVisual>();
				}
				symbolMap.CircuitMap.displays.Add(probe);
				return probe;
			}
			return null;
		}

		private static CircuitFunction DefineTriState(CircuitState circuitState, SymbolMap symbolMap) {
			//The jams have special meaning here, so lets go from them
			List<Jam> jam = symbolMap.CircuitSymbol.Jams().ToList();
			Tracer.Assert(jam != null && jam.Count == 3);
			jam.Sort(JamComparer.Comparer);
			Tracer.Assert(jam[0].Pin.PinType == PinType.Input && jam[1].Pin.PinType == PinType.Input && jam[2].Pin.PinType == PinType.Output);
			Result result = symbolMap.Result(jam[2], 0);
			Tracer.Assert(result != null);
			if(0 < result.TriStateGroup.Count) {
				int[] group = new int[result.TriStateGroup.Count];
				int index = 0;
				foreach(Result r in result.TriStateGroup) {
					Tracer.Assert(0 < r.PrivateIndex);
					group[index++] = r.PrivateIndex;
				}
				CircuitMap.DefineTriStateGroup(circuitState, group, result.StateIndex);
				result.TriStateGroup.Clear();
			}
			Parameter parameter = symbolMap.Parameter(jam[0], 0);
			Parameter enable = symbolMap.Parameter(jam[1], 0);
			bool build = true;
			if(enable == null) {
				Tracer.FullInfo("DefineTriState", "Enable bit of {0} on {1}{2} is not connected",
					symbolMap.CircuitSymbol.Circuit, symbolMap.CircuitSymbol.LogicalCircuit, symbolMap.CircuitSymbol.Point
				);
				build = false;
			}
			if(parameter == null) {
				Tracer.FullInfo("DefineTriState", "Data bit of {0} on {1}{2} is not connected",
					symbolMap.CircuitSymbol.Circuit, symbolMap.CircuitSymbol.LogicalCircuit, symbolMap.CircuitSymbol.Point
				);
				build = false;
			}
			if(build) {
				return new FunctionTriState(circuitState,
					(parameter != null) ? parameter.Result.StateIndex : 0,
					(enable != null) ? enable.Result.StateIndex : 0,
					result.PrivateIndex
				);
			}
			return null;
		}

		private static CircuitFunction DefineTriStateGroup(CircuitState circuitState, int[] group, int stateIndex) {
			return new FunctionTriStateGroup(circuitState, group, stateIndex);
		}

		private static CircuitFunction DefineButton(CircuitState circuitState, SymbolMap symbolMap) {
			int index = CircuitMap.SingleResult(symbolMap);
			Tracer.Assert(0 < index);
			FunctionButton button = new FunctionButton(circuitState, symbolMap.CircuitSymbol, index);
			if(symbolMap.CircuitMap.inputs == null) {
				symbolMap.CircuitMap.inputs = new Dictionary<CircuitSymbol, CircuitFunction>();
			}
			symbolMap.CircuitMap.inputs.Add(symbolMap.CircuitSymbol, button);
			if(button.IsToggle) {
				if(symbolMap.CircuitMap.displays == null) {
					symbolMap.CircuitMap.displays = new HashSet<IFunctionVisual>();
				}
				symbolMap.CircuitMap.displays.Add(button);
			}
			return button;
		}

		private static CircuitFunction DefineConstant(CircuitState circuitState, SymbolMap symbolMap) {
			List<Result> results = new List<Result>(symbolMap.Results);
			results.Sort(ResultComparer.BitOrderComparer);
			int[] map = CircuitMap.Results(results);
			Constant c = (Constant)symbolMap.CircuitSymbol.Circuit;
			Tracer.Assert(map.Length == c.BitWidth);
			FunctionConstant constant = new FunctionConstant(circuitState, symbolMap.CircuitSymbol, map);
			if(symbolMap.CircuitMap.inputs == null) {
				symbolMap.CircuitMap.inputs = new Dictionary<CircuitSymbol, CircuitFunction>();
			}
			symbolMap.CircuitMap.inputs.Add(symbolMap.CircuitSymbol, constant);
			return constant;
		}

		private static CircuitFunction DefineRom(CircuitState circuitState, SymbolMap symbolMap) {
			Memory memory = (Memory)symbolMap.CircuitSymbol.Circuit;
			Tracer.Assert(!memory.Writable && symbolMap.CircuitSymbol.Jams().Count() == 2);
			List<Result> results = new List<Result>(symbolMap.Results);
			List<Parameter> parameters = new List<Parameter>(symbolMap.Parameters);
			Tracer.Assert(results.Count <= memory.DataOutPin.BitWidth);
			Tracer.Assert(parameters.Count <= memory.AddressPin.BitWidth);
			if(parameters.Count < memory.AddressPin.BitWidth) {
				throw new CircuitException(Cause.UserError, Resources.ErrorAddressNotConnected(symbolMap.CircuitMap.Path(symbolMap.CircuitSymbol)));
			}
			Tracer.Assert(results.TrueForAll(r => r.Jam.Pin == memory.DataOutPin));
			Tracer.Assert(parameters.TrueForAll(p => p.Jam.Pin == memory.AddressPin));
			results.Sort(ResultComparer.BitOrderComparer);
			parameters.Sort(ParameterComparer.BitOrderComparer);
			FunctionRom rom = new FunctionRom(circuitState, CircuitMap.Parameters(parameters), CircuitMap.Results(results), memory.RomValue());
			if(symbolMap.CircuitMap.memories == null) {
				symbolMap.CircuitMap.memories = new Dictionary<CircuitSymbol, FunctionMemory>();
			}
			symbolMap.CircuitMap.memories.Add(symbolMap.CircuitSymbol, rom);
			return rom;
		}

		private static CircuitFunction DefineRam(CircuitState circuitState, SymbolMap symbolMap) {
			Memory memory = (Memory)symbolMap.CircuitSymbol.Circuit;
			Tracer.Assert(memory.Writable && symbolMap.CircuitSymbol.Jams().Count() == 4);
			List<Result> dataOut = new List<Result>(symbolMap.Results);
			Tracer.Assert(dataOut.TrueForAll(o => o.Jam.Pin == memory.DataOutPin));
			List<Parameter> address = new List<Parameter>();
			List<Parameter> dataIn = new List<Parameter>();
			Parameter write = null;
			foreach(Parameter p in symbolMap.Parameters) {
				if(p.Jam.Pin == memory.AddressPin) {
					address.Add(p);
				} else if(p.Jam.Pin == memory.DataInPin) {
					dataIn.Add(p);
				} else {
					Tracer.Assert(p.Jam.Pin == memory.WritePin && write == null);
					write = p;
				}
			}
			Tracer.Assert(address.Count <= memory.AddressBitWidth);
			Tracer.Assert(dataOut.Count <= memory.DataBitWidth);
			Tracer.Assert(dataIn.Count <= memory.DataBitWidth);

			if(address.Count != memory.AddressBitWidth) {
				throw new CircuitException(Cause.UserError, Resources.ErrorAddressNotConnected(symbolMap.CircuitMap.Path(symbolMap.CircuitSymbol)));
			}
			if(dataIn.Count != memory.DataBitWidth) {
				throw new CircuitException(Cause.UserError, Resources.ErrorDataInNotConnected(symbolMap.CircuitMap.Path(symbolMap.CircuitSymbol)));
			}
			dataOut.Sort(ResultComparer.BitOrderComparer);
			address.Sort(ParameterComparer.BitOrderComparer);
			dataIn.Sort(ParameterComparer.BitOrderComparer);
			FunctionRam ram = new FunctionRam(circuitState,
				CircuitMap.Parameters(address), CircuitMap.Parameters(dataIn), CircuitMap.Results(dataOut),
				(write != null) ? write.Result.StateIndex : 0, memory.WriteOn1
			);
			if(symbolMap.CircuitMap.memories == null) {
				symbolMap.CircuitMap.memories = new Dictionary<CircuitSymbol, FunctionMemory>();
			}
			symbolMap.CircuitMap.memories.Add(symbolMap.CircuitSymbol, ram);
			return ram;
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(Resources.Culture, "CircuitMap of {0}", this.Circuit.Notation);
			}
		#endif
	}
}
