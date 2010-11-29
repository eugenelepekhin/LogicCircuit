using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	public abstract class BasePin : Circuit {

		public const int MaxBitWidth = 32;

		public BasePin(CircuitProject store, RowId rowId) : base(store, rowId) {
		}

		public static int CheckBitWidth(int value) {
			return Math.Max(1, Math.Min(value, BasePin.MaxBitWidth));
		}

		public static PinSide DefaultSide(PinType pinType) {
			return (pinType == PinType.Input) ? PinSide.Left : PinSide.Right;
		}

		public static string DefaultName(PinType pinType) {
			return (pinType == PinType.Input) ? Resources.PinInName : Resources.PinOutName;
		}

		public GridPoint GridPoint { get; set; }
		
		public abstract PinSide PinSide { get; set; }
		public abstract PinType PinType { get; set; }
		public abstract int BitWidth { get; set; }
		//public abstract string Name { get; set; }
		public abstract string Note { get; set; }
		public abstract bool Inverted { get; set; }

		public override string ToolTip {
			get {
				if(this.PinType == PinType.Input) {
					return this.AppendNote(Resources.ToolTipInputPin(this.BitWidth, this.Name));
				} else if(this.PinType == PinType.Output) {
					return this.AppendNote(Resources.ToolTipOutputPin(this.BitWidth, this.Name));
				} else {
					return this.AppendNote(Resources.ToolTipNonePin(this.BitWidth, this.Name));
				}
			}
		}

		public override string Category {
			get { return Resources.CategoryInputOutput; }
			set { throw new InvalidOperationException(); }
		}

		private string AppendNote(string toolTip) {
			string n = this.Note;
			if(!string.IsNullOrEmpty(n)) {
				return toolTip + "\n" + n;
			}
			return toolTip;
		}
	}
}
