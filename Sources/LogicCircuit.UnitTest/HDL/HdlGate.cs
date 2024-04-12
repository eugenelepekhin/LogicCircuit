namespace LogicCircuit.UnitTest.HDL {
	internal static class HdlGate {
		public static HdlChip CreateGate(HdlContext hdlContext, string gateName) {
			switch(gateName) {
			case "Nand":	return new HdlNand(hdlContext);
			case "Not":		return new HdlNot(hdlContext);
			case "Or":		return new HdlNot(hdlContext);
			}
			return null;
		}

		private class HdlNand : HdlChip {
			public HdlNand(HdlContext hdlContext) : base(hdlContext, "Nand") {
				this.AddInput(new HdlIOPin(hdlContext, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, "out", 1));
			}
		}

		private class HdlAnd : HdlChip {
			public HdlAnd(HdlContext hdlContext) : base(hdlContext, "And") {
				this.AddInput(new HdlIOPin(hdlContext, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, "out", 1));
			}
		}

		private class HdlNot : HdlChip {
			public HdlNot(HdlContext hdlContext) : base(hdlContext, "Not") {
				this.AddInput(new HdlIOPin(hdlContext, "in", 1));
				this.AddOutput(new HdlIOPin(hdlContext, "out", 1));
			}
		}

		private class HdlOr : HdlChip {
			public HdlOr(HdlContext hdlContext) : base(hdlContext, "Or") {
				this.AddInput(new HdlIOPin(hdlContext, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, "out", 1));
			}
		}

		private class HdlXor : HdlChip {
			public HdlXor(HdlContext hdlContext) : base(hdlContext, "Xor") {
				this.AddInput(new HdlIOPin(hdlContext, "a", 1));
				this.AddInput(new HdlIOPin(hdlContext, "b", 1));
				this.AddOutput(new HdlIOPin(hdlContext, "out", 1));
			}
		}
	}
}
