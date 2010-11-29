using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
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

		public override FrameworkElement Glyph {
			get { return this.glyph ?? (this.glyph = Plotter.CreateGlyph(this)); }
		}

		public override void Shift(int x, int y) {
			this.X1 += x;
			this.Y1 += y;
			this.X2 += x;
			this.Y2 += y;
		}

		public override void CopyTo(CircuitProject project) {
			project.WireSet.Copy(this);
		}
	}

	public partial class WireSet {
		public void Load(XmlNodeList list) {
			WireData.Load(this.Table, list, rowId => this.Create(rowId));
		}

		public Wire Create(LogicalCircuit logicalCircuit, GridPoint point1, GridPoint point2) {
			return this.CreateItem(Guid.NewGuid(), logicalCircuit, point1.X, point1.Y, point2.X, point2.Y);
		}

		public Wire Copy(Wire other) {
			WireData data;
			other.CircuitProject.WireSet.Table.GetData(other.WireRowId, out data);
			return this.Create(this.Table.Insert(ref data));
		}
	}
}
