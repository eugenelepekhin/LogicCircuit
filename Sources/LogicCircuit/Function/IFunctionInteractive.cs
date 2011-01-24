using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public interface IFunctionInteractive {
		void OnSymbolPress();
		void OnSymbolRelease();
		void OnSymbolDoubleClick();
	}
}
