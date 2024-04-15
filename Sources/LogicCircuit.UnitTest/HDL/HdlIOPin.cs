using System.Diagnostics;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlIOPin : HdlItem {
		internal enum PinType {
			Input,
			Output,
			Internal,
		}

		public HdlChip Chip { get; }
		public string Name { get; }
		public int BitWidth { get; }
		public int Index { get; }
		public PinType Type { get; }

		public HdlIOPin(HdlContext hdlContext, HdlChip chip, string name, int bitWidth, PinType type) : base(hdlContext) {
			Debug.Assert(0 < bitWidth);
			this.Chip = chip;
			this.Name = name;
			this.BitWidth = bitWidth;
			this.Index = this.Chip.PinsCount;
			this.Type = type;
		}

		public override string ToString() {
			return (1 != this.BitWidth) ? $"{this.Name}[{this.BitWidth}]" : this.Name;
		}
	}
}
