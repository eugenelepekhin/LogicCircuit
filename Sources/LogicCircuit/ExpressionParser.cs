using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LogicCircuit {
	/// <summary>
	/// Parse expressions over input and output pins.
	/// </summary>
	public class ExpressionParser {

		// BNF:
		// Expression ::= LogicalOr
		// LogicalOr ::= LogicalAnd || LogicalAnd
		// LogicalAnd ::= Comparison && Comparison
		// Comparison ::= Addition CMP Addition
		// CMP ::= = | == | != | <> | < | <= | >= | >
		// Addition ::= Multiplication ADD Multiplication
		// ADD ::= + | -
		// Multiplication ::= Conjunction MUL Conjunction
		// MUL ::= * | / | %
		// Conjunction ::= Disjunction CON Disjunction
		// CON ::= ^ | '|'
		// Disjunction ::= Shift DIS Shift
		// DIS ::= &
		// Shift ::= Primary SHT Primary
		// SHT ::= << | >>
		// Primary ::= ( Expression ) | - Primary | ~ Primary | ! Primary | ID | Literal
		// ID ::= SimpleId | QuotedId
		// SimpleId ::= Letter | Letter LettersOrDigids
		// LettersOrDigids ::= LettersOrDigids | Letter | Digit
		// QuotedId ::= " (ESC | [~"\])* "
		// ESC ::= \\ | \" | \.

		private enum TokenType {
			IntBin,
			IntOct,
			IntDec,
			IntHex,
			Id,
			Open,
			Close,
			Binary,
			Unary,
			EOS,
		}

		private struct Token {
			public string Value;
			public TokenType TokenType;

			public static Token Eos() {
				return new Token() { Value = Properties.Resources.ParserEOS, TokenType = TokenType.EOS };
			}

			public bool Is(string value) {
				return this.Value == value;
			}
			public bool Is(string value1, string value2) {
				return this.Value == value1 || this.Value == value2;
			}
			public bool Is(string value1, string value2, string value3) {
				return this.Value == value1 || this.Value == value2 || this.Value == value3;
			}
		}

		private readonly ParameterExpression stateParameter = Expression.Parameter(typeof(TruthState), "state");

		private CircuitTestSocket socket;
		private StringReader reader;
		private StringBuilder buffer = new StringBuilder();
		private Token current;

		private string error;
		public string Error {
			get { return this.error; }
			private set {
				if(value == null || this.error == null) {
					this.error = value;
				}
			}
		}

		public ExpressionParser(CircuitTestSocket socket) {
			this.socket = socket;
		}

		public Func<TruthState, int> Parse(string text) {
			Expression body = this.ParseExpression(text);
			if(body == null) {
				return null;
			}
			Expression<Func<TruthState, int>> lambda = Expression.Lambda<Func<TruthState, int>>(
				body,
				this.stateParameter
			);
			return lambda.Compile();
		}

		public Predicate<TruthState> Parse(string text, bool inverted) {
			Expression body = this.ParseExpression(text);
			if(body == null) {
				return null;
			}
			Expression<Predicate<TruthState>> lambda = Expression.Lambda<Predicate<TruthState>>(
				inverted ? Expression.Equal(body, Expression.Constant(0)) : Expression.NotEqual(body, Expression.Constant(0)),
				this.stateParameter
			);
			return lambda.Compile();
		}

		private Expression ParseExpression(string text) {
			this.Error = null;
			this.current = new Token();
			Expression body = null;
			using(this.reader = new StringReader(text)) {
				body = this.CircuitExpression();
				if(body == null) {
					Tracer.Assert(this.Error != null);
					return null;
				}
				if(this.Current().TokenType != TokenType.EOS) {
					this.ErrorUnexpected(this.Current());
					return null;
				}
			}
			return body;
		}

		private Expression ErrorUnexpected(Token token) {
			this.Error = Properties.Resources.ParserErrorUnexpected(token.Value);
			return null;
		}

		private Token UnclosedQuote(string text) {
			this.Error = Properties.Resources.ParserErrorUnclosedQuote(text);
			return Token.Eos();
		}

		private Expression ExprMissing(Token after) {
			this.Error = Properties.Resources.ParserErrorExpressionMissing(after.Value);
			return null;
		}

		private Token Next() {
			this.current = this.NextToken();
			return this.current;
		}

		private Token Current() {
			if(this.current.Value == null) {
				return this.Next();
			}
			return this.current;
		}

		private Token NextToken() {
			while(char.IsWhiteSpace((char)this.reader.Peek())) {
				this.reader.Read();
			}
			if(this.reader.Peek() == -1) {
				return Token.Eos();
			}
			char c = (char)this.reader.Peek();
			if(char.IsLetter(c)) {
				return this.NextId();
			}
			if(c == '"') {
				return this.NextQuote();
			}
			if(char.IsDigit(c)) {
				return this.NextNumber();
			}
			if("()+-*/%&|^~!=<>".Contains(c)) {
				return this.NextOperator();
			}
			this.Error = Properties.Resources.ParserErrorUnknownChar(c);
			return Token.Eos();
		}

		private Token NextId() {
			this.buffer.Length = 0;
			do {
				this.buffer.Append((char)this.reader.Read());
			} while(this.reader.Peek() != -1 && char.IsLetterOrDigit((char)this.reader.Peek()));
			return new Token() { Value = this.buffer.ToString(), TokenType = TokenType.Id };
		}

		private Token NextQuote() {
			this.buffer.Length = 0;
			int quote = this.reader.Read();
			int next = this.reader.Read();
			while(next != quote) {
				if(next == -1) {
					return this.UnclosedQuote(this.buffer.ToString().Trim());
				}
				char c = (char)next;
				if(c == '\\') {
					next = this.reader.Read();
					if(next == -1) {
						return this.UnclosedQuote(this.buffer.ToString().Trim());
					}
					c = (char)next;
				}
				this.buffer.Append(c);
				next = this.reader.Read();
			}
			return new Token() { Value = this.buffer.ToString().Trim(), TokenType = TokenType.Id };
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private Token NextNumber() {
			this.buffer.Length = 0;
			int maxLength = 10;
			Predicate<char> isValid = c => char.IsDigit(c);
			TokenType tokenType = TokenType.IntDec;
			int next = this.reader.Peek();
			if(next == '0') {
				this.reader.Read();
				switch(this.reader.Peek()) {
				case 'x':
				case 'X':
					maxLength = 8;
					isValid = c => char.IsDigit(c) || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F');
					this.reader.Read();
					next = this.reader.Peek();
					tokenType = TokenType.IntHex;
					break;
				case 'b':
				case 'B':
					maxLength = 32;
					isValid = c => '0' == c || '1' == c;
					this.reader.Read();
					next = this.reader.Peek();
					tokenType = TokenType.IntBin;
					break;
				case -1:
					this.buffer.Append('0');
					break;
				default:
					maxLength = 11;
					isValid = c => '0' <= c && c <= '7';
					next = reader.Peek();
					if(!isValid((char)next)) {
						this.buffer.Append('0');
						tokenType = TokenType.IntDec;
					} else {
						tokenType = TokenType.IntOct;
					}
					break;
				}
			}
			while(next != -1 && isValid((char)next)) {
				this.buffer.Append((char)next);
				this.reader.Read();
				next = this.reader.Peek();
			}
			if(maxLength < this.buffer.Length || this.buffer.Length < 1) {
				this.Error = Properties.Resources.ParserErrorInvalidNumber(this.buffer.ToString());
				return Token.Eos();
			}
			return new Token() { Value = this.buffer.ToString(), TokenType = tokenType };
		}

		private Token NextOperator() {
			this.buffer.Length = 0;
			TokenType tokenType = TokenType.Binary;
			int c = this.reader.Peek();
			switch(c) {
			case '(': return new Token() { Value = new string((char)this.reader.Read(), 1), TokenType = TokenType.Open };
			case ')': return new Token() { Value = new string((char)this.reader.Read(), 1), TokenType = TokenType.Close };
			case '+':
			case '-':
			case '*':
			case '/':
			case '%':
				this.buffer.Append((char)this.reader.Read());
				break;
			case '~':
				this.buffer.Append((char)this.reader.Read());
				tokenType = TokenType.Unary;
				break;
			case '&':
			case '|':
			case '^':
				this.buffer.Append((char)this.reader.Read());
				if(this.reader.Peek() == c) {
					this.buffer.Append((char)this.reader.Read());
				}
				break;
			case '!':
				this.buffer.Append((char)this.reader.Read());
				if(this.reader.Peek() == '=') {
					this.buffer.Append((char)this.reader.Read());
				} else {
					tokenType = TokenType.Unary;
				}
				break;
			case '=':
				this.buffer.Append((char)this.reader.Read());
				if(this.reader.Peek() == '=') {
					this.reader.Read();
				}
				break;
			case '<':
				this.buffer.Append((char)this.reader.Read());
				switch(this.reader.Peek()) {
				case '=':
				case '>':
				case '<':
					this.buffer.Append((char)this.reader.Read());
					break;
				}
				break;
			case '>':
				this.buffer.Append((char)this.reader.Read());
				switch(this.reader.Peek()) {
				case '=':
				case '>':
					this.buffer.Append((char)this.reader.Read());
					break;
				}
				break;
			}
			return new Token() { Value = this.buffer.ToString(), TokenType = tokenType };
		}

		private Expression CircuitExpression() {
			return this.LogicalOr();
		}

		private Expression LogicalOr() {
			Expression left = this.LogicalAnd();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("||")) {
					this.Next();
					Expression right = this.LogicalAnd();
					if(right == null) {
						return this.ExprMissing(token);
					}
					left = Expression.Condition(
						Expression.Or(
							Expression.NotEqual(left, Expression.Constant(0)),
							Expression.NotEqual(right, Expression.Constant(0))
						),
						Expression.Constant(1),
						Expression.Constant(0)
					);
					token = this.Current();
				}
			}
			return left;
		}

		private Expression LogicalAnd() {
			Expression left = this.Comparison();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("&&")) {
					this.Next();
					Expression right = this.Comparison();
					if(right == null) {
						return this.ExprMissing(token);
					}
					left = Expression.Condition(
						Expression.And(
							Expression.NotEqual(left, Expression.Constant(0)),
							Expression.NotEqual(right, Expression.Constant(0))
						),
						Expression.Constant(1),
						Expression.Constant(0)
					);
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Comparison() {
			Expression left = this.Addition();
			if(left != null) {
				Token token = this.Current();
				if(token.TokenType == TokenType.Binary) {
					Func<Expression, Expression, Expression> compare = null;
					switch(token.Value) {
					case "=":
					case "==":
						compare = Expression.Equal;
						break;
					case "!=":
					case "<>":
						compare = Expression.NotEqual;
						break;
					case "<":
						compare = Expression.LessThan;
						break;
					case "<=":
						compare = Expression.LessThanOrEqual;
						break;
					case ">=":
						compare = Expression.GreaterThanOrEqual;
						break;
					case ">":
						compare = Expression.GreaterThan;
						break;
					}
					if(compare != null) {
						this.Next();
						Expression right = this.Addition();
						if(right == null) {
							return this.ExprMissing(token);
						}
						return Expression.Condition(
							compare(left, right),
							Expression.Constant(1),
							Expression.Constant(0)
						);
					}
				}
			}
			return left;
		}

		private Expression Addition() {
			Expression left = this.Multiplication();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("+", "-")) {
					this.Next();
					Expression right = this.Multiplication();
					if(right == null) {
						return this.ExprMissing(token);
					}
					switch(token.Value) {
					case "+":
						left = Expression.Add(left, right);
						break;
					case "-":
						left = Expression.Subtract(left, right);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Multiplication() {
			Expression left = this.Conjunction();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("*", "/", "%")) {
					this.Next();
					Expression right = this.Conjunction();
					if(right == null) {
						return this.ExprMissing(token);
					}
					switch(token.Value) {
					case "*":
						left = Expression.Multiply(left, right);
						break;
					case "/":
						left = Expression.Divide(left, right);
						break;
					case "%":
						left = Expression.Modulo(left, right);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Conjunction() {
			Expression left = this.Disjunction();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("|", "^")) {
					this.Next();
					Expression right = this.Disjunction();
					if(right == null) {
						return this.ExprMissing(token);
					}
					switch(token.Value) {
					case "|":
						left = Expression.Or(left, right);
						break;
					case "^":
						left = Expression.ExclusiveOr(left, right);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Disjunction() {
			Expression left = this.Shift();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("&")) {
					this.Next();
					Expression right = this.Shift();
					if(right == null) {
						return this.ExprMissing(token);
					}
					left = Expression.And(left, right);
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Shift() {
			Expression left = this.Primary();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("<<", ">>")) {
					this.Next();
					Expression right = this.Primary();
					if(right == null) {
						return this.ExprMissing(token);
					}
					switch(token.Value) {
					case "<<":
						left = Expression.LeftShift(left, right);
						break;
					case ">>":
						left = Expression.RightShift(left, right);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Expression Primary() {
			Token token = this.Current();
			if(token.TokenType == TokenType.Open) {
				this.Next();
				Expression expr = this.CircuitExpression();
				if(expr != null) {
					token = this.Current();
					if(token.TokenType != TokenType.Close) {
						this.Error = Properties.Resources.ParserErrorCloseParenMissing(token.Value);
						return null;
					}
					this.Next();
				} else {
					return this.ExprMissing(token);
				}
				return expr;
			}
			if(token.TokenType == TokenType.Binary && token.Is("-")) {
				this.Next();
				Expression expr = this.Primary();
				if(expr != null) {
					return Expression.Negate(expr);
				} else {
					return this.ExprMissing(token);
				}
			}
			if(token.TokenType == TokenType.Unary && token.Value == "~") {
				this.Next();
				Expression expr = this.Primary();
				if(expr != null) {
					return Expression.Not(expr);
				} else {
					return this.ExprMissing(token);
				}
			}
			if(token.TokenType == TokenType.Unary && token.Is("!")) {
				this.Next();
				Expression expr = this.Primary();
				if(expr != null) {
					return Expression.Condition(
						Expression.Equal(expr, Expression.Constant(0)),
						Expression.Constant(1),
						Expression.Constant(0)
					);
				} else {
					return this.ExprMissing(token);
				}
			}
			if(token.TokenType == TokenType.Id) {
				this.Next();
				return this.Variable(token.Value);
			}
			switch(token.TokenType) {
			case TokenType.IntBin:
			case TokenType.IntDec:
			case TokenType.IntHex:
			case TokenType.IntOct:
				this.Next();
				return ExpressionParser.Literal(token);
			}
			return this.ErrorUnexpected(token);
		}

		private Expression Variable(string name) {
			int index = 0;
			foreach(InputPinSocket pin in this.socket.Inputs) {
				if(StringComparer.Ordinal.Equals(pin.Pin.Name, name)) {
					return Expression.MakeIndex(
						Expression.Property(
							this.stateParameter,
							typeof(TruthState).GetProperty("Input")
						),
						typeof(int[]).GetProperty("Item"),
						new Expression[] { Expression.Constant(index) }
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
							typeof(TruthState).GetProperty("Output")
						),
						typeof(int[]).GetProperty("Item"),
						new Expression[] { Expression.Constant(index) }
					);
				}
				index++;
			}
			this.Error = Properties.Resources.ParserErrorUnknownPin(name);
			return null;
		}

		private static Expression Literal(Token token) {
			switch(token.TokenType) {
			case TokenType.IntBin: return Expression.Constant(ExpressionParser.FromBin(token.Value));
			case TokenType.IntDec: return Expression.Constant(ExpressionParser.FromDec(token.Value));
			case TokenType.IntHex: return Expression.Constant(ExpressionParser.FromHex(token.Value));
			case TokenType.IntOct: return Expression.Constant(ExpressionParser.FromOct(token.Value));
			}
			return null;
		}

		private static int FromBin(string text) {
			int value = 0;
			foreach(char c in text) {
				value <<= 1;
				if(c == '1') {
					value |= 1;
				}
			}
			return value;
		}

		private static int FromDec(string text) {
			return int.Parse(text, CultureInfo.InvariantCulture);
		}

		private static int FromHex(string text) {
			return int.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static int FromOct(string text) {
			int value = 0;
			foreach(char c in text) {
				value <<= 3;
				value |= c - '0';
			}
			return value;
		}
	}
}
