// Ignore Spelling: Hdl

using System.Diagnostics;
using Antlr4.Runtime;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlContext {
		public string Folder { get; }
		private readonly Action<string> message;
		public int ErrorCount { get; private set; }

		private readonly Dictionary<string, HdlChip> chips = new(StringComparer.OrdinalIgnoreCase);

		public HdlContext(string folder, Action<string> message) {
			Debug.Assert(folder != null && Directory.Exists(folder));
			this.Folder = folder;
			this.message = message;
		}

		public void Message(string message) {
			Debug.WriteLine(message);
			this.message(message);
		}

		public void Error(string message) {
			this.ErrorCount++;
			this.Message(message);
		}

		public HdlState Load(string chipName) {
			Debug.Assert(0 == this.chips.Count);

			HdlChip chip = this.Chip(chipName);
			if(chip.Link() && !this.HasLoop(chip, chip)) {
				return new HdlState(this, chip);
			}
			return null;
		}

		public HdlChip Chip(string chipName) {
			HdlChip chip;
			if(this.chips.TryGetValue(chipName, out chip)) {
				return chip;
			}
			string file = Path.Combine(this.Folder, chipName + ".hdl");
			if(File.Exists(file)) {
				chip = this.Parse(file);
				if(chip != null) {
					if(!StringComparer.OrdinalIgnoreCase.Equals(chip.Name, chipName)) {
						this.Error($"File name {file} doesn't match contained chip name {chip.Name}.");
					} else {
						this.chips.Add(chip.Name, chip);
						return chip;
					}
				}
				return null;
			}
			HdlChip gate = HdlGate.CreateGate(this, chipName);
			if(gate != null) {
				this.chips.Add(gate.Name, gate);
			} else {
				this.Error($"Chip {chipName} not found");
			}
			return gate;
		}

		// TODO: this is wrong rewrite it.
		private bool HasLoop(HdlChip chip, HdlChip root) {
			foreach(HdlPart part in chip.Parts) {
				if(part.Chip == root) {
					this.Error($"Chip {root.Name} is using itself directly or indirectly.");
					return true;
				}
				if(this.HasLoop(part.Chip, root) || this.HasLoop(part.Chip, part.Chip)) {
					return true;
				}
			}
			return false;
		}

		private HdlChip Parse(string file) {
			Debug.Assert(File.Exists(file));
			HdlParser parser = this.Parser(file);
			HdlParser.ChipContext chipContext = parser.chip();
			if(this.ErrorCount == 0) {
				//this.Message(chipContext.ToStringTree(parser));
				HdlVisitor visitor = new HdlVisitor(this);
				if(visitor.Visit(chipContext) && this.ErrorCount == 0) {
					//this.Message(visitor.Chip.ToString());
					return visitor.Chip;
				}
			}
			return null;
		}

		private HdlParser Parser(string file) {
			HdlErrorListener errorListner = new HdlErrorListener(this, file);
			using TextReader reader = new StreamReader(file);
			// AntlrInputStream will read the entire file here, so reader is safe to dispose.
			HdlLexer lexer = new HdlLexer(new AntlrInputStream(reader));
			lexer.RemoveErrorListeners();
			lexer.AddErrorListener(errorListner);
			CommonTokenStream tokenStream = new CommonTokenStream(lexer);
			HdlParser parser = new HdlParser(tokenStream);
			parser.RemoveErrorListeners();
			parser.AddErrorListener(errorListner);
			return parser;
		}
	}
}
