using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class GraphicsArray {
		public const int MaxBitsPerPixel = 8;
		public const int MaxWidth = 640;
		public const int MaxHeight = 480;

		public static MemoryOnStart CheckOnStart(MemoryOnStart value) {
			return (MemoryOnStart.Random <= value && value <= MemoryOnStart.Ones) ? value : MemoryOnStart.Zeros;
		}

		private static int CheckPower2(int maxPower, int value) {
			int max = 1;
			for(int power = 0; power < maxPower; power++) {
				if(value <= max) {
					return max;
				}
				max <<= 1;
			}
			return max;
		}

		public static int CheckBitWidth(int value) {
			return CheckPower2(5, value);
		}

		public static int CheckBitsPerPixel(int value) {
			return CheckPower2(3, value);
		}

		public static int CheckWidth(int value) {
			return Math.Max(1, Math.Min(value, GraphicsArray.MaxWidth));
		}

		public static int CheckHeight(int value) {
			return Math.Max(1, Math.Min(value, GraphicsArray.MaxHeight));
		}

		public static int NumberCellsFor(int dataBitWidth, int bitsPerPixel, int width, int height) {
			int w = width * bitsPerPixel;
			int stride = w / dataBitWidth + (((w % dataBitWidth) == 0) ? 0 : 1);
			return stride * height;
		}

		public static int AddressBitsFor(int dataBitWidth, int bitsPerPixel, int width, int height) {
			int bits = 0;
			int max = GraphicsArray.NumberCellsFor(dataBitWidth, bitsPerPixel, width, height) - 1;
			while(max != 0) {
				bits++;
				max >>= 1;
			}
			return Math.Max(1, bits);
		}

		public int AddressBitWidth {
			get { return GraphicsArray.AddressBitsFor(this.DataBitWidth, this.BitsPerPixel, this.Width, this.Height); }
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
			get { return Properties.Resources.NameGraphicsArray; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip {
			get {
				int bpp = this.BitsPerPixel;
				return Circuit.BuildToolTip(Properties.Resources.ToolTipGraphicsArray(this.Name, this.Width, this.Height, bpp, 1 << bpp), this.Note);
			}
		}

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public DevicePin AddressPin { get; private set; }
		public DevicePin DataOutPin { get; private set; }
		public DevicePin DataInPin { get; private set; }
		public DevicePin WritePin { get; private set; }

		internal void SetPins(DevicePin addressPin, DevicePin dataOutPin, DevicePin dataInPin, DevicePin writePin) {
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
			return target.CircuitProject.GraphicsArraySet.Copy(this);
		}

		public override bool IsDisplay {
			get { return true; }
			set { base.IsDisplay = value; }
		}

		private static int PixelsToGridSize(int pixels) {
			pixels += 3;
			return pixels / Symbol.GridSize + (((pixels % Symbol.GridSize) == 0) ? 0 : 1);
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			return base.CircuitSymbolWidth(Math.Max(2, GraphicsArray.PixelsToGridSize(this.Width)));
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			return base.CircuitSymbolHeight(Math.Max(3, GraphicsArray.PixelsToGridSize(this.Height)));
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateSimpleGlyph(SymbolShape.GraphicsArray, symbol);
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateSimpleGlyph(SymbolShape.GraphicsArray, mainSymbol);
		}

		partial void OnGraphicsArrayChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class GraphicsArraySet {

		public static void UpdateWritePinName(GraphicsArray graphicsArray) {
			graphicsArray.WritePin.Name = Properties.Resources.MemoryWritePinName(graphicsArray.WriteOn1 ? Properties.Resources.WriteOn1 : Properties.Resources.WriteOn0);
		}

		private GraphicsArray Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, GraphicsArrayData.GraphicsArrayIdField.Field)
			};
			GraphicsArray graphicsArray = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePins(graphicsArray);
			return graphicsArray;
		}

		private void CreateDevicePins(GraphicsArray graphicsArray) {
			Tracer.Assert(!this.CircuitProject.DevicePinSet.SelectByCircuit(graphicsArray).Any());
			// The order of creation of the pins is essential for expansion algorithm.

			DevicePin address = this.CircuitProject.DevicePinSet.Create(graphicsArray, PinType.Input, graphicsArray.AddressBitWidth);
			address.PinSide = PinSide.Left;
			address.Name = Properties.Resources.MemoryAddressPinName;
			DevicePin data = this.CircuitProject.DevicePinSet.Create(graphicsArray, PinType.Output, graphicsArray.DataBitWidth);
			data.PinSide = PinSide.Right;
			data.Name = Properties.Resources.MemoryDataPinName;

			DevicePin dataIn = this.CircuitProject.DevicePinSet.Create(graphicsArray, PinType.Input, graphicsArray.DataBitWidth);
			dataIn.PinSide = PinSide.Left;
			dataIn.Name = Properties.Resources.MemoryDataInPinName;
			DevicePin write = this.CircuitProject.DevicePinSet.Create(graphicsArray, PinType.Input, 1);
			write.PinSide = PinSide.Bottom;
			write.Name = Properties.Resources.MemoryWritePinName(graphicsArray.WriteOn1 ? Properties.Resources.WriteOn1 : Properties.Resources.WriteOn0);
			graphicsArray.SetPins(address, data, dataIn, write);
			GraphicsArraySet.UpdateWritePinName(graphicsArray);
		}

		public GraphicsArray Create(int dataBitWidth, int bitsPerPixel, int width, int height) {
			GraphicsArray graphicsArray = this.CreateItem(Guid.NewGuid(),
				GraphicsArrayData.WriteOn1Field.Field.DefaultValue,
				GraphicsArrayData.OnStartField.Field.DefaultValue,
				dataBitWidth,
				bitsPerPixel,
				width,
				height,
				GraphicsArrayData.NoteField.Field.DefaultValue
			);
			this.CreateDevicePins(graphicsArray);
			return graphicsArray;
		}

		public GraphicsArray Copy(GraphicsArray other) {
			GraphicsArrayData data;
			other.CircuitProject.GraphicsArraySet.Table.GetData(other.GraphicsArrayRowId, out data);
			if(this.FindByGraphicsArrayId(data.GraphicsArrayId) != null) {
				data.GraphicsArrayId = Guid.NewGuid();
			}
			data.GraphicsArray = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<GraphicsArrayData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
