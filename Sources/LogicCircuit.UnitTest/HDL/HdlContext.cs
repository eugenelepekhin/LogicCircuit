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

		public HdlChip Chip(string chipName) {
			if(this.chips.TryGetValue(chipName, out HdlChip chip)) {
				return chip;
			}
			string file = chipName + ".hdl";
			if(File.Exists(Path.Combine(this.Folder, file)) && this.Parse(file)) {
				return this.chips[chipName];
			}
			HdlChip gate = HdlGate.CreateGate(this, chipName);
			if(gate != null) {
				this.chips.TryAdd(gate.Name, gate);
			} else {
				this.Error($"Chip {chipName} not found");
			}
			return gate;
		}

		private bool Parse(string file) {
			string path = Path.Combine(this.Folder, file);
			if(File.Exists(path)) {
				HdlParser parser = this.Parser(path);
				HdlParser.ChipContext chipContext = parser.chip();
				if(this.ErrorCount == 0) {
					//string tree = chipContext.ToStringTree(parser);
					//this.Message(tree);
					HdlVisitor visitor = new HdlVisitor(this);
					HdlChip chip = (HdlChip)visitor.Visit(chipContext);
					string text = chip.ToString();
					this.Message(text);
					this.chips.Add(chip.Name, chip);
					return true;
				}
			} else {
				this.Error($"File not found: {path}");
			}
			return false;
		}

		private HdlParser Parser(string file) {
			HdlErrorListener errorListner = new HdlErrorListener(this, file);
			using TextReader reader = new StreamReader(file);
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
