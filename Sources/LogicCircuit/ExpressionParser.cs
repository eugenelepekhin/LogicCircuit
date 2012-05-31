using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Globalization;

namespace LogicCircuit {
	/// <summary>
	/// Parse expressions over it input and output pins.
	/// BNF:
	/// Expr ::= LogicalOr
	/// LogicalOr ::= LogicalAnd || LogicalAnd
	/// LogicalAnd ::= Comparison && Comparison
	/// Comparison ::= Addition CMP Addition
	/// CMP ::= = | == | != | <> | < | <= | >= | >
	/// Addition ::= Multiplication ADD Multiplication
	/// ADD ::= + | -
	/// Multiplication ::= Conjunction MUL Conjunction
	/// MUL ::= * | / | %
	/// Conjunction ::= Disjunction CON Disjunction
	/// CON ::= ^ | '|'
	/// Disjunction ::= Shift DIS Shift
	/// DIS ::= &
	/// Shift ::= Primary SHT Primary
	/// SHT ::= << | >>
	/// Primary ::= ( Expr ) | - Primary | ~ Primary | ! Primary | ID | Literal
	/// ID ::= SimpleId | QuotedId
	/// SimpleId ::= Letter | Letter LettersOrDigids
	/// LettersOrDigids ::= LettersOrDigids | Letter | Digit
	/// QuotedId ::= " (ESC | [~"\])* "
	/// ESC ::= \\ | \" | \.
	/// </summary>
	public class ExpressionParser {
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
				return new Token() { Value = "End Of Text", TokenType = TokenType.EOS };
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
			this.Error = null;
			this.current = new Token();
			using(this.reader = new StringReader(text)) {
				Func<TruthState, int> expr = this.Expression();
				if(this.Current().TokenType != TokenType.EOS) {
					this.Error = this.Current().Value + " Unexpected";
					return null;
				}
				return expr;
			}
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
			this.Error = "Unknown char: " + c.ToString();
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
			while(next != quote && next != -1) {
				char c = (char)next;
				if(c == '\\') {
					next = this.reader.Read();
					if(next == -1) {
						this.buffer.Append(c);
						this.Error = "Quoted identifier does not have closing quote: " + this.buffer.ToString().Trim();
						return Token.Eos();
					}
					c = (char)next;
				}
				this.buffer.Append(c);
				next = this.reader.Read();
			}
			return new Token() { Value = this.buffer.ToString().Trim(), TokenType = TokenType.Id };
		}

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
				this.Error = "Invalid number: " + this.buffer.ToString();
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

		private Func<TruthState, int> ExprMissing(Token after) {
			this.Error = "Expression is missing after " + after.Value;
			return null;
		}

		private Func<TruthState, int> Expression() {
			return this.LogicalOr();
		}

		private Func<TruthState, int> LogicalOr() {
			Func<TruthState, int> left = this.LogicalAnd();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("||")) {
					this.Next();
					Func<TruthState, int> right = this.LogicalAnd();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					left = s => (original(s) != 0 || right(s) != 0) ? 1 : 0;
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> LogicalAnd() {
			Func<TruthState, int> left = this.Comparison();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("&&")) {
					this.Next();
					Func<TruthState, int> right = this.Comparison();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					left = s => (original(s) != 0 && right(s) != 0) ? 1 : 0;
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Comparison() {
			Func<TruthState, int> left = this.Addition();
			if(left != null) {
				Token token = this.Current();
				if(token.TokenType == TokenType.Binary && token.Is("=", "==")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) == right(s)) ? 1 : 0;
				}
				if(token.TokenType == TokenType.Binary && token.Is("!=", "<>")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) != right(s)) ? 1 : 0;
				}
				if(token.TokenType == TokenType.Binary && token.Is("<")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) < right(s)) ? 1 : 0;
				}
				if(token.TokenType == TokenType.Binary && token.Is("<=")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) <= right(s)) ? 1 : 0;
				}
				if(token.TokenType == TokenType.Binary && token.Is(">")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) > right(s)) ? 1 : 0;
				}
				if(token.TokenType == TokenType.Binary && token.Is(">=")) {
					this.Next();
					Func<TruthState, int> right = this.Addition();
					if(right == null) {
						return this.ExprMissing(token);
					}
					return s => (left(s) >= right(s)) ? 1 : 0;
				}
			}
			return left;
		}

		private Func<TruthState, int> Addition() {
			Func<TruthState, int> left = this.Multiplication();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("+", "-")) {
					this.Next();
					Func<TruthState, int> right = this.Multiplication();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					switch(token.Value) {
					case "+":
						left = s => original(s) + right(s);
						break;
					case "-":
						left = s => original(s) - right(s);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Multiplication() {
			Func<TruthState, int> left = this.Conjunction();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("*", "/", "%")) {
					this.Next();
					Func<TruthState, int> right = this.Conjunction();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					switch(token.Value) {
					case "*":
						left = s => original(s) * right(s);
						break;
					case "/":
						left = s => original(s) / right(s);
						break;
					case "%":
						left = s => original(s) % right(s);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Conjunction() {
			Func<TruthState, int> left = this.Disjunction();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("|", "^")) {
					this.Next();
					Func<TruthState, int> right = this.Disjunction();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					switch(token.Value) {
					case "|":
						left = s => original(s) | right(s);
						break;
					case "^":
						left = s => original(s) ^ right(s);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Disjunction() {
			Func<TruthState, int> left = this.Shift();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("&")) {
					this.Next();
					Func<TruthState, int> right = this.Shift();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					left = s => original(s) & right(s);
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Shift() {
			Func<TruthState, int> left = this.Primary();
			if(left != null) {
				Token token = this.Current();
				while(token.TokenType == TokenType.Binary && token.Is("<<", ">>")) {
					this.Next();
					Func<TruthState, int> right = this.Primary();
					if(right == null) {
						return this.ExprMissing(token);
					}
					Func<TruthState, int> original = left;
					switch(token.Value) {
					case "<<":
						left = s => original(s) << right(s);
						break;
					case ">>":
						left = s => original(s) >> right(s);
						break;
					}
					token = this.Current();
				}
			}
			return left;
		}

		private Func<TruthState, int> Primary() {
			Token token = this.Current();
			if(token.TokenType == TokenType.Open) {
				this.Next();
				Func<TruthState, int> expr = this.Expression();
				if(expr != null) {
					token = this.Current();
					if(token.TokenType != TokenType.Close) {
						this.Error = "Missing ')' instead of: " + token.Value;
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
				Func<TruthState, int> expr = this.Primary();
				if(expr != null) {
					return s => -expr(s);
				} else {
					return this.ExprMissing(token);
				}
			}
			if(token.TokenType == TokenType.Unary && token.Value == "~") {
				this.Next();
				Func<TruthState, int> expr = this.Primary();
				if(expr != null) {
					return s => ~expr(s);
				} else {
					return this.ExprMissing(token);
				}
			}
			if(token.TokenType == TokenType.Unary && token.Is("!")) {
				this.Next();
				Func<TruthState, int> expr = this.Primary();
				if(expr != null) {
					return s => (expr(s) == 0) ? 1 : 0;
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
			this.Error = token.Value + " - unexpected";
			return null;
		}

		private Func<TruthState, int> Variable(string name) {
			int index = 0;
			foreach(InputPinSocket pin in this.socket.Inputs) {
				if(StringComparer.OrdinalIgnoreCase.Equals(pin.Pin.Name, name)) {
					return s => s.Input[index];
				}
				index++;
			}
			index = 0;
			foreach(OutputPinSocket pin in this.socket.Outputs) {
				if(StringComparer.OrdinalIgnoreCase.Equals(pin.Pin.Name, name)) {
					return s => s.Output[index];
				}
				index++;
			}
			this.Error = name + " - input or output pin not found";
			return null;
		}

		private static Func<TruthState, int> Literal(Token token) {
			switch(token.TokenType) {
			case TokenType.IntBin: return ExpressionParser.Literal(ExpressionParser.FromBin(token.Value));
			case TokenType.IntDec: return ExpressionParser.Literal(ExpressionParser.FromDec(token.Value));
			case TokenType.IntHex: return ExpressionParser.Literal(ExpressionParser.FromHex(token.Value));
			case TokenType.IntOct: return ExpressionParser.Literal(ExpressionParser.FromOct(token.Value));
			}
			return null;
		}

		private static Func<TruthState, int> Literal(int value) {
			return s => value;
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
