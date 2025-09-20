﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml;
using DataPersistent;

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
			get { return Properties.Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override string Name {
			get { return Properties.Resources.NameLedMatrix; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return this.Name; }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip { get { return Circuit.BuildToolTip(Properties.Resources.ToolTipLedMatrix(this.Rows, this.Columns, this.Colors, 1 << this.Colors), this.Note); } }

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.LedMatrixSet.Copy(this);
		}

		public override bool IsDisplay {
			get { return true; }
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			Tracer.Assert((this.MatrixType == LedMatrixType.Selector && defaultWidth == this.Columns + 1) || (this.MatrixType == LedMatrixType.Individual && defaultWidth == 1));
			return this.Columns + 1;
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			Tracer.Assert(defaultHeight == this.Rows + 1);
			return base.CircuitSymbolHeight(defaultHeight);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateLedMatrixGlyph(symbol);
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			return symbol.CreateLedMatrixGlyph(mainSymbol);
		}

		public void UpdatePins() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			int rows = this.Rows;
			int columns = this.Columns;
			int colors = this.Colors;
			if(this.MatrixType == LedMatrixType.Individual) {
				int bitWidth = columns * colors;
				for(int i = 0; i < rows; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, bitWidth);
					pin.Name = Properties.Resources.LedMatrixRowIndividual(i + 1);
				}
			} else { //this.MatrixType == LedMatrixType.Selector
				Tracer.Assert(this.MatrixType == LedMatrixType.Selector);
				for(int i = 0; i < columns; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, colors);
					pin.Name = Properties.Resources.LedMatrixColumnSelector(i + 1);
					pin.PinSide = PinSide.Top;
				}
				for(int i = 0; i < rows; i++) {
					DevicePin pin = this.CircuitProject.DevicePinSet.Create(this, PinType.Input, 1);
					pin.Name = Properties.Resources.LedMatrixRowSelector(i + 1);
				}
			}
		}

		partial void OnLedMatrixChanged() {
			this.ResetPins();
			this.InvalidateDistinctSymbol();
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class LedMatrixSet {
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
				LedMatrixData.CellShapeField.Field.DefaultValue,
				rows,
				columns,
				LedMatrixData.ColorsField.Field.DefaultValue,
				LedMatrixData.NoteField.Field.DefaultValue
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

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<LedMatrixData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
