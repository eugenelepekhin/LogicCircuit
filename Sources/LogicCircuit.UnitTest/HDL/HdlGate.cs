namespace LogicCircuit.UnitTest.HDL {
	internal static class HdlGate {
		public static HdlChip CreateGate(HdlContext hdlContext, string gateName) {
			switch(gateName) {
			case "Nand":	return new HdlNand(hdlContext);
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
			public HdlNand(HdlContext hdlContext) : base(hdlContext, "Nand") {
				this.AddInput(new HdlIOPin(hdlContext, this, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, this, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, this, "out", 1));
			}
		}

		private class HdlAnd : Gate {
			public HdlAnd(HdlContext hdlContext) : base(hdlContext, "And") {
				this.AddInput(new HdlIOPin(hdlContext, this, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, this, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, this, "out", 1));
			}
		}

		private class HdlNot : Gate {
			public HdlNot(HdlContext hdlContext) : base(hdlContext, "Not") {
				this.AddInput(new HdlIOPin(hdlContext, this, "in", 1));
				this.AddOutput(new HdlIOPin(hdlContext, this, "out", 1));
			}
		}

		private class HdlOr : Gate {
			public HdlOr(HdlContext hdlContext) : base(hdlContext, "Or") {
				this.AddInput(new HdlIOPin(hdlContext, this, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, this, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, this, "out", 1));
			}
		}

		private class HdlXor : Gate {
			public HdlXor(HdlContext hdlContext) : base(hdlContext, "Xor") {
				this.AddInput(new HdlIOPin(hdlContext, this, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, this, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, this, "out", 1));
			}
		}
	}
}
