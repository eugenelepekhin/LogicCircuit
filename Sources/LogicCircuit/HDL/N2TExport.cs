// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogicCircuit {
	/// <summary>
	/// Nand to Tetris export.
	/// Some useful links.
	/// Actual homepage of N2T: https://www.nand2tetris.org/
	/// N2T HDL documentation: https://drive.google.com/file/d/1dPj4XNby9iuAs-47U9k3xtYy9hJ-ET0T/view
	/// Same but in Verilog:
	/// https://github.com/jopdorp/nand2tetris-verilog
	/// </summary>
	internal class N2TExport : HdlExport {
		private readonly Dictionary<string, string> PinName = new Dictionary<string, string>() {
			{ "x", "in" },
			{ "x1", "a" },
			{ "x2", "b" },
			{ "q", "out" },
		};

		private readonly Regex identifier = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		private readonly HashSet<string> keywords = new HashSet<string>() { "CHIP", "PARTS", "IN", "OUT", "true", "false", };

		private CircuitProject? replacmentProject;

		public N2TExport(bool exportTests, bool commentPoints, Action<string> logMessage, Action<string> logError, Action<string> logWarning) : base(exportTests, commentPoints, logMessage, logError, logWarning) {
		}

		protected override HdlTransformation? CreateTransformation(string name, IList<HdlSymbol> inputPins, IList<HdlSymbol> outputPins, IList<HdlSymbol> parts) {
			if(this.FixBigGates(parts)) {
				return new N2THdl(name, inputPins, outputPins, parts);
			}
			return null;
		}
		protected override string FileName(LogicalCircuit circuit) => circuit.Name + ".hdl";

		public override bool CanExport(Circuit circuit) {
			return !(
				circuit is CircuitButton ||
				circuit is Sensor ||
				circuit is Gate gate && (gate.GateType == GateType.Clock || gate.GateType == GateType.Led || gate.GateType == GateType.TriState1 || gate.GateType == GateType.TriState2) ||
				circuit is LedMatrix ||
				circuit is Sound
			);
		}

		public override bool IsValid(string name) => this.identifier.IsMatch(name) && !this.keywords.Contains(name);

		protected override bool Validate(HdlTransformation transformation) {
			bool valid = base.Validate(transformation);
			foreach(HdlConnection connection in transformation.InputPins.SelectMany(p => p.HdlConnections())) {
				if(connection.ConnectsInputWithOutput()) {
					this.Error(Properties.Resources.ErrorHdlPinDirectConnect(connection.InHdlSymbol.CircuitSymbol.Circuit.Name, connection.OutHdlSymbol.CircuitSymbol.Circuit.Name));
					valid = false;
				}
			}
			foreach(HdlSymbol symbol in transformation.Parts) {
				if(symbol.CircuitSymbol.Circuit is Memory memory) {
					if(memory.DualPort) {
						this.Error(Properties.Resources.ErrorHdlMemoryDialPort(memory.Name, symbol.CircuitSymbol.Point));
						valid = false;
					}
					if(memory.Writable) {
						int[] address = [3, 6, 9, 12, 14];
						if(!address.Contains(memory.AddressBitWidth) || memory.DataBitWidth != 16) {
							this.Error(Properties.Resources.ErrorHdlRAMBitWidth(memory.Name, symbol.CircuitSymbol.Point));
							valid = false;
						}
						if(memory.WriteOn1 != false) {
							this.Error(Properties.Resources.ErrorHdlRAMWriteOn1(memory.Name, symbol.CircuitSymbol.Point));
							valid = false;
						}
						if(this.ExportTests) {
							this.Warning(Properties.Resources.WarningN2TRamTest(memory.Name, symbol.CircuitSymbol.Point));
						}
					} else {
						if(memory.AddressBitWidth != 15 || memory.DataBitWidth != 16) {
							this.Error(Properties.Resources.ErrorHdlROMBitWidth(memory.Name, symbol.CircuitSymbol.Point));
							valid = false;
						}
					}
				}
			}
			return valid;
		}

		public override string HdlName(HdlSymbol symbol) {
			Circuit circuit = symbol.CircuitSymbol.Circuit;
			Debug.Assert(circuit is not Splitter && circuit is not CircuitProbe);
			if(circuit is Gate gate) {
				switch(gate.GateType) {
				case GateType.And:	return gate.InvertedOutput ? "Nand" : "And";
				case GateType.Not:	return "Not";
				case GateType.Or:	return gate.InvertedOutput ? throw new InvalidOperationException() : "Or";
				case GateType.Xor:	return gate.InvertedOutput ? throw new InvalidOperationException() : "Xor";
				default:
					throw new InvalidOperationException();
				}
			}
			if(circuit is Memory memory) {
				if(memory.Writable) {
					switch(memory.AddressBitWidth) {
					case 3:		return "RAM8";
					case 6:		return "RAM64";
					case 9:		return "RAM512";
					case 12:	return "RAM4K";
					case 14:	return "RAM16K";
					default:	return "RAM";
					}
				} else {
					return "ROM32K";
				}
			}
			return circuit.Name.Trim();
		}

		public override string HdlName(Jam jam) {
			string name = base.HdlName(jam);
			if(jam.CircuitSymbol.Circuit is Gate && this.PinName.TryGetValue(name, out string? other)) {
				return other;
			} else if(jam.CircuitSymbol.Circuit is Memory memory) {
				BasePin pin = jam.Pin;
				if(pin == memory.AddressPin) return "address";
				if(pin == memory.DataOutPin) return "out";
				if(pin == memory.DataInPin)  return "in";
				if(pin == memory.WritePin)   return "load";
			}

			return name;
		}

		private bool FixBigGates(IList<HdlSymbol> parts) {
			bool success = true;
			bool NeedReplacement(HdlSymbol symbol) {
				if(symbol.CircuitSymbol.Circuit is Gate gate) {
					HashSet<Jam> inputs = new HashSet<Jam>(symbol.HdlConnections().Where(c => c.InHdlSymbol == symbol).Select(c => c.InJam));
					if(inputs.Count <= 1 && 1 < gate.InputCount) {
						success = false;
						this.Error(Properties.Resources.ErrorHdlUnconnectedGate(gate.Name, symbol.CircuitSymbol.LogicalCircuit.Name));
						return false;
					}
					return 2 < gate.InputCount || gate.InvertedOutput && (gate.GateType == GateType.Or || gate.GateType == GateType.Xor);
				}
				return false;
			}

			for(int i = 0; i < parts.Count; i++) {
				HdlSymbol symbol = parts[i];
				if(NeedReplacement(symbol)) {
					parts.RemoveAt(i);
					List<HdlSymbol> replacement = this.Replace(symbol);
					for(int j = 0; j < replacement.Count; j++) {
						parts.Insert(i + j, replacement[j]);
					}
					i += replacement.Count - 1;
				}
			}
			return success;
		}

		private List<HdlSymbol> Replace(HdlSymbol symbol) {
			Debug.Assert(symbol.CircuitSymbol.Circuit is Gate);
			Jam OutputJam(HdlSymbol hdlSymbol) => hdlSymbol.CircuitSymbol.Jams().First(j => j.Pin.PinType == PinType.Output);
			HdlSymbol? gate = null;
			List<Jam> newInputs = new List<Jam>();
			List<HdlSymbol> replacement = new List<HdlSymbol>();
			void AddGate(bool last) {
				gate = this.ReplaceSymbol(symbol, last);
				newInputs.Clear();
				newInputs.AddRange(gate.CircuitSymbol.Jams().Where(j => j.Pin.PinType == PinType.Input));
				replacement.Add(gate);
				gate.Subindex = replacement.Count;
			}

			List<HdlConnection> symbolOutputs = new List<HdlConnection>();
			OneToMany<Jam, HdlConnection> symbolInputs = new OneToMany<Jam, HdlConnection>();
			foreach(HdlConnection connection in symbol.HdlConnections()) {
				if(connection.OutHdlSymbol == symbol) {
					Debug.Assert(connection.OutJam.CircuitSymbol == symbol.CircuitSymbol && symbolOutputs.All(c => c.OutJam == connection.OutJam));
					symbolOutputs.Add(connection);
				} else {
					Debug.Assert(connection.InJam.CircuitSymbol == symbol.CircuitSymbol);
					symbolInputs.Add(connection.InJam, connection);
				}
			}
			int inputIndex = 0;
			foreach(Jam oldInput in symbolInputs.Keys) {
				if((inputIndex & 1) == 0) {
					AddGate(symbolInputs.Count <= 3 && (0 < inputIndex || symbolInputs.Count <= 2));
				} else {
					gate = replacement[replacement.Count - 1];
				}
				Debug.Assert(gate != null);
				foreach(HdlConnection oldConnection in symbolInputs[oldInput]) {
					HdlConnection newConnection = oldConnection.CreateCopy(oldConnection.OutHdlSymbol, oldConnection.OutJam, gate, newInputs[inputIndex & 1]);
					oldConnection.OutHdlSymbol.Replace(oldConnection, newConnection);
					gate.Add(newConnection);
				}
				inputIndex++;
			}
			Debug.Assert(gate != null);
			int waveStart = inputIndex & 1;
			if(0 < waveStart) {
				Debug.Assert(1 < replacement.Count);
				HdlConnection.Create(replacement[0], replacement[replacement.Count - 1], new Connection(newInputs[1], OutputJam(replacement[0])));
			}
			int waveEnd = replacement.Count;
			while(1 < waveEnd - waveStart) {
				for(int i = 0; i < (waveEnd - waveStart) / 2; i++) {
					AddGate(waveEnd - waveStart == 2);
					HdlSymbol out1 = replacement[waveStart + i * 2];
					HdlSymbol out2 = replacement[waveStart + i * 2 + 1];
					HdlConnection.Create(out1, gate, new Connection(newInputs[0], OutputJam(out1)));
					HdlConnection.Create(out2, gate, new Connection(newInputs[1], OutputJam(out2)));
				}
				if(((waveEnd - waveStart) & 1) != 0) {
					AddGate(waveEnd - waveStart == 2);
					HdlSymbol out1 = replacement[waveEnd - 1];
					HdlSymbol out2 = replacement[waveEnd];
					waveEnd++;
					HdlConnection.Create(out1, gate, new Connection(newInputs[0], OutputJam(out1)));
					HdlConnection.Create(out2, gate, new Connection(newInputs[1], OutputJam(out2)));
				}
				waveStart = waveEnd;
				waveEnd = replacement.Count;
			}
			HdlSymbol last = replacement[replacement.Count - 1];
			if(((Gate)(last.CircuitSymbol.Circuit)).InvertedOutput != ((Gate)symbol.CircuitSymbol.Circuit).InvertedOutput) {
				Debug.Assert(symbol.CircuitSymbol.Circuit is Gate g && g.InvertedOutput && g.GateType != GateType.And && g.GateType != GateType.Not);
				HdlSymbol not = this.CreateSymbol(GateType.Not, true, symbol);
				replacement.Add(not);
				HdlConnection.Create(last, not, new Connection(not.CircuitSymbol.Jams().First(j => j.Pin.PinType == PinType.Input), OutputJam(last)));
				last = not;
			}
			foreach(HdlConnection connection in symbolOutputs) {
				HdlConnection connectionReplacement = connection.CreateCopy(last, OutputJam(last), connection.InHdlSymbol, connection.InJam);
				connection.InHdlSymbol.Replace(connection, connectionReplacement);
				last.Add(connectionReplacement);
			}
			last.Subindex = 0;
			replacement.ForEach(s => s.SortConnections());

			return replacement;
		}

		private HdlSymbol ReplaceSymbol(HdlSymbol symbol, bool final) {
			Gate gate = (Gate)symbol.CircuitSymbol.Circuit;
			switch(gate.GateType) {
			case GateType.And:
				break;
			case GateType.Or:
			case GateType.Xor:
				final = false; // Nand to Tetris doesn't have Nor and NXor, so prevent it to be inverted.
				break;
			default:
				throw new InvalidProgramException();
			}
			return this.CreateSymbol(gate.GateType, gate.InvertedOutput && final, symbol);
		}

		private HdlSymbol CreateSymbol(GateType gateType, bool inverted, HdlSymbol hdlSymbol) {
			if(this.replacmentProject == null) {
				this.replacmentProject = CircuitProject.Create(null);
				this.replacmentProject.StartTransaction();
			}

			Gate gate = this.replacmentProject.GateSet.Gate(gateType, (gateType == GateType.Not) ? 1 : 2, inverted);

			int dy = (((Gate)hdlSymbol.CircuitSymbol.Circuit).InputCount + 1) / 2 - 2;

			CircuitSymbol symbol = this.replacmentProject.CircuitSymbolSet.Create(gate, this.replacmentProject.ProjectSet.Project.LogicalCircuit, hdlSymbol.CircuitSymbol.X, hdlSymbol.CircuitSymbol.Y + dy);
			symbol.Rotation = hdlSymbol.CircuitSymbol.Rotation;
			return new HdlSymbol(this, symbol) {
				AutoGenerated = true,
				Comment = hdlSymbol.Comment
			};
		}

		protected override void ExportTest(
			string circuitName, List<InputPinSocket> inputs, List<OutputPinSocket> outputs, IList<TruthState> table, string folder
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

			List<string> inputNames = inputs.Select(i => i.Pin.Name).ToList();
			List<string> outputNames = outputs.Select(o => o.Pin.Name).ToList();

			foreach(string field in inputNames.Concat(outputNames)) {
				script.Append(CultureInfo.InvariantCulture, $" {field}%X1.1.1");
				expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(field)}");
			}
			script.AppendLine(";");

			script.AppendLine();
			expect.AppendLine("|");

			foreach(TruthState state in table) {
				int index = 0;
				foreach(string input in inputNames) {
					script.AppendLine(CultureInfo.InvariantCulture, $"set {input} {state.Input[index]},");
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state.Input[index].ToString("x", CultureInfo.InvariantCulture))}");
					index++;
				}
				script.AppendLine("eval,");
				script.AppendLine("output;");
				script.AppendLine();
				index = 0;
				foreach(string output in outputNames) {
					expect.Append(CultureInfo.InvariantCulture, $"|{formatExpect(state.Output[index].ToString("x", CultureInfo.InvariantCulture))}");
					index++;
				}
				expect.AppendLine("|");
			}

			string testFile = Path.Combine(folder, circuitName + ".tst");
			File.WriteAllText(testFile, script.ToString());
			this.Message(Properties.Resources.MessageHdlSavingTestFile(testFile));

			string cmpFile = Path.Combine(folder, circuitName + ".cmp");
			File.WriteAllText(cmpFile, expect.ToString());
			this.Message(Properties.Resources.MessageHdlSavingCmpFile(cmpFile));
		}
	}
}
