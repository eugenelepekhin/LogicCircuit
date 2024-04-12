using System.Diagnostics;
using Antlr4.Runtime;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlContext {
		private readonly Action<string> message;
		public string File { get; }
		public int ErrorCount { get; private set; }

		public HdlContext(string file, Action<string> message) {
			this.message = message;
			this.File = file;
		}

		public void Message(string message) {
			Debug.WriteLine(message);
			this.message(message);
		}

		public void Error(string message) {
			this.ErrorCount++;
			this.Message(message);
		}

		public void Parse() {
			this.ErrorCount = 0;
			HdlParser parser = this.Parser(this.File);
			HdlParser.ChipContext chipContext = parser.chip();
			if(this.ErrorCount == 0) {
				//string tree = chipContext.ToStringTree(parser);
				//this.Message(tree);
				HdlVisitor visitor = new HdlVisitor(this);
				HdlChip chip = (HdlChip)visitor.Visit(chipContext);
				string text = chip.ToString();
				this.Message(text);
			}
		}

		private HdlParser Parser(string file) {
			HdlErrorListner errorListner = new HdlErrorListner(this);
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
