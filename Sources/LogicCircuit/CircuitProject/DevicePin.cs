using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public partial class DevicePin {

		public override int BitWidth {
			get {
				Pin pin = this.Circuit as Pin;
				if(pin != null) {
					return pin.BitWidth;
				}
				Constant constant = this.Circuit as Constant;
				if(constant != null) {
					return constant.BitWidth;
				}
				Memory memory = this.Circuit as Memory;
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
				return this.PinBitWidth;
			}
			set { this.PinBitWidth = value; }
		}

		public int Order { get; set; }

		public override void CopyTo(CircuitProject project) {
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
				DevicePinData.NoteField.Field.DefaultValue, DevicePinData.NotationField.Field.DefaultValue
			);
			pin.Order = this.order++;
			return pin;
		}
	}
}
