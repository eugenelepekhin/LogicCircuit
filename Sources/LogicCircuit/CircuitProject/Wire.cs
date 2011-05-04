using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml;

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
			Panel.SetZIndex(line, 0);
			return line;
		}

		public override void Shift(int dx, int dy) {
			this.X1 += dx;
			this.Y1 += dy;
			this.X2 += dx;
			this.Y2 += dy;
		}

		public override Symbol CopyTo(LogicalCircuit target) {
			return target.CircuitProject.WireSet.Copy(this, target);
		}

		partial void OnWireChanged() {
			this.PositionGlyph();
		}
	}

	public partial class WireSet {
		public event EventHandler WireSetChanged;

		public void Load(XmlNodeList list) {
			WireData.Load(this.Table, list, rowId => this.Create(rowId));
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

		partial void EndNotifyWireSetChanged() {
			EventHandler handler = this.WireSetChanged;
			if(handler != null) {
				handler(this, EventArgs.Empty);
			}
		}
	}
}
