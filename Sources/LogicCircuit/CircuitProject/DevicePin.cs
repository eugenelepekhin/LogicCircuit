using System;
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
					Pin pin = circuit as Pin;
					if(pin != null) {
						return pin.BitWidth;
					}
					Constant constant = circuit as Constant;
					if(constant != null) {
						return constant.BitWidth;
					}
					Memory memory = circuit as Memory;
					if(memory != null) {
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
				Pin pin = this.Circuit as Pin;
				if(pin != null) {
					return pin.Inverted;
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

	public partial class DevicePinSet : NamedItemSet  {
		private int order = 0;

		protected override bool Exists(string name) {
			throw new NotSupportedException();
		}

		protected override bool Exists(string name, Circuit group) {
			return this.FindByCircuitAndName(group, name) != null;
		}

		public DevicePin Create(Circuit circuit, PinType pinType, int bitWidth) {
			DevicePin pin = this.CreateItem(Guid.NewGuid(), circuit, bitWidth, pinType, BasePin.DefaultSide(pinType), false,
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
