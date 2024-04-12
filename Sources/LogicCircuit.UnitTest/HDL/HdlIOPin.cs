namespace LogicCircuit.UnitTest.HDL {
	internal class HdlIOPin : HdlItem {

		public string Name { get; }
		public int BitWidth { get; }
		public HdlChip Chip { get; set; }

		public HdlIOPin(HdlContext hdlContext, string name, int bitWidth) : base(hdlContext) {
			this.Name = name;
			this.BitWidth = bitWidth;
		}

		public override string ToString() {
			return (1 < this.BitWidth) ? $"{this.Name}[{this.BitWidth}]" : this.Name;
		}
	}
}
