using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Globalization;

namespace LogicCircuit {
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
		}

		private CircuitTestSocket socket;
		private StringReader reader;
		private StringBuilder buffer = new StringBuilder();
		private Token current;
		public string Error { get; private set; }

		public ExpressionParser(CircuitTestSocket socket) {
			this.socket = socket;
		}

		public Expression<Func<TruthState, TriNumber>> Parse(string text) {
			this.Error = null;
			using(this.reader = new StringReader(text)) {
				return this.Comparison();
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
				return new Token() { Value = string.Empty, TokenType = TokenType.EOS };
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
			if("()+-*/%&|^!=<>".Contains(c)) {
				return this.NextOperator();
			}
			this.Error = "Unknown char";
			return new Token() { Value = string.Empty, TokenType = TokenType.EOS };
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
						this.Error = "Quoted identifier does not have closing quote";
						break;
					}
					c = (char)next;
				}
				this.buffer.Append(c);
				next = this.reader.Read();
			}
			return new Token() { Value = this.buffer.ToString(), TokenType = TokenType.Id };
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
				this.Error = "Invalid number";
			}
			return new Token() { Value = this.buffer.ToString(), TokenType = tokenType };
		}

		private Token NextOperator() {
			this.buffer.Length = 0;
			TokenType tokenType = TokenType.Binary;
			switch(this.reader.Peek()) {
			case '(': return new Token() { Value = new string((char)this.reader.Read(), 1), TokenType = TokenType.Open };
			case ')': return new Token() { Value = new string((char)this.reader.Read(), 1), TokenType = TokenType.Close };
			case '+':
			case '-':
			case '*':
			case '/':
			case '%':
			case '&': // allow &&
			case '|':
			case '^':
				this.buffer.Append((char)this.reader.Read());
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

		private Expression<Func<TruthState, TriNumber>> Comparison() {
			Expression<Func<TruthState, TriNumber>> left = this.Addition();
			Token token = this.Current();
			if(token.TokenType == TokenType.Binary && token.Value == "=") {
				this.Next();
				Expression<Func<TruthState, TriNumber>> right = this.Disjunction();
				if(right == null && this.Error == null) {
					this.Error = "Expression is missing after " + token.Value;
					return null;
				}
				return s => TriNumber.Equal(left.Compile()(s), right.Compile()(s));
			}
			return left;
		}

		private Expression<Func<TruthState, TriNumber>> Addition() {
			Expression<Func<TruthState, TriNumber>> left = this.Multiplication();
			return left;
		}

		private Expression<Func<TruthState, TriNumber>> Multiplication() {
			Expression<Func<TruthState, TriNumber>> left = this.Conjunction();
			return left;
		}

		private Expression<Func<TruthState, TriNumber>> Conjunction() {
			Expression<Func<TruthState, TriNumber>> left = this.Disjunction();
			Token token = this.Current();
			if(token.TokenType == TokenType.Binary && token.Value == "|") {
				this.Next();
				Expression<Func<TruthState, TriNumber>> right = this.Disjunction();
				if(right == null && this.Error == null) {
					this.Error = "Expression is missing after " + token.Value;
					return null;
				}
				return s => TriNumber.Or(left.Compile()(s), right.Compile()(s));
			}
			return left;
		}

		private Expression<Func<TruthState, TriNumber>> Disjunction() {
			Expression<Func<TruthState, TriNumber>> left = this.Primary();
			Token token = this.Current();
			if(token.TokenType == TokenType.Binary && token.Value == "&") {
				this.Next();
				Expression<Func<TruthState, TriNumber>> right = this.Primary();
				if(right == null && this.Error == null) {
					this.Error = "Expression is missing after " + token.Value;
					return null;
				}
				return s => TriNumber.And(left.Compile()(s), right.Compile()(s));
			}
			return left;
		}

		private Expression<Func<TruthState, TriNumber>> Primary() {
			Token token = this.Current();
			if(token.TokenType == TokenType.Open) {
				this.Next();
				Expression<Func<TruthState, TriNumber>> expr = this.Comparison();
				if(expr != null) {
					token = this.Current();
					if(token.TokenType != TokenType.Close) {
						this.Error = "Missing ')'";
					}
					this.Next();
				}
				return expr;
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
				return this.Literal(token);
			}
			this.Error = token.Value + " - unexpected";
			return null;
		}

		private Expression<Func<TruthState, TriNumber>> Variable(string name) {
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

		private Expression<Func<TruthState, TriNumber>> Literal(Token token) {
			switch(token.TokenType) {
			case TokenType.IntBin: return this.Literal(this.FromBin(token.Value));
			case TokenType.IntDec: return this.Literal(this.FromDec(token.Value));
			case TokenType.IntHex: return this.Literal(this.FromHex(token.Value));
			case TokenType.IntOct: return this.Literal(this.FromOct(token.Value));
			}
			return null;
		}

		private Expression<Func<TruthState, TriNumber>> Literal(int value) {
			TriNumber n = new TriNumber(32, value);
			return s => n;
		}

		private int FromBin(string text) {
			int value = 0;
			foreach(char c in text) {
				value <<= 1;
				if(c == '1') {
					value |= 1;
				}
			}
			return value;
		}

		private int FromDec(string text) {
			int value = 0;
			int.TryParse(text, out value);
			return value;
		}

		private int FromHex(string text) {
			int value = 0;
			int.TryParse(text, NumberStyles.HexNumber, null, out value);
			return value;
		}

		private int FromOct(string text) {
			int value = 0;
			foreach(char c in text) {
				value <<= 3;
				value |= c - '0';
			}
			return value;
		}
	}
}
