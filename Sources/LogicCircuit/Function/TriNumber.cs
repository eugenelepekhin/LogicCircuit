using System;
using System.Text;

namespace LogicCircuit {
	public struct TriNumber {
		public static readonly TriNumber NaN = new TriNumber(32, -1L);

		private readonly int BitWidth;
		private readonly long Data;

		public TriNumber(Probe probe) {
			this.BitWidth = probe.BitWidth;
			long pack = 0;
			for(int i = 0; i < this.BitWidth; i++) {
				pack |= ((long)((int)probe[i] & 0x03)) << (2 * i);
			}
			this.Data = pack;
		}

		public TriNumber(int bitWidth, int value) {
			Tracer.Assert(0 < bitWidth && bitWidth <= 32);
			this.BitWidth = bitWidth;
			long pack = 0;
			for(int i = 0; i < bitWidth; i++) {
				pack |= ((long)(((value & (1 << i)) != 0) ? State.On1 : State.On0)) << (2 * i);
			}
			this.Data = pack;
		}

		private TriNumber(int bitWidth, long pack) {
			Tracer.Assert(0 < bitWidth && bitWidth <= 32);
			this.BitWidth = bitWidth;
			this.Data = pack;
		}

		public static bool operator ==(TriNumber x, TriNumber y) {
			return x.BitWidth == y.BitWidth && x.Data == y.Data;
		}

		public static bool operator !=(TriNumber x, TriNumber y) {
			return x.BitWidth != y.BitWidth || x.Data != y.Data;
		}

		public override bool Equals(object obj) {
			if(obj is TriNumber) {
				TriNumber other = (TriNumber)obj;
				return this == other;
			}
			return false;
		}

		public override int GetHashCode() {
			return this.BitWidth.GetHashCode() ^ this.Data.GetHashCode();
		}

		public override string ToString() {
			if(this.BitWidth < 3) {
				return this.ToBinString();
			} else {
				return this.ToHexString();
			}
		}

		private static void Validate(State state) {
			switch(state) {
			case State.Off:
			case State.On0:
			case State.On1:
				break;
			default:
				Tracer.Fail("Invalid state");
				break;
			}
		}

		private State Unpack(int bitNumber) {
			Tracer.Assert(0 <= bitNumber && bitNumber < this.BitWidth);
			State state = (State)(((int)(this.Data >> (2 * bitNumber))) & 0x03);
			TriNumber.Validate(state);
			return state;
		}

		private bool TryUnpack(out int value) {
			int number = value = 0;
			for(int i = 0; i < this.BitWidth; i++) {
				switch(this.Unpack(i)) {
				case State.Off:
					return false;
				case State.On0:
					break;
				case State.On1:
					number |= (1 << i);
					break;
				default:
					Tracer.Fail();
					break;
				}
			}
			value = number;
			return true;
		}

		private string ToBinString() {
			char[] text = new char[this.BitWidth];
			for(int i = 0; i < this.BitWidth; i++) {
				text[i] = CircuitFunction.ToChar(this.Unpack(i));
			}
			Array.Reverse(text);
			return new string(text);
		}

		private string ToHexString() {
			int value = 0;
			if(this.TryUnpack(out value)) {
				return LogicCircuit.Resources.ProbeHistoryHex(value);
			} else {
				return this.ToBinString();
			}
		}

		#if false
		private static long Fill(long value, State bit, int fromBit, int toBit) {
			Tracer.Assert(0 <= fromBit && fromBit < 32);
			Tracer.Assert(0 <= toBit && toBit < 32);
			int start = Math.Min(fromBit, toBit);
			int end = Math.Max(fromBit, toBit);
			for(int i = fromBit; i <= toBit; i++) {
				value |= ((long)bit) << (2 * i);
			}
			return value;
		}
		
		private static long Normalize(long value, int fromBitWidth, int toBitWidth) {
			if(fromBitWidth < toBitWidth) {
				return Fill(value, State.On0, fromBitWidth, toBitWidth - 1);
			} else if(toBitWidth < fromBitWidth) {
				return Fill(value, State.Off, toBitWidth, fromBitWidth - 1);
			}
			return value;
		}

		private static long SoftOr(int bitWidth, long x, long y) {
			long result = 0;
			for(int i = 0; i < bitWidth; i++) {
				result |= ((long)((((x | y) & (3 << (2 * i))) != 0) ? State.On1 : State.On0)) << (2 * i);
			}
			return result;
		}

		private static long HardOr(int bitWidth, long x, long y) {
			long result = 0;
			for(int i = 0; i < bitWidth; i++) {
				long mask = 3 << (2 * i);
				result |= ((long)((((x | y) & (3 << (2 * i))) != 0) ? State.On1 : State.On0)) << (2 * i);
			}
			return result;
		}
		
		private static long SoftAdd(int bitWidth, long x, long y) {
			long result = 0;
			for(int i = 0; i < bitWidth; i++) {
			}
			return result;
		}
		#endif
	}
}
