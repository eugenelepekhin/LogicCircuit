// Ignore Spelling: Hdl

using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlState {
		public HdlContext Context { get; }
		public HdlChip Chip { get; }

		private readonly int[] values;

		private readonly HdlState[] states;
		private readonly Dictionary<string, HdlIOPin> internalPins;

		public HdlState(HdlContext context, HdlChip chip) {
			this.Context = context;
			this.Chip = chip;
			this.values = new int[chip.PinsCount + chip.InternalsCount];
			this.states = new HdlState[this.Chip.Parts.Count()];
			if(0 < this.Chip.InternalsCount) {
				this.internalPins = new Dictionary<string, HdlIOPin>();
				int pinCount = chip.PinsCount;
				foreach(HdlPart.Connection connection in chip.Parts.SelectMany(p => p.Connections.Where(c => c.PinsPin == null))) {
					string name = connection.Pin.Name;
					if(!this.internalPins.ContainsKey(name)) {
						HdlIOPin pin = new HdlIOPin(context, pinCount + this.internalPins.Count, connection.Pin.Name, connection.BitWidth, HdlIOPin.PinType.Internal);
						this.internalPins.Add(pin.Name, pin);
						if(name == "true") {
							this.values[pin.Index] = 1;
						//} else if(name == "false") { // it's already 0
						}
					}
				}
			}
		}

		public HdlState PartState(HdlPart part) {
			Debug.Assert(part != null && part.Parent == this.Chip);
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

		public void Set(HdlPart part, HdlIOPin pin, int value) {
			HdlState state = (pin.Chip == null || pin.Chip == this.Chip) ? this : this.PartState(part);
			state.values[pin.Index] = ((1 << pin.BitWidth) - 1) & value;
		}
		public void Set(HdlPart part, HdlIOPin pin, int value, int first, int last) {
			HdlState state = (pin.Chip == null || pin.Chip == this.Chip) ? this : this.PartState(part);
			HdlState.SetBits(ref state.values[pin.Index], value, first, last);
		}

		public int Get(HdlPart part, HdlIOPin pin) {
			HdlState state = (pin.Chip == null || pin.Chip == this.Chip) ? this : this.PartState(part);
			return state.values[pin.Index];
		}
		public int Get(HdlPart part, HdlIOPin pin, int first, int last) {
			HdlState state = (pin.Chip == null || pin.Chip == this.Chip) ? this : this.PartState(part);
			return HdlState.GetBits(state.values[pin.Index], first, last);
		}

		public bool Assign(HdlPart.Connection connection) {
			int jamValue = connection.Jam.IsBitRange ? this.Get(connection.Parent, connection.JamsPin, connection.Jam.First, connection.Jam.Last) : this.Get(connection.Parent, connection.JamsPin);
			HdlIOPin pin = connection.PinsPin;
			if(pin == null) {
				pin = this.internalPins[connection.Pin.Name];
				Debug.Assert(pin != null);
			}
			int pinValue = connection.Pin.IsBitRange ? this.Get(connection.Parent, pin, connection.Pin.First, connection.Pin.Last) : this.Get(connection.Parent, pin);
			if(pinValue != jamValue) {
				if(connection.JamsPin.Type == HdlIOPin.PinType.Input) {
					if(connection.Jam.IsBitRange) {
						this.Set(connection.Parent, connection.JamsPin, pinValue, connection.Jam.First, connection.Jam.Last);
					} else {
						this.Set(connection.Parent, connection.JamsPin, pinValue);
					}
				} else {
					if(connection.Pin.IsBitRange) {
						this.Set(connection.Parent, pin, jamValue, connection.Pin.First, connection.Pin.Last);
					} else {
						this.Set(connection.Parent, pin, jamValue);
					}
				}
				return true;
			}
			return false;
		}

		public List<TruthState> BuildTruthTable() {
			List<HdlIOPin> inputs = this.Chip.Inputs.ToList();
			List<HdlIOPin> outputs = this.Chip.Outputs.ToList();
			inputs.Reverse();
			outputs.Reverse();
			int inputBits = inputs.Sum(i => i.BitWidth);
			Debug.Assert(inputBits < 16, "Too many input bits");
			List<TruthState> list = new List<TruthState>();
			for(int i = 0; i < (1 << inputBits); i++) {
				int first = 0;
				int input = inputs.Count - 1;
				TruthState truthState = new TruthState(inputs.Count, outputs.Count());
				truthState.Shortcut();
				foreach(HdlIOPin pin in inputs) {
					int value = HdlState.GetBits(i, first, first + pin.BitWidth - 1);
					first += pin.BitWidth;
					this.Set(null, pin, value);
					truthState.Input[input] = value;
					input--;
				}
				this.Evaluate();
				int output = outputs.Count() - 1;
				foreach(HdlIOPin pin in outputs) {
					int value = this.Get(null, pin);
					truthState.Output[output] = value;
					output--;
				}
				list.Add(truthState);
			}
			return list;
		}

		public void SetInput(string pinName, int value) {
			HdlIOPin pin = this.Chip.Inputs.FirstOrDefault(p => p.Name == pinName);
			if(pin == null) {
				throw new ArgumentOutOfRangeException(nameof(pinName), "Input pin not found");
			}
			this.Set(null, pin, value);
		}
		public int GetOutput(string pinName) {
			HdlIOPin pin = this.Chip.Outputs.FirstOrDefault(p => p.Name == pinName);
			if(pin == null) {
				throw new ArgumentOutOfRangeException(nameof(pinName), "Output pin not found");
			}
			return this.Get(null, pin);
		}
		public int this[string pinName] {
			get => this.GetOutput(pinName);
			set => this.SetInput(pinName, value);
		}

		/// <summary>
		/// Returns true if successful, false if it's oscillates
		/// </summary>
		/// <returns></returns>
		public bool Evaluate() => !this.Chip.Evaluate(this);

		#if DEBUG
			public override string ToString() => $"State of {this.Chip.Name}";
		#endif
	}
}
