﻿// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	internal class N2THdl : HdlTransformation {
		private readonly Func<string, string> fixName;
		public N2THdl(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts, Func<string, string> fixName) : base(name, inputPins, outputPins, parts) {
			this.fixName = fixName;
		}

		private string PinsText(IEnumerable<HdlSymbol> pins) {
			StringBuilder text = new StringBuilder();
			bool comma = false;
			foreach(HdlSymbol symbol in pins) {
				if(comma) {
					text.Append(", ");
				} else {
					comma = true;
				}
				Pin pin = (Pin)symbol.CircuitSymbol.Circuit;
				text.Append(this.fixName(pin.Name));
				if(1 < pin.BitWidth) {
					text.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", pin.BitWidth);
				}
			}
			return text.ToString();
		}

		private static string RangeText(HdlConnection.BitRange range) => string.Format(CultureInfo.InvariantCulture, (range.First == range.Last) ? "[{0}]" : "[{0}..{1}]", range.First, range.Last);

		private static string SymbolJamName(HdlSymbol symbol, HdlConnection connection) {
			Debug.Assert(symbol == connection.OutHdlSymbol || symbol == connection.InHdlSymbol);
			Debug.Assert(symbol.CircuitSymbol.Circuit is not Pin);
			Debug.Assert(connection.OutHdlSymbol.CircuitSymbol.Circuit is not Constant);
			Jam jam = connection.SymbolJam(symbol);
			string name = symbol.HdlExport.HdlName(jam);
			if(connection.IsBitRange(symbol)) {
				name += N2THdl.RangeText((jam == connection.OutJam) ? connection.OutBits : connection.InBits);
			}
			return name;
		}

		private string PinName(HdlSymbol symbol, HdlConnection connection) {
			Debug.Assert(symbol == connection.OutHdlSymbol || symbol == connection.InHdlSymbol);
			Debug.Assert(symbol.CircuitSymbol.Circuit is not Constant);
			HdlSymbol otherSymbol;
			Jam otherJam;
			HdlConnection.BitRange otherBits;
			if(symbol == connection.OutHdlSymbol) {
				otherSymbol = connection.InHdlSymbol;
				otherJam = connection.InJam;
				otherBits = connection.InBits;
			} else {
				otherSymbol = connection.OutHdlSymbol;
				otherJam = connection.OutJam;
				otherBits = connection.OutBits;
			}
			if(otherJam.CircuitSymbol.Circuit is Pin pin) {
				string name = this.fixName(pin.Name);
				if(connection.IsBitRange(otherSymbol)) {
					name += N2THdl.RangeText(otherBits);
				}
				return name;
			}
			GridPoint point = connection.OutJam.AbsolutePoint;
			string pinName = string.Format(CultureInfo.InvariantCulture, "Pin{0}x{1}", point.X, point.Y);
			if(connection.IsBitRange(connection.OutHdlSymbol)) {
				HdlConnection.BitRange outBits = connection.OutBits;
				if(outBits.First == outBits.Last) {
					pinName += string.Format(CultureInfo.InvariantCulture, "b{0}", outBits.First);
				} else {
					pinName += string.Format(CultureInfo.InvariantCulture, "s{0}e{1}", outBits.First, outBits.Last);
				}
			}
			if(0 < connection.OutHdlSymbol.Subindex) {
				pinName += string.Format(CultureInfo.InvariantCulture, "v{0}", connection.OutHdlSymbol.Subindex);
			}
			return pinName;
		}

		public override string TransformText() {
			this.WriteLine("CHIP {0} {{", this.Name);
			if(this.HasInputPins) {
				this.WriteLine("IN {0};", this.PinsText(this.InputPins));
			}
			if(this.HasOutputPins) {
				this.WriteLine("OUT {0};", this.PinsText(this.OutputPins));
			}
			this.WriteLine("PARTS:");
			foreach(HdlSymbol symbol in this.Parts) {
				bool comma = false;
				if(this.CommentPoints && (!symbol.AutoGenerated || symbol.Subindex == 1)) {
					this.WriteLine("\t// {0}", symbol.Comment);
				}
				this.Write("\t{0}(", symbol.HdlExport.HdlName(symbol));
				foreach(HdlConnection connection in symbol.HdlConnections().Where(c => c.GenerateOutput(symbol))) {
					if(comma) {
						this.Write(", ");
					}
					comma = true;
					if(connection.OutHdlSymbol.CircuitSymbol.Circuit is Constant constant) {
						int value = connection.OutBits.Extract(constant.ConstantValue);
						int width = connection.InBits.BitWidth;
						Debug.Assert(connection.OutBits.BitWidth == width);
						for(int i = 0; i < width; i++) {
							if(0 < i) {
								this.Write(", ");
							}
							this.Write(symbol.HdlExport.HdlName(connection.InJam));
							if(1 < connection.InJam.Pin.BitWidth) {
								this.Write("[{0}]", i + connection.InBits.First);
							}
							this.Write("={0}", ((value >> i) & 1) != 0 ? "true" : "false");
						}
					} else {
						this.Write("{0}={1}", N2THdl.SymbolJamName(symbol, connection), this.PinName(symbol, connection));
					}
				}
				this.WriteLine(");");
			}
			this.WriteLine("}");
            return this.GenerationEnvironment.ToString();
		}
	}
}
