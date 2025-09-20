using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml;
using DataPersistent;

namespace LogicCircuit {
	partial struct CircuitProbeData {
		private string CheckName(string value) {
			if(string.IsNullOrWhiteSpace(value)) {
				return this.CircuitProbeId.ToString();
			}
			return value.Trim();
		}
	}

	public partial class CircuitProbe {
		public override string Notation {
			get {
				string text = Properties.Resources.CircuitProbeNotation;
				if(this.HasName) {
					return text + "\n" + this.Name;
				}
				return text;
			}
			set => throw new InvalidOperationException();
		}

		public override string ToolTip => Circuit.BuildToolTip(Properties.Resources.CircuitProbeToolTip(this.DisplayName).Trim(), this.Note);

		public override string Category {
			get => Properties.Resources.CategoryInputOutput;
			set => throw new InvalidOperationException();
		}

		public bool HasName => !StringComparer.OrdinalIgnoreCase.Equals(this.CircuitProbeId.ToString(), this.Name);

		public string DisplayName => this.HasName ? this.Name : string.Empty;

		public void Rename(string name) {
			if(string.IsNullOrWhiteSpace(name)) {
				this.Name = this.CircuitProbeId.ToString();
			} else {
				name = name.Trim();
				if(CircuitProbeData.NameField.Field.Compare(this.DisplayName, name) != 0) {
					this.Name = this.CircuitProject.CircuitProbeSet.UniqueName(name);
				}
			}
		}

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.CircuitProbeSet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateSimpleGlyph(SymbolShape.Probe, symbol);
		}

		partial void OnCircuitProbeChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class CircuitProbeSet : NamedItemSet {
		private void CreateDevicePin(CircuitProbe probe) {
			DevicePin pin = this.CircuitProject.DevicePinSet.Create(probe, PinType.Input, 1);
			pin.PinSide = probe.PinSide;
		}

		private CircuitProbe Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, CircuitProbeData.CircuitProbeIdField.Field)
			};
			CircuitProbe probe = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreateDevicePin(probe);
			return probe;
		}

		public CircuitProbe Create(string? name, PinSide pinSide) {
			Guid id = Guid.NewGuid();
			CircuitProbe probe = this.CreateItem(id,
				string.IsNullOrWhiteSpace(name) ? id.ToString() : this.UniqueName(name.Trim()),
				pinSide,
				CircuitProbeData.NoteField.Field.DefaultValue
			);
			this.CreateDevicePin(probe);
			return probe;
		}

		public CircuitProbe Copy(CircuitProbe other) {
			CircuitProbeData data;
			other.CircuitProject.CircuitProbeSet.Table.GetData(other.CircuitProbeRowId, out data);
			if(this.FindByCircuitProbeId(data.CircuitProbeId) != null) {
				data.CircuitProbeId = Guid.NewGuid();
				if(!other.HasName) {
					data.Name = data.CircuitProbeId.ToString();
				}
			}
			data.Name = this.UniqueName(data.Name);
			data.CircuitProbe = null;
			return this.Register(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<CircuitProbeData>(nameTable, this.Table, rowId => this.Register(rowId));
		}

		protected override bool Exists(string name) {
			return this.FindByName(name) != null;
		}

		protected override bool Exists(string name, Circuit group) {
			throw new NotSupportedException();
		}
	}
}
