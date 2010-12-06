using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace LogicCircuit {
	public abstract class Symbol : INotifyPropertyChanged {

		public const int PinRadius = 3;
		public const int GridSize = Symbol.PinRadius * 6;

		public event PropertyChangedEventHandler PropertyChanged;

		protected Symbol() {
		}

		public abstract LogicalCircuit LogicalCircuit { get; set; }
		public abstract void Shift(int x, int y);
		public abstract int Z { get; }

		public abstract FrameworkElement Glyph { get; }

		protected void NotifyPropertyChanged(string name) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		protected bool HasListener { get { return this.PropertyChanged != null; } }

		public abstract void CopyTo(CircuitProject project);
	}
}
