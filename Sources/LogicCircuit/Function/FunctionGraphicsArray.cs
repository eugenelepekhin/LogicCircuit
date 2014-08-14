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

		private GraphicsArray graphicsArray;
		private int[] address;
		private int[] inputData;
		private int[] outputData;
		private int write;
		private State writeOn;
		private State oldWriteState = State.Off;
		private byte[] data;
		private WriteableBitmap bitmap;
		private Int32Rect drawingRect;
		private int stride;
		private readonly List<CircuitSymbol> circuitSymbol;
		private readonly Project project;
		private LogicalCircuit lastLogicalCircuit = null;
		private Image lastImage = null;

		public int AddressBitWidth { get { return this.address.Length; } }
		public int DataBitWidth { get { return this.inputData.Length; } }

		public int this[int index] {
			get { return Memory.CellValue(this.data, this.DataBitWidth, index); }
		}

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
			this.graphicsArray = (GraphicsArray)this.circuitSymbol.First().Circuit;
			this.project = this.graphicsArray.CircuitProject.ProjectSet.Project;

			this.address = address;
			this.inputData = inputData;
			this.outputData = outputData;
			this.write = write;
			this.writeOn = this.graphicsArray.WriteOn1 ? State.On1 : State.On0;

			Tracer.Assert(this.inputData.Length == this.outputData.Length);

			switch(this.graphicsArray.OnStart) {
			case MemoryOnStart.Random:
				this.data = this.Allocate();
				circuitState.Random.NextBytes(this.data);

				#if DEBUG
				{
					int w = this.graphicsArray.Width;
					int h = this.graphicsArray.Height;
					int bpp = this.graphicsArray.BitsPerPixel;
					int c = 1 << bpp;
					int d = 8 / bpp;
					int s = w / d + Math.Sign(w % d);
					if(c < w) {
						for(int i = 0; i < c; i++) {
							for(int j = 0; j < h; j++) {
								this.data[j * s + i] = (byte)i;
							}
						}
					}
				}
				#endif

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
			return new byte[Memory.BytesPerCellFor(this.DataBitWidth) * Memory.NumberCellsFor(this.address.Length)];
		}

		private void Write() {
			Memory.SetCellValue(this.data, this.DataBitWidth, this.ReadNumericState(this.address), this.ReadNumericState(this.inputData));
		}

		private bool Read() {
			return this.SetResult(Memory.CellValue(this.data, this.DataBitWidth, this.ReadNumericState(this.address)));
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
			int bitsPerPixel = this.graphicsArray.BitsPerPixel;
			this.drawingRect = new Int32Rect(0, 0, this.graphicsArray.Width, this.graphicsArray.Height);
			int w = this.drawingRect.Width * bitsPerPixel;
			this.stride = w / 8 + (((w % 8) == 0) ? 0 : 1);

			PixelFormat format = new PixelFormat();
			BitmapPalette palette = null;

			switch(bitsPerPixel) {
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
			this.bitmap.WritePixels(this.drawingRect, this.data, this.stride, 0);

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
