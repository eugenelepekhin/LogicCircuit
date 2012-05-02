using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class CircuitTestSocket {
		public static bool IsTestable(LogicalCircuit circuit) {
			IEnumerable<Pin> pins = circuit.CircuitProject.PinSet.SelectByCircuit(circuit);
			return pins.Any(p => p.PinType == PinType.Input) && pins.Any(p => p.PinType == PinType.Output);
		}

		public LogicalCircuit LogicalCircuit { get; private set; }

		public CircuitTestSocket(LogicalCircuit circuit) {
			Tracer.Assert(CircuitTestSocket.IsTestable(circuit));
			this.LogicalCircuit = circuit;
		}
	}
}
