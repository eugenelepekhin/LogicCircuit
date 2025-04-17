// Ignore Spelling: Paren lexer

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace LogicCircuit {
	public class ExpressionParser {
		private class ErrorStrategy : DefaultErrorStrategy {
			protected override void ReportInputMismatch(Parser recognizer, InputMismatchException e) {
				//string message = "mismatched input " + this.GetTokenErrorDisplay(e.OffendingToken) + " expecting " + e.GetExpectedTokens().ToString(recognizer.Vocabulary);
				string tokenErrorDisplay = this.GetTokenErrorDisplay(e.OffendingToken);
				string message = Properties.Resources.ParserErrorUnexpected(tokenErrorDisplay);
				this.NotifyErrorListeners(recognizer, message, e);
			}

			protected override void ReportMissingToken(Parser recognizer) {
				if(!this.InErrorRecoveryMode(recognizer)) {
					this.BeginErrorCondition(recognizer);
					IToken currentToken = recognizer.CurrentToken;
					IntervalSet expectedTokens = this.GetExpectedTokens(recognizer);
					//string message = "missing " + expectedTokens.ToString(recognizer.Vocabulary) + " at " + this.GetTokenErrorDisplay(currentToken);
					string message = Properties.Resources.ParserErrorMissing(expectedTokens.ToString(recognizer.Vocabulary), this.GetTokenErrorDisplay(currentToken));
					recognizer.NotifyErrorListeners(currentToken, message, null);
				}
			}

			protected override void ReportNoViableAlternative(Parser recognizer, NoViableAltException e) {
				ITokenStream tokenStream = (ITokenStream)recognizer.InputStream;
				//string s = ((tokenStream == null) ? "<unknown input>" : ((e.StartToken.Type != -1) ? tokenStream.GetText(e.StartToken, e.OffendingToken) : "<EOF>"));
				//string message = "no viable alternative at input " + this.EscapeWSAndQuote(s);
				string tokenErrorDisplay = this.GetTokenErrorDisplay(e.OffendingToken);
				string message = Properties.Resources.ParserErrorUnexpected(tokenErrorDisplay);
				this.NotifyErrorListeners(recognizer, message, e);
			}

			protected override void ReportUnwantedToken(Parser recognizer) {
				if(!this.InErrorRecoveryMode(recognizer)) {
					this.BeginErrorCondition(recognizer);
					IToken currentToken = recognizer.CurrentToken;
					string tokenErrorDisplay = this.GetTokenErrorDisplay(currentToken);
					IntervalSet expectedTokens = this.GetExpectedTokens(recognizer);
					//string message = "extraneous input " + tokenErrorDisplay + " expecting " + expectedTokens.ToString(recognizer.Vocabulary);
					string message = Properties.Resources.ParserErrorUnexpected(tokenErrorDisplay);
					recognizer.NotifyErrorListeners(currentToken, message, null);
				}
			}

			protected override void ReportFailedPredicate(Parser recognizer, FailedPredicateException e) {
				string text = recognizer.RuleNames[recognizer.RuleContext.RuleIndex];
				string message = "rule " + text + " " + e.Message;
				NotifyErrorListeners(recognizer, message, e);
			}
		}

		private sealed class ErrorListener : BaseErrorListener, IAntlrErrorListener<int> {
			public int ErrorCount { get; private set; }

			private readonly StringBuilder buffer = new StringBuilder();
			public string Errors => this.buffer.ToString();

			private void AddError(string message) {
				Debug.WriteLine(message);
				this.buffer.AppendLine(message);
				this.ErrorCount++;
			}

			private void AddSyntaxError(string message) {
				this.AddError($"Syntax error: {message}");
			}

			// parser error
			public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
				this.AddSyntaxError(msg);
			}

			// lexer error
			public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
				this.AddSyntaxError(msg);
			}

			public void Error(string msg) {
				this.AddError($"Error: {msg}");
			}
		}

		private sealed class ExprVisitor : TruthTableFilterParserBaseVisitor<Expression?> {
			private readonly CircuitTestSocket socket;
			private readonly ErrorListener errorListener;
			private readonly ParameterExpression stateParameter;

			public ExprVisitor(CircuitTestSocket socket, ErrorListener errorListener, ParameterExpression stateParameter) {
				this.socket = socket;
				this.errorListener = errorListener;
				this.stateParameter = stateParameter;
			}

			public override Expression? VisitFilter(TruthTableFilterParser.FilterContext context) {
				return this.Visit(context.expr());
			}

			public override Expression? VisitParenExpr(TruthTableFilterParser.ParenExprContext context) {
				return this.Visit(context.expr());
			}

			public override Expression? VisitUnary(TruthTableFilterParser.UnaryContext context) {
				string op = context.Start.Text;
				Expression? expression = this.Visit(context.expr());
				if(expression != null) {
					switch(op) {
					case "+": return expression;
					case "-": return Expression.Negate(expression);
					case "!": return Expression.Condition(Expression.Equal(expression, Expression.Constant(0)), Expression.Constant(1), Expression.Constant(0));
					case "~": return Expression.Not(expression);
					}
					throw new InvalidOperationException();
				}
				return expression;
			}

			public override Expression? VisitBin(TruthTableFilterParser.BinContext context) {
				Expression fromBool(Expression expression) => Expression.Condition(expression, Expression.Constant(1), Expression.Constant(0));
				Expression? left = this.Visit(context.left);
				string op = context.op.Text;
				Expression? right = this.Visit(context.right);
				if((left != null && right != null)) {
					switch(op) {
					case "*":	return Expression.Multiply(left, right);
					case "/":	return Expression.Divide(left, right);
					case "%":	return Expression.Modulo(left, right);
					case "+":	return Expression.Add(left, right);
					case "-":	return Expression.Subtract(left, right);
					case "<<":	return Expression.LeftShift(left, right);
					case ">>":	return Expression.RightShift(left, right);
					case "<":	return fromBool(Expression.LessThan(left, right));
					case "<=":	return fromBool(Expression.LessThanOrEqual(left, right));
					case ">=":	return fromBool(Expression.GreaterThanOrEqual(left, right));
					case ">":	return fromBool(Expression.GreaterThan(left, right));
					case "=":
					case "==":	return fromBool(Expression.Equal(left, right));
					case "<>":
					case "!=":	return fromBool(Expression.NotEqual(left, right));
					case "&":	return Expression.And(left, right);
					case "^":	return Expression.ExclusiveOr(left, right);
					case "|":	return Expression.Or(left, right);
					case "&&":	return fromBool(Expression.And(Expression.NotEqual(left, Expression.Constant(0)), Expression.NotEqual(right, Expression.Constant(0))));
					case "||":	return fromBool(Expression.Or(Expression.NotEqual(left, Expression.Constant(0)), Expression.NotEqual(right, Expression.Constant(0))));
					}
					throw new InvalidOperationException();
				}
				return null;
			}

			public override Expression? VisitLiteral(TruthTableFilterParser.LiteralContext context) {
				string token = context.GetText().Replace("_", "", StringComparison.Ordinal).Replace("'", "", StringComparison.Ordinal);
				string text = token.Replace("_", "", StringComparison.Ordinal).Replace("'", "", StringComparison.Ordinal);
				int value = 0;
				if(text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
					if(!int.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
						this.errorListener.Error(Properties.Resources.ParserErrorInvalidNumber(token));
					}
				} else if(text.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) {
					if(!int.TryParse(text.AsSpan(2), NumberStyles.BinaryNumber, CultureInfo.InvariantCulture, out value)) {
						this.errorListener.Error(Properties.Resources.ParserErrorInvalidNumber(token));
					}
				} else if(text.StartsWith('0')) {
					if(!ExprVisitor.TryParseOctal(text, out value)) {
						this.errorListener.Error(Properties.Resources.ParserErrorInvalidNumber(token));
					}
				} else {
					if(!int.TryParse(text, out value)) {
						this.errorListener.Error(Properties.Resources.ParserErrorInvalidNumber(token));
					}
				}
				return Expression.Constant(value);
			}

			public override Expression? VisitVariable(TruthTableFilterParser.VariableContext context) {
				string token = context.GetText();
				string name = token.StartsWith('"') ? token.Substring(1, token.Length - 2).Replace("\\\"", "\"", StringComparison.Ordinal) : token;
				int index = 0;
				foreach(InputPinSocket pin in this.socket.Inputs) {
					if(StringComparer.Ordinal.Equals(pin.Pin.Name, name)) {
						return Expression.MakeIndex(
							Expression.Property(
								this.stateParameter,
								typeof(TruthState).GetProperty(nameof(TruthState.Input))!
							),
							typeof(int[]).GetProperty("Item"),
							[Expression.Constant(index)]
						);
					}
					index++;
				}
				index = 0;
				foreach(OutputPinSocket pin in this.socket.Outputs) {
					if(StringComparer.Ordinal.Equals(pin.Pin.Name, name)) {
						return Expression.MakeIndex(
							Expression.Property(
								this.stateParameter,
								typeof(TruthState).GetProperty(nameof(TruthState.Output))!
							),
							typeof(int[]).GetProperty("Item"),
							[Expression.Constant(index)]
						);
					}
					index++;
				}
				this.errorListener.Error(Properties.Resources.ParserErrorUnknownPin(name));
				return null;
			}

			private static bool TryParseOctal(string text, out int result) {
				long value = 0;
				foreach(char c in text) {
					value <<= 3;
					Debug.Assert('0' <= c && c <= '7');
					value |= (ushort)(c - '0');
					if(int.MaxValue < value) {
						result = 0;
						return false;
					}
				}
				Debug.Assert(0 <= value && value <= int.MaxValue);
				result = (int)value;
				return true;
			}
		}

		private readonly CircuitTestSocket socket;
		public int ErrorCount { get; private set; }
		public string ErrorText { get; private set; } = string.Empty;

		public ExpressionParser(CircuitTestSocket socket) {
			this.socket = socket;
		}

		private Expression? ParseExpression(string text, ParameterExpression stateParameter) {
			ErrorListener errorListener = new ErrorListener();
			TruthTableFilterLexer lexer = new TruthTableFilterLexer(new AntlrInputStream(text));
			lexer.RemoveErrorListeners();
			lexer.AddErrorListener(errorListener);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			TruthTableFilterParser parser = new TruthTableFilterParser(tokens);
			parser.ErrorHandler = new ErrorStrategy();
			parser.RemoveErrorListeners();
			parser.AddErrorListener(errorListener);

			TruthTableFilterParser.FilterContext? filter = parser.filter();
			this.ErrorCount = errorListener.ErrorCount;
			this.ErrorText = errorListener.Errors;
			if(this.ErrorCount == 0 && filter != null) {
				#if DEBUG && !true
					Debug.WriteLine("Expression:");
					Debug.WriteLine(text);
					Debug.WriteLine("Parse tree:");
					Debug.WriteLine(filter.ToStringTree(parser));
				#endif

				ExprVisitor visitor = new ExprVisitor(this.socket, errorListener, stateParameter);
				Expression? expr = visitor.VisitFilter(filter);
				this.ErrorCount = errorListener.ErrorCount;
				this.ErrorText = errorListener.Errors;
				if(this.ErrorCount == 0) {
					return expr;
				}
			}
			return null;
		}

		public Func<TruthState, int>? Parse(string text) {
			ParameterExpression stateParameter = Expression.Parameter(typeof(TruthState), "state");
			Expression? body = this.ParseExpression(text, stateParameter);
			if(body != null) {
				Expression<Func<TruthState, int>> lambda = Expression.Lambda<Func<TruthState, int>>(
					body,
					stateParameter
				);
				return lambda.Compile();
			}
			return null;
		}

		public Predicate<TruthState>? Parse(string text, bool inverted) {
			Func<TruthState, int>? body = this.Parse(text);
			if(body != null) {
				if(inverted) {
					//return truthState => { int result = body(truthState); Debug.WriteLine($"Expression evaluated to !{result}"); return 0 == result;};
					return truthState => 0 == body(truthState);
				} else {
					//return truthState => { int result = body(truthState); Debug.WriteLine($"Expression evaluated to {result}"); return 0 != result;};
					return truthState => 0 != body(truthState);
				}
			}
			return null;
		}
	}
}
