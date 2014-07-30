using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Sound {

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Name {
			get { return Properties.Resources.NameSound; }
			set { throw new NotSupportedException(); }
		}

		public override string ToolTip { get { return Circuit.BuildToolTip(Properties.Resources.ToolTipSound(this.Notation), this.Note); } }

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.SoundSet.Copy(this);
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			return base.CircuitSymbolWidth(3);
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			return base.CircuitSymbolHeight(3);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateSimpleGlyph(SymbolShape.Sound, symbol);
		}

		partial void OnSoundChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class SoundSet {
		private Sound Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, SoundData.SoundIdField.Field)
			};
			Sound sound = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePin(sound);
			return sound;
		}

		private void CreateDevicePin(Sound sound) {
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(sound, PinType.Input, 1);
			pin.PinSide = sound.PinSide;
		}

		public Sound Create(PinSide pinSide, string notation) {
			Sound sound = this.CreateItem(Guid.NewGuid(), SoundData.LoopingField.Field.DefaultValue, pinSide, notation, SoundData.NoteField.Field.DefaultValue, SoundData.DataField.Field.DefaultValue);
			this.CreateDevicePin(sound);
			return sound;
		}

		public Sound Copy(Sound other) {
			SoundData data;
			other.CircuitProject.SoundSet.Table.GetData(other.SoundRowId, out data);
			if(this.FindBySoundId(data.SoundId) != null) {
				data.SoundId = Guid.NewGuid();
			}
			data.Sound = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<SoundData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
