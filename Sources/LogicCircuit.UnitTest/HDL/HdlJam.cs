// Ignore Spelling: Hdl

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlJam : HdlItem {

		public string Name { get; }
		public int First { get; }
		public int Last { get; }
		public bool IsBitRange { get; }

		public HdlJam(HdlContext hdlContext, string name) : base(hdlContext) {
			this.Name = name;
		}

		public HdlJam(HdlContext hdlContext, string name, int first, int last) : this(hdlContext, name) {
			this.First = first;
			this.Last = last;
			this.IsBitRange = true;
		}

		public override string ToString() {
			string range = this.IsBitRange ? $"[{this.First}{((this.First == this.Last) ? string.Empty : $"..{this.Last}")}]" : string.Empty;
			return $"{this.Name}{range}";
		}
	}
}
