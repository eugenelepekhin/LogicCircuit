// Ignore Spelling: Hdl

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	partial class N2THdl {
		public string Name { get; }

		[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
		private static string PinsText(IEnumerable<HdlSymbol> pins) {
			StringBuilder text = new StringBuilder();
			if(pins.Any()) {
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
			}
			return text.ToString();
		}

		public N2THdl(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) : base(inputPins, outputPins, parts) {
			this.Name = name;
		}
	}
}
