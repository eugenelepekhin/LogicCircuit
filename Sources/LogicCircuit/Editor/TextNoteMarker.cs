using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LogicCircuit {
	partial class EditorDiagram {
		private class TextNoteMarker : Marker {
			public TextNote TextNote { get; private set; }
			public override Symbol Symbol { get { return this.TextNote; } }

			public Canvas MarkerGlyph { get; private set; }
			public override FrameworkElement Glyph { get { return this.MarkerGlyph; } }

			private readonly Rectangle rectangle = Symbol.Skin<Rectangle>(SymbolShape.MarkerRectangle);

			public TextNoteMarker(TextNote textNote) {
				this.TextNote = textNote;
				this.MarkerGlyph = new Canvas();
				this.MarkerGlyph.DataContext = this;
				this.MarkerGlyph.ToolTip = "text note";
				this.MarkerGlyph.Children.Add(this.rectangle);
				this.PositionGlyph();
			}

			public override void Move(EditorDiagram editor, Point point) {
				editor.MoveSelection(point);
			}

			public override void Commit(EditorDiagram editor, Point point, bool withWires) {
				editor.CommitMove(point, withWires);
			}

			public override void Shift(int dx, int dy) {
				this.TextNote.Shift(dx, dy);
				this.PositionTextNoteGliph();

			}

			private void PositionTextNoteGliph() {
				this.TextNote.PositionGlyph();
				this.PositionGlyph();
			}

			public void PositionGlyph() {
				Canvas.SetLeft(this.MarkerGlyph, Symbol.ScreenPoint(this.TextNote.X));
				Canvas.SetTop(this.MarkerGlyph, Symbol.ScreenPoint(this.TextNote.Y));
				this.rectangle.Width = Symbol.ScreenPoint(this.TextNote.Width);
				this.rectangle.Height = Symbol.ScreenPoint(this.TextNote.Height);
			}
		}
	}
}
