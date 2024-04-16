using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal static class HdlGate {
		public static HdlChip CreateGate(HdlContext hdlContext, string gateName) {
			switch(gateName) {
			case "Nand":	return new HdlNand(hdlContext);
			case "And":		return new HdlAnd(hdlContext);
			case "Not":		return new HdlNot(hdlContext);
			case "Or":		return new HdlOr(hdlContext);
			case "Xor":		return new HdlXor(hdlContext);
			}
			return null;
		}

		private class Gate : HdlChip {
			protected Gate(HdlContext hdlContext, string name) : base(hdlContext, name) {
			}

			public override bool Link() {
				return true;
			}
		}

		private class HdlNand : Gate {
			private readonly HdlIOPin a;
			private readonly HdlIOPin b;
			private readonly HdlIOPin o;
			public HdlNand(HdlContext hdlContext) : base(hdlContext, "Nand") {
				this.a = this.AddPin("a", 1, HdlIOPin.PinType.Input);
				this.b = this.AddPin("b", 1, HdlIOPin.PinType.Input);
				this.o = this.AddPin("out", 1, HdlIOPin.PinType.Output);
			}

			public override bool Evaluate(HdlState state) {
				Debug.Assert(state.Chip == this);
				int value = ((0 != state.Get(null, this.a)) && (0 != state.Get(null, this.b))) ? 0 : 1;
				state.Set(null, o, value);
				return base.Evaluate(state);
			}
		}

		private class HdlAnd : Gate {
			private readonly HdlIOPin a;
			private readonly HdlIOPin b;
			private readonly HdlIOPin o;

			public HdlAnd(HdlContext hdlContext) : base(hdlContext, "And") {
				this.a = this.AddPin("a", 1, HdlIOPin.PinType.Input);
				this.b = this.AddPin("b", 1, HdlIOPin.PinType.Input);
				this.o = this.AddPin("out", 1, HdlIOPin.PinType.Output);
			}

			public override bool Evaluate(HdlState state) {
				Debug.Assert(state.Chip == this);
				int value = ((0 != state.Get(null, this.a)) && (0 != state.Get(null, this.b))) ? 1 : 0;
				state.Set(null, o, value);
				return base.Evaluate(state);
			}
		}

		private class HdlNot : Gate {
			private readonly HdlIOPin i;
			private readonly HdlIOPin o;

			public HdlNot(HdlContext hdlContext) : base(hdlContext, "Not") {
				this.i = this.AddPin("in", 1, HdlIOPin.PinType.Input);
				this.o = this.AddPin("out", 1, HdlIOPin.PinType.Output);
			}

			public override bool Evaluate(HdlState state) {
				Debug.Assert(state.Chip == this);
				int value = (0 != state.Get(null, this.i)) ? 0 : 1;
				int old = state.Get(null, this.o);
				if(value != old) {
					state.Set(null, this.o, value);
					return true;
				}
				return false;
			}
		}

		private class HdlOr : Gate {
			private readonly HdlIOPin a;
			private readonly HdlIOPin b;
			private readonly HdlIOPin o;

			public HdlOr(HdlContext hdlContext) : base(hdlContext, "Or") {
				this.a = this.AddPin("a", 1, HdlIOPin.PinType.Input);
				this.b = this.AddPin("b", 1, HdlIOPin.PinType.Input);
				this.o = this.AddPin("out", 1, HdlIOPin.PinType.Output);
			}

			public override bool Evaluate(HdlState state) {
				Debug.Assert(state.Chip == this);
				int value = ((0 != state.Get(null, this.a)) || (0 != state.Get(null, this.b))) ? 1 : 0;
				state.Set(null, o, value);
				return base.Evaluate(state);
			}
		}

		private class HdlXor : Gate {
			private readonly HdlIOPin a;
			private readonly HdlIOPin b;
			private readonly HdlIOPin o;

			public HdlXor(HdlContext hdlContext) : base(hdlContext, "Xor") {
				this.a = this.AddPin("a", 1, HdlIOPin.PinType.Input);
				this.b = this.AddPin("b", 1, HdlIOPin.PinType.Input);
				this.o = this.AddPin("out", 1, HdlIOPin.PinType.Output);
			}

			public override bool Evaluate(HdlState state) {
				Debug.Assert(state.Chip == this);
				int value = ((0 != state.Get(null, this.a)) ^ (0 != state.Get(null, this.b))) ? 1 : 0;
				state.Set(null, o, value);
				return base.Evaluate(state);
			}
		}
	}
}
