// Ignore Spelling: Verilog Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	internal class VerilogHdl : HdlTransformation {
		public static string Range(BasePin pin) {
			Debug.Assert(0 < pin.BitWidth);
			return (pin.BitWidth == 1) ? "" : string.Format(CultureInfo.InvariantCulture, "[{0}:0]", pin.BitWidth - 1);
		}

		private OneToMany<Jam, HdlConnection> wires = new OneToMany<Jam, HdlConnection>();

		public VerilogHdl(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) : base(name, inputPins, outputPins, parts) {
			OneToMany<Jam, HdlConnection> connections = new OneToMany<Jam, HdlConnection>();
			foreach(HdlConnection connection in this.Parts.SelectMany(p => p.HdlConnections())) {
				connections.Add(connection.OutJam, connection);
			}
			foreach(HdlSymbol symbol in this.Parts) {
				foreach(Jam jam in symbol.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Output)) {
					IList<HdlConnection> list = connections[jam];
					Debug.Assert(0 < list.Count);
					if(1 < list.Count || list[0].OtherSymbol(symbol).CircuitSymbol.Circuit is not Pin) {
						this.wires.Add(jam, list);
					}
				}
			}
		}

		private static string WireName(Jam jam) {
			BasePin pin = jam.Pin;
			GridPoint point = jam.AbsolutePoint;
			return string.Format(CultureInfo.InvariantCulture, "Pin_{0}x{1}", point.X, point.Y);
		}

		private static string DefineWireName(Jam jam) {
			string text = VerilogHdl.WireName(jam);
			BasePin pin = jam.Pin;
			if(1 < pin.BitWidth) {
				text = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", VerilogHdl.Range(pin), text);
			}
			return text;
		}

		private IEnumerable<string> WireDefinitions() => this.wires.Keys.Select(j => VerilogHdl.DefineWireName(j)).Order();

		private static string JamName(Jam jam) {
			if(jam.CircuitSymbol.Circuit is Gate gate) {
				if(jam.Pin.PinType == PinType.Output) {
					return "out";
				}
				switch(gate.GateType) {
				case GateType.Not:
				case GateType.TriState1:
				case GateType.TriState2:
					Debug.Assert(jam.Pin.PinType == PinType.Input);
					if(jam.Pin.PinSide != PinSide.Left) {
						return "control";
					}
					return "in";
				}
				int min = gate.Pins.Min(p => ((DevicePin)p).Order);
				DevicePin pin = (DevicePin)jam.Pin;
				return string.Format(CultureInfo.InvariantCulture, "i{0}", pin.Order - min + 1);
			}
			return jam.Pin.Name.Trim();
		}

		private static string PartName(HdlSymbol part) {
			if(part.CircuitSymbol.Circuit is Gate gate) {
				switch(gate.GateType) {
				case GateType.Clock:return "Clock";
				case GateType.Not:	return "not";
				case GateType.Or:	return gate.InvertedOutput ? "nor" : "or";
				case GateType.And:	return gate.InvertedOutput ? "nand" : "and";
				case GateType.Xor:	return gate.InvertedOutput ? "xnor" : "xor";
				case GateType.Led:	return "LED";
				case GateType.Probe:return "Probe";
				case GateType.TriState1:
				case GateType.TriState2: return "bufif1";
				}
			}
			return part.Name;
		}

		private string WireName(HdlSymbol symbol, Jam jam) {
			string range(HdlConnection.BitRange bitRange) {
				if(bitRange.First != bitRange.Last) {
					return string.Format(CultureInfo.InvariantCulture, "[{0}:{1}]", bitRange.Last, bitRange.First);
				}
				return string.Format(CultureInfo.InvariantCulture, "[{0}]", bitRange.First);
			}
			string bitRange(HdlSymbol symbol, HdlConnection connection) {
				if(connection.OutHdlSymbol == symbol) {
					if(connection.IsBitRange(symbol)) {
						return range(connection.OutBits);
					}
				} else {
					if(connection.IsBitRange(connection.InHdlSymbol)) {
						return range(connection.InBits);
					}
				}
				return string.Empty;
			}
			Debug.Assert(jam.CircuitSymbol == symbol.CircuitSymbol);
			if(this.wires.ContainsKey(jam)) {
				Debug.Assert(jam.Pin.PinType == PinType.Output);
				return VerilogHdl.WireName(jam);
			}
			foreach(HdlConnection connection in symbol.HdlConnections()) {
				if(connection.OutJam == jam) {
					Debug.Assert(connection.InHdlSymbol.CircuitSymbol.Circuit is Pin);
					return connection.InHdlSymbol.CircuitSymbol.Circuit.Name + bitRange(connection.InHdlSymbol, connection);
				}
				if(connection.InJam == jam) {
					if(this.wires.ContainsKey(connection.OutJam)) {
						return VerilogHdl.WireName(connection.OutJam) + bitRange(connection.InHdlSymbol, connection);
					}
					Circuit circuit = connection.OutHdlSymbol.CircuitSymbol.Circuit;
					if(circuit is Constant constant) {
						int value = constant.ConstantValue;
						if(connection.IsBitRange(connection.InHdlSymbol)) {
							value = connection.InBits.Extract(value);
						}
						return value.ToString(CultureInfo.InvariantCulture);
					}
					Debug.Assert(circuit is Pin);
					return circuit.Name + bitRange(connection.OutHdlSymbol, connection);
				}
			}
			throw new InvalidOperationException();
		}

		public override string TransformText() {
			this.WriteLine("module {0}(", this.Name);

			string comma = "";
			foreach(HdlSymbol symbol in this.InputPins.Concat(this.OutputPins)) {
				Pin pin = (Pin)symbol.CircuitSymbol.Circuit;
				this.Write("{0}\t{1}", comma, pin.PinType == PinType.Input ? "input" : "output");
				if(1 < pin.BitWidth) {
					this.Write(VerilogHdl.Range(pin));
				}
				this.Write("\t{0}", pin.Name);
				comma = ",\n";
			}
			this.WriteLine();
			this.WriteLine(");");

			foreach(string wireName in this.WireDefinitions()) {
				this.WriteLine("\twire {0};", wireName);
			}
			this.WriteLine();

			foreach(HdlSymbol part in this.Parts) {
				this.WriteLine("\t{0} {0}_{1}x{2}(", VerilogHdl.PartName(part), part.CircuitSymbol.X, part.CircuitSymbol.Y);
				if(part.CircuitSymbol.Circuit is Gate gate && GateType.Not <= gate.GateType && gate.GateType <= GateType.Xor) {
					List<Jam> inputs = part.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Input).ToList();
					Jam output = part.CircuitSymbol.Jams().First(j => j.Pin.PinType == PinType.Output);
					this.Write("\t\t{0}", this.WireName(part, output));
					foreach(Jam input in inputs) {
						this.Write(",\n\t\t{0}", this.WireName(part, input));
					}
				} else {
					comma = "";
					foreach(Jam jam in part.CircuitSymbol.Jams()) {
						this.Write("{0}\t\t.{1}({2})", comma, VerilogHdl.JamName(jam), this.WireName(part, jam));
						comma = ",\n";
					}
				}
				this.WriteLine();
				this.WriteLine("\t);");
			}
			this.WriteLine();

			this.WriteLine("endmodule // {0}", this.Name);

			return this.GenerationEnvironment.ToString();
		}
	}
}
