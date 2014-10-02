using System;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	partial class EditorDiagram {
		private abstract class Marker {
			public Symbol Symbol { get; private set; }
			public abstract FrameworkElement Glyph { get; }

			protected Marker(Symbol symbol) {
				this.Symbol = symbol;
			}

			public virtual void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public virtual void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public virtual void CancelMove(Panel selectionLayer) {
			}

			public abstract void Refresh();
		}
	}
}
