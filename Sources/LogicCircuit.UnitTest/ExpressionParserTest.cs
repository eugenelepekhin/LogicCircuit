using System;
using System.Text;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for ExpressionParser and is intended
	/// to contain all ExpressionParser Unit Tests
	/// </summary>
	[TestClass()]
	public class ExpressionParserTest {
		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		private void Valid(ExpressionParser parser, TruthState state, int expected, string text) {
			Func<TruthState, int> expr = parser.Parse(text);
			Assert.IsNotNull(expr, "Parse failed: " + parser.Error + ">> " + text);
			Assert.IsNull(parser.Error, "Unexpected parsing error");
			Assert.AreEqual(expected, expr(state), "Expression evaluated to unexpected value");
		}

		private void Invalid(ExpressionParser parser, TruthState state, string text) {
			Func<TruthState, int> expr = parser.Parse(text);
			Assert.IsNull(expr, "Expecting parse to faile: " + text);
			Assert.IsNotNull(parser.Error, "Expecting parsing error");
		}

		private void Revert(StringBuilder text) {
			int s = 0;
			int e = text.Length - 1;
			while(s < e) {
				char c = text[s];
				text[s] = text[e];
				text[e] = c;
				s++;
				e--;
			}
		}

		private string ToBinary(int n) {
			int sign = Math.Sign(n);
			n = Math.Abs(n);
			StringBuilder text = new StringBuilder();
			do {
				text.Append(n & 1);
				n >>= 1;
			} while(n != 0);
			text.Append("b0");
			if(sign < 0) {
				text.Append("-");
			}
			this.Revert(text);
			return text.ToString();
		}

		private string ToOctal(int n) {
			int sign = Math.Sign(n);
			n = Math.Abs(n);
			StringBuilder text = new StringBuilder();
			do {
				text.Append(n & 7);
				n >>= 3;
			} while(n != 0);
			text.Append("0");
			if(sign < 0) {
				text.Append("-");
			}
			this.Revert(text);
			return text.ToString();
		}

		private string ToHex(int n) {
			return "0x" + n.ToString("x");
		}

		/// <summary>
		/// A test for Parse of decimal literals
		/// </summary>
		[TestMethod()]
		public void ExpressionParserDecimalLiteralsParseTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("Seed={0}", seed);
			Random rand = new Random(seed);

			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0,   "0");
			this.Valid(parser, state, 1,   "1");
			this.Valid(parser, state, 10,  "10");
			this.Valid(parser, state, 123, "123");
			this.Valid(parser, state, 345, "345");
			this.Valid(parser, state, 567, "567");
			this.Valid(parser, state, 789, "789");
			this.Valid(parser, state, 900, "900");
			this.Valid(parser, state, int.MaxValue, int.MaxValue.ToString());

			for(int i = 0; i < 1000; i++) {
				int n = Math.Abs(rand.Next());
				this.Valid(parser, state, n, n.ToString());
			}

			this.Invalid(parser, state, "12345678901");
			this.Invalid(parser, state, "123456789012345678901234567890");
		}

		/// <summary>
		/// A test for Parse of binary literals
		/// </summary>
		[TestMethod()]
		public void ExpressionParserBinaryLiteralsParseTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("Seed={0}", seed);
			Random rand = new Random(seed);

			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0,   this.ToBinary(0));
			this.Valid(parser, state, 1,   this.ToBinary(1));
			this.Valid(parser, state, 10,  this.ToBinary(10));
			this.Valid(parser, state, 123, this.ToBinary(123));
			this.Valid(parser, state, 345, this.ToBinary(345));
			this.Valid(parser, state, 567, this.ToBinary(567));
			this.Valid(parser, state, 789, this.ToBinary(789));
			this.Valid(parser, state, 900, this.ToBinary(900));
			this.Valid(parser, state, int.MaxValue, this.ToBinary(int.MaxValue));

			for(int i = 0; i < 1000; i++) {
				int n = Math.Abs(rand.Next());
				this.Valid(parser, state, n, this.ToBinary(n));
			}

			this.Invalid(parser, state, "0b1010101010101010101010101010101010101010101010101");
			this.Invalid(parser, state, "0b101010101010101010101010101010101010101010101010101010101");
			this.Invalid(parser, state, "0b102");
			this.Invalid(parser, state, "0b105");
			this.Invalid(parser, state, "0b109");
			this.Invalid(parser, state, "0b103");
		}

		/// <summary>
		/// A test for Parse of octal literals
		/// </summary>
		[TestMethod()]
		public void ExpressionParserOctalLiteralsParseTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("Seed={0}", seed);
			Random rand = new Random(seed);

			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0,   this.ToOctal(0));
			this.Valid(parser, state, 1,   this.ToOctal(1));
			this.Valid(parser, state, 10,  this.ToOctal(10));
			this.Valid(parser, state, 123, this.ToOctal(123));
			this.Valid(parser, state, 345, this.ToOctal(345));
			this.Valid(parser, state, 567, this.ToOctal(567));
			this.Valid(parser, state, 789, this.ToOctal(789));
			this.Valid(parser, state, 900, this.ToOctal(900));
			this.Valid(parser, state, int.MaxValue, this.ToOctal(int.MaxValue));

			for(int i = 0; i < 1000; i++) {
				int n = Math.Abs(rand.Next());
				this.Valid(parser, state, n, this.ToOctal(n));
			}

			this.Invalid(parser, state, "0123456701234");
			this.Invalid(parser, state, "012345670123567");
			this.Invalid(parser, state, "012345678");
			this.Invalid(parser, state, "012345679");
		}

		/// <summary>
		/// A test for Parse of hex literals
		/// </summary>
		[TestMethod()]
		public void ExpressionParserHexLiteralsParseTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("Seed={0}", seed);
			Random rand = new Random(seed);

			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0,   this.ToHex(0));
			this.Valid(parser, state, 1,   this.ToHex(1));
			this.Valid(parser, state, 10,  this.ToHex(10));
			this.Valid(parser, state, 123, this.ToHex(123));
			this.Valid(parser, state, 345, this.ToHex(345));
			this.Valid(parser, state, 567, this.ToHex(567));
			this.Valid(parser, state, 789, this.ToHex(789));
			this.Valid(parser, state, 900, this.ToHex(900));
			this.Valid(parser, state, int.MaxValue, this.ToHex(int.MaxValue));
			this.Valid(parser, state, int.MinValue, this.ToHex(int.MinValue));

			for(int i = 0; i < 1000; i++) {
				int n = rand.Next() - rand.Next();
				this.Valid(parser, state, n, this.ToHex(n));
			}

			this.Invalid(parser, state, "0x123456780");
			this.Invalid(parser, state, "0x123456780123456");
		}

		/// <summary>
		/// A test for Parse of || expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserLogicalOrParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 || 0");
			this.Valid(parser, state, 0, "0 || 0 || 0 || 0 || 0");
			this.Valid(parser, state, 1, "0 || 2");
			this.Valid(parser, state, 1, "5 || 0");
			this.Valid(parser, state, 1, "1 || 3");
			this.Valid(parser, state, 1, "0 || 0 || 0 || 0 || 0 || 5");

			this.Invalid(parser, state, "1 || ");
			this.Invalid(parser, state, "1 || 0 ||");
		}

		/// <summary>
		/// A test for Parse of && expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserLogicalAndParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 && 0");
			this.Valid(parser, state, 0, "0 && 0 && 0 && 0 && 0");
			this.Valid(parser, state, 0, "0 && 2");
			this.Valid(parser, state, 0, "5 && 0");
			this.Valid(parser, state, 1, "1 && 3");
			this.Valid(parser, state, 0, "1 && 2 && 3 && 4 && 5 && 0");

			this.Invalid(parser, state, "1 && ");
			this.Invalid(parser, state, "1 && 0 &&");
		}
	}
}
