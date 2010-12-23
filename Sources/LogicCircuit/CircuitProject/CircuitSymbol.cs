using System;
using System.Windows;
using System.Xml;

namespace LogicCircuit {

	public partial class CircuitSymbol {

		partial void OnCircuitSymbolChanged() {
			Pin pin = this.Circuit as Pin;
			if(pin != null) {
				pin.LogicalCircuit.ResetPins();
			}
		}

		public override void Shift(int dx, int dy) {
			this.X += dx;
			this.Y += dy;
		}

		public override GridPoint Point {
			get { return new GridPoint(this.X, this.Y); }
			set { this.X = value.X; this.Y = value.Y; }
		}

		public override int Z { get { return 2; } }

		public override void CopyTo(CircuitProject project) {
			project.CircuitSymbolSet.Copy(this);
		}
	}

	public partial class CircuitSymbolSet {
		public void Load(XmlNodeList list) {
			CircuitSymbolData.Load(this.Table, list, rowId => this.Create(rowId));
		}

		public CircuitSymbol Create(Circuit circuit, LogicalCircuit logicalCircuit, int x, int y) {
			return this.CreateItem(Guid.NewGuid(), circuit, logicalCircuit, x, y);
		}

		public CircuitSymbol Copy(CircuitSymbol other) {
			other.Circuit.CopyTo(this.CircuitProject);
			CircuitSymbolData data;
			other.CircuitProject.CircuitSymbolSet.Table.GetData(other.CircuitSymbolRowId, out data);
			return this.Create(this.Table.Insert(ref data));
		}
	}
}
