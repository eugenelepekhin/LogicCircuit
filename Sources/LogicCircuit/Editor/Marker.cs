using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	partial class EditorDiagram {
		private abstract class Marker {
			public abstract Symbol Symbol { get; }
			public abstract FrameworkElement Glyph { get; }
			public abstract void Move(EditorDiagram editor, Point point);
			public abstract void Commit(EditorDiagram editor, Point point, bool withWires);
			public abstract void Shift(int dx, int dy);
		}
	}
}
