using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public partial class Gate {
		public const int MaxInputCount = 18;

		public override string Name { get; set; }
		public override string ToolTip { get { return this.Name; } }
		public override string Notation { get; set; }
		public override string Category { get; set; }
		
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

		public override void CopyTo(CircuitProject project) {
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
			return this.FindByGateId(GateSet.GateGuid(gateType, inputCount, invertedOutput));
		}

		public static Guid GateGuid(GateType gateType, int inputCount, bool invertedOutput) {
			Tracer.Assert(gateType != GateType.Nop && GateSet.IsValid(gateType, inputCount));
			return new Guid(0, 0, 0, 0, 0, 0, 0, 0,
				(byte)(int)gateType,
				(byte)inputCount,
				(byte)(invertedOutput ? 1 : 0)
			);
		}

		private static void SetStrings(Gate gate, bool invertedOutput) {
			switch(gate.GateType) {
			case GateType.Clock:
				gate.Name = Resources.GateClockName;
				gate.Notation = Resources.GateClockNotation;
				gate.Category = Resources.CategoryInputOutput;
				break;
			case GateType.Not:
				gate.Name = Resources.GateNotName;
				gate.Notation = Resources.GateNotNotation;
				gate.Category = Resources.CategoryBuffer;
				break;
			case GateType.Or:
				gate.Name = invertedOutput ? Resources.GateOrNotName : Resources.GateOrName;
				gate.Notation = Resources.GateOrNotation;
				gate.Category = Resources.GateOrName;
				break;
			case GateType.And:
				gate.Name = invertedOutput ? Resources.GateAndNotName : Resources.GateAndName;
				gate.Notation = Resources.GateAndNotation;
				gate.Category = Resources.GateAndName;
				break;
			case GateType.Xor:
				gate.Name = invertedOutput ? Resources.GateXorNotName : Resources.GateXorName;
				gate.Notation = Resources.GateXorNotation;
				gate.Category = Resources.GateXorName;
				break;
			case GateType.Odd:
				gate.Name = Resources.GateOddName;
				gate.Notation = Resources.GateOddNotation;
				gate.Category = Resources.CategoryParity;
				break;
			case GateType.Even:
				gate.Name = Resources.GateEvenName;
				gate.Notation = Resources.GateEvenNotation;
				gate.Category = Resources.CategoryParity;
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
				gate.Category = Resources.CategoryBuffer;
				break;
			default:
				Tracer.Fail();
				break;
			}
		}

		private Gate Create(GateType gateType, int inputCount, bool invertedOutput) {
			return this.CreateItem(GateSet.GateGuid(gateType, inputCount, invertedOutput), gateType);
		}

		private Gate Generate(GateType gateType, int inputCount, bool invertedOutput) {
			Gate gate = this.Create(gateType, inputCount, invertedOutput);
			for(int i = 0; i < inputCount; i++) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
				pin.Name = Resources.PinName(Resources.PinInName, i + 1);
			}
			if(GateSet.HasOutput(gateType)) {
				DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
				pin.Inverted = invertedOutput;
				pin.Name = Resources.PinOutName;
			}
			GateSet.SetStrings(gate, invertedOutput);
			return gate;
		}

		private void Generate(GateType gateType) {
			for(int i = 2; i <= LogicCircuit.Gate.MaxInputCount; i++) {
				this.Generate(gateType, i, false);
				if(gateType != GateType.Even && gateType != GateType.Odd) {
					this.Generate(gateType, i, true);
				}
			}
		}

		private Gate GenerateSevenSegmentIndicator() {
			Gate gate = this.Create(GateType.Led, 8, false);
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
			GateSet.SetStrings(gate, false);
			return gate;
		}

		private Gate GenerateTriState(bool invertedOutput) {
			Gate gate = this.Create(GateType.TriState, 2, invertedOutput);
			DevicePin pinX = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinX.Name = Resources.PinInName;
			DevicePin pinE = this.CircuitProject.DevicePinSet.Create(gate, PinType.Input, 1);
			pinE.Name = Resources.PinEnableName;
			pinE.PinSide = PinSide.Bottom;
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(gate, PinType.Output, 1);
			pin.Inverted = invertedOutput;
			pin.Name = Resources.PinOutName;
			GateSet.SetStrings(gate, false);
			return gate;
		}

		public void Generate() {
			Tracer.Assert(!this.Table.Any());
			this.Generate(GateType.Clock, 0, false);
			this.Generate(GateType.Not, 1, true);
			this.Generate(GateType.Or);
			this.Generate(GateType.And);
			this.Generate(GateType.Xor);
			this.Generate(GateType.Odd);
			this.Generate(GateType.Even);
			this.Generate(GateType.Led, 1, false);
			this.GenerateSevenSegmentIndicator();
			this.Generate(GateType.Probe, 1, false);
			this.GenerateTriState(false);
		}
	}
}
