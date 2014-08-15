using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LogicCircuit {
	public class FunctionGraphicsArray : CircuitFunction, IFunctionVisual, IFunctionMemory {
		public override string ReportName { get { return Properties.Resources.NameGraphicsArray; } }
		public bool Invalid { get; set; }

		private readonly List<CircuitSymbol> circuitSymbol;
		private readonly Project project;

		private readonly int[] address;
		private readonly int[] inputData;
		private readonly int write;
		private readonly State writeOn;
		private State oldWriteState = State.Off;

		private readonly int bitsPerPixel;
		private readonly Int32Rect drawingRect;
		private readonly int memoryStride;
		private readonly int bitmapStride;

		private readonly byte[] data;
		private WriteableBitmap bitmap;
		private LogicalCircuit lastLogicalCircuit = null;
		private Image lastImage = null;

		public int AddressBitWidth { get { return this.address.Length; } }
		public int DataBitWidth { get { return this.inputData.Length; } }
		public int this[int index] { get { return this.Read(index); } }

		public FunctionGraphicsArray(
			CircuitState circuitState,
			int[] address,
			int[] inputData,
			int[] outputData,
			int write,
			IEnumerable<CircuitSymbol> symbols
		) : base(circuitState, FunctionMemory.Input(address, inputData, write), outputData) {

			this.circuitSymbol = symbols.ToList();
			Tracer.Assert(0 < this.circuitSymbol.Count);

			GraphicsArray graphicsArray = (GraphicsArray)this.circuitSymbol[0].Circuit;
			this.project = graphicsArray.CircuitProject.ProjectSet.Project;

			this.address = address;
			this.inputData = inputData;
			this.write = write;
			this.writeOn = graphicsArray.WriteOn1 ? State.On1 : State.On0;

			Tracer.Assert(this.inputData.Length == outputData.Length && this.inputData.Length == graphicsArray.DataBitWidth);

			this.bitsPerPixel = graphicsArray.BitsPerPixel;
			this.drawingRect = new Int32Rect(0, 0, graphicsArray.Width, graphicsArray.Height);
			
			int w = this.drawingRect.Width * this.bitsPerPixel;
			this.memoryStride = w / this.DataBitWidth + (((w % this.DataBitWidth) == 0) ? 0 : 1);
			int byteStride = w / 8 + (((w % 8) == 0) ? 0 : 1);
			this.bitmapStride = Math.Max(byteStride * 8, this.memoryStride * this.DataBitWidth) / 8;
			Tracer.Assert(this.memoryStride * this.DataBitWidth <= this.bitmapStride * 8);

			switch(graphicsArray.OnStart) {
			case MemoryOnStart.Random:
				this.data = this.Allocate();
				circuitState.Random.NextBytes(this.data);
				break;
			case MemoryOnStart.Zeros:
				this.data = this.Allocate();
				break;
			case MemoryOnStart.Ones:
				this.data = this.Allocate();
				for(int i = 0; i < this.data.Length; i++) {
					this.data[i] = 0xFF;
				}
				break;
			case MemoryOnStart.Data:
			default:
				Tracer.Fail();
				break;
			}
		}

		private byte[] Allocate() {
			// Allocate only needed for bitmap size.
			return new byte[this.bitmapStride * this.drawingRect.Height];
		}

		private void Write() {
			int addr = this.ReadNumericState(this.address);
			int row = addr / this.memoryStride;
			if(row < this.drawingRect.Height) {
				int value = this.ReadNumericState(this.inputData);
				int cell = addr % this.memoryStride;
				int firstByte = row * this.bitmapStride + cell * this.DataBitWidth / 8;
				if(this.DataBitWidth < 8) {
					int shift = (cell * this.DataBitWidth) % 8;
					int mask = ((1 << this.DataBitWidth) - 1) << (8 - shift - this.DataBitWidth);
					value = (value << (8 - shift - this.DataBitWidth)) & mask;
					value = value | (this.data[firstByte] & ~mask);
					this.data[firstByte] = (byte)value;
				} else {
					int count = this.DataBitWidth / 8;
					for(int i = 0; i < count; i++) {
						data[firstByte + i] = (byte)(value >> (i * 8));
					}
				}
			}
		}

		private int Read(int addr) {
			int row = addr / this.memoryStride;
			int value = 0;
			if(row < this.drawingRect.Height) {
				int cell = addr % this.memoryStride;
				int firstByte = row * this.bitmapStride + cell * this.DataBitWidth / 8;
				if(this.DataBitWidth < 8) {
					int shift = cell * this.DataBitWidth % 8;
					int mask = ((1 << this.DataBitWidth) - 1) << (8 - shift - this.DataBitWidth);
					value = (this.data[firstByte] & mask) >> (8 - shift - this.DataBitWidth);
				} else {
					int count = this.DataBitWidth / 8;
					for(int i = 0; i < count; i++) {
						value |= ((int)data[firstByte + i]) << (i * 8);
					}
				}
			}
			return value;
		}

		private bool Read() {
			int addr = this.ReadNumericState(this.address);
			return this.SetResult(this.Read(addr));
		}

		private bool IsWriteAllowed() {
			State state = this.CircuitState[this.write];
			bool allowed = (state == this.writeOn && CircuitFunction.Not(state) == this.oldWriteState);
			this.oldWriteState = state;
			return allowed;
		}

		public override bool Evaluate() {
			if(this.IsWriteAllowed()) {
				this.Write();
				this.Invalid = true;
			}
			return this.Read();
		}

		public void TurnOn() {
			// Bitmap should be created on UI thread, so this is the right place for it.
			PixelFormat format = new PixelFormat();
			BitmapPalette palette = null;
			switch(this.bitsPerPixel) {
			case 1:
				format = PixelFormats.Indexed1;
				palette = BitmapPalettes.BlackAndWhite;
				break;
			case 2:
				format = PixelFormats.Indexed2;
				palette = BitmapPalettes.Gray4;
				break;
			case 4:
				format = PixelFormats.Indexed4;
				palette = BitmapPalettes.Halftone8;
				break;
			case 8:
				format = PixelFormats.Indexed8;
				palette = BitmapPalettes.Halftone256;
				break;
			default:
				Tracer.Fail();
				break;
			}
			this.bitmap = new WriteableBitmap(this.drawingRect.Width, this.drawingRect.Height, 96, 96, format, palette);
			this.Invalid = true;
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol) {
				if(symbol.HasCreatedGlyph) {
					Image image = this.ProbeView(symbol);
					image.Source = null;
				}
			}
		}

		public void Redraw() {
			this.bitmap.WritePixels(this.drawingRect, this.data, this.bitmapStride, 0);

			LogicalCircuit currentCircuit = this.project.LogicalCircuit;
			if(this.lastLogicalCircuit != currentCircuit) {
				this.lastLogicalCircuit = currentCircuit;
				this.lastImage = null;
			}
			if(this.lastImage == null) {
				if(this.circuitSymbol.Count == 1) {
					this.lastImage = (Image)this.circuitSymbol[0].ProbeView;
				} else {
					CircuitSymbol symbol = this.circuitSymbol.First(s => s.LogicalCircuit == currentCircuit);
					this.lastImage = this.ProbeView(symbol);
				}
				this.lastImage.Source = this.bitmap;
				this.lastImage.Width = this.bitmap.Width;
				this.lastImage.Height = this.bitmap.Height;
			}
		}

		private Image ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return (Image)this.circuitSymbol[0].ProbeView;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return (Image)canvas.DisplayOf(this.circuitSymbol);
			}
		}
	}
}
