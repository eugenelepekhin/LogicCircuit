using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Wire {
		private Line glyph = null;

		public GridPoint Point1 {
			get { return new GridPoint(this.X1, this.Y1); }
			set { this.X1 = value.X; this.Y1 = value.Y; }
		}

		public GridPoint Point2 {
			get { return new GridPoint(this.X2, this.Y2); }
			set { this.X2 = value.X; this.Y2 = value.Y; }
		}

		public override int Z { get { return 1; } }

		public override FrameworkElement Glyph { get { return this.WireGlyph; } }
		public override bool HasCreatedGlyph { get { return this.glyph != null; } }

		public Line WireGlyph {
			get { return this.glyph ?? (this.glyph = this.CreateGlyph()); }
		}

		public void Reset() {
			this.glyph = null;
		}

		public override void PositionGlyph() {
			Line line = this.WireGlyph;
			line.X1 = Symbol.ScreenPoint(this.X1);
			line.Y1 = Symbol.ScreenPoint(this.Y1);
			line.X2 = Symbol.ScreenPoint(this.X2);
			line.Y2 = Symbol.ScreenPoint(this.Y2);
		}

		public Line CreateGlyph() {
			Line line = new Line();
			line.Stroke = Symbol.WireStroke;
			line.StrokeThickness = 1;
			line.ToolTip = Resources.ToolTipWire;
			line.DataContext = this;
			Panel.SetZIndex(line, this.Z);
			return line;
		}

		public override void Shift(int dx, int dy) {
			this.X1 += dx;
			this.Y1 += dy;
			this.X2 += dx;
			this.Y2 += dy;
		}

		public override void DeleteSymbol() {
			this.Delete();
		}

		public override Symbol CopyTo(LogicalCircuit target) {
			return target.CircuitProject.WireSet.Copy(this, target);
		}

		partial void OnWireChanged() {
			this.PositionGlyph();
		}
	}

	public sealed partial class WireSet : IRecordLoader {
		public event EventHandler WireSetChanged;

		//Holds logical circuits that holds wires that was changed in latest transaction
		private HashSet<LogicalCircuit> invalidLogicalCircuit = null;

		void IRecordLoader.Load(XmlReader reader) {
			this.Create(WireData.Load(this.Table, reader));
		}

		public Wire Create(LogicalCircuit logicalCircuit, GridPoint point1, GridPoint point2) {
			return this.CreateItem(Guid.NewGuid(), logicalCircuit, point1.X, point1.Y, point2.X, point2.Y);
		}

		public Wire Copy(Wire other, LogicalCircuit target) {
			WireData data;
			other.CircuitProject.WireSet.Table.GetData(other.WireRowId, out data);
			if(this.Find(data.WireId) != null) {
				data.WireId = Guid.NewGuid();
			}
			data.LogicalCircuitId = target.LogicalCircuitId;
			data.Wire = null;
			return this.Create(this.Table.Insert(ref data));
		}

		partial void NotifyWireSetChanged(TableChange<WireData> change) {
			if(this.invalidLogicalCircuit != null) {
				LogicalCircuit circuit = this.CircuitProject.LogicalCircuitSet.FindByLogicalCircuitId(
					(change.Action == SnapTableAction.Delete) ? change.GetOldField(WireData.LogicalCircuitIdField.Field) : change.GetNewField(WireData.LogicalCircuitIdField.Field)
				);
				if(circuit != null) {
					this.invalidLogicalCircuit.Add(circuit);
				}
			}
		}

		partial void EndNotifyWireSetChanged() {
			EventHandler handler = this.WireSetChanged;
			if(handler != null) {
				handler(this, EventArgs.Empty);
			}
			if(this.invalidLogicalCircuit != null) {
				foreach(LogicalCircuit circuit in this.invalidLogicalCircuit) {
					circuit.UpdateConductorMap();
				}
				this.invalidLogicalCircuit.Clear();
			} else {
				// This is optimization to avoid the very first transaction that is loading of project. So in loading just update all the logical circuits.
				this.invalidLogicalCircuit = new HashSet<LogicalCircuit>();
				this.CircuitProject.LogicalCircuitSet.UpdateConductorMaps();
			}
		}
	}
}
