// Ignore Spelling: Verilog Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LogicCircuit {
	internal sealed class VerilogHdl : HdlTransformation {
		private readonly Dictionary<Jam, Jam> wires = new();
		private readonly OneToMany<Jam, HdlConnection> connections = new(true);
		private readonly OneToMany<Jam, HdlConnection> assignments = new(true);
		private readonly Func<string, string> fixName;

		public const string HWIfTypeNamePrefix = "hwif";
		public const string HWIfParamName = "hw";

		public List<(string, string)> HWIfInstances;
		public List<(string, bool)> HWIfWires;

		public string HWIfTypeName {
			get {
				return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", HWIfTypeNamePrefix, this.Name);
			}
		}

		public VerilogHdl(string name, string note, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts, Func<string, string> fixName) : base(
			name, note, inputPins, outputPins, parts
		) {
			this.fixName = fixName;
			this.HWIfInstances = new List<(string, string)>();
			this.HWIfWires = new List<(string, bool)>();
		}

		public static string HWIfPartTypeName(HdlSymbol part) {
			return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", HWIfTypeNamePrefix, part.HdlExport.HdlName(part));
		}

		public static string HWIfPartInstanceFieldName(HdlSymbol part) {
			return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}x{2}", part.HdlExport.HdlName(part), part.CircuitSymbol.X, part.CircuitSymbol.Y);
		}

		public static string HWIfPartInstanceFieldPinName(HdlSymbol part, string pin) {
			return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", HWIfPartInstanceFieldName(part), pin);
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
				foreach(HdlConnection connection in output.HdlConnections().Where(c => c.OtherJam(jam).CircuitSymbol.Circuit is Constant)) {
					this.assignments.Add(connection.OtherJam(jam), connection);
				}
			}
			foreach(HdlConnection connection in this.Parts.SelectMany(p => p.HdlConnections())) {
				this.connections.Add(connection.OutJam, connection);
				this.connections.Add(connection.InJam, connection);
			}
			foreach(KeyValuePair<Jam, ICollection<HdlConnection>> pair in this.connections) {
				Jam jam = pair.Key;
				ICollection<HdlConnection> list = pair.Value;
				Debug.Assert(0 < list.Count);
				if(jam.Pin.PinType == PinType.Output && jam.CircuitSymbol.Circuit is not Constant) {
					// Find first Pin of the same bit width as the jam, but on the other side of connection. This pin will serve as wire. With exception if it's the only pin is connected to the jam.
					Jam? other = list.Select(c => c.OtherJam(jam)).FirstOrDefault(j => j.CircuitSymbol.Circuit is Pin pin && pin.BitWidth == jam.Pin.BitWidth);
					if(other != null) {
						this.wires.Add(jam, other);
					} else {
						this.wires.Add(jam, jam);
					}
					foreach(HdlConnection connection in list) {
						Jam j = connection.OtherJam(jam);
						if(j != other && j.CircuitSymbol.Circuit is Pin) {
							this.assignments.Add(jam, connection);
						}
					}
				}
			}
		}

		public string Range(BasePin pin) {
			Debug.Assert(0 < pin.BitWidth);
			return (pin.BitWidth == 1) ? "" : this.Format("[{0}:0]", pin.BitWidth - 1);
		}
		public string Range(HdlConnection.BitRange range) => this.Format((range.First == range.Last) ? "[{0}]" : "[{0}:{1}]", range.Last, range.First);

		private string WireName(Jam jam) {
			Tracer.Assert(jam.Pin.PinType != PinType.Input);
			if(jam.CircuitSymbol.Circuit is Pin pin) {
				return this.fixName(pin.Name);
			} else {
				GridPoint point = jam.AbsolutePoint;
				return this.Format("Pin{0}x{1}", point.X, point.Y);
			}
		}

		private string Wire(Jam jam) {
			Debug.Assert(jam.Pin.PinType != PinType.Input);
			return this.WireName(this.wires[jam]);
		}

		private string WirePlug(HdlConnection connection, Jam jam) {
			Jam otherJam = connection.OtherJam(jam);
			if(otherJam.CircuitSymbol.Circuit is Constant constant) {
				int value = connection.OutBits.Extract(constant.ConstantValue);
				return this.Format("{0}'h{1:x}", connection.OutBits.BitWidth, value);
			} else {
				if(otherJam.Pin.PinType == PinType.Input) {
					otherJam = jam;
				}
				string text = this.Wire(otherJam);
				if(connection.IsBitRange(connection.Symbol(otherJam))) {
					text += this.Range(connection.JamRange(otherJam));
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

			this.Write("module {0}(input{1} address, output{2} dataOut", symbol.HdlExport.HdlName(symbol), this.Range(memory.AddressPin), this.Range(memory.DataOutPin));
			if(memory.Writable) {
				this.Write(", input{0} dataIn, input write", this.Range(memory.DataOutPin));
			}
			if(memory.DualPort) {
				this.Write(", input{0} address2, output{1} dataOut2", this.Range(memory.Address2Pin), this.Range(memory.DataOut2Pin));
			}
			this.WriteLine(");");

			// allocate memory array
			this.WriteLine("\treg{0} memory[0:{1}];", this.Range(memory.DataOutPin), (1 << memory.AddressBitWidth) - 1);
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
					this.WriteLine("\t\tmemory[{0}] = {1};", i, Memory.CellValue(data, dataWidth, i));
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

		private void WriteComment(int indent, string comment) {
			if(this.CommentPoints && !string.IsNullOrWhiteSpace(comment)) {
				foreach(ReadOnlySpan<char> line in comment.EnumerateLines()) {
					if(0 < indent) {
						this.Write(new string('\t', indent));
					}
					this.Write("// ");
					this.WriteLine(line);
				}
			}
		}

		public override string TransformText() {
			this.Prepare();

			this.WriteLine("// This file is generated by LogicCircuit version {0} from circuit {1}", DialogAbout.CurrentVersion(),  this.Name);
			this.WriteLine();

			foreach(HdlSymbol symbol in this.Parts) {
				if(symbol.CircuitSymbol.Circuit is Memory) {
					this.WriteMemory(symbol);
				}
			}

			this.WriteComment(0, this.Note);
			this.WriteLine("module {0}(", this.Name);
			this.Write("\t{0} {1}", this.HWIfTypeName, HWIfParamName);
			bool comma = true;

			foreach(HdlSymbol symbol in this.InputPins.Concat(this.OutputPins)) {
				Pin pin = (Pin)symbol.CircuitSymbol.Circuit;
				if(comma) this.WriteLine(",");
				this.WriteComment(1, pin.Note);
				this.Write("\t{0}", pin.PinType == PinType.Input ? "input" : "output");
				this.Write(this.Range(pin));
				this.Write("\t{0}", this.fixName(pin.Name));
				comma = true;
			}
			this.WriteLine();
			this.WriteLine(");");

			foreach(Jam jam in this.wires.Values.Where(j => j.CircuitSymbol.Circuit is not Pin).OrderBy(j => j.AbsolutePoint, Comparer<GridPoint>.Create(VerilogHdl.CompareGridPoint))) {
				this.Write("\twire");
				this.Write(this.Range(jam.Pin));
				this.WriteLine(" {0};", this.WireName(jam));
			}
			this.WriteLine();

			foreach(HdlSymbol part in this.Parts) {
				bool external = false;
				comma = false;

				if(this.CommentPoints) {
					this.WriteLine("\t// {0}", part.Comment);
				}

				if(part.CircuitSymbol.Circuit is CircuitButton ||
					part.CircuitSymbol.Circuit is Sensor ||
					(part.CircuitSymbol.Circuit is Gate gate1 &&
						(GateType.Clock == gate1.GateType || GateType.Led == gate1.GateType))) {
					external = true;
				} else {
					this.WriteLine("\t{0} {0}_{1}x{2}(", part.HdlExport.HdlName(part), part.CircuitSymbol.X, part.CircuitSymbol.Y);
					if(!(part.CircuitSymbol.Circuit is Gate gate2 &&
							(GateType.Not <= gate2.GateType && gate2.GateType <= GateType.Xor || GateType.TriState1 <= gate2.GateType)) &&
							!(part.CircuitSymbol.Circuit is Memory)) {
						this.HWIfInstances.Add((HWIfPartTypeName(part), HWIfPartInstanceFieldName(part)));
						this.Write("\t\t.{0}({0}.{1})", HWIfParamName, HWIfPartInstanceFieldName(part));
						comma = true;
					}
				}

				if(part.CircuitSymbol.Circuit is Gate gate) {
					if(GateType.Not <= gate.GateType && gate.GateType <= GateType.Xor || GateType.TriState1 <= gate.GateType || GateType.Clock == gate.GateType || GateType.Led == gate.GateType) {
						// In gate level model put output pin first, and other pins later
						Jam? output = part.CircuitSymbol.Jams().FirstOrDefault(j => j.Pin.PinType == PinType.Output);
						if(output != null && this.connections.TryGetValue(output, out ICollection<HdlConnection>? recipients)) {
							if(external) {
								// Add the used input external port to the collection (input since here it is used for output).
								this.HWIfWires.Add((HWIfPartInstanceFieldPinName(part, output.Pin.Name), false));
								this.WriteLine("\tassign {0} = {1}.{2};", this.Wire(output), HWIfParamName, HWIfPartInstanceFieldPinName(part, output.Pin.Name));
							} else {
								if(1 < recipients.Count || recipients.First().IsBitRange()) {
									this.Write("\t\t{0}", this.Wire(output));
								} else {
									Debug.Assert(1 == recipients.Count);
									this.Write("\t\t{0}", this.WirePlug(recipients.First(), output));
								}
							}
						}

						// All the other gate connections are inputs.
						List<Jam> inputs = part.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Input).ToList();
						inputs.Sort(JamComparer.Comparer); // this will place tri-state enable pin at the end - this is how Verilog expects it.
						foreach(Jam input in inputs) {
							if(this.connections.TryGetValue(input, out ICollection<HdlConnection>? list)) {
								// Here it is possible to have more than one connection sending data to the input.
								// If these connections are note tri state capable it should be an error.
								// However it is not possible to check it here as there is no full information on what exactly connected here.
								// So, ignore it here, made all the connections, and hope the user was checking the circuit before exporting it to Verilog.
								// If user is exporting test bench for this circuit the error will pop up during truth table building.
								// Otherwise is user's mistake.
								// TODO: Here it's must generate AND connecting all the inputs as the LogicCircuit doing exactly that.
								foreach(HdlConnection connection in list) {
									if(external) {
										string member_if_instance = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}x{2}_{3}", part.HdlExport.HdlName(part), part.CircuitSymbol.X, part.CircuitSymbol.Y, input.Pin.Name);
										// Add the used output external port to the collection (output since here it is used for input).
										this.HWIfWires.Add((HWIfPartInstanceFieldPinName(part, input.Pin.Name), true));
										this.WriteLine("\tassign {0}.{1} = {2};", HWIfParamName, HWIfPartInstanceFieldPinName(part, input.Pin.Name), this.WirePlug(list.First(), input));
									} else {
										this.WriteLine(",");
										this.Write("\t\t{0}", this.WirePlug(connection, input));
									}
								}
							}
						}
					} else {
						Debug.Fail("Not implemented yet.");
					}
				} else {
					foreach(Jam jam in part.CircuitSymbol.Jams()) {
						if(!external) {
							if(comma) this.WriteLine(",");
							comma = true;
							this.Write("\t\t.{0}(", part.HdlExport.HdlName(jam));
						}
						if(this.connections.TryGetValue(jam, out ICollection<HdlConnection>? list)) {
							if(jam.Pin.PinType == PinType.Output) {
								if(external) {
									// Add the used input external port to the collection (input since here it is used for output).
									this.HWIfWires.Add((HWIfPartInstanceFieldPinName(part, jam.Pin.Name), false));
									this.WriteLine("\tassign {0} = {1}.{2};", this.Wire(jam), HWIfParamName, HWIfPartInstanceFieldPinName(part, jam.Pin.Name));
								} else {
									if(1 < list.Count || list.First().IsBitRange()) {
										this.Write(this.Wire(jam));
									} else {
										this.Write(this.WirePlug(list.First(), jam));
									}
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
						}
						if(!external) {
							this.Write(")");
						}
					}
				}
				this.WriteLine();
				if(!external) {
					this.WriteLine("\t);");
					this.WriteLine("");
				}
			}
			this.WriteLine();

			if(0 < this.assignments.Count) {
				List<string> lines = new();

				foreach(KeyValuePair<Jam, ICollection<HdlConnection>> pair in this.assignments) {
					Jam jam = pair.Key;
					ICollection<HdlConnection> list = pair.Value;
					foreach(HdlConnection connection in list) {
						lines.Add(this.Format("\tassign {0} = {1};", this.WirePlug(connection, jam), this.WirePlug(connection, connection.OtherJam(jam))));
					}
				}
				lines.Sort();
				foreach(string line in lines) {
					this.WriteLine(line);
				}
				this.WriteLine();
			}

			this.WriteLine("endmodule // {0}", this.Name);

			return this.GenerationEnvironment.ToString();
		}
	}
}
