// Ignore Spelling: Verilog Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogicCircuit {
	/// <summary>
	/// Export to Verilog language.
	/// Some useful links.
	/// Good tutorial: https://www.chipverify.com/
	/// Another one: https://www.asic-world.com/
	/// And another one: http://www.emmelmann.org/Pages/Library_TutorialsWS.html
	/// Online test bench. Run generated code there: https://www.edaplayground.com/
	/// </summary>
	internal sealed class VerilogExport : HdlExport {
		private VerilogHdl? lastVerilogHdl;
		private readonly Regex identifier = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_$]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		private readonly Regex notSupportedChars = new Regex(@"\s|[,.?/!@#$%^&*()\-+={}[\]|\\<>~`]", RegexOptions.Compiled);

		private readonly HashSet<string> keywords = new HashSet<string>() {
			"always", "assign", "attribute", "begin", "buf", "bufif0", "case", "casex", "casez",
			"cmos", "deassign", "default", "defparam", "disable", "edge", "else", "end", "endattribute", "endcase",
			"endfunction", "endmodule", "endprimitive", "endspecify", "endtable", "endtask", "event", "for", "force",
			"forever", "fork", "function", "highz0", "highz1", "if", "ifnone", "initial", "inout", "input", "integer",
			"join", "medium", "module", "large", "macromodule", "negedge", "nmos", "notif0",
			"notif1", "output", "parameter", "pmos", "posedge", "primitive", "pull0", "pull1", "pulldown",
			"pullup", "rcmos", "real", "realtime", "reg", "release", "repeat", "rnmos", "rpmos", "rtran", "rtranif0",
			"rtranif1", "scalared", "signed", "small", "specify", "specparam", "strength", "strong0", "strong1",
			"supply0", "supply1", "table", "task", "time", "tran", "tranif0", "tranif1", "tri", "tri0", "tri1",
			"triand", "trior", "trireg", "unsigned", "vectored", "wait", "wand", "weak0", "weak1", "while", "wire",
			"wor",

			//"and", "bufif1", "nand", "nor", "not", "or", "xnor", "xor",
		};

		// Hardware Interface aggregated collections.
		//
		private Dictionary<LogicalCircuit, string> HWIfInterfaceMap;
		private Dictionary<string, (List<(string, string)>, List<(string, bool)>)> HWIfMemberMap;

		public VerilogExport(bool exportTests, bool commentPoints, bool fixNames, Action<string> logMessage, Action<string> logError, Action<string> logWarning) : base(
			exportTests, commentPoints, fixNames, logMessage, logError, logWarning
		) {
			HWIfInterfaceMap = new Dictionary<LogicalCircuit, string>();
			HWIfMemberMap = new Dictionary<string, (List<(string, string)>, List<(string, bool)>)>();
		}

		protected override string FileName(LogicalCircuit circuit) => this.FixName(circuit.Name) + ".sv";

		public override bool CanExport(Circuit circuit) {
			return !(
				circuit is LedMatrix ||
				circuit is Sound
			);
		}

		public override bool IsValid(string name) {
			name = this.FixName(name);
			return this.identifier.IsMatch(name) && !this.keywords.Contains(name);
		}

		private string FixName(string name) => this.FixNames ? this.notSupportedChars.Replace(name, "_") : name;

		protected override bool Validate(HdlTransformation transformation) {
			bool valid = base.Validate(transformation);
			OneToMany<Jam, HdlConnection> jams = new OneToMany<Jam, HdlConnection>(true);
			foreach(HdlSymbol symbol in transformation.Parts.Concat(transformation.OutputPins)) {
				// check for floating IO ports. Most of the gates can be excluded as floating port will not be generated.
				Gate? gate = symbol.CircuitSymbol.Circuit as Gate;
				if(gate == null || gate.GateType == GateType.TriState1 || gate.GateType == GateType.TriState2) {
					jams.Clear();
					foreach(HdlConnection connection in symbol.HdlConnections()) {
						jams.Add(connection.OutJam, connection);
						jams.Add(connection.InJam, connection);
					}
					foreach(Jam jam in symbol.CircuitSymbol.Jams().Where(j => j.Pin.PinType != PinType.Output)) {
						bool cover = true;
						if(jams.TryGetValue(jam, out ICollection<HdlConnection>? connections)) {
							List<HdlConnection> list = connections.ToList();
							Debug.Assert(0 < list.Count);
							list.Sort((x, y) => x.InBits.First - y.InBits.First);
							HdlConnection.BitRange range = list[0].InBits;
							foreach(HdlConnection.BitRange inRange in list.Select(c => c.InBits)) {
								if(range.CanAdd(inRange)) {
									range = range.Add(inRange);
								} else {
									cover = false;
								}
							}
							if(0 < range.First || range.Last < jam.Pin.BitWidth - 1) {
								cover = false;
							}
						} else {
							cover = false;
						}
						if(!cover) {
							string text = Properties.Resources.WarningVerilogFloatingJam(jam.Pin.Name, jam.CircuitSymbol.Circuit.Name, jam.CircuitSymbol.Point, transformation.Name);
							if(gate != null) {
								this.Error(text);
								valid = false;
							} else {
								this.Warning(text);
							}
						}
					}
				}
			}
			return valid;
		}

		protected override HdlTransformation? CreateTransformation(string name, string note, IList<HdlSymbol> inputPins, IList<HdlSymbol> outputPins, IList<HdlSymbol> parts) {
			VerilogHdl verilogHdl = new VerilogHdl(this.FixName(name), note, inputPins, outputPins, parts, this.FixName);
			this.lastVerilogHdl = verilogHdl;
			return verilogHdl;
		}

		public override string HdlName(HdlSymbol symbol) {
			Circuit circuit = symbol.CircuitSymbol.Circuit;
			Debug.Assert(circuit is not Splitter && circuit is not CircuitProbe);
			if(circuit is Gate gate) {
				switch(gate.GateType) {
					case GateType.Not: return "not";
					case GateType.Or: return gate.InvertedOutput ? "nor" : "or";
					case GateType.And: return gate.InvertedOutput ? "nand" : "and";
					case GateType.Xor: return gate.InvertedOutput ? "xnor" : "xor";
					case GateType.Clock: return gate.InvertedOutput ? throw new InvalidOperationException() : "clock";
					case GateType.Led: return gate.InvertedOutput ? throw new InvalidOperationException() : "led";
					case GateType.TriState1:
					case GateType.TriState2: return "bufif1";
				}
			}
			if(circuit is CircuitButton) {
				return "button";
			}
			if(circuit is Memory memory) {
				if(memory.Writable) {
					return this.Format("{0}_RAM_{1}x{2}", this.FixName(symbol.CircuitSymbol.LogicalCircuit.Name), symbol.CircuitSymbol.X, symbol.CircuitSymbol.Y);
				} else {
					return this.Format("{0}_ROM_{1}x{2}", this.FixName(symbol.CircuitSymbol.LogicalCircuit.Name), symbol.CircuitSymbol.X, symbol.CircuitSymbol.Y);
				}
			}
			return this.FixName(circuit.Name.Trim());
		}

		public override string HdlName(Jam jam) {
			if(jam.CircuitSymbol.Circuit is Memory memory) {
				BasePin pin = jam.Pin;
				if(pin == memory.AddressPin) return "address";
				if(pin == memory.DataInPin) return "dataIn";
				if(pin == memory.DataOutPin) return "dataOut";
				if(pin == memory.WritePin) return "write";
				if(pin == memory.Address2Pin) return "address2";
				if(pin == memory.DataOut2Pin) return "dataOut2";
			}
			return this.FixName(base.HdlName(jam));
		}

		protected override void ExportTest(string circuitName, List<InputPinSocket> inputs, List<OutputPinSocket> outputs, IList<TruthState> table, string folder) {
			Debug.Assert(this.lastVerilogHdl is not null);
			VerilogTestBench verilogTest = new VerilogTestBench(
				this.lastVerilogHdl,
				this.FixName(circuitName),
				this.TestBenchModuleName(circuitName),
				inputs,
				outputs,
				table,
				this.FixName
			);
			string text = verilogTest.TransformText();
			if(!string.IsNullOrWhiteSpace(text)) {
				string testFile = Path.Combine(folder, this.TestBenchFileName(circuitName));
				File.WriteAllText(testFile, text);
				this.Message(Properties.Resources.MessageHdlSavingTestFile(testFile));
			}
		}

		public string TestBenchModuleName(string circuitName) => this.FixName(circuitName) + "_TestBench";
		public string TestBenchFileName(string circuitName) => this.TestBenchModuleName(circuitName) + ".sv";

		protected override void FinalizeTransformation(HdlTransformation transformation, LogicalCircuit circuit) {
			VerilogHdl? verilogTransform = transformation as VerilogHdl;
			Trace.Assert(verilogTransform != null);

			this.HWIfInterfaceMap[circuit] = verilogTransform.HWIfTypeName;
			this.HWIfMemberMap[verilogTransform.HWIfTypeName] = (verilogTransform.HWIfInstances, verilogTransform.HWIfWires);
		}

		protected override void FinalizeExport(CircuitMap circuitMap, ConnectionSet connectionSet, string folder) {
			List<(string, bool)> ports = new List<(string, bool)>();

			void extract(string hwifPath, CircuitMap circuitMap) {
				string? hwifTypeName;
				this.HWIfInterfaceMap.TryGetValue(circuitMap.Circuit, out hwifTypeName);
				if(hwifTypeName != null) {
					var (interfaces, wires) = this.HWIfMemberMap[hwifTypeName];
					foreach(var (portName, isOutput) in wires) {
						ports.Add((String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", hwifPath, portName), isOutput));
					}
				}
			}

			void walk(string hwifPath, CircuitMap circuitMap) {
				extract(hwifPath, circuitMap);
				foreach(CircuitMap child in circuitMap.Children) {
					if(child.CircuitSymbol != null) {
						Dictionary<CircuitSymbol, HdlSymbol> symbolMap = this.Collect(circuitMap.Circuit, connectionSet);
						HdlSymbol hdlChildSymbol = symbolMap[child.CircuitSymbol];
						string hwifChildPath = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", hwifPath, VerilogHdl.HWIfPartInstanceFieldName(hdlChildSymbol));
						walk(hwifChildPath, child);
					}
				}
			}

			// Extract all external ports from the hierarchy including their hierarchical hwif paths.
			//
			string hwifPath = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", VerilogHdl.HWIfParamName, this.FixName(circuitMap.Circuit.Name));
			walk(hwifPath, circuitMap);

			using(StreamWriter writer = new StreamWriter(Path.Combine(folder, "hwif.sv"))) {
				GenerateHWInterfaces(writer, this.FixName(circuitMap.Circuit.Name));
			}

			using(StreamWriter writer = new StreamWriter(Path.Combine(folder, "top.sv.example"))) {
				GenerateTopModule(writer, this.FixName(circuitMap.Circuit.Name), ports);
			}
		}

		private void GenerateHWInterfaces(StreamWriter writer, string circuitName) {

			foreach(var (hwifTypeName, children) in this.HWIfMemberMap.Reverse()) {
				writer.WriteLine("interface {0};", hwifTypeName);
				var (interfaces, wires) = this.HWIfMemberMap[hwifTypeName];
				foreach(var (hwifChildTypeName, hwifChildFieldName) in interfaces) {
					writer.WriteLine("\t{0}\t{1}();", hwifChildTypeName, hwifChildFieldName);
				}
				foreach(var (portName, isOutput) in wires) {
					writer.WriteLine("\tlogic\t{0};", portName);
				}
				writer.WriteLine("endinterface");
				writer.WriteLine();
			}

			writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "interface {0};", VerilogHdl.HWIfTypeNamePrefix));
			writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\t{0}_{1}\t{1}();", VerilogHdl.HWIfTypeNamePrefix, circuitName));
			writer.WriteLine("endinterface");
		}

#pragma warning disable CA1822 // Mark members as static
		private void GenerateTopModule(StreamWriter writer, string circuitName, List<(string, bool)> ports) {
#pragma warning restore CA1822 // Mark members as static

			writer.WriteLine("module top(");

			// Generate external input ports.
			//
			bool comma = false;
			foreach(var (port, isOutput) in ports) {
				if(!isOutput) {
					if(comma) {
						writer.WriteLine(",");
					}
					writer.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\tinput logic {0}", port.Replace('.', '_')));
					comma = true;
				}
			}

			if(comma) {
				writer.WriteLine(",");
				comma = false;
			}

			// Generate external output ports.
			//
			foreach(var (port, isOutput) in ports) {
				if(isOutput) {
					if(comma) {
						writer.WriteLine(",");
					}
					writer.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\toutput logic {0}", port.Replace('.', '_')));
					comma = true;
				}
			}
			if(comma) {
				writer.WriteLine("");
			}

			writer.WriteLine(");");

			// Instantiate the top-level HWIf.
			//
			writer.WriteLine("");
			writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\t{0}\t{1}();", VerilogHdl.HWIfTypeNamePrefix, VerilogHdl.HWIfParamName));

			// Connect external input ports to corresponding HWIf's.
			//
			writer.WriteLine("");
			foreach(var (port, isOutput) in ports) {
				if(!isOutput) {
					writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\tassign {0} = {1};", port, port.Replace('.', '_')));
				}
			}

			// Connect external output ports to corresponding HWIf's.
			//
			writer.WriteLine("");
			foreach(var (port, isOutput) in ports) {
				if(isOutput) {
					writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\tassign {0} = {1};", port.Replace('.', '_'), port));
				}
			}

			// Instantiate the top-level circuit.
			//
			writer.WriteLine("");
			writer.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "\t{0} m_{0}(.{1}({1}.{0}));", circuitName, VerilogHdl.HWIfParamName));

			writer.WriteLine("endmodule");
		}
	}
}
