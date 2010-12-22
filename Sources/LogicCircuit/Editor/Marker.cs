using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	partial class Editor {
		private abstract class Marker {
			public abstract Symbol Symbol { get; }
			public abstract FrameworkElement Glyph { get; }
			public abstract void Move(Editor editor, Point point);
			public abstract void Commit(Editor editor, Point point, bool withWires);
		}
	}
}
