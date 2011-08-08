using System;
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
		
		public override bool IsSmallSymbol {
			get {
				switch(this.GateType) {
				case LogicCircuit.GateType.Clock:
				case LogicCircuit.GateType.Probe:
					return true;
				case LogicCircuit.GateType.Led:
					return this.Pins.Count() == 1;
				}
				return false;
			}
		}

		public bool InvertedOutput {
			get {
				BasePin pin = this.Pins.FirstOrDefault(p => p.PinType == PinType.Output);
				return pin != null && pin.Inverted;
			}
		}

		public int InputCount { get { return this.Pins.Count(p => p.PinType == PinType.Input); } }

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.GateSet.Gate(this.GateId);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			string skin;
			switch(this.GateType) {
			case GateType.Clock:
				skin = SymbolShape.Clock;
				break;
			case GateType.Odd:
			case GateType.Even:
				return symbol.CreateRectangularGlyph();
			case GateType.Led:
				if(this.InputCount == 1) {
					skin = SymbolShape.Led;
				} else {
					Tracer.Assert(this.InputCount == 8);
					skin = SymbolShape.SevenSegment;
				}
				break;
			case GateType.Probe:
				skin = SymbolShape.Probe;
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
			return symbol.CreateSimpleGlyph(skin);
		}
	}

	public partial class GateSet {
		private static bool IsValid(GateType gateType) {
			return Enum.IsDefined(typeof(GateType), gateType);
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
			case GateType.Odd:
			case GateType.Even:
				return 1 < inputCount && inputCount <= LogicCircuit.Gate.MaxInputCount;
			case GateType.Led:
				return inputCount == 1 || inputCount == 8;
			case GateType.Probe:
				return inputCount == 1;
			case GateType.TriState:
				return inputCount == 2;
			default:
				return false;
			}
		}

		private static bool HasOutput(GateType gateType) {
			Tracer.Assert(GateSet.IsValid(gateType));
			return gateType != GateType.Nop && gateType != GateType.Led && gateType != GateType.Probe;
		}

		public Gate Gate(GateType gateType, int inputCount, bool invertedOutput) {
			if(!GateSet.IsValid(gateType)) {
				throw new ArgumentOutOfRangeException("gateType");
			}
			if(!GateSet.IsValid(gateType, inputCount)) {
				throw new ArgumentOutOfRangeException("inputCount");
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
			if(GateSet.IsValid(gateType) && GateSet.IsValid(gateType, inputCount) && GateSet.GateGuid(gateType, inputCount, invertedOutput) == gateId) {
				return this.Create(gateType, inputCount, invertedOutput);
			}
			return null;
		}

		private static Guid GateGuid(GateType gateType, int inputCount, bool invertedOutput) {
			Tracer.Assert(gateType != GateType.Nop && GateSet.IsValid(gateType, inputCount));
			return new Guid(0, 0, 0, 0, 0, 0, 0, 0,
				(byte)(int)gateType,
				(byte)inputCount,
				(byte)(invertedOutput ? 1 : 0)
			);
		}

		private Gate Create(GateType gateType, int inputCount, bool invertedOutput) {
			Gate gate = this.CreateItem(GateSet.GateGuid(gateType, inputCount, invertedOutput));
			gate.GateType = gateType;
			switch(gate.GateType) {
			case GateType.Clock:
				gate.Name = Resources.GateClockName;
				gate.Notation = Resources.GateClockNotation;
				gate.Category = Resources.CategoryInputOutput;
				break;
			case GateType.Not:
				gate.Name = Resources.GateNotName;
				gate.Notation = Resources.GateNotNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.Or:
				gate.Name = invertedOutput ? Resources.GateOrNotName : Resources.GateOrName;
				gate.Notation = Resources.GateOrNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.And:
				gate.Name = invertedOutput ? Resources.GateAndNotName : Resources.GateAndName;
				gate.Notation = Resources.GateAndNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.Xor:
				gate.Name = invertedOutput ? Resources.GateXorNotName : Resources.GateXorName;
				gate.Notation = Resources.GateXorNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.Odd:
				gate.Name = Resources.GateOddName;
				gate.Notation = Resources.GateOddNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.Even:
				gate.Name = Resources.GateEvenName;
				gate.Notation = Resources.GateEvenNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
			case GateType.Led:
				gate.Name = Resources.GateLedName;
				gate.Notation = Resources.GateLedNotation;
				gate.Category = Resources.CategoryInputOutput;
				break;
			case GateType.Probe:
				gate.Name = Resources.GateProbeName;
				gate.Notation = Resources.GateProbeNotation;
				gate.Category = Resources.CategoryInputOutput;
				break;
			case GateType.TriState:
				gate.Name = Resources.GateTriStateName;
				gate.Notation = Resources.GateTriStateNotation;
				gate.Category = Resources.CategoryPrimitives;
				break;
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
			for(int i = 0; i < inputCount; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Resources.PinName(Resources.PinInName, i + 1);
			}
			if(GateSet.HasOutput(gate.GateType)) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
				pin.Inverted = invertedOutput;
				pin.Name = Resources.PinOutName;
			}
		}

		private void GenerateSevenSegmentIndicatorPins(Gate gate) {
			string prefix = "Led7Pin";
			int name = 1;
			for(int i = 0; i < 4; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Resources.ResourceManager.GetString(prefix + name);
				name++;
			}
			for(int i = 0; i < 3; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Resources.ResourceManager.GetString(prefix + name);
				pin.PinSide = PinSide.Right;
				name++;
			}
			DevicePin pinDot = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinDot.Name = Resources.ResourceManager.GetString(prefix + name);
			pinDot.PinSide = PinSide.Right;
		}

		private void GenerateTriStatePins(Gate gate) {
			DevicePin pinX = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinX.Name = Resources.PinInName;
			DevicePin pinE = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinE.Name = Resources.PinEnableName;
			pinE.PinSide = PinSide.Bottom;
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
			pin.Inverted = false;
			pin.Name = Resources.PinOutName;
		}
	}
}
