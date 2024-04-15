using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlState {
		public HdlContext Context { get; }
		public HdlChip Chip { get; }

		private readonly int[] values;

		private readonly HdlState[] states;

		public HdlState(HdlContext context, HdlChip chip) {
			this.Context = context;
			this.Chip = chip;
			this.values = new int[this.Chip.PinsCount];
			this.states = new HdlState[this.Chip.Parts.Count()];
		}

		public HdlState PartState(HdlPart part) {
			Debug.Assert(part.Parent == this.Chip);
			if(states[part.Index] == null) {
				states[part.Index] = new HdlState(this.Context, part.Chip);
			}
			return states[part.Index];
		}

		private static int Mask(int first, int last) {
			long f = (1L << first) - 1;
			long l = (1L << (last + 1)) - 1;
			int mask = (int)(f ^ l);
			return mask;
		}

		private static void SetBits(ref int location, int value, int first, int last) {
			int mask = HdlState.Mask(first, last);
			int old = location & ~mask;
			location = (old | (value << first) & mask);
		}
		private static int GetBits(int value, int first, int last) => (value & HdlState.Mask(first, last)) >> first;

		public void Set(HdlIOPin pin, int value) {
			HdlState state = (pin.Chip == this.Chip) ? this : this.states.FirstOrDefault(s => s.Chip == pin.Chip);
			state.values[pin.Index] = value;
		}
		public void Set(HdlIOPin pin, int value, int first, int last) {
			HdlState state = (pin.Chip == this.Chip) ? this : this.states.FirstOrDefault(s => s.Chip == pin.Chip);
			HdlState.SetBits(ref state.values[pin.Index], value, first, last);
		}

		public int Get(HdlIOPin pin) {
			HdlState state = (pin.Chip == this.Chip) ? this : this.states.FirstOrDefault(s => s.Chip == pin.Chip);
			return state.values[pin.Index];
		}
		public int Get(HdlIOPin pin, int first, int last) {
			HdlState state = (pin.Chip == this.Chip) ? this : this.states.FirstOrDefault(s => s.Chip == pin.Chip);
			return HdlState.GetBits(state.values[pin.Index], first, last);
		}

		public bool Assign(HdlPart.Connection connection) {
			int pinValue = connection.Pin.IsBitRange ? this.Get(connection.PinsPin, connection.Pin.First, connection.Pin.Last) : this.Get(connection.PinsPin);
			int jamValue = connection.Jam.IsBitRange ? this.Get(connection.JamsPin, connection.Jam.First, connection.Jam.Last) : this.Get(connection.JamsPin);
			if(pinValue != jamValue) {
				if(connection.JamsPin.Type == HdlIOPin.PinType.Input) {
					if(connection.Jam.IsBitRange) {
						this.Set(connection.JamsPin, pinValue, connection.Jam.First, connection.Jam.Last);
					} else {
						this.Set(connection.JamsPin, pinValue);
					}
				} else {
					if(connection.Pin.IsBitRange) {
						this.Set(connection.PinsPin, jamValue, connection.Pin.First, connection.Pin.Last);
					} else {
						this.Set(connection.PinsPin, jamValue);
					}
				}
				return true;
			}
			return false;
		}
	}
}
