// Ignore Spelling: Hdl

using Antlr4.Runtime;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlErrorListener : BaseErrorListener, IAntlrErrorListener<int> {
		private readonly HdlContext context;
		private readonly string file;

		public HdlErrorListener(HdlContext context, string file) {
			this.context = context;
			this.file = file;
		}

		public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
			this.context.Error(this.Message(line, charPositionInLine + 1, msg));
		}

		public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
			this.context.Error(this.Message(line, charPositionInLine + 1, msg));
		}

		private string Message(int line, int column, string message) => $"{this.file} ({line}, {column}): Syntax error: {message}";
	}
}
