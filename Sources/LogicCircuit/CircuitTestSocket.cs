using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class CircuitTestSocket : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public LogicalCircuit LogicalCircuit { get; private set; }
		private readonly List<InputPinSocket> inputs = new List<InputPinSocket>();
		private readonly List<OutputPinSocket> outputs = new List<OutputPinSocket>();
		private CircuitMap CircuitMap { get; set; }
		private CircuitState CircuitState { get; set; }

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

		public void BuildTruthTable() {
			int inputCount = this.inputs.Count;
			int outputCount = this.outputs.Count;

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

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
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

	public struct TruthState {
		public readonly int[] Input;
		public readonly int[] Output;

		public TruthState(int inputCount, int outputCount) {
			this.Input = new int[inputCount];
			this.Output = new int[outputCount];
		}
	}
}
