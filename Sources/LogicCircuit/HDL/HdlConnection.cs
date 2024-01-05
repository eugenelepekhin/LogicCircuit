// Ignore Spelling: Hdl

using System.Diagnostics;
using System.Globalization;

namespace LogicCircuit {
	public class HdlConnection {
		public HdlSymbol HdlSymbol { get; }
		public Connection Connection { get; }

		public Jam SymbolJam => (this.Connection.InJam.CircuitSymbol == this.HdlSymbol.CircuitSymbol) ? this.Connection.InJam : this.Connection.OutJam;
		public Jam OtherJam() => (this.Connection.InJam.CircuitSymbol == this.HdlSymbol.CircuitSymbol) ? this.Connection.OutJam : this.Connection.InJam;
		//public Jam OgherJam(Jam jam) {
		//	if(this.Connection.InJam == jam) {
		//		return this.Connection.OutJam;
		//	}
		//	if(this.Connection.OutJam == jam) {
		//		return this.Connection.InJam;
		//	}
		//	Debug.Fail("The jam does not belong to the connection");
		//	return null;
		//}

		public string JamName {
			get {
				HdlExportType type = this.HdlSymbol.HdlExport.HdlExportType;
				bool isN2t = (type == HdlExportType.N2T) || (type == HdlExportType.N2TFull);
				if(isN2t && this.HdlSymbol.CircuitSymbol.Circuit is Gate) {
					switch(this.SymbolJam.Pin.Name) {
					case "x": return "in";
					case "x1" : return "a";
					case "x2" : return "b";
					case "q" : return "out";
					}
				}
				return this.SymbolJam.Pin.Name;
			}
		}
		public string PinName => (this.OtherJam().CircuitSymbol.Circuit is Pin) ? this.OtherJam().CircuitSymbol.Circuit.Name : this.OutputPinName();

		public HdlConnection(HdlSymbol symbol, Connection connection) {
			this.HdlSymbol = symbol;
			this.Connection = connection;
		}

		public bool ConnectsInputWithOutput() =>
			(this.Connection.InJam.CircuitSymbol.Circuit is Pin) &&
			(this.Connection.OutJam.CircuitSymbol.Circuit is Pin)
		;

		private string OutputPinName() {
			Jam jam = this.SymbolJam.Pin.PinType == PinType.Output ? this.SymbolJam : this.OtherJam();
			GridPoint point = jam.AbsolutePoint;
			return string.Format(CultureInfo.InvariantCulture, "Pin{0}x{1}", point.X, point.Y);
		}

		public string InputName() {
			Debug.Assert(this.ConnectsInputWithOutput());
			Pin pin(Jam jam) => (Pin)jam.CircuitSymbol.Circuit;
			Pin p = pin(this.Connection.InJam);
			if(p.PinType == PinType.Input) {
				return p.Name;
			}
			return pin(this.Connection.OutJam).Name;
		}

		public string OutputName() {
			Debug.Assert(this.ConnectsInputWithOutput());
			Pin pin(Jam jam) => (Pin)jam.CircuitSymbol.Circuit;
			Pin p = pin(this.Connection.OutJam);
			if(p.PinType == PinType.Output) {
				return p.Name;
			}
			return pin(this.Connection.InJam).Name;
		}
	}
}
