using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public abstract class CircuitGlyph : Symbol {
		private List<Jam>[] jams;
		private bool isUpdated;
		public abstract Circuit Circuit { get; }
		public abstract GridPoint Point { get; set; }

		protected CircuitGlyph() : base() {
		}

		public IList<Jam> Left {
			get {
				this.Update();
				return this.jams[(int)PinSide.Left];
			}
		}
		public IList<Jam> Top {
			get {
				this.Update();
				return this.jams[(int)PinSide.Top];
			}
		}
		public IList<Jam> Right {
			get {
				this.Update();
				return this.jams[(int)PinSide.Right];
			}
		}
		public IList<Jam> Bottom {
			get {
				this.Update();
				return this.jams[(int)PinSide.Bottom];
			}
		}

		public IEnumerable<Jam> Jams() {
			return this.Left.Concat(this.Top).Concat(this.Right).Concat(this.Bottom);
		}

		public void ResetJams() {
			this.isUpdated = false;
			this.NotifyPropertyChanged("Left");
			this.NotifyPropertyChanged("Top");
			this.NotifyPropertyChanged("Right");
			this.NotifyPropertyChanged("Bottom");
		}

		private void Update() {
			if(!this.isUpdated) {
				if(this.jams == null) {
					this.jams = new List<Jam>[] { new List<Jam>(), new List<Jam>(), new List<Jam>(), new List<Jam>() };
				} else {
					foreach(List<Jam> j in this.jams) {
						j.Clear();
					}
				}
				IList<Jam> list = this.jams[(int)PinSide.Left];
				foreach(BasePin pin in this.Circuit.Left) {
					list.Add(new JamItem(pin, this));
				}
				list = this.jams[(int)PinSide.Top];
				foreach(BasePin pin in this.Circuit.Top) {
					list.Add(new JamItem(pin, this));
				}
				list = this.jams[(int)PinSide.Right];
				foreach(BasePin pin in this.Circuit.Right) {
					list.Add(new JamItem(pin, this));
				}
				list = this.jams[(int)PinSide.Bottom];
				foreach(BasePin pin in this.Circuit.Bottom) {
					list.Add(new JamItem(pin, this));
				}
				this.isUpdated = true;
			}
		}

		private class JamItem : Jam {
			public JamItem(BasePin pin, CircuitGlyph symbol) {
				this.Pin = pin;
				this.CircuitGlyph = symbol;
			}
		}
	}
}
