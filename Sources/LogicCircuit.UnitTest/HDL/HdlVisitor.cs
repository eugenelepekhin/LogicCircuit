using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlVisitor : HdlParserBaseVisitor<HdlItem> {
		private readonly HdlContext hdlContext;
		private HdlChip currentChip;

		public HdlVisitor(HdlContext hdlContext) {
			this.hdlContext = hdlContext;
		}

		public override HdlItem VisitChip([NotNull] HdlParser.ChipContext context) {
			HdlChip chip = new HdlChip(this.hdlContext, context.chipName().GetText());
			this.currentChip = chip;

			foreach(HdlParser.IoPinContext pinContext in context.inputPins().pins().ioPin()) {
				chip.AddInput((HdlIOPin)this.Visit(pinContext));
			}
			foreach(HdlParser.IoPinContext pinContext in context.outputPins().pins().ioPin()) {
				chip.AddOutput((HdlIOPin)this.Visit(pinContext));
			}
			foreach(HdlParser.PartContext partContext in context.parts().part()) {
				chip.AddPart((HdlPart)this.Visit(partContext));
			}
			return chip;
		}

		public override HdlItem VisitIoPin([NotNull] HdlParser.IoPinContext context) {
			string name = context.pinName().GetText();
			string decNumber = context.DecNumber()?.GetText();
			int bitWidth = 1;
			if(decNumber != null) {
				bitWidth = int.Parse(decNumber);
			}
			return new HdlIOPin(hdlContext, this.currentChip, name, bitWidth);
		}

		public override HdlItem VisitPart([NotNull] HdlParser.PartContext context) {
			string name = context.partName().GetText();
			HdlPart part = new HdlPart(this.hdlContext, this.currentChip, name);
			foreach(HdlParser.PartConnectionContext partConnection in context.partConnections().partConnection()) {
				part.AddConnection((HdlJam)this.Visit(partConnection.jam()), (HdlJam)this.Visit(partConnection.pin()));
			}
			return part;
		}

		public override HdlItem VisitJam([NotNull] HdlParser.JamContext context) {
			string name = context.jamName().GetText();
			HdlParser.BitsContext bitsContext = context.bits();
			if(bitsContext != null) {
				ITerminalNode[] nodes = bitsContext.DecNumber();
				int first = int.Parse(nodes[0].GetText());
				int last = first;
				if(nodes.Length > 1) {
					last = int.Parse(nodes[1].GetText());
				}
				return new HdlJam(this.hdlContext, name, first, last);
			}
			return new HdlJam(this.hdlContext, name);
		}
	}
}
