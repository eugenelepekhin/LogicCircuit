using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	public interface IFunctionVisual {
		void TurnOn();
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TurnOff")]
		void TurnOff();
		void Redraw();
		bool Invalid { get; set; }
	}
}
