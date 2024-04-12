using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlErrorListner : BaseErrorListener, IAntlrErrorListener<int> {
		private readonly HdlContext context;
		public HdlErrorListner(HdlContext context) {
			this.context = context;
		}

		public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e) {
			this.context.Error(this.Message(line, charPositionInLine + 1, msg));
		}
		public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e) {
			this.context.Error(this.Message(line, charPositionInLine + 1, msg));
		}

		private string Message(int line, int column, string message) => $"{this.context.File} ({line}, {column}): Syntax error: {message}";
	}
}
