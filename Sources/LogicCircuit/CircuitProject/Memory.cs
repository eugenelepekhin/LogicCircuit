using System;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Memory {

		public const int MaxAddressBitWidth = 16;

		public static int CheckAddressBitWidth(int value) {
			return Math.Max(1, Math.Min(value, Memory.MaxAddressBitWidth));
		}

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Name {
			get { return this.Notation; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return this.Writable ? Resources.RAMNotation : Resources.ROMNotation; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip {
			get { return this.AppendNote(this.Writable ? Resources.ToolTipRAM(this.AddressBitWidth, this.DataBitWidth) : Resources.ToolTipROM(this.AddressBitWidth, this.DataBitWidth)); }
		}

		public override string Category {
			get { return Resources.CategoryPrimitives; }
			set { throw new InvalidOperationException(); }
		}

		public DevicePin AddressPin { get; private set; }
		public DevicePin DataOutPin { get; private set; }
		public DevicePin DataInPin { get; private set; }
		public DevicePin WritePin { get; private set; }

		public int BytesPerCell {
			get { return Memory.BytesPerCellFor(this.DataBitWidth); }
		}

		public int TotalCells {
			get { return Memory.NumberCellsFor(this.AddressBitWidth); }
		}

		public int TotalBytes {
			get { return this.BytesPerCell * this.TotalCells; }
		}

		public static int BytesPerCellFor(int dataBitWidth) {
			Tracer.Assert(0 < dataBitWidth && dataBitWidth <= Pin.MaxBitWidth);
			return dataBitWidth / 8 + (((dataBitWidth % 8) == 0) ? 0 : 1);
		}

		public static int NumberCellsFor(int addressBitWidth) {
			Tracer.Assert(0 < addressBitWidth && addressBitWidth <= Memory.MaxAddressBitWidth);
			return 1 << addressBitWidth;
		}

		public static int CellValue(byte[] data, int bitWidth, int index) {
			int cellSize = Memory.BytesPerCellFor(bitWidth);
			int cellStart = index * cellSize;
			int value = 0;
			for(int i = 0; i < cellSize; i++) {
				value |= ((int)data[cellStart + i]) << (i * 8);
			}
			value = Constant.Normalize(value, bitWidth);
			return value;
		}

		public static void SetCellValue(byte[] data, int bitWidth, int index, int value) {
			int cellSize = Memory.BytesPerCellFor(bitWidth);
			int cellStart = index * cellSize;
			value = Constant.Normalize(value, bitWidth);
			for(int i = 0; i < cellSize; i++) {
				data[cellStart + i] = (byte)(value >> (i * 8));
			}
		}

		public static byte[] Reallocate(byte[] old, int oldAddressBitWidth, int oldDataBitWidth, int newAddressBitWidth, int newDataBitWidth) {
			Tracer.Assert(old.Length == Memory.BytesPerCellFor(oldDataBitWidth) * Memory.NumberCellsFor(oldAddressBitWidth));
			if(oldAddressBitWidth != newAddressBitWidth || oldDataBitWidth != newDataBitWidth) {
				byte[] data = new byte[Memory.BytesPerCellFor(newDataBitWidth) * Memory.NumberCellsFor(newAddressBitWidth)];
				int count = Math.Min(Memory.NumberCellsFor(oldAddressBitWidth), Memory.NumberCellsFor(newAddressBitWidth));
				for(int i = 0; i < count; i++) {
					Memory.SetCellValue(data, newDataBitWidth, i, Memory.CellValue(old, oldDataBitWidth, i));
				}
				return data;
			}
			return old;
		}

		private static byte[] Reallocate(byte[] old, int addressBitWidth, int dataBitWidth) {
			int size = Memory.BytesPerCellFor(dataBitWidth) * Memory.NumberCellsFor(addressBitWidth);
			if(old == null || old.Length != size) {
				byte[] data = new byte[size];
				if(old != null) {
					Array.Copy(old, data, Math.Min(size, old.Length));
				}
				return data;
			}
			return old;
		}

		public byte[] RomValue() {
			Tracer.Assert(!this.Writable);
			byte[] d = Convert.FromBase64String(this.Data);
			if(d.Length != this.BytesPerCell * this.TotalCells) {
				d = Memory.Reallocate(d, this.AddressBitWidth, this.DataBitWidth);
			}
			return d;
		}

		public void SetRomValue(byte[] value) {
			Tracer.Assert(!this.Writable);
			Tracer.Assert(value != null && value.Length == this.BytesPerCell * this.TotalCells);
			this.Data = Convert.ToBase64String(value, Base64FormattingOptions.InsertLineBreaks);
		}

		internal void SetPins(DevicePin addressPin, DevicePin dataPin) {
			Tracer.Assert(!this.Writable);
			Tracer.Assert(this.AddressPin == null);
			Tracer.Assert(addressPin != null && dataPin != null);

			this.AddressPin = addressPin;
			this.DataOutPin = dataPin;

			Tracer.Assert(addressPin.BitWidth == this.AddressBitWidth);
			Tracer.Assert(dataPin.BitWidth == this.DataBitWidth);
		}

		internal void SetPins(DevicePin addressPin, DevicePin dataOutPin, DevicePin dataInPin, DevicePin writePin) {
			Tracer.Assert(this.Writable);
			Tracer.Assert(this.AddressPin == null);
			Tracer.Assert(addressPin != null && dataOutPin != null && dataInPin != null && writePin != null);

			this.AddressPin = addressPin;
			this.DataOutPin = dataOutPin;
			this.DataInPin = dataInPin;
			this.WritePin = writePin;

			Tracer.Assert(addressPin.BitWidth == this.AddressBitWidth);
			Tracer.Assert(dataInPin.BitWidth == dataOutPin.BitWidth && dataOutPin.BitWidth == this.DataBitWidth);
			Tracer.Assert(writePin.BitWidth == 1);
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.MemorySet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateRectangularGlyph();
		}

		partial void OnMemoryChanged() {
			this.InvalidateDistinctSymbol();
		}

		private string AppendNote(string toolTip) {
			return Circuit.BuildToolTip(toolTip, this.Note);
		}
	}

	public sealed partial class MemorySet {
		private Memory Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, MemoryData.MemoryIdField.Field)
			};
			Memory memory = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreatePins(memory);
			return memory;
		}

		private void CreatePins(Memory memory) {
			Tracer.Assert(!this.CircuitProject.DevicePinSet.SelectByCircuit(memory).Any());
			// The order of creation of the pins is essential for expansion algorithm.

			DevicePin address = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, memory.AddressBitWidth);
			address.PinSide = PinSide.Left;
			address.Name = Resources.MemoryAddressPinName;
			address.JamNotation = Resources.MemoryAddressPinNotation;
			DevicePin data = this.CircuitProject.DevicePinSet.Create(memory, PinType.Output, memory.DataBitWidth);
			data.PinSide = PinSide.Right;
			data.Name = Resources.MemoryDataPinName;
			data.JamNotation = Resources.MemoryDataPinNotation;
			if(memory.Writable) {
				DevicePin dataIn = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, memory.DataBitWidth);
				dataIn.PinSide = PinSide.Left;
				dataIn.Name = Resources.MemoryDataInPinName;
				dataIn.JamNotation = Resources.MemoryDataPinNotation;
				DevicePin write = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, 1);
				write.PinSide = PinSide.Bottom;
				write.Name = Resources.MemoryWritePinName(memory.WriteOn1 ? Resources.WriteOn1 : Resources.WriteOn0);
				write.JamNotation = Resources.MemoryWritePinNotation;
				memory.SetPins(address, data, dataIn, write);
				MemorySet.UpdateWritePinName(memory);
			} else {
				memory.SetPins(address, data);
			}
		}

		public static void UpdateWritePinName(Memory memory) {
			Tracer.Assert(memory.Writable);
			memory.WritePin.Name = Resources.MemoryWritePinName(memory.WriteOn1 ? Resources.WriteOn1 : Resources.WriteOn0);
		}

		public Memory Create(bool writable, int addressBitWidth, int dataBitWidth) {
			Memory memory = this.CreateItem(Guid.NewGuid(), writable, MemoryData.WriteOn1Field.Field.DefaultValue, addressBitWidth, dataBitWidth,
				MemoryData.DataField.Field.DefaultValue, MemoryData.NoteField.Field.DefaultValue
			);
			this.CreatePins(memory);
			return memory;
		}

		public Memory Copy(Memory other) {
			MemoryData data;
			other.CircuitProject.MemorySet.Table.GetData(other.MemoryRowId, out data);
			if(this.FindByMemoryId(data.MemoryId) != null) {
				data.MemoryId = Guid.NewGuid();
			}
			data.Memory = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public ARecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<MemoryData>(nameTable, this.Table, this.Table.Fields, rowId => this.Register(rowId));
		}
	}
}
