using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class Editor {
		private class CircuitSymbolMarker : Marker {
			public CircuitSymbol CircuitSymbol { get; private set; }
			public Rectangle MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			public CircuitSymbolMarker(CircuitSymbol symbol) {
				this.CircuitSymbol = symbol;
				this.MarkerGlyph = Symbol.Skin<Rectangle>(SymbolShape.Rectangular);
				this.MarkerGlyph.DataContext = this;
			}

			public override void Move(Editor editor, Point point) {
				throw new NotImplementedException();
			}

			public override void Commit(Editor editor, Point point) {
				throw new NotImplementedException();
			}
		}
	}
}
