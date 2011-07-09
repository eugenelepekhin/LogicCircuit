using System;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Splitter {
		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override bool IsSmallSymbol { get { return true; } }

		public override string Name {
			get { return Resources.NameSplitter; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return this.Name; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip {
			get { return Resources.ToolTipSplitter(this.BitWidth, this.PinCount); }
		}

		public override string Category {
			get { return Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.SplitterSet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateSimpleGlyph(SymbolShape.Splitter);
		}
	}

	public partial class SplitterSet {
		public void Load(XmlNodeList list) {
			SplitterData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private Splitter Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, SplitterData.SplitterIdField.Field)
			};
			Splitter splitter = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreatePins(splitter);
			return splitter;
		}

		private void CreatePins(Splitter splitter) {
			// The order of creation of the pins is essential for expantion algorithm (CircuitMap.Connect).
			// The wide pin should go first and then thin pins starting from lower bits to higher
			Tracer.Assert(!this.CircuitProject.DevicePinSet.SelectByCircuit(splitter).Any());
			if(splitter.PinCount < 2) {
				splitter.PinCount = 2;
			}
			if(splitter.BitWidth < splitter.PinCount) {
				splitter.BitWidth = splitter.PinCount;
			}
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, splitter.BitWidth);
			pin.Name = Resources.SplitterWidePinName;
			PinSide pinSide;
			switch(splitter.Rotation) {
			case CircuitRotation.Up:
				pinSide = PinSide.Bottom;
				pin.PinSide = PinSide.Top;
				break;
			case CircuitRotation.Right:
				pinSide = PinSide.Left;
				pin.PinSide = PinSide.Right;
				break;
			case CircuitRotation.Down:
				pinSide = PinSide.Top;
				pin.PinSide = PinSide.Bottom;
				break;
			case CircuitRotation.Left:
				pinSide = PinSide.Right;
				pin.PinSide = PinSide.Left;
				break;
			default:
				Tracer.Fail();
				return;
			}
			int pinWidth = splitter.BitWidth / splitter.PinCount + Math.Sign(splitter.BitWidth % splitter.PinCount);
			int width = 0;
			for(int i = 0; i < splitter.PinCount; i++) {
				pinWidth = Math.Min(pinWidth, splitter.BitWidth - width);
				if(pinWidth < 1) {
					splitter.PinCount = i;
					Tracer.Assert(1 < i);
					break;
				}
				pin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, pinWidth);
				pin.PinSide = pinSide;
				if(pinWidth == 1) {
					pin.Name = Resources.SplitterThin1PinName(width);
				} else {
					pin.Name = Resources.SplitterThin2PinName(width, width + pinWidth - 1);
				}
				width += pinWidth;
			}
		}

		public Splitter Create(int bitWidth, int pinCount, CircuitRotation rotation) {
			Splitter splitter = this.CreateItem(Guid.NewGuid(), bitWidth, pinCount, rotation);
			this.CreatePins(splitter);
			return splitter;
		}

		public Splitter Copy(Splitter other) {
			SplitterData data;
			other.CircuitProject.SplitterSet.Table.GetData(other.SplitterRowId, out data);
			if(this.FindBySplitterId(data.SplitterId) != null) {
				data.SplitterId = Guid.NewGuid();
			}
			data.Splitter = null;
			return this.Register(this.Table.Insert(ref data));
		}
	}
}
