// Ignore Spelling: Hdl

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace LogicCircuit.UnitTest.HDL {
	internal class HdlVisitor : HdlParserBaseVisitor<bool> {
		private readonly HdlContext hdlContext;
		public HdlChip Chip { get; private set; }

		private bool isInput;
		private JamVisitor jamVisitor;

		public HdlVisitor(HdlContext hdlContext) {
			this.hdlContext = hdlContext;
			this.jamVisitor = new JamVisitor(hdlContext);
		}

		public override bool VisitChip([NotNull] HdlParser.ChipContext context) {
			HdlChip chip = new HdlChip(this.hdlContext, context.chipName().GetText());
			this.Chip = chip;

			foreach(HdlParser.IoPinContext pinContext in context.inputPins().pins().ioPin()) {
				this.isInput = true;
				this.Visit(pinContext);
			}
			foreach(HdlParser.IoPinContext pinContext in context.outputPins().pins().ioPin()) {
				this.isInput = false;
				this.Visit(pinContext);
			}
			foreach(HdlParser.PartContext partContext in context.parts().part()) {
				this.Visit(partContext);
			}
			return true;
		}

		public override bool VisitIoPin([NotNull] HdlParser.IoPinContext context) {
			string name = context.pinName().GetText();
			string decNumber = context.DecNumber()?.GetText();
			int bitWidth = 1;
			if(decNumber != null) {
				bitWidth = int.Parse(decNumber);
			}
			this.Chip.AddPin(name, bitWidth, this.isInput ? HdlIOPin.PinType.Input : HdlIOPin.PinType.Output);
			return true;
		}

		public override bool VisitPart([NotNull] HdlParser.PartContext context) {
			string name = context.partName().GetText();
			HdlPart part = new HdlPart(this.hdlContext, this.Chip, name, this.Chip.Parts.Count());
			foreach(HdlParser.PartConnectionContext partConnection in context.partConnections().partConnection()) {
				part.AddConnection(this.jamVisitor.Visit(partConnection.jam()), this.jamVisitor.Visit(partConnection.pin()));
			}
			this.Chip.AddPart(part);
			return true;
		}

		private class JamVisitor : HdlParserBaseVisitor<HdlJam> {
			private readonly HdlContext hdlContext;

			public JamVisitor(HdlContext hdlContext) {
				this.hdlContext = hdlContext;
			}

			public override HdlJam VisitJam([NotNull] HdlParser.JamContext context) {
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
}
