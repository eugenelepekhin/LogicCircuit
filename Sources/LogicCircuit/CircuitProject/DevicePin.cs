using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace LogicCircuit {
	public partial class DevicePin {

		public override string Notation {
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		private int bitWidth;
		public override int BitWidth {
			get {
				if(this.bitWidth < 1) {
					Circuit circuit = this.Circuit;
					if(circuit is Pin pin) {
						return pin.BitWidth;
					}
					if(circuit is Constant constant) {
						return constant.BitWidth;
					}
					if(circuit is Memory memory) {
						if(this == memory.AddressPin) {
							return memory.AddressBitWidth;
						} else if(this == memory.DataInPin || this == memory.DataOutPin) {
							return memory.DataBitWidth;
						} else if(this == memory.WritePin) {
							return 1;
						} else {
							Tracer.Fail("Unknown pin");
						}
					}
					if(circuit is Sensor sensor) {
						return sensor.BitWidth;
					}
					if(circuit is GraphicsArray graphicsArray) {
						if(this == graphicsArray.AddressPin) {
							return graphicsArray.AddressBitWidth;
						} else if(this == graphicsArray.DataInPin || this == graphicsArray.DataOutPin) {
							return graphicsArray.DataBitWidth;
						} else if(this == graphicsArray.WritePin) {
							return 1;
						} else {
							Tracer.Fail("Unknown pin");
						}
					}
					this.bitWidth = this.PinBitWidth;
				}
				return this.bitWidth;
			}
			set {
				Tracer.Fail();
				//this.PinBitWidth = value;
			}
		}

		public override bool Inverted {
			get {
				if(this.Circuit is Pin pin) {
					return pin.Inverted;
				}
				if(this.Circuit is CircuitButton button) {
					return button.Inverted;
				}
				return this.PinInverted;
			}
			set { this.PinInverted = value; }
		}

		public int Order { get; set; }

		public override Circuit CopyTo(LogicalCircuit target) {
			throw new InvalidOperationException();
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			throw new InvalidOperationException();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class DevicePinSet : NamedItemSet  {
		private int order = 0;

		protected override bool Exists(string name) {
			throw new NotSupportedException();
		}

		protected override bool Exists(string name, Circuit group) {
			return this.FindByCircuitAndName(group, name) != null;
		}

		public DevicePin Create(Circuit circuit, PinType pinType, int bitWidth) {
			PinSide pinSide = BasePin.DefaultSide((circuit is Pin circuitPin) ? (circuitPin.PinType == PinType.Input ? PinType.Output : PinType.Input) : pinType);
			DevicePin pin = this.CreateItem(Guid.NewGuid(), circuit, bitWidth, pinType, pinSide, false,
				this.UniqueName(BasePin.DefaultName(pinType), circuit),
				DevicePinData.NoteField.Field.DefaultValue, DevicePinData.JamNotationField.Field.DefaultValue
			);
			pin.Order = this.order++;
			return pin;
		}

		public void DeleteAllPins(Circuit circuit) {
			this.SelectByCircuit(circuit).ToList().ForEach(pin => pin.Delete());
		}
	}
}
