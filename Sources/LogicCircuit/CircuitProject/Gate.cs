using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	public partial class Gate {
		public const int MaxInputCount = 18;

		public override string Name { get; set; }
		public override string ToolTip { get { return this.Name; } }
		public override string Notation { get; set; }
		public override string Category { get; set; }
		public GateType GateType { get; internal set; }

		public override void Delete() {
			throw new InvalidOperationException();
		}

		public bool InvertedOutput { get; internal set; }

		public int InputCount { get { return this.Pins.Count(p => p.PinType == PinType.Input); } }

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.GateSet.Gate(this.GateId);
		}

		public override bool IsDisplay {
			get { return this.GateType == LogicCircuit.GateType.Led; }
			set { base.IsDisplay = value; }
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			Tracer.Assert(defaultWidth == (this.GateType == LogicCircuit.GateType.TriState ? 2 : 1));
			switch(this.GateType) {
			case LogicCircuit.GateType.Clock:
				return base.CircuitSymbolWidth(defaultWidth);
			case LogicCircuit.GateType.Led:
				if(this.Pins.Count() == 1) {
					return base.CircuitSymbolWidth(defaultWidth);
				}
				break;
			}
			return 3;
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			switch(this.GateType) {
			case LogicCircuit.GateType.Clock:
				return base.CircuitSymbolHeight(defaultHeight);
			case LogicCircuit.GateType.Led:
				if(this.Pins.Count() == 1) {
					return base.CircuitSymbolHeight(defaultHeight);
				}
				break;
			}
			return Math.Max(4, defaultHeight);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			string skin;
			switch(this.GateType) {
			case GateType.Clock:
				skin = SymbolShape.Clock;
				break;
			case GateType.Odd:
			case GateType.Even:
			case GateType.Probe:
				Tracer.Fail();
				return null;
			case GateType.Led:
				if(this.InputCount == 1) {
					skin = SymbolShape.Led;
				} else {
					Tracer.Assert(this.InputCount == 8);
					skin = SymbolShape.SevenSegment;
				}
				break;
			default:
				if(Settings.User.GateShape == GateShape.Rectangular) {
					return symbol.CreateRectangularGlyph();
				} else {
					switch(this.GateType) {
					case GateType.Not:
						skin = SymbolShape.ShapedNot;
						break;
					case GateType.Or:
						skin = SymbolShape.ShapedOr;
						break;
					case GateType.And:
						skin = SymbolShape.ShapedAnd;
						break;
					case GateType.Xor:
						skin = SymbolShape.ShapedXor;
						break;
					case GateType.TriState:
						skin = SymbolShape.ShapedTriState;
						break;
					default:
						Tracer.Fail();
						return null;
					}
					return symbol.CreateShapedGlyph(skin);
				}
			}
			return symbol.CreateSimpleGlyph(skin, symbol);
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			if(this.GateType == LogicCircuit.GateType.Led) {
				string skin;
				if(this.InputCount == 1) {
					skin = SymbolShape.Led;
				} else {
					Tracer.Assert(this.InputCount == 8);
					skin = SymbolShape.SevenSegment;
				}
				return symbol.CreateSimpleGlyph(skin, mainSymbol);
			}
			return base.CreateDisplay(symbol, mainSymbol);
		}

		public override bool Similar(Circuit other) {
			if(this != other) {
				Gate g = other as Gate;
				return g != null && this.GateType == g.GateType;
			}
			return true;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class GateSet {
		private static bool IsValid(GateType gateType) {
			return Enum.IsDefined(typeof(GateType), gateType) && gateType != GateType.Odd && gateType != GateType.Even && gateType != GateType.Probe;
		}

		private static bool IsValid(GateType gateType, int inputCount) {
			Tracer.Assert(GateSet.IsValid(gateType));
			switch(gateType) {
			case GateType.Nop:
			case GateType.Clock:
				return inputCount == 0;
			case GateType.Not:
				return inputCount == 1;
			case GateType.Or:
			case GateType.And:
			case GateType.Xor:
				return 1 < inputCount && inputCount <= LogicCircuit.Gate.MaxInputCount;
			case GateType.Led:
				return inputCount == 1 || inputCount == 8;
			case GateType.TriState:
				return inputCount == 2;
			case GateType.Odd:
			case GateType.Even:
			case GateType.Probe:
				Tracer.Fail();
				return false;
			default:
				return false;
			}
		}

		private static bool HasOutput(GateType gateType) {
			Tracer.Assert(GateSet.IsValid(gateType));
			return gateType != GateType.Nop && gateType != GateType.Led;
		}

		public Gate Gate(GateType gateType, int inputCount, bool invertedOutput) {
			if(!GateSet.IsValid(gateType)) {
				throw new ArgumentOutOfRangeException("gateType");
			}
			if(!GateSet.IsValid(gateType, inputCount)) {
				throw new ArgumentOutOfRangeException("inputCount");
			}
			if(invertedOutput && !GateSet.HasOutput(gateType)) {
				throw new ArgumentOutOfRangeException("invertedOutput");
			}
			Gate gate = this.FindByGateId(GateSet.GateGuid(gateType, inputCount, invertedOutput));
			if(gate == null) {
				return this.Create(gateType, inputCount, invertedOutput);
			}
			return gate;
		}

		public Gate Gate(Guid gateId) {
			Gate gate = this.FindByGateId(gateId);
			if(gate != null) {
				return gate;
			}
			byte[] id = gateId.ToByteArray();
			GateType gateType = (GateType)(int)id[13];
			int inputCount = (int)id[14];
			bool invertedOutput = (id[15] == 0) ? false : true;
			if(GateSet.IsValid(gateType) && GateSet.IsValid(gateType, inputCount) && (!invertedOutput || GateSet.HasOutput(gateType)) && GateSet.GateGuid(gateType, inputCount, invertedOutput) == gateId) {
				return this.Create(gateType, inputCount, invertedOutput);
			}
			return null;
		}

		private static Guid GateGuid(GateType gateType, int inputCount, bool invertedOutput) {
			Tracer.Assert(gateType != GateType.Nop && GateSet.IsValid(gateType, inputCount) && (!invertedOutput || GateSet.HasOutput(gateType)));
			return new Guid(0, 0, 0, 0, 0, 0, 0, 0,
				(byte)(int)gateType,
				(byte)inputCount,
				(byte)(invertedOutput ? 1 : 0)
			);
		}

		private Gate Create(GateType gateType, int inputCount, bool invertedOutput) {
			Gate gate = this.CreateItem(GateSet.GateGuid(gateType, inputCount, invertedOutput));
			gate.GateType = gateType;
			gate.InvertedOutput = invertedOutput;
			switch(gate.GateType) {
			case GateType.Clock:
				gate.Name = Properties.Resources.GateClockName;
				gate.Notation = Properties.Resources.GateClockNotation;
				gate.Category = Properties.Resources.CategoryInputOutput;
				break;
			case GateType.Not:
				gate.Name = Properties.Resources.GateNotName;
				gate.Notation = Properties.Resources.GateNotNotation;
				gate.Category = Properties.Resources.CategoryPrimitives;
				break;
			case GateType.Or:
				gate.Name = invertedOutput ? Properties.Resources.GateOrNotName : Properties.Resources.GateOrName;
				gate.Notation = Properties.Resources.GateOrNotation;
				gate.Category = Properties.Resources.CategoryPrimitives;
				break;
			case GateType.And:
				gate.Name = invertedOutput ? Properties.Resources.GateAndNotName : Properties.Resources.GateAndName;
				gate.Notation = Properties.Resources.GateAndNotation;
				gate.Category = Properties.Resources.CategoryPrimitives;
				break;
			case GateType.Xor:
				gate.Name = invertedOutput ? Properties.Resources.GateXorNotName : Properties.Resources.GateXorName;
				gate.Notation = Properties.Resources.GateXorNotation;
				gate.Category = Properties.Resources.CategoryPrimitives;
				break;
			case GateType.Led:
				gate.Name = Properties.Resources.GateLedName;
				gate.Notation = Properties.Resources.GateLedNotation;
				gate.Category = Properties.Resources.CategoryInputOutput;
				break;
			case GateType.TriState:
				gate.Name = Properties.Resources.GateTriStateName;
				gate.Notation = Properties.Resources.GateTriStateNotation;
				gate.Category = Properties.Resources.CategoryPrimitives;
				break;
			case GateType.Odd:
			case GateType.Even:
			case GateType.Probe:
			default:
				Tracer.Fail();
				break;
			}
			if(gate.GateType == GateType.TriState) {
				this.GenerateTriStatePins(gate);
			} else if(gate.GateType == GateType.Led && inputCount == 8) {
				this.GenerateSevenSegmentIndicatorPins(gate);
			} else {
				this.GeneratePins(gate, inputCount, invertedOutput);
			}
			return gate;
		}

		private void GeneratePins(Gate gate, int inputCount, bool invertedOutput) {
			if(inputCount == 1) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Properties.Resources.PinInName;
			} else {
				for(int i = 0; i < inputCount; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
					pin.Name = Properties.Resources.PinName(Properties.Resources.PinInName, i + 1);
				}
			}
			if(GateSet.HasOutput(gate.GateType)) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
				pin.Inverted = invertedOutput;
				pin.Name = Properties.Resources.PinOutName;
			}
		}

		private void GenerateSevenSegmentIndicatorPins(Gate gate) {
			string prefix = "Led7Pin";
			int name = 1;
			for(int i = 0; i < 4; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Properties.Resources.ResourceManager.GetString(prefix + name);
				name++;
			}
			for(int i = 0; i < 3; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Properties.Resources.ResourceManager.GetString(prefix + name);
				pin.PinSide = PinSide.Right;
				name++;
			}
			DevicePin pinDot = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinDot.Name = Properties.Resources.ResourceManager.GetString(prefix + name);
			pinDot.PinSide = PinSide.Right;
		}

		private void GenerateTriStatePins(Gate gate) {
			DevicePin pinX = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinX.Name = Properties.Resources.PinInName;
			DevicePin pinE = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinE.Name = Properties.Resources.PinEnableName;
			pinE.PinSide = PinSide.Bottom;
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
			pin.Inverted = false;
			pin.Name = Properties.Resources.PinOutName;
		}
	}
}
