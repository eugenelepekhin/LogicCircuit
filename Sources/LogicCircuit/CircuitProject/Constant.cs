using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public partial class Constant {
		public override void Delete() {
			this.CircuitProject.DevicePinSet.DeleteAllPins(this);
			base.Delete();
		}

		public override bool IsSmallSymbol { get { return true; } }

		public static int Normalize(int value, int bitWidth) {
			bitWidth = BasePin.CheckBitWidth(bitWidth);
			if(bitWidth < 32) {
				return value & ((1 << bitWidth) - 1);
			}
			return value;
		}

		public int ConstantValue {
			get { return Constant.Normalize(this.Value, this.BitWidth); }
			set { this.Value = Constant.Normalize(value, this.BitWidth); }
		}

		public override string Name {
			get { return Resources.NameConstant; }
			set { throw new NotSupportedException(); }
		}

		public override string Notation {
			get { return Resources.ConstantNotation(this.Value); }
			set { throw new InvalidOperationException(); }
		}

		public override string ToolTip { get { return Resources.ToolTipConstant(this.BitWidth, this.Value); } }

		public override string Category {
			get { return Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		public override Circuit CopyTo(CircuitProject project) {
			return project.ConstantSet.Copy(this);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			return symbol.CreateSimpleGlyph(SymbolShape.Constant);
		}

		partial void OnConstantChanged() {
			int count = 0;
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				Tracer.Assert(count++ == 0, "Only one symbol expected");
				TextBlock text = (TextBlock)symbol.ProbeView;
				text.Text = this.Notation;
				symbol.Glyph.ToolTip = this.ToolTip;
			}
		}
	}

	public partial class ConstantSet {
		public void Load(XmlNodeList list) {
			ConstantData.Load(this.Table, list, rowId => this.Register(rowId));
		}

		private Constant Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, ConstantData.ConstantIdField.Field)
			};
			Constant constant = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			this.CircuitProject.DevicePinSet.Create(constant, PinType.Output, constant.BitWidth);
			return constant;
		}

		public Constant Create(int bitWidth, int value) {
			Constant constant = this.CreateItem(Guid.NewGuid(), bitWidth, value);
			this.CircuitProject.DevicePinSet.Create(constant, PinType.Output, constant.BitWidth);
			return constant;
		}

		public Constant Copy(Constant other) {
			ConstantData data;
			other.CircuitProject.ConstantSet.Table.GetData(other.ConstantRowId, out data);
			if(this.FindByConstantId(data.ConstantId) != null) {
				data.ConstantId = Guid.NewGuid();
			}
			return this.Register(this.Table.Insert(ref data));
		}
	}
}
