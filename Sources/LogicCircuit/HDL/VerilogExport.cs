// Ignore Spelling: Verilog Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogicCircuit {
	internal class VerilogExport : HdlExport {
		private readonly Regex identifier = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_$]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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

		public VerilogExport(bool exportTests, bool commentPoints, Action<string> logMessage, Action<string> logError) : base(exportTests, commentPoints, logMessage, logError) {
		}

		protected override string FileName(LogicalCircuit circuit) => circuit.Name + ".sv";

		public override bool CanExport(Circuit circuit) {
			return !(
				circuit is CircuitButton ||
				circuit is Sensor ||
				circuit is Gate gate && (gate.GateType == GateType.Clock || gate.GateType == GateType.Led) ||
				circuit is LedMatrix ||
				circuit is Sound
			);
		}

		public override bool IsValid(string name) => this.identifier.IsMatch(name) && !this.keywords.Contains(name);

		protected override bool Validate(HdlTransformation transformation) {
			bool valid = base.Validate(transformation);
			OneToMany<Jam, HdlConnection> jams = new OneToMany<Jam, HdlConnection>(true);
			foreach(HdlSymbol symbol in transformation.Parts.Where(s => s.CircuitSymbol.Circuit is not Gate).Concat(transformation.OutputPins)) {
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
						this.Message(
							Properties.Resources.WarningVerilogFloatingJam(jam.Pin.Name, jam.CircuitSymbol.Circuit.Name, jam.CircuitSymbol.Point, transformation.Name)
						);
					}
				}
			}
			return valid;
		}

		protected override HdlTransformation? CreateTransformation(string name, IList<HdlSymbol> inputPins, IList<HdlSymbol> outputPins, IList<HdlSymbol> parts) {
			return new VerilogHdl(name, inputPins, outputPins, parts);
		}

		public override string HdlName(HdlSymbol symbol) {
			Circuit circuit = symbol.CircuitSymbol.Circuit;
			Debug.Assert(circuit is not Splitter && circuit is not CircuitProbe);
			if(circuit is Gate gate) {
				switch(gate.GateType) {
				case GateType.Not:	return "not";
				case GateType.Or:	return gate.InvertedOutput ? "nor" : "or";
				case GateType.And:	return gate.InvertedOutput ? "nand" : "and";
				case GateType.Xor:	return gate.InvertedOutput ? "xnor" : "xor";
				case GateType.TriState1:
				case GateType.TriState2: return "bufif1";
				}
			}
			return circuit.Name.Trim();
		}

		protected override void ExportTest(string circuitName, List<InputPinSocket> inputs, List<OutputPinSocket> outputs, IList<TruthState> table, string folder) {
			VerilogTestBench verilogTest = new VerilogTestBench(
				circuitName,
				inputs,
				outputs,
				table
			);
			string text = verilogTest.TransformText();
			if(!string.IsNullOrWhiteSpace(text)) {
				string testFile = Path.Combine(folder, circuitName + "_TestBench.sv");
				File.WriteAllText(testFile, text);
				this.Message(Properties.Resources.MessageHdlSavingTestFile(testFile));
			}
		}
	}
}
