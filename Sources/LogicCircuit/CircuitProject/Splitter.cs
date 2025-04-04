using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Splitter {
		public static void Pass(Jam enterJam, int enterBit, out Jam exitJam, out int exitBit) {
			Tracer.Assert(enterJam.CircuitSymbol.Circuit is Splitter splitter && 0 <= enterBit && enterBit < enterJam.Pin.BitWidth);
			List<Jam> list = enterJam.CircuitSymbol.Jams().ToList();
			// Sort jams in order of their device pins. Assuming first one will be the wide pin and the rest are thin ones,
			// starting from lower bits to higher. This implies that creating of the pins should happened in that order.
			list.Sort(JamComparer.Comparer);
			Tracer.Assert(2 < list.Count && list[0].Pin.BitWidth == list.Skip(1).Sum(j => j.Pin.BitWidth));
			if(enterJam == list[0]) { //wide jam. so find thin one, this bit will be redirected to
				int width = 0;
				for(int i = 1; i < list.Count; i++) {
					if(enterBit < width + list[i].Pin.BitWidth) {
						exitJam = list[i];
						exitBit = enterBit - width;
						return;
					}
					width += list[i].Pin.BitWidth;
				}
			} else { // thin jam. find position of this bit in wide pin
				int width = 0;
				for(int i = 1; i < list.Count; i++) {
					if(enterJam == list[i]) {
						exitJam = list[0];
						exitBit = enterBit + width;
						return;
					}
					width += list[i].Pin.BitWidth;
				}
			}
			throw new InvalidOperationException();
		}

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Name {
			get { return Properties.Resources.NameSplitter; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return this.Name; }
			set { throw new InvalidOperationException(); }
		}

		public override string Note { get; set; }

		public override string ToolTip {
			get { return Circuit.BuildToolTip(Properties.Resources.ToolTipSplitter(this.BitWidth, this.PinCount), this.Note); }
		}

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.SplitterSet.Copy(this);
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			Tracer.Assert(defaultWidth == 1);
			return 1;
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			// With our new implementation, defaultHeight may not match PinCount+1 when BitWidth < PinCount
			// because we're creating only min(PinCount, BitWidth) pins plus the wide pin
			return base.CircuitSymbolHeight(defaultHeight);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			// Go back to using the original approach which should work
			return symbol.CreateSimpleGlyph(SymbolShape.Splitter, symbol);
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class SplitterSet {
		private Splitter Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, SplitterData.SplitterIdField.Field)
			};
			Splitter splitter = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreatePins(splitter);
			return splitter;
		}

		private void CreatePins(Splitter splitter) {
			// The order of creation of the pins is essential for expansion algorithm (CircuitMap.Connect).
			// The wide pin should go first and then thin pins starting from lower bits to higher
			Tracer.Assert(!this.CircuitProject.DevicePinSet.SelectByCircuit(splitter).Any());

			// Create the wide pin first
			DevicePin widePin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, splitter.BitWidth);
			widePin.Name = Properties.Resources.SplitterWidePinName;
			PinSide sideForThinPins;
			if(splitter.Clockwise) {
				sideForThinPins = PinSide.Right;
				widePin.PinSide = PinSide.Left;
			} else {
				sideForThinPins = PinSide.Left;
				widePin.PinSide = PinSide.Right;
			}

			// Create exactly the number of pins specified by PinCount, regardless of BitWidth
			for(int i = 0; i < splitter.PinCount; i++) {
				// Distribute bits appropriately
				int pinWidth;
				int startBit;
				
				if(splitter.BitWidth <= splitter.PinCount) {
					// Simple case: one bit per pin, or fewer
					// For the case where BitWidth < PinCount, some pins might get 0 bits
					// But we still create the pin for visual consistency
					pinWidth = (i < splitter.BitWidth) ? 1 : 0;
					startBit = i;
				} else {
					// Distribute bits evenly
					pinWidth = splitter.BitWidth / splitter.PinCount;
					int remainder = splitter.BitWidth % splitter.PinCount;
					
					// Add an extra bit to the first 'remainder' pins
					if(i < remainder) {
						pinWidth++;
						startBit = i * pinWidth;
					} else {
						startBit = (remainder * (pinWidth + 1)) + ((i - remainder) * pinWidth);
					}
				}
				
				DevicePin thinPin = this.CircuitProject.DevicePinSet.Create(splitter, PinType.None, Math.Max(pinWidth, 1));
				thinPin.PinSide = sideForThinPins;
				SplitterSet.SetName(thinPin, startBit, pinWidth);
			}
		}

		private static void SetName(DevicePin pin, int firstBit, int pinWidth) {
			if(pinWidth == 1) {
				pin.Name = Properties.Resources.SplitterThin1PinName(firstBit);
			} else {
				pin.Name = Properties.Resources.SplitterThin2PinName(firstBit, firstBit + pinWidth - 1);
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

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<SplitterData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
