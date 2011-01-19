using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class LogicalCircuit {

		public Point ScrollOffset { get; set; }

		public override void Delete() {
			this.CircuitSymbols().ToList().ForEach(symbol => symbol.DeleteSymbol());
			base.Delete();
		}

		public IEnumerable<Pin> LogicalPins {
			get { return this.CircuitProject.PinSet.SelectByCircuit(this); }
		}

		public override IEnumerable<BasePin> Pins {
			get { return (IEnumerable<BasePin>)this.LogicalPins; }
		}

		public override string ToolTip {
			get {
				string d = this.Description;
				if(!string.IsNullOrEmpty(d)) {
					return this.Name + "\n" + d;
				}
				return this.Name;
			}
		}

		public IEnumerable<CircuitSymbol> CircuitSymbols() {
			return this.CircuitProject.CircuitSymbolSet.SelectByLogicalCircuit(this);
		}

		public IEnumerable<Wire> Wires() {
			return this.CircuitProject.WireSet.SelectByLogicalCircuit(this);
		}

		public bool IsEmpty() {
			return !this.CircuitSymbols().Any() && !this.Wires().Any();
		}

		public override Circuit CopyTo(CircuitProject project) {
			return project.LogicalCircuitSet.Copy(this, true);
		}

		public void Rename(string name) {
			if(LogicalCircuitData.NameField.Field.Compare(this.Name, name) != 0) {
				this.Name = this.CircuitProject.LogicalCircuitSet.UniqueName(name);
			}
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateRectangularGlyph();
		}
	}

	public partial class LogicalCircuitSet : NamedItemSet {
		public event EventHandler LogicalCircuitSetChanged;

		public void Load(XmlNodeList list) {
			LogicalCircuitData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private LogicalCircuit Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, LogicalCircuitData.LogicalCircuitIdField.Field)
			};
			return this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
		}

		protected override bool Exists(string name) {
			return this.FindByName(name) != null;
		}

		protected override bool Exists(string name, Circuit group) {
			throw new NotSupportedException();
		}

		public IEnumerable<LogicalCircuit> SelectByCategory(string category) {
			return this.Select(this.Table.Select(LogicalCircuitData.CategoryField.Field, category));
		}

		public LogicalCircuit Create() {
			return this.CreateItem(Guid.NewGuid(),
				this.UniqueName(Resources.LogicalCircuitName),
				LogicalCircuitData.NotationField.Field.DefaultValue,
				LogicalCircuitData.DescriptionField.Field.DefaultValue,
				LogicalCircuitData.CategoryField.Field.DefaultValue
			);
		}

		public LogicalCircuit Copy(LogicalCircuit other, bool deepCopy) {
			LogicalCircuit logicalCircuit = this.FindByLogicalCircuitId(other.LogicalCircuitId);
			Tracer.Assert(deepCopy || logicalCircuit == null);
			if(logicalCircuit == null) {
				LogicalCircuitData data;
				other.CircuitProject.LogicalCircuitSet.Table.GetData(other.LogicalCircuitRowId, out data);
				data.Name = this.UniqueName(data.Name);
				logicalCircuit = this.Register(this.Table.Insert(ref data));
				if(deepCopy) {
					CircuitProject circuitProject = this.CircuitProject;
					foreach(CircuitSymbol symbol in other.CircuitSymbols()) {
						symbol.CopyTo(circuitProject);
					}
					foreach(Wire wire in other.Wires()) {
						wire.CopyTo(circuitProject);
					}
				}
			}
			return logicalCircuit;
		}

		partial void EndNotifyLogicalCircuitSetChanged() {
			//TODO: this is only used for updatelist of descriptors. Consider removing it.
			EventHandler handler = this.LogicalCircuitSetChanged;
			if(handler != null) {
				handler(this, EventArgs.Empty);
			}
		}
	}
}
