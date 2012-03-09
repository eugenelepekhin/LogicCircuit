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
			UniformGrid grid = (UniformGrid)symbol.ProbeView;
			grid.Children.Clear();
			grid.Columns = this.Columns;
			grid.Rows = this.Rows;
			string skin = (this.CellType == LedMatrixCellType.Round) ? SymbolShape.LedMatrixRoundCell : SymbolShape.LedMatrixRectCell;
			int cellCount = this.Rows * this.Columns;
			for(int i = 0; i < cellCount; i++) {
				grid.Children.Add(Symbol.Skin(skin));
			}
		}

		public void UpdatePins() {
			this.CircuitProject.DevicePinSet.SelectByCircuit(this).ToList().ForEach(p => p.Delete());
			int rows = this.Rows;
			int columns = this.Columns;
			if(this.MatrixType == LedMatrixType.Individual) {
				for(int i = 0; i < rows; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, columns);
					pin.Name = Resources.LedMatrixRowIndividual(i + 1);
				}
			} else { //this.MatrixType == LedMatrixType.Selector
				Tracer.Assert(this.MatrixType == LedMatrixType.Selector);
				int colors = this.Colors;
				for(int i = 0; i < rows; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, colors);
					pin.Name = Resources.LedMatrixRowSelector(i + 1);
				}
				for(int i = 0; i < rows; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, colors);
					pin.Name = Resources.LedMatrixColumnSelector(i + 1);
					pin.PinSide = PinSide.Bottom;
				}
			}
		}

		partial void OnLedMatrixChanged() {
			this.ResetPins();
		}
	}

	public partial class LedMatrixSet {
		public void Load(XmlNodeList list) {
			LedMatrixData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private LedMatrix Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, LedMatrixData.LedMatrixIdField.Field)
			};
			LedMatrix ledMatrix = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			ledMatrix.UpdatePins();
			return ledMatrix;
		}

		public LedMatrix Create(LedMatrixType ledMatrixType, int rows, int columns) {
			LedMatrix ledMatrix = this.CreateItem(Guid.NewGuid(),
				ledMatrixType,
				LedMatrixData.CellTypeField.Field.DefaultValue,
				rows,
				columns,
				LedMatrixData.ColorsField.Field.DefaultValue,
				LedMatrixData.Color1Field.Field.DefaultValue,
				LedMatrixData.Color2Field.Field.DefaultValue,
				LedMatrixData.Color3Field.Field.DefaultValue
			);
			ledMatrix.UpdatePins();
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
