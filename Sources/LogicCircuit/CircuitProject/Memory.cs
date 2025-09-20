﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Xml;
using DataPersistent;

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
			get { return this.Writable ? Properties.Resources.RAMNotation : Properties.Resources.ROMNotation; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip {
			get { return this.AppendNote(this.Writable ? Properties.Resources.ToolTipRAM(this.AddressBitWidth, this.DataBitWidth) : Properties.Resources.ToolTipROM(this.AddressBitWidth, this.DataBitWidth)); }
		}

		public override string Category {
			get { return Properties.Resources.CategoryPrimitives; }
			set { throw new InvalidOperationException(); }
		}

		public DevicePin AddressPin { get; private set; }
		public DevicePin DataOutPin { get; private set; }
		public DevicePin DataInPin { get; private set; }
		public DevicePin WritePin { get; private set; }
		public DevicePin Address2Pin { get; private set; }
		public DevicePin DataOut2Pin { get; private set; }

		public override IEnumerable<BasePin> Pins => this.DualPort ? base.Pins : base.Pins.Where(p => p != this.Address2Pin && p != this.DataOut2Pin);

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
			// GraphicsArray can have up to 22 address bits
			Tracer.Assert(0 < addressBitWidth && addressBitWidth <= 22);
			return 1 << addressBitWidth;
		}

		public static void AssertAddressBitWidth(int addressBitWidth) {
			Tracer.Assert(0 < addressBitWidth && addressBitWidth <= Memory.MaxAddressBitWidth);
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
			Memory.AssertAddressBitWidth(oldAddressBitWidth);
			Memory.AssertAddressBitWidth(newAddressBitWidth);
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

		public byte[] MemoryValue() {
			string data = this.Data;
			byte[] d = !string.IsNullOrEmpty(data) ? Convert.FromBase64String(data) : new byte[this.BytesPerCell * this.TotalCells];
			if(d.Length != this.BytesPerCell * this.TotalCells) {
				d = Memory.Reallocate(d, this.AddressBitWidth, this.DataBitWidth);
			}
			return d;
		}

		public void SetMemoryValue(byte[]? value) {
			Tracer.Assert(value == null || value.Length == this.BytesPerCell * this.TotalCells);
			int index = ((value != null) ? value.Length : 0) - 1;
			while(0 <= index && value![index] == 0) {
				index--;
			}
			this.Data = (0 <= index) ? Convert.ToBase64String(value!, 0, index + 1, (index < 3 * 1024) ? Base64FormattingOptions.None : Base64FormattingOptions.InsertLineBreaks) : string.Empty;
		}

		internal void SetPins(DevicePin addressPin, DevicePin dataPin, DevicePin address2Pin, DevicePin data2Pin) {
			Tracer.Assert(!this.Writable);
			Tracer.Assert(this.AddressPin == null);
			Tracer.Assert(addressPin != null && dataPin != null);

			this.AddressPin = addressPin!;
			this.DataOutPin = dataPin!;
			this.Address2Pin = address2Pin;
			this.DataOut2Pin = data2Pin;

			Tracer.Assert(this.AddressPin.BitWidth == this.AddressBitWidth);
			Tracer.Assert(this.DataOutPin.BitWidth == this.DataBitWidth);
			Tracer.Assert(this.Address2Pin.BitWidth == this.AddressBitWidth);
			Tracer.Assert(this.DataOut2Pin.BitWidth == this.DataBitWidth);
		}

		internal void SetPins(DevicePin addressPin, DevicePin dataOutPin, DevicePin dataInPin, DevicePin writePin, DevicePin address2Pin, DevicePin data2Pin) {
			Tracer.Assert(this.Writable);
			Tracer.Assert(this.AddressPin == null);
			Tracer.Assert(addressPin != null && dataOutPin != null && dataInPin != null && writePin != null);

			this.AddressPin = addressPin!;
			this.DataOutPin = dataOutPin!;
			this.DataInPin = dataInPin!;
			this.WritePin = writePin!;
			this.Address2Pin = address2Pin;
			this.DataOut2Pin = data2Pin;

			Tracer.Assert(this.AddressPin.BitWidth == this.AddressBitWidth);
			Tracer.Assert(this.DataInPin.BitWidth == this.DataOutPin.BitWidth && this.DataOutPin.BitWidth == this.DataBitWidth);
			Tracer.Assert(this.WritePin.BitWidth == 1);
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.MemorySet.Copy(this);
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			Tracer.Assert(defaultWidth == (this.Writable ? 2 : 1));
			return 3;
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			Tracer.Assert(defaultHeight == ((this.Writable ? 3 : 2) + (this.DualPort ? 1 : 0)));
			return Math.Max(4, defaultHeight);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateRectangularGlyph();
		}

		partial void OnMemoryChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}

		private string AppendNote(string toolTip) {
			return Circuit.BuildToolTip(toolTip, this.Note);
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
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
			address.Name = Properties.Resources.MemoryAddressPinName;
			address.JamNotation = Properties.Resources.MemoryAddressPinNotation;
			DevicePin data = this.CircuitProject.DevicePinSet.Create(memory, PinType.Output, memory.DataBitWidth);
			data.PinSide = PinSide.Right;
			data.Name = Properties.Resources.MemoryDataPinName;
			data.JamNotation = Properties.Resources.MemoryDataPinNotation;

			DevicePin dataIn = null!;
			DevicePin write = null!;
			if(memory.Writable) {
				dataIn = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, memory.DataBitWidth);
				dataIn.PinSide = PinSide.Left;
				dataIn.Name = Properties.Resources.MemoryDataInPinName;
				dataIn.JamNotation = Properties.Resources.MemoryDataPinNotation;
				write = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, 1);
				write.PinSide = PinSide.Bottom;
				write.Name = Properties.Resources.MemoryWritePinName(memory.WriteOn1 ? Properties.Resources.WriteOn1 : Properties.Resources.WriteOn0);
				write.JamNotation = Properties.Resources.MemoryWritePinNotation;
			}

			DevicePin address2 = this.CircuitProject.DevicePinSet.Create(memory, PinType.Input, memory.AddressBitWidth);
			address2.PinSide = PinSide.Left;
			address2.Name = Properties.Resources.MemoryAddress2PinName;
			address2.JamNotation = Properties.Resources.MemoryAddress2PinNotation;

			DevicePin data2 = this.CircuitProject.DevicePinSet.Create(memory, PinType.Output, memory.DataBitWidth);
			data2.PinSide = PinSide.Right;
			data2.Name = Properties.Resources.MemoryData2PinName;
			data2.JamNotation = Properties.Resources.MemoryData2PinNotation;

			if(memory.Writable) {
				memory.SetPins(address, data, dataIn, write, address2, data2);
				MemorySet.UpdateWritePinName(memory);
			} else {
				memory.SetPins(address, data, address2, data2);
			}
		}

		public static void UpdateWritePinName(Memory memory) {
			Tracer.Assert(memory.Writable);
			memory.WritePin.Name = Properties.Resources.MemoryWritePinName(memory.WriteOn1 ? Properties.Resources.WriteOn1 : Properties.Resources.WriteOn0);
		}

		public Memory Create(bool writable, int addressBitWidth, int dataBitWidth) {
			Memory memory = this.CreateItem(Guid.NewGuid(), writable, MemoryData.WriteOn1Field.Field.DefaultValue, MemoryData.OnStartField.Field.DefaultValue,
				addressBitWidth, dataBitWidth, MemoryData.DualPortField.Field.DefaultValue, MemoryData.DataField.Field.DefaultValue, MemoryData.NoteField.Field.DefaultValue
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

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<MemoryData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
