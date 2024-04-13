using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlIOPin : HdlItem {

		public string Name { get; }
		public int BitWidth { get; }
		public HdlChip Chip { get; }

		public HdlIOPin(HdlContext hdlContext, HdlChip chip, string name, int bitWidth) : base(hdlContext) {
			Debug.Assert(0 < bitWidth);
			this.Chip = chip;
			this.Name = name;
			this.BitWidth = bitWidth;
		}

		public override string ToString() {
			return (1 != this.BitWidth) ? $"{this.Name}[{this.BitWidth}]" : this.Name;
		}
	}
}
