using System;
using System.Linq;
using System.Text;
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

		private void Valid(ExpressionParser parser, TruthState state, bool expected, string text, bool inverted) {
			Predicate<TruthState> expr = parser.Parse(text, inverted);
			Assert.IsNotNull(expr, "Parse failed: " + parser.Error + ">> " + text);
			Assert.IsNull(parser.Error, "Unexpected parsing error");
			Assert.AreEqual(expected, expr(state), "Expression evaluated to unexpected value");
		}

		private void Invalid(ExpressionParser parser, TruthState state, string text) {
			Func<TruthState, int> expr = parser.Parse(text);
			Assert.IsNull(expr, "Expecting parse to fail: " + text);
			Assert.IsNotNull(parser.Error, "Expecting parsing error");
			this.TestContext.WriteLine("Expression: [{0}] Expected parsing error: {1}", text, parser.Error);
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

		private int InputIndex(CircuitTestSocket socket, string pinName) {
			int index = 0;
			foreach(InputPinSocket pin in socket.Inputs) {
				if(pin.Pin.Name == pinName) {
					return index;
				}
				index++;
			}
			return index;
		}

		private int OutputIndex(CircuitTestSocket socket, string pinName) {
			int index = 0;
			foreach(OutputPinSocket pin in socket.Outputs) {
				if(pin.Pin.Name == pinName) {
					return index;
				}
				index++;
			}
			return index;
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
			this.Valid(parser, state, 1, "1 || -3");
			this.Valid(parser, state, 1, "0 || 0 || 0 || 0 || 0 || 5");

			this.Invalid(parser, state, "1 || ");
			this.Invalid(parser, state, "1 || 0 ||");

			// check priority of expressions
			this.Valid(parser, state, 0, "0 || 3 && 0");
			this.Valid(parser, state, 1, "2 || 3 && 0");
			this.Valid(parser, state, 1, "-2 || -3 && 0");
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

			// check priority of expressions
			this.Valid(parser, state, 0, "1 && 3 == 0");
			this.Valid(parser, state, 0, "~1 && 3 == 0");
		}

		/// <summary>
		/// A test for Parse of comparison expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserComparisonParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 1, "0 = 0");
			this.Valid(parser, state, 1, "-1 == -1");
			this.Valid(parser, state, 1, "10 = 10");
			this.Valid(parser, state, 0, "0 = 10");
			this.Valid(parser, state, 0, "5 == 3");
			this.Invalid(parser, state, "1 = ");
			this.Invalid(parser, state, "1 = 1 ==");
			// check priority of expressions
			this.Valid(parser, state, 0, "3 = 3 + 1");

			this.Valid(parser, state, 0, "0 != 0");
			this.Valid(parser, state, 1, "0 != 1");
			this.Valid(parser, state, 1, "-1 <> 1");
			this.Valid(parser, state, 0, "1 <> 1");
			this.Valid(parser, state, 0, "-1 != -1");
			this.Valid(parser, state, 1, "-1 != -2");
			this.Invalid(parser, state, "3 != ");
			this.Invalid(parser, state, "3 != 2 = ");
			// check priority of expressions
			this.Valid(parser, state, 1, "3 != 3 + 10");

			this.Valid(parser, state, 0, "0 < 0");
			this.Valid(parser, state, 1, "0 < 1");
			this.Valid(parser, state, 1, "-1 < 1");
			this.Valid(parser, state, 1, "-3 < -1");
			this.Valid(parser, state, 0, "-1 < -3");
			this.Valid(parser, state, 0, "-1 < -1");
			this.Invalid(parser, state, "3 < ");
			this.Invalid(parser, state, "3 < 2 < ");
			// check priority of expressions
			this.Valid(parser, state, 1, "3 < 3 + 10");

			this.Valid(parser, state, 1, "0 <= 0");
			this.Valid(parser, state, 1, "0 <= 1");
			this.Valid(parser, state, 1, "-1 <= 1");
			this.Valid(parser, state, 1, "-3 <= -1");
			this.Valid(parser, state, 0, "-1 <= -3");
			this.Valid(parser, state, 1, "-1 <= -1");
			this.Invalid(parser, state, "3 <= ");
			this.Invalid(parser, state, "3 <= 2 <= ");
			// check priority of expressions
			this.Valid(parser, state, 1, "3 <= 3 + 10");

			this.Valid(parser, state, 1, "0 >= 0");
			this.Valid(parser, state, 0, "0 >= 1");
			this.Valid(parser, state, 0, "-1 >= 1");
			this.Valid(parser, state, 0, "-3 >= -1");
			this.Valid(parser, state, 1, "-1 >= -3");
			this.Valid(parser, state, 1, "-1 >= -1");
			this.Invalid(parser, state, "3 >= ");
			this.Invalid(parser, state, "3 >= 2 >= ");
			// check priority of expressions
			this.Valid(parser, state, 1, "30 >= 3 + 10");

			this.Valid(parser, state, 0, "0 > 0");
			this.Valid(parser, state, 0, "0 > 1");
			this.Valid(parser, state, 0, "-1 > 1");
			this.Valid(parser, state, 0, "-3 > -1");
			this.Valid(parser, state, 1, "-1 > -3");
			this.Valid(parser, state, 0, "-1 > -1");
			this.Invalid(parser, state, "3 > ");
			this.Invalid(parser, state, "3 > 2 > ");
			// check priority of expressions
			this.Valid(parser, state, 1, "30 > 3 + 10");
		}

		/// <summary>
		/// A test for Parse of addition expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserAdditionParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 + 0");
			this.Valid(parser, state, 2, "1 + 1");
			this.Valid(parser, state, 6, "1 + 2 + 3");
			this.Valid(parser, state, -18, "10 + 2 - 30");
			this.Valid(parser, state, 8, "10 - 2");
			this.Valid(parser, state, -12, "-10 - 2");

			this.Invalid(parser, state, "3 + ");
			this.Invalid(parser, state, "5 - ");
			this.Invalid(parser, state, "3 - 2 + ");
			this.Invalid(parser, state, "3 + 2 - ");

			// check priority of expressions
			this.Valid(parser, state, 7, "1 + 3 * 2");
			this.Valid(parser, state, 8, "(1 + 3) * 2");
			this.Valid(parser, state, 26, "2 * 3 + 5 * 4");
			this.Valid(parser, state, -26, "-2 * 3 - 5 * 4");
		}

		/// <summary>
		/// A test for Parse of multiplication expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserMultiplicationParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 * 0");
			this.Valid(parser, state, 1, "1 * 1");
			this.Valid(parser, state, 6, "1 * 2 * 3");
			this.Valid(parser, state, -20, "10 * -2");
			this.Valid(parser, state, 5, "10 / 2");
			this.Valid(parser, state, -4, "100 / -25");
			this.Valid(parser, state, 1, "10 % 3");
			this.Valid(parser, state, 2, "10 % 4");
			this.Valid(parser, state, 0, "10 % 5");
			this.Valid(parser, state, 2, "10 * 4 / 5 % 3");

			this.Invalid(parser, state, "3 * ");
			this.Invalid(parser, state, "5 / ");
			this.Invalid(parser, state, "5 % ");
			this.Invalid(parser, state, "3 * 2 * ");
			this.Invalid(parser, state, "3 / 2 / ");

			// check priority of expressions
			this.Valid(parser, state, 9, "3 * 2 | 1");
			this.Valid(parser, state, 36, "1 | 2 * 4 | 8");
			this.Valid(parser, state, 4, "4 | 8 / 1 | 2");
			this.Valid(parser, state, 2, "4 | 8 | 2 % 0b1 | 0b10");
		}

		/// <summary>
		/// A test for Parse of conjunction expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserConjunctionParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 | 0");
			this.Valid(parser, state, 1, "1 | 1");
			this.Valid(parser, state, 7, "1 | 2 | 4");
			this.Valid(parser, state, 1, "1 ^ 1 ^ 1");
			this.Valid(parser, state, 0, "1 ^ 1 ^ 1 ^ 1");
			this.Valid(parser, state, 0, "1 ^ 1");
			this.Valid(parser, state, 0, "198 ^ 198");
			this.Valid(parser, state, 3, "1 | 2");

			this.Invalid(parser, state, "3 | ");
			this.Invalid(parser, state, "5 ^ ");
			this.Invalid(parser, state, "3 | 2 ^ ");
			this.Invalid(parser, state, "3 ^ 2 | ");

			// check priority of expressions
			this.Valid(parser, state, 7, "7 | 2 & 1");
			this.Valid(parser, state, 6, "7 ^ 3 & 1");
		}

		/// <summary>
		/// A test for Parse of disjunction expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserDisjunctionParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 & 0");
			this.Valid(parser, state, 1, "1 & 1");
			this.Valid(parser, state, 1, "3 & 1");
			this.Valid(parser, state, 198, "198 & 198");
			this.Valid(parser, state, 0, "1 & 2");

			this.Invalid(parser, state, "3 & ");
			this.Invalid(parser, state, "3 & 2 & ");

			// check priority of expressions
			this.Valid(parser, state, 0xFC, "0xFF & 0xFF << 2");
		}

		/// <summary>
		/// A test for Parse of shift expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserShiftParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, 0, "0 << 0");
			this.Valid(parser, state, 2, "1 << 1");
			this.Valid(parser, state, 6, "3 << 1");
			this.Valid(parser, state, 12, "3 << 2");
			this.Valid(parser, state, 3, "7 >> 1");
			this.Valid(parser, state, 1, "7 >> 2");
			this.Valid(parser, state, 7 >> -1, "7 >> -1");

			this.Invalid(parser, state, "3 << ");
			this.Invalid(parser, state, "3 << 2 << ");

			// check priority of expressions
			this.Valid(parser, state, 3, "7 >> --1");
		}

		/// <summary>
		/// A test for Parse of primary expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserPrimaryParseTest() {
			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, -1, "-1");
			this.Valid(parser, state, 3, "--3");
			this.Valid(parser, state, 0, "!3");
			this.Valid(parser, state, 1, "!0");
			this.Valid(parser, state, -8, "~7");

			this.Invalid(parser, state, "-");
			this.Invalid(parser, state, "!");
			this.Invalid(parser, state, "~");
			this.Invalid(parser, state, "(3 + 2");

			// check priority of expressions
			this.Valid(parser, state, 6, "3 * (1 + 1)");
		}

		/// <summary>
		/// A test for Parse of primary expression - name of input or output pin
		/// </summary>
		[TestMethod()]
		public void ExpressionParserVariableParseTest() {
			CircuitProject project = ProjectTester.Load(this.TestContext, Properties.Resources.Digital_Clock, "4 bit adder");
			CircuitTestSocket socket = new CircuitTestSocket(project.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(socket.Inputs.Count(), socket.Outputs.Count());

			state.Input[this.InputIndex(socket, "c")]  = 1;
			state.Input[this.InputIndex(socket, "x1")] = 5;
			state.Input[this.InputIndex(socket, "x2")] = 4;
			state.Output[this.OutputIndex(socket, "s")] = 9;
			state.Output[this.OutputIndex(socket, "c'")] = 1;
			
			this.Valid(parser, state, 1, "c");
			this.Valid(parser, state, 5, "x1");
			this.Valid(parser, state, 4, "x2");
			this.Valid(parser, state, 9, "s");
			this.Valid(parser, state, 1, "\"c'\"");

			this.Invalid(parser, state, "d");
			this.Invalid(parser, state, "\"c'");
			this.Invalid(parser, state, "\"c'\\");
			this.Invalid(parser, state, "\"c'\\\"\"");
		}

		/// <summary>
		/// A test for Parse of primary expression - case sensitivity of name of input or output pin
		/// </summary>
		[TestMethod()]
		public void ExpressionParserVariableCaseParseTest() {
			CircuitProject project = ProjectTester.Load(this.TestContext, Properties.Resources.Digital_Clock, "4 bit adder");
			Pin x1 = null, x2 = null, s = null, c = null;

			foreach(Pin pin in project.PinSet.SelectByCircuit(project.ProjectSet.Project.LogicalCircuit)) {
				switch(pin.Name) {
				case "x1":
					x1 = pin;
					break;
				case "x2":
					x2 = pin;
					break;
				case "s":
					s = pin;
					break;
				case "c'":
					c = pin;
					break;
				}
			}
			Assert.IsNotNull(x1);
			Assert.IsNotNull(x2);
			Assert.IsNotNull(s);
			Assert.IsNotNull(c);
			project.InTransaction(() => {
				x1.Name = "variant";
				x2.Name = "vaRIAnt";
				s.Name = "VAriaNT";
				c.Name = "VARIANT";
			});

			CircuitTestSocket socket = new CircuitTestSocket(project.ProjectSet.Project.LogicalCircuit);
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(socket.Inputs.Count(), socket.Outputs.Count());

			state.Input[this.InputIndex(socket, "c")]  = 1;
			state.Input[this.InputIndex(socket, "variant")] = 5;
			state.Input[this.InputIndex(socket, "vaRIAnt")] = 4;
			state.Output[this.OutputIndex(socket, "VAriaNT")] = 9;
			state.Output[this.OutputIndex(socket, "VARIANT")] = 1;

			this.Valid(parser, state, 1, "c");
			this.Valid(parser, state, 5, "variant");
			this.Valid(parser, state, 4, "vaRIAnt");
			this.Valid(parser, state, 9, "VAriaNT");
			this.Valid(parser, state, 1, "VARIANT");
		}

		/// <summary>
		/// A test for Parse of Predicate flavor of expression
		/// </summary>
		[TestMethod()]
		public void ExpressionParserPredicateParseTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("Seed={0}", seed);
			Random rand = new Random(seed);

			CircuitTestSocket socket = null;
			ExpressionParser parser = new ExpressionParser(socket);
			TruthState state = new TruthState(0, 0);

			this.Valid(parser, state, false, "1", true);
			this.Valid(parser, state, false, "-1", true);
			this.Valid(parser, state, true, "0", true);
			this.Valid(parser, state, false, "10", true);
			this.Valid(parser, state, true, "5 - 5", true);
			this.Valid(parser, state, true, "-5 + 5", true);

			this.Valid(parser, state, true, "1", false);
			this.Valid(parser, state, true, "-1", false);
			this.Valid(parser, state, false, "0", false);
			this.Valid(parser, state, true, "10", false);
			this.Valid(parser, state, false, "5 - 5", false);
			this.Valid(parser, state, false, "-5 + 5", false);

			for(int i = 0; i < 1000; i++) {
				int n = rand.Next(1) * Math.Abs(rand.Next());
				this.Valid(parser, state, n == 0, n.ToString(), true);
			}

			for(int i = 0; i < 1000; i++) {
				int n = rand.Next(1) * Math.Abs(rand.Next());
				this.Valid(parser, state, n != 0, n.ToString(), false);
			}
		}
	}
}
