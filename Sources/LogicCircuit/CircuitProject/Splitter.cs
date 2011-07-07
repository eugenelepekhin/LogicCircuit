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
			//TODO: revert to throwing
			get { return string.Empty; /*throw new InvalidOperationException();*/ }
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
			if(splitter.Clockwise) {
				pinSide = PinSide.Right;
				pin.PinSide = PinSide.Left;
			} else {
				pinSide = PinSide.Left;
				pin.PinSide = PinSide.Right;
			}
			int pinWidth = splitter.BitWidth / splitter.PinCount;
			int rem = splitter.BitWidth % splitter.PinCount;
			for(int i = 0; i < rem; i++) {
				pin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, pinWidth + 1);
				pin.PinSide = pinSide;
				SplitterSet.SetName(pin, i * (pinWidth + 1), pinWidth + 1);
			}
			for(int i = rem; i < splitter.PinCount; i++) {
				pin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, pinWidth);
				pin.PinSide = pinSide;
				SplitterSet.SetName(pin, i * pinWidth + rem, pinWidth);
			}
		}

		private static void SetName(DevicePin pin, int firstBit, int pinWidth) {
			if(pinWidth == 1) {
				pin.Name = Resources.SplitterThin1PinName(firstBit);
			} else {
				pin.Name = Resources.SplitterThin2PinName(firstBit, firstBit + pinWidth - 1);
			}
		}

		public Splitter Create(int bitWidth, int pinCount, bool clockwise) {
			Splitter splitter = this.CreateItem(Guid.NewGuid(), bitWidth, pinCount, clockwise);
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
