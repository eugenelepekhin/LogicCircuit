using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using LogicCircuit.DataPersistent;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace LogicCircuit {
	public partial class LedMatrix {
		public const int MinBitsPerLed = 1;
		public const int MaxBitsPerLed = 3;
		public const int MinLedCount = 1;
		public const int MaxLedCount = BasePin.MaxBitWidth / LedMatrix.MaxBitsPerLed;

		public static int Check(int value) {
			return Math.Max(LedMatrix.MinLedCount, Math.Min(value, LedMatrix.MaxLedCount));
		}

		public static int CheckColors(int value) {
			return Math.Max(LedMatrix.MinBitsPerLed, Math.Min(value, LedMatrix.MaxBitsPerLed));
		}

		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override string Category {
			get { return Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override string Name {
			get { return Resources.NameLedMatrix; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return this.Name; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip { get { return Resources.ToolTipLedMatrix(this.Rows, this.Columns); } }

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.LedMatrixSet.Copy(this);
		}

		public override bool IsSmallSymbol { get { return true; } }
		public override int SymbolWidth { get { return this.Columns + 1; } }
		public override int SymbolHeight { get { return this.Rows + 1; } }

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			FrameworkElement glyph = symbol.CreateSimpleGlyph(SymbolShape.LedMatrix);
			this.UpdateGlyph(symbol);
			return glyph;
		}

		private void UpdateGlyph(CircuitGlyph symbol) {
			int cellCount = this.Rows * this.Columns;
			UniformGrid grid = (UniformGrid)symbol.ProbeView;
			if(cellCount < grid.Children.Count) {
				grid.Children.RemoveRange(cellCount, grid.Children.Count - cellCount);
			}
			grid.Columns = this.Columns;
			grid.Rows = this.Rows;
			for(int i = grid.Children.Count; i < cellCount; i++) {
				grid.Children.Add(Symbol.Skin(SymbolShape.LedMatrixRoundCell));
			}
		}

		partial void OnLedMatrixChanged() {
			int symbolCount = 0;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				Tracer.Assert(symbolCount++ == 0, "Only one symbol expected");
				this.UpdateGlyph(symbol);
			}
		}
	}

	public partial class LedMatrixSet {
		public void Load(XmlNodeList list) {
			LedMatrixData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private void CreatePins(LedMatrix ledMatrix) {
			Tracer.Assert(!this.CircuitProject.DevicePinSet.SelectByCircuit(ledMatrix).Any());

			DevicePin rows = this.CircuitProject.DevicePinSet.Create(ledMatrix, PinType.Input, ledMatrix.Rows);
			rows.PinSide = PinSide.Left;
			//rows.Name = Resources.LedMatrixRowsPinName;

			DevicePin columns = this.CircuitProject.DevicePinSet.Create(ledMatrix, PinType.Input, ledMatrix.Columns);
			columns.PinSide = PinSide.Top;
			//columns.Name = Resources.LedMatrixColumnsPinName;
		}

		private LedMatrix Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, LedMatrixData.LedMatrixIdField.Field)
			};
			LedMatrix ledMatrix = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CreatePins(ledMatrix);
			return ledMatrix;
		}

		public LedMatrix Create(int rows, int columns) {
			LedMatrix ledMatrix = this.CreateItem(Guid.NewGuid(),
				LedMatrixData.MatrixTypeField.Field.DefaultValue,
				LedMatrixData.CellTypeField.Field.DefaultValue,
				rows,
				columns,
				LedMatrixData.ColorsField.Field.DefaultValue,
				LedMatrixData.Color1Field.Field.DefaultValue,
				LedMatrixData.Color2Field.Field.DefaultValue,
				LedMatrixData.Color3Field.Field.DefaultValue
			);
			this.CreatePins(ledMatrix);
			return ledMatrix;
		}

		public LedMatrix Copy(LedMatrix other) {
			LedMatrixData data;
			other.CircuitProject.LedMatrixSet.Table.GetData(other.LedMatrixRowId, out data);
			if(this.FindByLedMatrixId(data.LedMatrixId) != null) {
				data.LedMatrixId = Guid.NewGuid();
			}
			data.LedMatrix = null;
			return this.Register(this.Table.Insert(ref data));
		}
		
	}
}
