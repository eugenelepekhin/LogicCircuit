// Ignore Spelling: Hdl

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LogicCircuit {
	partial class N2THdl {
		public N2THdl(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) : base(name, inputPins, outputPins, parts) {
		}

		private static string PinsText(IEnumerable<HdlSymbol> pins) {
			StringBuilder text = new StringBuilder();
			bool comma = false;
			foreach(HdlSymbol symbol in pins) {
				if(comma) {
					text.Append(", ");
				} else {
					comma = true;
				}
				Pin pin = (Pin)symbol.CircuitSymbol.Circuit;
				text.Append(pin.Name);
				if(1 < pin.BitWidth) {
					text.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", pin.BitWidth);
				}
			}
			return text.ToString();
		}
	}
}
