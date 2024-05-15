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
			"module", "input", "output", "inout", "endmodule", "wire", "wand", "wor", "reg", "integer", "time", "real",
			"realtime", "event", "task", "begin", "end", "endtask", "function", "endfunction", "assign", "deassign",
			"initial", "always", "defparam", "strength", "delay", "if", "else", "case", "casex", "casez", "endcase", "endcase",
			"repeat", "while", "for", "fork", "join", "specify", "endspecify", 
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
