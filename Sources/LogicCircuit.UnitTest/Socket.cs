using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	public struct InputSocket {
		private FunctionConstant input;

		public InputSocket(FunctionConstant input) {
			this.input = input;
			int width = this.BitWidth;
			Assert.IsTrue(0 < width && width <= 32);
		}

		public int BitWidth { get { return this.input.BitWidth; } }

		public int Value {
			get { return this.input.Value; }
			set {
				int width = this.BitWidth;
				if(width < 32) {
					Assert.IsTrue(0 <= value && value <= (1 << width) - 1, "Value out of range");
				}
				this.input.Value = value;
			}
		}
	}

	public struct OutputSocket {
		private FunctionProbe output;

		public OutputSocket(FunctionProbe output) {
			this.output = output;
		}

		public int BitWidth { get { return this.output.BitWidth; } }

		public State this[int bit] {
			get {
				Assert.IsTrue(0 <= bit && bit < this.output.BitWidth, "incorrect bit number");
				return this.output[bit];
			}
		}

		private static int BinaryInt(State state) {
			switch(state) {
			case State.On0: return 0;
			case State.On1: return 1;
			default:
				Assert.Fail("State is not 0 or 1");
				return -1;
			}
		}

		public int BinaryInt() {
			int width = this.output.BitWidth;
			int value = 0;
			for(int i = 0; i < width; i++) {
				value |= OutputSocket.BinaryInt(this.output[i]) << i;
			}
			return value;
		}

		public int FromBinaryDecimal() {
			Assert.AreEqual(8, this.BitWidth, "Only 8 bit values can be interpreted as Binary Decimal");
			int value = this.BinaryInt();
			Assert.IsTrue((value & ~0xFF) == 0);
			return (value >> 4) * 10 + (value & 0xF);
		}
	}
}
