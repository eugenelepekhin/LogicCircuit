using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogicCircuit {
	/// <summary>
	/// Nand to Tetris export.
	/// </summary>
	public class N2TExport : HdlExport {
		private static readonly Dictionary<string, string> N2TGateName = new Dictionary<string, string>() {
			{ "x", "in" },
			{ "x1", "a" },
			{ "x2", "b" },
			{ "q", "out" },
		};

		private readonly bool exportTests;

		public N2TExport(bool exportTests, bool commentPoints, Action<string> logMessage, Action<string> logError) : base(commentPoints, logMessage, logError) {
			this.exportTests = exportTests;
		}

		protected override HdlTransformation CreateTransformation(string name, IEnumerable<HdlSymbol> inputPins, IEnumerable<HdlSymbol> outputPins, IEnumerable<HdlSymbol> parts) {
			return new N2THdl(name, inputPins, outputPins, parts);
		}

		protected override bool PostExport(LogicalCircuit logicalCircuit, string folder) {
			if(this.exportTests) {
				this.ExportN2TTest(logicalCircuit, folder);
			}
			return base.PostExport(logicalCircuit, folder);
		}

		public override string Name(HdlSymbol symbol) {
			Circuit circuit = symbol.CircuitSymbol.Circuit;
			Debug.Assert(circuit is not Splitter && circuit is not CircuitProbe);
			if(circuit is Gate gate && gate.GateType == GateType.And && gate.InvertedOutput) {
				return "Nand";
			}
			return base.Name(symbol);
		}

		public override string Name(Jam jam) {
			string name = base.Name(jam);
			if(jam.CircuitSymbol.Circuit is Gate && N2TExport.N2TGateName.TryGetValue(name, out string? other)) {
				name = other;
			}

			return name;
		}

		private void ExportN2TTest(LogicalCircuit circuit, string folder) {
			CircuitTestSocket socket = new CircuitTestSocket(circuit);
			
			void reportProgress(double progress) => this.DispatchMessage($"Building truth table for {circuit.Name} {progress:f1}% done");

			ThreadPool.QueueUserWorkItem(o => {
				try {
					bool isTrancated = false;
					IList<TruthState>? table = socket.BuildTruthTable(reportProgress, () => true, null, DialogTruthTable.MaxRows, out isTrancated);
					if (table == null || isTrancated) {
						this.DispatchMessage("Failed to build truth table.");
					} else {
						this.ExportN2TTest(
							circuit.Name,
							socket.Inputs.Select(i => i.Pin.Name).ToList(),
							socket.Outputs.Select(o => o.Pin.Name).ToList(),
							folder,
							table
						);
					}
				} catch(Exception exception) {
					App.Mainframe.ReportException(exception);
				}
			});
		}

		private void ExportN2TTest(
			string circuitName, List<string> inputs, List<string> outputs, string folder, IList<TruthState> table
		) {
			string formatExpect(string text) {
				if(3 <= text.Length) {
					text = text.Substring(0, 3);
					return text;
				}
				int trail = 0;
				int lead = 0;
				switch(text.Length) {
				case 1:
					lead = 2;
					trail = 1;
					break;
				case 2:
					lead = 0;
					trail = 1;
					break;
				}
				string format = string.Format(CultureInfo.InvariantCulture, "{{0,{0}}}", lead);
				string result = string.Format(CultureInfo.InvariantCulture, format, text) + new string(' ', trail);
				return result;
			}
			StringBuilder expect = new StringBuilder();
			StringBuilder script = new StringBuilder();

			script.AppendLine(CultureInfo.InvariantCulture, $"load {circuitName + ".hdl"},");
			script.AppendLine(CultureInfo.InvariantCulture, $"output-file {circuitName + ".out"},");
			script.AppendLine(CultureInfo.InvariantCulture, $"compare-to {circuitName + ".cmp"},");
			script.Append("output-list");

			foreach(string field in inputs.Concat(outputs)) {
				script.Append(CultureInfo.InvariantCulture, $" {field}%X1.1.1");
				expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(field)}");
			}
			script.AppendLine(";");

			script.AppendLine();
			expect.AppendLine("|");

			foreach(TruthState state in table) {
				int index = 0;
				foreach(string input in inputs) {
					script.AppendLine(CultureInfo.InvariantCulture, $"set {input} {state.Input[index]},");
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state.Input[index].ToString(CultureInfo.InvariantCulture))}");
					index++;
				}
				script.AppendLine("eval,");
				script.AppendLine("output;");
				script.AppendLine();
				index = 0;
				foreach(string output in outputs) {
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state[index].ToString(CultureInfo.InvariantCulture))}");
					index++;
				}
				expect.AppendLine("|");
			}

			string testFile = Path.Combine(folder, circuitName + ".tst");
			File.WriteAllText(testFile, script.ToString());
			this.DispatchMessage($"Saving test file {testFile}");

			string cmpFile = Path.Combine(folder, circuitName + ".cmp");
			File.WriteAllText(cmpFile, expect.ToString());
			this.DispatchMessage($"Saving .cmp file {cmpFile}");
		}
	}
}
