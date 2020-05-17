using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	partial class CircuitMap {
		private class SymbolMapList {
			private readonly Dictionary<SymbolMapKey, SymbolMap> symbols = new Dictionary<SymbolMapKey, SymbolMap>();

			public SymbolMap AddSymbol(CircuitMap circuitMap, CircuitSymbol circuitSymbol) {
				SymbolMapKey key = new SymbolMapKey(circuitMap, circuitSymbol);
				SymbolMap map;
				if(!this.symbols.TryGetValue(key, out map)) {
					map = new SymbolMap(key);
					this.symbols.Add(key, map);
				}
				return map;
			}

			public Result AddResult(CircuitMap circuitMap, Jam jam, int bitNumber) {
				Tracer.Assert(CircuitMap.IsPrimitive(jam.CircuitSymbol.Circuit));
				SymbolMap map = this.AddSymbol(circuitMap, (CircuitSymbol)jam.CircuitSymbol);
				return map.AddResult(circuitMap, jam, bitNumber);
			}

			public Parameter AddParameter(Result result, CircuitMap circuitMap, Jam jam, int bitNumber) {
				Tracer.Assert(CircuitMap.IsPrimitive(jam.CircuitSymbol.Circuit));
				SymbolMap map = this.AddSymbol(circuitMap, (CircuitSymbol)jam.CircuitSymbol);
				return map.AddParameter(result, circuitMap, jam, bitNumber);
			}

			public IEnumerable<SymbolMap> SymbolMaps {
				get { return this.symbols.Values; }
			}

			public void Remove(SymbolMap map) {
				this.symbols.Remove(new SymbolMapKey(map.CircuitMap, map.CircuitSymbol));
			}
		}

		private struct SymbolMapKey {
			public CircuitMap CircuitMap { get; }
			public CircuitSymbol CircuitSymbol { get; }

			public SymbolMapKey(CircuitMap circuitMap, CircuitSymbol circuitSymbol) {
				this.CircuitMap = circuitMap;
				this.CircuitSymbol = circuitSymbol;
			}

			public override bool Equals(object obj) {
				if(obj != null && this.GetType() == obj.GetType()) {
					SymbolMapKey other = (SymbolMapKey)obj;
					return this.CircuitMap == other.CircuitMap && this.CircuitSymbol == other.CircuitSymbol;
				}
				return false;
			}

			public override int GetHashCode() {
				return this.CircuitMap.GetHashCode() ^ this.CircuitSymbol.GetHashCode();
			}

			#if DEBUG
				public override string ToString() {
					return string.Format(CultureInfo.InvariantCulture, "SymbolMapKey of {0}", this.CircuitMap.Path(this.CircuitSymbol));
				}
			#endif
		}

		private class SymbolMap {

			private struct JamKey {
				public Jam Jam { get; }
				public int BitNumber { get; }

				public JamKey(Jam jam, int bitNumber) {
					Tracer.Assert(jam.IsValid(bitNumber));
					this.Jam = jam;
					this.BitNumber = bitNumber;
				}

				public override bool Equals(object obj) {
					if(obj != null && this.GetType() == obj.GetType()) {
						JamKey other = (JamKey)obj;
						return this.Jam == other.Jam && this.BitNumber == other.BitNumber;
					}
					return false;
				}

				public override int GetHashCode() {
					return this.Jam.GetHashCode() ^ this.BitNumber;
				}

				#if DEBUG
					public override string ToString() {
						return string.Format(CultureInfo.InvariantCulture, "JamKey of jam{0} bit #{1}",
							this.Jam.AbsolutePoint.ToString(), this.BitNumber
						);
					}
				#endif
			}

			private readonly Dictionary<JamKey, Result> results = new Dictionary<JamKey, Result>();
			private readonly Dictionary<JamKey, Parameter> parameters = new Dictionary<JamKey, Parameter>();
			private readonly SymbolMapKey key;

			public SymbolMap(SymbolMapKey key) {
				this.key = key;
			}

			public Result AddResult(CircuitMap circuitMap, Jam jam, int bitNumber) {
				Tracer.Assert(circuitMap.Circuit == jam.CircuitSymbol.LogicalCircuit && jam.CircuitSymbol == this.key.CircuitSymbol);
				Result result =  new Result(circuitMap, jam, bitNumber);
				this.results.Add(new JamKey(jam, bitNumber), result);
				return result;
			}

			public Parameter AddParameter(Result result, CircuitMap circuitMap, Jam jam, int bitNumber) {
				Tracer.Assert(circuitMap.Circuit == jam.CircuitSymbol.LogicalCircuit && jam.CircuitSymbol == this.key.CircuitSymbol);
				JamKey jamKey = new JamKey(jam, bitNumber);
				Parameter parameter;
				if(this.parameters.TryGetValue(jamKey, out parameter)) {
					if(!parameter.Result.IsTriState || !result.IsTriState) {
						if(parameter.Result == result && parameter.CircuitMap == circuitMap && parameter.Jam == jam && parameter.BitNumber == bitNumber) {
							return parameter;
						}
						CircuitGlyph symbol = jam.CircuitSymbol;
						throw new CircuitException(Cause.UserError,
							Properties.Resources.ErrorManyResults(jam.Pin.Name, symbol.Circuit.Notation + symbol.Point.ToString())
						);
					}
					parameter.Result.Link(result);
				} else {
					parameter = new Parameter(result, circuitMap, jam, bitNumber);
					this.parameters.Add(jamKey, parameter);
					result.Add(parameter);
				}
				return parameter;
			}

			public CircuitMap CircuitMap {
				get { return this.key.CircuitMap; }
			}

			public CircuitSymbol CircuitSymbol {
				get { return this.key.CircuitSymbol; }
			}

			public bool HasResults {
				get { return 0 < this.results.Count; }
			}

			public IEnumerable<Result> Results {
				get { return this.results.Values; }
			}

			public IEnumerable<Parameter> Parameters {
				get { return this.parameters.Values; }
			}

			public Result Result(Jam jam, int bitNumber) {
				if(this.results.TryGetValue(new JamKey(jam, bitNumber), out Result resut)) {
					return resut;
				}
				return null;
			}

			public Parameter Parameter(Jam jam, int bitNumber) {
				if(this.parameters.TryGetValue(new JamKey(jam, bitNumber), out Parameter parameter)) {
					return parameter;
				}
				return null;
			}

			public IEnumerable<Parameter> AllParameters(Jam jam) => this.parameters.Where(p => p.Key.Jam == jam).OrderBy(p => p.Key.BitNumber).Select(p => p.Value);

			#if DEBUG
				public override string ToString() {
					return string.Format(CultureInfo.InvariantCulture, "SymbolMap of {0}", this.CircuitMap.Path(this.CircuitSymbol));
				}
			#endif
		}

		private abstract class StateIndex {

			public CircuitMap CircuitMap { get; private set; }
			public Jam Jam { get; private set; }
			public int BitNumber { get; private set; }

			protected StateIndex(CircuitMap circuitMap, Jam jam, int bitNumber) {
				Tracer.Assert(circuitMap.Circuit == jam.CircuitSymbol.LogicalCircuit);
				Tracer.Assert(jam.IsValid(bitNumber));
				this.CircuitMap = circuitMap;
				this.Jam = jam;
				this.BitNumber = bitNumber;
			}

			public override bool Equals(object obj) {
				if(object.ReferenceEquals(this, obj)) {
					return true;
				}
				if(obj != null && this.GetType() == obj.GetType()) {
					StateIndex other = (StateIndex)obj;
					return this.CircuitMap == other.CircuitMap && this.Jam == other.Jam && this.BitNumber == other.BitNumber;
				}
				return false;
			}

			public override int GetHashCode() {
				return this.CircuitMap.GetHashCode() ^ this.Jam.GetHashCode() ^ this.BitNumber;
			}
		}

		private class Result : StateIndex {

			public HashSet<Result> TriStateGroup { get; private set; }
			public bool IsTriState { get { return this.TriStateGroup != null; } }
			public List<Parameter> Parameters { get; private set; }
			public int StateIndex { get; private set; }
			public int PrivateIndex { get; private set; }

			public Result(CircuitMap circuitMap, Jam jam, int bitNumber) : base(circuitMap, jam, bitNumber) {
				Tracer.Assert(jam.Pin.PinType == PinType.Output);
				this.Parameters = new List<Parameter>();
				if(jam.Pin.Circuit is Gate gate && (gate.GateType == GateType.TriState1 || gate.GateType == GateType.TriState2)) {
					this.TriStateGroup = new HashSet<Result> { this };
				}
			}

			public void Add(Parameter parameter) {
				this.Parameters.Add(parameter);
			}

			public void Link(Result other) {
				Tracer.Assert(this.IsTriState);
				if(this.TriStateGroup != other.TriStateGroup) {
					this.Parameters.AddRange(other.Parameters);
					this.TriStateGroup.UnionWith(other.TriStateGroup);
					foreach(Result r in other.TriStateGroup) {
						r.TriStateGroup = this.TriStateGroup;
						r.Parameters = this.Parameters;
					}
				}
			}

			public void Allocate(CircuitState circuitState) {
				Tracer.Assert(this.PrivateIndex == 0);
				if(this.TriStateGroup != null && 0 < this.TriStateGroup.Count) {
					if(this.StateIndex == 0) {
						this.StateIndex = circuitState.ReserveState();
						foreach(Result r in this.TriStateGroup) {
							Tracer.Assert(r == this || r.StateIndex == 0);
							r.StateIndex = this.StateIndex;
						}
					}
					this.PrivateIndex = circuitState.ReserveState();
					Tracer.Assert(0 < this.PrivateIndex);
				} else {
					Tracer.Assert(this.StateIndex == 0);
					this.StateIndex = circuitState.ReserveState();
				}
				Tracer.Assert(0 < this.StateIndex);
			}

			#if DEBUG
				public override string ToString() {
					return string.Format(CultureInfo.InvariantCulture, "Result: jam{0}, bit#{1} of {2}",
						this.Jam.AbsolutePoint.ToString(), this.BitNumber, this.CircuitMap.Path((CircuitSymbol)this.Jam.CircuitSymbol)
					);
				}
			#endif
		}

		private class Parameter : StateIndex {

			public Result Result { get; private set; }

			public Parameter(Result result, CircuitMap circuitMap, Jam jam, int bitNumber) : base(circuitMap, jam, bitNumber) {
				Tracer.Assert(jam.Pin.PinType == PinType.Input);
				this.Result = result;
			}

			#if DEBUG
				public override string ToString() {
					return string.Format(CultureInfo.InvariantCulture, "Parameter: jam{0}, bit#{1} of {2}",
						this.Jam.AbsolutePoint.ToString(), this.BitNumber, this.CircuitMap.Path((CircuitSymbol)this.Jam.CircuitSymbol)
					);
				}
			#endif
		}

		private class ResultComparer : IComparer<Result> {
			public static readonly IComparer<Result> BitOrderComparer = new ResultComparer();

			public int Compare(Result x, Result y) {
				Tracer.Assert(x.CircuitMap == y.CircuitMap && x.Jam == y.Jam);
				return x.BitNumber - y.BitNumber;
			}
		}

		private class ParameterComparer : IComparer<Parameter> {
			public static readonly ParameterComparer BitOrderComparer = new ParameterComparer();

			public int Compare(Parameter x, Parameter y) {
				return x.BitNumber - y.BitNumber;
			}
		}

		private struct JamBit {
			public CircuitMap Map { get; }
			public Jam Jam { get; }
			public int Bit { get; }

			public JamBit(CircuitMap circuitMap, Jam jam, int bit) {
				this.Map = circuitMap;
				this.Jam = jam;
				this.Bit = bit;
			}

			public bool Equals(JamBit other) => this.Map == other.Map && this.Jam == other.Jam && this.Bit == other.Bit;

			public override bool Equals(object obj) => obj is JamBit other && this.Equals(other);

			public override int GetHashCode() => this.Map.GetHashCode() ^ this.Jam.GetHashCode() ^ this.Bit;
		}
	}
}
