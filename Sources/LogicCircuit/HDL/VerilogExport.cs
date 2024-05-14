// Ignore Spelling: Verilog

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Scripting.Hosting;

namespace LogicCircuit {
	internal class VerilogExport : HdlExport {
		private readonly Regex identifier = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_$]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		private readonly HashSet<string> keywords = new HashSet<string>() {
			"module", "input", "output", "inout", "endmodule", "wire", "wand", "wor", "reg", "integer", "time", "real",
			"realtime", "event", "task", "begin", "end", "endtask", "function", "endfunction", "assign", "deassign",
			"initial", "always", "defparam", "strength", "delay", "if", "else", "case", "casex", "casez", "endcase", "endcase",
			"repeat", "while", "for", "fork", "join", "specify", "endspecify", 
		};

		private readonly bool exportTests;

		public VerilogExport(bool exportTests, bool commentPoints, Action<string> logMessage, Action<string> logError) : base(commentPoints, logMessage, logError) {
			this.exportTests = exportTests;
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

		protected override bool PostExport(LogicalCircuit logicalCircuit, string folder) {
			if(this.exportTests) {
				this.ExportTest(logicalCircuit, folder);
			}
			return base.PostExport(logicalCircuit, folder);
		}

		private void ExportTest(LogicalCircuit circuit, string folder) {
			if(CircuitTestSocket.IsTestable(circuit)) {
				CircuitTestSocket socket = new CircuitTestSocket(circuit);
				if(socket.Inputs.Sum(i => i.Pin.BitWidth) <= HdlExport.MaxTestableInputBits) {
					void reportProgress(double progress) => this.Message(Properties.Resources.MessageHdlBuildingTruthTable(circuit.Name, progress));

					ThreadPool.QueueUserWorkItem(o => {
						try {
							bool isTrancated = false;
							IList<TruthState>? table = socket.BuildTruthTable(reportProgress, () => true, null, DialogTruthTable.MaxRows, out isTrancated);
							if (table == null || isTrancated) {
								this.Message(Properties.Resources.ErrorHdlTruthTableFailed);
							} else {
								VerilogTestBench verilogTest = new VerilogTestBench(
									circuit.Name,
									socket.Inputs.ToList(),
									socket.Outputs.ToList(),
									table
								);
								string text = verilogTest.TransformText();
								if(!string.IsNullOrWhiteSpace(text)) {
									string testFile = Path.Combine(folder, circuit.Name + "_TestBench.sv");
									File.WriteAllText(testFile, text);
									this.Message(Properties.Resources.MessageHdlSavingTestFile(testFile));
								}
							}
						} catch(Exception exception) {
							App.Mainframe.ReportException(exception);
						}
					});
				} else {
					this.Message(Properties.Resources.ErrorHdlInputTooBig(circuit.Name));
				}
			} else {
				this.Message(Properties.Resources.MessageInputOutputPinsMissing);
			}
		}
	}
}
