namespace LogicCircuit.UnitTest.HDL {
	internal class HdlState {
		public HdlContext Context { get; }
		public HdlChip Chip { get; }

		private readonly int[] inputs;
		private readonly int[] outputs;

		public HdlState(HdlContext context, HdlChip chip) {
			this.Context = context;
			this.Chip = chip;
			this.inputs = new int[this.Chip.Inputs.Count()];
			this.outputs = new int[this.Chip.Outputs.Count()];
		}
	}
}
