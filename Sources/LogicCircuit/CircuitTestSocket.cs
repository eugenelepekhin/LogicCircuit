using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace LogicCircuit {
	public class CircuitTestSocket {
		private TableChank chank;
		private TableChank[] chankList;
		public IEnumerable<InputPinSocket> Inputs { get { return this.chank.Inputs; } }
		public IEnumerable<OutputPinSocket> Outputs { get { return this.chank.Outputs; } }

		public CircuitTestSocket(LogicalCircuit circuit, bool multithreaded) {
			Tracer.Assert(CircuitTestSocket.IsTestable(circuit));
			this.chank = new TableChank(circuit);
			if(multithreaded && 1 < Environment.ProcessorCount && 15 < this.chank.InputBitCount) {
				this.chankList = new TableChank[Environment.ProcessorCount];
				BigInteger count = this.chank.Count / this.chankList.Length;
				for(int i = 0; i < this.chankList.Length; i++) {
					if(i == 0) {
						this.chankList[i] = this.chank;
					} else {
						this.chankList[i] = new TableChank(circuit);
					}
					this.chankList[i].Count = count;
					this.chankList[i].Start = count * i;
				}
			}
		}

		public CircuitTestSocket(LogicalCircuit circuit) : this(circuit, true) {
		}

		public static bool IsTestable(LogicalCircuit circuit) {
			IEnumerable<Pin> pins = circuit.CircuitProject.PinSet.SelectByCircuit(circuit);
			return pins.Any(p => p.PinType == PinType.Input) && pins.Any(p => p.PinType == PinType.Output);
		}

		public bool Evaluate() {
			return this.chank.Evaluate();
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public IList<TruthState> BuildTruthTable(Action<double> reportProgress, Func<bool> keepGoing, Predicate<TruthState> include, int maxCount, out bool truncated) {
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			if(this.chankList == null) {
				this.chank.BuildTruthTable(reportProgress, keepGoing, include, maxCount);
				truncated = this.chank.Trancated;
				if(this.chank.Oscillation) {
					return null;
				}
				watch.Stop();
				Tracer.FullInfo("CircuitTestSocket.BuildTruthTable", "Single threaded time: {0}", watch.Elapsed);
				return this.chank.Results;
			}
			double[] progress = new double[this.chankList.Length];
			Parallel.For(0, this.chankList.Length, i =>
				this.chankList[i].BuildTruthTable(
					d => {
						progress[i] = d;
						reportProgress(progress.Sum() / progress.Length);
					},
					() => keepGoing() && !this.chankList.Take(i).Any(c => c.Trancated) && !this.chankList.Any(c => c.Oscillation),
					include,
					maxCount
				)
			);
			truncated = this.chankList.Any(c => c.Trancated);
			if(this.chankList.Any(c => c.Oscillation)) {
				return null;
			}
			List<TruthState> list = new List<TruthState>();
			foreach(TableChank table in this.chankList) {
				if(table.Results != null) {
					list.AddRange(table.Results);
				}
				if(maxCount <= list.Count) {
					break;
				}
			}
			if(maxCount < list.Count) {
				list.RemoveRange(maxCount, list.Count - maxCount);
			}
			watch.Stop();
			Tracer.FullInfo("CircuitTestSocket.BuildTruthTable", "Multi threaded time: {0}", watch.Elapsed);
			return list;
		}

		private class TableChank {
			private readonly LogicalCircuit LogicalCircuit;
			private readonly CircuitState CircuitState;
			public readonly List<InputPinSocket> Inputs = new List<InputPinSocket>();
			public readonly List<OutputPinSocket> Outputs = new List<OutputPinSocket>();
			public readonly int InputBitCount;
			public List<TruthState> Results { get; private set; }
			public BigInteger Start = 0;
			public BigInteger Count;
			public bool Oscillation { get; private set; }
			public bool Trancated { get; private set; }
			
			public TableChank(LogicalCircuit logicalCircuit) {
				this.LogicalCircuit = TableChank.Copy(logicalCircuit);
				this.Plug();

				// Create map and state
				CircuitMap circuitMap = new CircuitMap(this.LogicalCircuit);
				this.CircuitState = circuitMap.Apply(CircuitRunner.HistorySize);

				this.Inputs.ForEach(s => s.Function = circuitMap.FunctionConstant(s.Symbol));
				this.Outputs.ForEach(s => s.Function = circuitMap.FunctionProbe(s.Symbol));

				this.Inputs.Where(s => s.Function == null).ToList().ForEach(s => this.Inputs.Remove(s));
				this.Outputs.Where(s => s.Function == null).ToList().ForEach(s => this.Outputs.Remove(s));
			
				Tracer.Assert(this.Inputs.All(s => s.Function != null) && this.Outputs.All(s => s.Function != null));

				this.InputBitCount = this.Inputs.Sum(p => p.Pin.BitWidth);
				this.Count = BigInteger.One << this.InputBitCount;

				circuitMap.TurnOn();
			}

			public bool Evaluate() {
				return this.CircuitState.Evaluate(true);
			}

			public void BuildTruthTable(Action<double> reportProgress, Func<bool> keepGoing, Predicate<TruthState> include, int maxCount) {
				this.LogicalCircuit.CircuitProject.InOmitTransaction(() => this.Build(reportProgress, keepGoing, include, maxCount));
			}

			private void Build(Action<double> reportProgress, Func<bool> keepGoing, Predicate<TruthState> include, int maxCount) {
				this.Results = new List<TruthState>();
				this.Oscillation = false;
				this.Trancated = false;
				int inputCount = this.Inputs.Count;
				int outputCount = this.Outputs.Count;
				if(0 < inputCount && 0 < outputCount) {
					BigInteger end = this.Start + this.Count;
					BigInteger onePercent = this.Count / 100;
					BigInteger count = 0;
					double progress = 0;

					TruthState state = new TruthState(inputCount, outputCount);
					for(BigInteger value = this.Start; value < end; value++) {
						if(maxCount <= this.Results.Count || !keepGoing()) {
							this.Trancated = true;
							break;
						}
						int bit = 0;
						for(int i = this.Inputs.Count - 1; 0 <= i; i--) {
							InputPinSocket pin = this.Inputs[i];
							int v = (int)((value >> bit) & int.MaxValue) & (pin.Pin.BitWidth < 32 ? (1 << pin.Pin.BitWidth) - 1 : ~0);
							pin.Function.Value = v;
							Tracer.Assert(pin.Function.Value == v, "Value get truncated");
							bit += pin.Pin.BitWidth;
						}
						if(!this.CircuitState.Evaluate(true)) {
							this.Oscillation = true;
							break;
						}
						for(int i = 0; i < inputCount; i++) {
							state.Input[i] = this.Inputs[i].Function.Value;
						}
						for(int i = 0; i < outputCount; i++) {
							state.Result[i] = this.Outputs[i].Function.Pack();
						}
						if(!state.Unpack(this.Outputs.Select(o => o.Function.ParameterCount).ToArray()) || include == null || include(state)) {
							this.Results.Add(state);
							state = new TruthState(inputCount, outputCount);
						}
						if(reportProgress != null) {
							count++;
							if(onePercent < count) {
								count = 0;
								if(onePercent == BigInteger.Zero) {
									Tracer.Assert(0 < this.Count && this.Count < 100);
									reportProgress((double)((value + 1 - this.Start) * 100) / (double)this.Count);
								} else {
									reportProgress(Math.Min(++progress, 100));
								}
							}
						}
					}
				}
			}

			private static LogicalCircuit Copy(LogicalCircuit circuit) {
				LogicalCircuit other = null;
				CircuitProject copy = new CircuitProject();
				copy.InTransaction(() => {
					copy.ProjectSet.Copy(circuit.CircuitProject.ProjectSet.Project);
					other = copy.LogicalCircuitSet.Copy(circuit, true);
				});
				foreach(CircuitSymbol symbol in other.CircuitSymbols()) {
					symbol.GuaranteeGlyph();
				}
				return other;
			}

			private void Plug() {
				List<Pin> pins = this.LogicalCircuit.CircuitProject.PinSet.SelectByCircuit(this.LogicalCircuit).ToList();
				pins.Sort(PinComparer.Comparer);
				this.LogicalCircuit.CircuitProject.InTransaction(() => {
					foreach(Pin pin in pins) {
						if(pin.PinType == PinType.Input) {
							this.Inputs.Add(new InputPinSocket(pin));
						} else if(pin.PinType == PinType.Output) {
							this.Outputs.Add(new OutputPinSocket(pin));
						} else {
							Tracer.Fail();
						}
					}
				});
			}
		}
	}

	public class InputPinSocket {
		public Pin Pin { get; private set; }
		public Constant Value { get; private set; }
		public CircuitSymbol Symbol { get; private set; }
		public FunctionConstant Function { get; set; }

		public InputPinSocket(Pin pin) {
			Tracer.Assert(pin.PinType == PinType.Input);
			CircuitProject project = pin.CircuitProject;
			this.Pin = pin;
			this.Value = project.ConstantSet.Create(pin.BitWidth, 0, PinSide.Right);
			CircuitSymbol pinSymbol = project.CircuitSymbolSet.SelectByCircuit(pin).FirstOrDefault();
			Tracer.Assert(pinSymbol != null);
			this.Symbol = project.CircuitSymbolSet.Create(this.Value, pin.LogicalCircuit, pinSymbol.X, pinSymbol.Y);
			this.Symbol.Rotation = pinSymbol.Rotation;
			pinSymbol.X = pinSymbol.Y = int.MinValue;
		}
	}

	public class OutputPinSocket {
		public Pin Pin { get; private set; }
		public CircuitProbe Value { get; private set; }
		public CircuitSymbol Symbol { get; private set; }
		public FunctionProbe Function { get; set; }

		public OutputPinSocket(Pin pin) {
			Tracer.Assert(pin.PinType == PinType.Output);
			CircuitProject project = pin.CircuitProject;
			this.Pin = pin;
			this.Value = project.CircuitProbeSet.Create(null);
			CircuitSymbol pinSymbol = project.CircuitSymbolSet.SelectByCircuit(pin).FirstOrDefault();
			Tracer.Assert(pinSymbol != null);
			this.Symbol = project.CircuitSymbolSet.Create(this.Value, pin.LogicalCircuit, pinSymbol.X, pinSymbol.Y);
			this.Symbol.Rotation = pinSymbol.Rotation;
			pinSymbol.X = pinSymbol.Y = int.MinValue;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct TruthState {
		private readonly int[] input;
		private readonly long[] result;
		private int[] output;
		private int[] bitWidth;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] Input { get { return this.input; } }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public long[] Result { get { return this.result; } }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] Output { get { return this.output; } }

		public TruthState(int inputCount, int outputCount) {
			this.input = new int[inputCount];
			this.result = new long[outputCount];
			this.output = null;
			this.bitWidth = null;
		}

		public bool Unpack(int[] bitWidthList) {
			Tracer.Assert(bitWidthList.Length == this.result.Length);
			this.bitWidth = bitWidthList;
			int[] res = new int[this.result.Length];
			for(int i = 0; i < res.Length; i++) {
				int unpacked;
				if(!FunctionProbe.ToInt(this.result[i], this.bitWidth[i], out unpacked)) {
					return false;
				}
				res[i] = unpacked;
			}
			this.output = res;
			return true;
		}

		public string this[int index] {
			get {
				if(this.output != null) {
					return this.output[index].ToString("X", CultureInfo.InvariantCulture);
				}
				int unpacked;
				if(FunctionProbe.ToInt(this.result[index], bitWidth[index], out unpacked)) {
					return unpacked.ToString("X", CultureInfo.InvariantCulture);
				}
				long res = this.result[index];
				return CircuitFunction.ToText(Enumerable.Range(0, this.bitWidth[index]).Select(i => (State)((res >> i * 2) & 0x3)), false);
			}
		}
	}
}
