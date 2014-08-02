using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

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
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip {
			get { return Circuit.BuildToolTip(Properties.Resources.CircuitProbeToolTip(this.DisplayName).Trim(), this.Note); }
		}

		public override string Category {
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public bool HasName {
			get { return !StringComparer.OrdinalIgnoreCase.Equals(this.CircuitProbeId.ToString(), this.Name); }
		}

		public string DisplayName {
			get { return this.HasName ? this.Name : string.Empty; }
		}

		public void Rename(string name) {
			name = string.IsNullOrWhiteSpace(name) ? this.CircuitProbeId.ToString() : name.Trim();
			if(CircuitProbeData.NameField.Field.Compare(this.DisplayName, name) != 0) {
				this.Name = this.CircuitProject.CircuitProbeSet.UniqueName(name);
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
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class CircuitProbeSet : NamedItemSet {
		private CircuitProbe Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, CircuitProbeData.CircuitProbeIdField.Field)
			};
			CircuitProbe probe = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CircuitProject.DevicePinSet.Create(probe, PinType.Input, 1);
			return probe;
		}

		public CircuitProbe Create(string name) {
			Guid id = Guid.NewGuid();
			CircuitProbe probe = this.CreateItem(id, string.IsNullOrWhiteSpace(name) ? id.ToString() : this.UniqueName(name.Trim()), CircuitProbeData.NoteField.Field.DefaultValue);
			this.CircuitProject.DevicePinSet.Create(probe, PinType.Input, 1);
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
