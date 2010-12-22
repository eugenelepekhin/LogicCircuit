using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogicCircuit {
	partial class Editor {
		private class MarkerPoint : Marker {
			public Marker Parent { get; private set; }
			public GridPoint Point { get; private set; }

			public override Symbol Symbol {
				get { throw new NotImplementedException(); }
			}

			public override FrameworkElement Glyph {
				get { throw new NotImplementedException(); }
			}

			public MarkerPoint(Marker parent, GridPoint point) {
				this.Parent = parent;
				this.Point = point;
			}

			public override void Move(Editor editor, Point point) {
				throw new NotImplementedException();
			}

			public override void Commit(Editor editor, Point point, bool withWires) {
				throw new NotImplementedException();
			}
		}
	}
}
