using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LogicCircuit {
	public class CircuitTester {
		public Editor Editor { get; private set; }

		private CircuitTestSocket socket;

		internal CircuitTester(Editor editor) {
			Tracer.Assert(editor != null);
			this.Editor = editor;
		}

		public LogicalCircuit LogicalCircuit(string name) {
			return this.Editor.CircuitProject.LogicalCircuitSet.FindByName(name);
		}

		public void TurnOn(LogicalCircuit circuit) {
			Tracer.Assert(circuit.CircuitProject == this.Editor.CircuitProject);
			this.TurnOff();
			if(this.Editor.Project.LogicalCircuit != circuit) {
				this.Editor.OpenLogicalCircuit(circuit);
			}
			this.socket = new CircuitTestSocket(circuit, false);
			bool started = this.socket.Inputs.First().Pin.CircuitProject.StartTransaction();
			Tracer.Assert(started);
		}

		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TurnOff")]
		public void TurnOff() {
			if(this.socket != null) {
				this.socket = null;
			}
		}

		public void SetInput(string inputName, int value) {
			InputPinSocket pin = this.socket.Inputs.First(i => i.Pin.Name == inputName);
			Tracer.Assert(pin != null);
			pin.Function.Value = value;
			Tracer.Assert(pin.Function.Value == value, "Value get truncated");
		}

		public long GetStateOutput(string outputName) {
			OutputPinSocket pin = this.socket.Outputs.First(o => o.Pin.Name == outputName);
			return pin.Function.Pack();
		}

		public int GetOutput(string outputName) {
			OutputPinSocket pin = this.socket.Outputs.First(o => o.Pin.Name == outputName);
			int value;
			if(FunctionProbe.ToInt(pin.Function.Pack(), pin.Pin.BitWidth, out value)) {
				return value;
			}
			return 0;
		}

		public bool Evaluate() {
			return this.socket.Evaluate();
		}
	}
}
