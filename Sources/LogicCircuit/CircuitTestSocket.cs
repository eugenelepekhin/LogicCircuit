using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	public class CircuitTestSocket {
		public LogicalCircuit LogicalCircuit { get; private set; }
		private readonly List<InputPinSocket> inputs = new List<InputPinSocket>();
		private readonly List<OutputPinSocket> outputs = new List<OutputPinSocket>();
		private CircuitMap CircuitMap { get; set; }
		private CircuitState CircuitState { get; set; }

		public IEnumerable<InputPinSocket> Inputs { get { return this.inputs; } }
		public IEnumerable<OutputPinSocket> Outputs { get { return this.outputs; } }

		public CircuitTestSocket(LogicalCircuit circuit) {
			Tracer.Assert(CircuitTestSocket.IsTestable(circuit));
			this.LogicalCircuit = CircuitTestSocket.Copy(circuit);
			this.Plug();

			// Create map and state
			this.CircuitMap = new CircuitMap(this.LogicalCircuit);
			this.CircuitState = this.CircuitMap.Apply(CircuitRunner.HistorySize);

			this.inputs.ForEach(s => s.Function = (FunctionConstant)this.CircuitMap.Input(s.Symbol));
			this.outputs.ForEach(s => s.Function = this.CircuitMap.FunctionProbe(s.Symbol));

			Tracer.Assert(this.inputs.All(s => s.Function != null) && this.outputs.All(s => s.Function != null));

			this.CircuitMap.TurnOn();
		}

		public static bool IsTestable(LogicalCircuit circuit) {
			IEnumerable<Pin> pins = circuit.CircuitProject.PinSet.SelectByCircuit(circuit);
			return pins.Any(p => p.PinType == PinType.Input) && pins.Any(p => p.PinType == PinType.Output);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
		public IList<TruthState> BuildTruthTable(Predicate<TruthState> include, int maxCount, out bool truncated) {
			truncated = false;
			int inputCount = this.inputs.Count;
			int outputCount = this.outputs.Count;
			List<TruthState> result = new List<TruthState>();
			for(;;) {
				if(maxCount <= result.Count) {
					truncated = true;
					return result;
				}
				if(!this.CircuitState.Evaluate(true)) {
					return null;
				}
				TruthState state = new TruthState(inputCount, outputCount);
				for(int i = 0; i < inputCount; i++) {
					state.Input[i] = this.inputs[i].Function.Value;
				}
				for(int i = 0; i < outputCount; i++) {
					state.Output[i] = this.outputs[i].Function.ToInt32();
				}
				if(include == null || include(state)) {
					result.Add(state);
				}
				for(int i = inputCount - 1; 0 <= i; i--) {
					this.inputs[i].Function.Value++;
					if(this.inputs[i].Function.Value != 0) {
						break;
					} else if(i == 0) {
						return result;
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
			return other;
		}

		private void Plug() {
			List<Pin> pins = this.LogicalCircuit.CircuitProject.PinSet.SelectByCircuit(this.LogicalCircuit).ToList();
			pins.Sort(PinComparer.Comparer);
			this.LogicalCircuit.CircuitProject.InTransaction(() => {
				foreach(Pin pin in pins) {
					if(pin.PinType == PinType.Input) {
						this.inputs.Add(new InputPinSocket(pin));
					} else if(pin.PinType == PinType.Output) {
						this.outputs.Add(new OutputPinSocket(pin));
					} else {
						Tracer.Fail();
					}
				}
			});
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
			this.Value = project.ConstantSet.Create(pin.BitWidth, 0);
			CircuitSymbol pinSymbol = project.CircuitSymbolSet.SelectByCircuit(pin).FirstOrDefault();
			Tracer.Assert(pinSymbol != null);
			this.Symbol = project.CircuitSymbolSet.Create(this.Value, pin.LogicalCircuit, pinSymbol.X, pinSymbol.Y);
			this.Symbol.Rotation = pinSymbol.Rotation;
			pinSymbol.X = pinSymbol.Y = int.MinValue;
		}
	}

	public class OutputPinSocket {
		public Pin Pin { get; private set; }
		public Gate Value { get; private set; }
		public CircuitSymbol Symbol { get; private set; }
		public FunctionProbe Function { get; set; }

		public OutputPinSocket(Pin pin) {
			Tracer.Assert(pin.PinType == PinType.Output);
			CircuitProject project = pin.CircuitProject;
			this.Pin = pin;
			this.Value = project.GateSet.Gate(GateType.Probe, 1, false);
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
		private readonly int[] output;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] Input { get { return this.input; } }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] Output { get { return this.output; } }

		public TruthState(int inputCount, int outputCount) {
			this.input = new int[inputCount];
			this.output = new int[outputCount];
		}
	}
}
