using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public interface IFunctionVisual {
		void TurnOn();
		void TurnOff();
		void Redraw();
		bool Invalid { get; set; }
	}
}
