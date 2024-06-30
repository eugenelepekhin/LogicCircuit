// Ignore Spelling: Verilog Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	internal class VerilogHdl : HdlTransformation {
		private readonly Dictionary<Jam, Jam> wires = new();
		private readonly OneToMany<Jam, HdlConnection> connections = new OneToMany<Jam, HdlConnection>(true);
		private readonly OneToMany<Jam, HdlConnection> assignments = new OneToMany<Jam, HdlConnection>(true);

		public VerilogHdl(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) : base(name, inputPins, outputPins, parts) {
		}

		private void Prepare() {
			foreach(HdlSymbol input in this.InputPins) {
				Jam jam = input.CircuitSymbol.Jams().First();
				this.wires.Add(jam, jam);
				foreach(HdlConnection connection in input.HdlConnections().Where(c => c.OtherJam(jam).CircuitSymbol.Circuit is Pin)) {
					this.assignments.Add(jam, connection);
				}
			}
			foreach(HdlSymbol output in this.OutputPins) {
				Jam jam = output.CircuitSymbol.Jams().First();
				this.wires.Add(jam, jam);
			}
			foreach(HdlConnection connection in this.Parts.SelectMany(p => p.HdlConnections())) {
				this.connections.Add(connection.OutJam, connection);
				this.connections.Add(connection.InJam, connection);
			}
			foreach(KeyValuePair<Jam, ICollection<HdlConnection>> pair in this.connections) {
				Jam jam = pair.Key;
				ICollection<HdlConnection> connections = pair.Value;
				Debug.Assert(0 < connections.Count);
				if(jam.Pin.PinType == PinType.Output && jam.CircuitSymbol.Circuit is not Constant) {
					// Find first Pin of the same bit width as the jam, but on the other side of connection. This pin will serve as wire. With exception if it's the only pin is connected to the jam.
					Jam? other = connections.Select(c => c.OtherJam(jam)).FirstOrDefault(j => j.CircuitSymbol.Circuit is Pin pin && (pin.BitWidth == jam.Pin.BitWidth || 1 == connections.Count));
					if(other != null) {
						this.wires.Add(jam, other);
					} else {
						this.wires.Add(jam, jam);
					}
					foreach(HdlConnection connection in connections) {
						Jam j = connection.OtherJam(jam);
						if(j != other && j.CircuitSymbol.Circuit is Pin) {
							assignments.Add(jam, connection);
						}
					}
				}
			}
		}

		public static string Range(BasePin pin) {
			Debug.Assert(0 < pin.BitWidth);
			return (pin.BitWidth == 1) ? "" : string.Format(CultureInfo.InvariantCulture, "[{0}:0]", pin.BitWidth - 1);
		}
		public static string Range(HdlConnection.BitRange range) => string.Format(CultureInfo.InvariantCulture, (range.First == range.Last) ? "[{0}]" : "[{0}:{1}]", range.Last, range.First);

		private void WriteRange(BasePin pin) {
			if(1 < pin.BitWidth) {
				this.Write(VerilogHdl.Range(pin));
			}
		}

		private static string WireName(Jam jam) {
			Debug.Assert(jam.Pin.PinType != PinType.Input);
			if(jam.CircuitSymbol.Circuit is Pin pin) {
				return pin.Name;
			} else {
				GridPoint point = jam.AbsolutePoint;
				return string.Format(CultureInfo.InvariantCulture, "Pin{0}x{1}", point.X, point.Y);
			}
		}

		private string Wire(Jam jam) {
			Debug.Assert(jam.Pin.PinType != PinType.Input);
			return VerilogHdl.WireName(this.wires[jam]);
		}

		private string WirePlug(HdlConnection connection, Jam jam) {
			Jam otherJam = connection.OtherJam(jam);
			if(otherJam.CircuitSymbol.Circuit is Constant constant) {
				int value = connection.OutBits.Extract(constant.ConstantValue);
				return string.Format(CultureInfo.InvariantCulture, "{0}'h{1:x}", connection.OutBits.BitWidth, value);
			} else {
				if(otherJam.Pin.PinType == PinType.Input) {
					otherJam = jam;
				}
				string text = this.Wire(otherJam);
				if(connection.IsBitRange(connection.Symbol(otherJam))) {
					text += VerilogHdl.Range(connection.JamRange(otherJam));
				}
				return text;
			}
		}

		private static int CompareGridPoint(GridPoint x, GridPoint y) {
			int delta = x.X - y.X;
			if(delta == 0) {
				delta = x.Y - y.Y;
			}
            return delta;
		}

		private static int CompareBitRange(HdlConnection.BitRange x, HdlConnection.BitRange y) {
			int delta = x.Last - y.Last;
			if(delta == 0) {
				delta = x.First - y.First;
			}
			return -delta;
		}

		private void WriteMemory(HdlSymbol symbol) {
			Memory memory = (Memory)symbol.CircuitSymbol.Circuit;

			this.Write("module {0}(input{1} address, output{2} dataOut", symbol.HdlExport.HdlName(symbol), VerilogHdl.Range(memory.AddressPin), VerilogHdl.Range(memory.DataOutPin));
			if(memory.Writable) {
				this.Write(", input{0} dataIn, input write", VerilogHdl.Range(memory.DataOutPin));
			}
			if(memory.DualPort) {
				this.Write(", input{0} address2, output{1} dataOut2", VerilogHdl.Range(memory.Address2Pin), VerilogHdl.Range(memory.DataOut2Pin));
			}
			this.WriteLine(");");

			// allocate memory array
			this.WriteLine("\treg{0} memory[0:{1}];", VerilogHdl.Range(memory.DataOutPin), 1 << memory.AddressBitWidth);
			if(memory.Writable && memory.OnStart != MemoryOnStart.Data) {
				this.WriteLine("\tinteger i;");
			}

			// Memory initialization
			this.WriteLine("\tinitial begin");
			if(!memory.Writable || memory.OnStart == MemoryOnStart.Data) {
				byte[] data = memory.MemoryValue();
				int addressWidth = memory.AddressBitWidth;
				int dataWidth = memory.DataBitWidth;
				int max = 1 << addressWidth;
				for(int i = 0; i < max; i++) {
					this.WriteLine("\t\tmemory[{0}] = {1};", i, Memory.CellValue(data, dataWidth, i).ToString(CultureInfo.InvariantCulture));
				}
			} else {
				string value = memory.OnStart switch {
					MemoryOnStart.Random => "$random",
					MemoryOnStart.Ones => "-1",
					MemoryOnStart.Zeros => "0",
					_ => throw new InvalidOperationException()
				};
				this.WriteLine("\t\tfor(i = 0; i < {0}; i = i + 1) begin", 1 << memory.AddressBitWidth);
				this.WriteLine("\t\t\tmemory[i] = {0};", value);
				this.WriteLine("\t\tend");
			}
			this.WriteLine("\tend // initial");
			this.WriteLine();

			// Memory logic itself
			if(memory.Writable) {
				this.WriteLine("\talways @({0} write) begin", memory.WriteOn1 ? "posedge" : "negedge");
				this.WriteLine("\t\tmemory[address] <= dataIn;");
				this.WriteLine("\tend");
			}

			this.WriteLine("\tassign dataOut = memory[address];");

			if(memory.DualPort) {
				this.WriteLine("\tassign dataOut2 = memory[address2];");
			}

			this.WriteLine("endmodule // {0}", symbol.HdlExport.HdlName(symbol));
			this.WriteLine();
		}

		public override string TransformText() {
			this.Prepare();

			foreach(HdlSymbol symbol in this.Parts) {
				if(symbol.CircuitSymbol.Circuit is Memory) {
					this.WriteMemory(symbol);
				}
			}

			this.WriteLine("module {0}(", this.Name);

			bool comma = false;
			foreach(HdlSymbol symbol in this.InputPins.Concat(this.OutputPins)) {
				Pin pin = (Pin)symbol.CircuitSymbol.Circuit;
				if(comma) this.WriteLine(",");
				this.Write("\t{0}", pin.PinType == PinType.Input ? "input" : "output");
				this.WriteRange(pin);
				this.Write("\t{0}", pin.Name);
				comma = true;
			}
			this.WriteLine();
			this.WriteLine(");");

			foreach(Jam jam in this.wires.Values.Where(j => j.CircuitSymbol.Circuit is not Pin).OrderBy(j => j.AbsolutePoint, Comparer<GridPoint>.Create(VerilogHdl.CompareGridPoint))) {
				this.Write("\twire");
				this.WriteRange(jam.Pin);
				this.WriteLine(" {0};", VerilogHdl.WireName(jam));
			}
			this.WriteLine();

			foreach(HdlSymbol part in this.Parts) {
				if(this.CommentPoints) {
					this.WriteLine("\t// {0}", part.Comment);
				}
				this.WriteLine("\t{0} {0}_{1}x{2}(", part.HdlExport.HdlName(part), part.CircuitSymbol.X, part.CircuitSymbol.Y);

				if(part.CircuitSymbol.Circuit is Gate gate) {
					if(GateType.Not <= gate.GateType && gate.GateType <= GateType.Xor || GateType.TriState1 <= gate.GateType) {
						Jam output = part.CircuitSymbol.Jams().First(j => j.Pin.PinType == PinType.Output);
						if(this.connections.TryGetValue(output, out ICollection<HdlConnection>? outputs)) {
							if(1 < outputs.Count) {
								this.Write("\t\t{0}", this.Wire(output));
							} else {
								Debug.Assert(1 == outputs.Count);
								this.Write("\t\t{0}", this.WirePlug(outputs.First(), output));
							}
							List<Jam> inputs = part.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Input).ToList();
							inputs.Sort(JamComparer.Comparer); // this will place tri-state enable pin at the end - this is how Verilog expects it.
							foreach(Jam input in inputs) {
								if(this.connections.TryGetValue(input, out ICollection<HdlConnection>? list)) {
									if(1 < list.Count) {
										part.HdlExport.Error(
											Properties.Resources.ErrorManyResults(input.Pin.Name, part.CircuitSymbol.Circuit.Name, part.CircuitSymbol.Point.ToString(), part.CircuitSymbol.LogicalCircuit.Name)
										);
										return string.Empty;
									}
									this.WriteLine(",");
									this.Write("\t\t{0}", this.WirePlug(list.First(), input));
								}
							}
						}
					} else {
						Debug.Fail("Not implemented yet.");
					}
				} else {
					comma = false;
					foreach(Jam jam in part.CircuitSymbol.Jams()) {
						if(this.connections.TryGetValue(jam, out ICollection<HdlConnection>? list)) {
							if(comma) this.WriteLine(",");
							comma = true;
							this.Write("\t\t.{0}(", part.HdlExport.HdlName(jam));
							if(jam.Pin.PinType == PinType.Output) {
								if(1 < list.Count) {
									this.Write(this.Wire(jam));
								} else {
									this.Write(this.WirePlug(list.First(), jam));
								}
							} else {
								if(1 < list.Count) {
									this.Write("{");
								}
								bool innerComma = false;
								foreach(HdlConnection connection in list.OrderBy(c => c.InBits, Comparer<HdlConnection.BitRange>.Create(VerilogHdl.CompareBitRange))) {
									if(innerComma) this.Write(", ");
									innerComma = true;
									this.Write(this.WirePlug(connection, jam));
								}
								if(1 < list.Count) {
									this.Write("}");
								}
							}
							this.Write(")");
						}
					}
				}
				this.WriteLine();
				this.WriteLine("\t);");
			}
			this.WriteLine();

			if(0 < this.assignments.Count) {
				foreach(KeyValuePair<Jam, ICollection<HdlConnection>> pair in this.assignments) {
					Jam jam = pair.Key;
					ICollection<HdlConnection> list = pair.Value;
					foreach(HdlConnection connection in list) {
						this.WriteLine("\tassign {0} = {1};", this.WirePlug(connection, jam), this.WirePlug(connection, connection.OtherJam(jam)));
					}
				}
				this.WriteLine();
			}

			this.WriteLine("endmodule // {0}", this.Name);

			return this.GenerationEnvironment.ToString();
		}
	}
}
