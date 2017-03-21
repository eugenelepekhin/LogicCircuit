using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	public class CircuitTester {
		private readonly CircuitTestSocket socket;
		private readonly string logicalCircuitName;
		private readonly WeakReference<Editor> originalEditor;
		private readonly int originalVersion;

		internal CircuitTester(Editor editor, LogicalCircuit circuit) {
			Tracer.Assert(editor != null);
			Tracer.Assert(circuit != null);
			Tracer.Assert(circuit.CircuitProject == editor.CircuitProject);
			Tracer.Assert(CircuitTestSocket.IsTestable(circuit));

			this.originalEditor = new WeakReference<Editor>(editor);
			this.originalVersion = editor.CircuitProject.Version;
			this.logicalCircuitName = circuit.Name;

			this.socket = new CircuitTestSocket(circuit, false);
		}

		public void SetInput(string inputName, int value) {
			this.ValidateEditor();
			if(string.IsNullOrEmpty(inputName)) {
				throw new ArgumentNullException(nameof(inputName));
			}
			InputPinSocket pin = this.socket.Inputs.FirstOrDefault(i => i.Pin.Name == inputName);
			if(pin == null) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Input pin {0} not found on Logical Circuit {1}", inputName, this.logicalCircuitName)
				);
			}
			pin.Function.Value = value;
			if(pin.Function.Value != value) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Value {0} get truncated by pin {1}. Make sure value can fit to {2} bit(s) of the pin.", value, inputName, pin.Pin.BitWidth)
				);
			}
		}

		public long GetStateOutput(string outputName) {
			this.ValidateEditor();
			if(string.IsNullOrEmpty(outputName)) {
				throw new ArgumentNullException(nameof(outputName));
			}
			OutputPinSocket pin = this.socket.Outputs.First(o => o.Pin.Name == outputName);
			if(pin == null) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Output pin {0} not found on Logical Circuit {1}", outputName, this.logicalCircuitName)
				);
			}
			return pin.Function.Pack();
		}

		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetStateOutput")]
		public int GetOutput(string outputName) {
			this.ValidateEditor();
			if(string.IsNullOrEmpty(outputName)) {
				throw new ArgumentNullException(nameof(outputName));
			}
			OutputPinSocket pin = this.socket.Outputs.FirstOrDefault(o => o.Pin.Name == outputName);
			if(pin == null) {
				throw new CircuitException(Cause.UserError,
					string.Format(CultureInfo.InvariantCulture, "Output pin {0} not found on Logical Circuit {1}", outputName, this.logicalCircuitName)
				);
			}
			int value;
			if(FunctionProbe.ToInt(pin.Function.Pack(), pin.Pin.BitWidth, out value)) {
				return value;
			}
			throw new CircuitException(Cause.UserError,
				string.Format(CultureInfo.InvariantCulture,
					"Output value cannot be represented by number because it contains bit(s) in high impedance state: {0}. Use GetStateOutput instead.",
					pin.Function.ToText()
				)
			);
		}

		public bool Evaluate() {
			this.ValidateEditor();
			return this.socket.Evaluate();
		}

		private void ValidateEditor() {
			Editor editor;
			if(!this.originalEditor.TryGetTarget(out editor) || editor != App.Editor || editor.CircuitProject.Version != this.originalVersion) {
				throw new InvalidOperationException("Circuit project was changed since this circuit tester was created.");
			}
		}
	}
}
