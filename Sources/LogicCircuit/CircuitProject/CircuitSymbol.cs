using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace LogicCircuit {
	public partial class CircuitSymbol : IRotatable {

		public override void DeleteSymbol() {
			if(this.Circuit is Gate || this.Circuit is LogicalCircuit) {
				this.Delete();
			} else {
				this.Circuit.Delete();
			}
		}

		partial void OnCircuitSymbolChanged() {
			Pin pin = this.Circuit as Pin;
			if(pin != null) {
				pin.LogicalCircuit.ResetPins();
			}
			if(this.LogicalCircuit.IsDisplay && (pin != null || this.Circuit.IsDisplay)) {
				this.CircuitProject.LogicalCircuitSet.Invalidate(this.LogicalCircuit);
			}
			this.PositionGlyph();
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

		public override void PositionGlyph() {
			FrameworkElement glyph = this.Glyph;
			Canvas.SetLeft(glyph, Symbol.ScreenPoint(this.X));
			Canvas.SetTop(glyph, Symbol.ScreenPoint(this.Y));
			glyph.RenderTransformOrigin = Symbol.RotationCenter(this.Circuit.SymbolWidth, this.Circuit.SymbolHeight);
			RotateTransform rotation = (RotateTransform)glyph.RenderTransform;
			rotation.Angle = Symbol.Angle(this.Rotation);
		}

		public Rect Bounds() {
			Rect bounds = new Rect(Symbol.ScreenPoint(this.Point),
				new Size(Symbol.ScreenPoint(this.Circuit.SymbolWidth), Symbol.ScreenPoint(this.Circuit.SymbolHeight))
			);
			if(this.Rotation != Rotation.Up) {
				bounds = Symbol.Transform(bounds, Symbol.RotationTransform(this.Rotation, this.X, this.Y, this.Circuit.SymbolWidth, this.Circuit.SymbolHeight));
			}
			return bounds;
		}

		public override Symbol CopyTo(LogicalCircuit target) {
			return target.CircuitProject.CircuitSymbolSet.Copy(this, target);
		}

		public override void Invalidate() {
			this.CircuitProject.CircuitSymbolSet.Invalidate(this);
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} of {1}{2} on \"{3}\"", this.GetType().Name, this.Circuit.Notation, this.Point, this.LogicalCircuit.Name);
			}
		#endif
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class CircuitSymbolSet {
		private HashSet<CircuitSymbol> invalid = new HashSet<CircuitSymbol>();

		public IEnumerable<CircuitSymbol> Invalid { get { return this.invalid; } }

		public void Invalidate(CircuitSymbol symbol) {
			this.invalid.Add(symbol);
		}

		public void ValidateAll() {
			this.invalid.Clear();
		}

		public CircuitSymbol Create(Circuit circuit, LogicalCircuit logicalCircuit, int x, int y) {
			return this.CreateItem(Guid.NewGuid(), circuit, logicalCircuit, x, y, CircuitSymbolData.RotationField.Field.DefaultValue);
		}

		public CircuitSymbol Copy(CircuitSymbol other, LogicalCircuit target) {
			CircuitSymbolData data;
			other.CircuitProject.CircuitSymbolSet.Table.GetData(other.CircuitSymbolRowId, out data);
			if(this.Find(data.CircuitSymbolId) != null) {
				data.CircuitSymbolId = Guid.NewGuid();
			}
			data.LogicalCircuitId = target.LogicalCircuitId;
			Circuit circuit = other.Circuit.CopyTo(target);
			data.CircuitId = circuit.CircuitId;
			data.CircuitSymbol = null;
			return this.Create(this.Table.Insert(ref data));
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<CircuitSymbolData>(nameTable, this.Table, rowId => this.Create(rowId));
		}
	}
}
