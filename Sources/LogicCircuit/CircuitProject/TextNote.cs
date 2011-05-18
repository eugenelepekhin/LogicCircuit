using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Input;

namespace LogicCircuit {
	public partial class TextNote {
		private FlowDocumentScrollViewer glyph = null;

		public FlowDocumentScrollViewer TextNoteGlyph {
			get { return this.glyph ?? (this.glyph = this.CreateGlyph()); }
		}

		public override FrameworkElement Glyph { get { return this.TextNoteGlyph; } }
		public override bool HasCreatedGlyph { get { return this.glyph != null; } }

		private FlowDocumentScrollViewer CreateGlyph() {
			FlowDocumentScrollViewer doc = Symbol.Skin<FlowDocumentScrollViewer>(SymbolShape.TextNote);
			doc.DataContext = this;
			doc.Document = this.Load();
			Panel.SetZIndex(doc, this.Z);
			return doc;
		}

		private static FlowDocument Load(string text) {
			return XamlReader.Parse(text) as FlowDocument;
		}

		private FlowDocument Load() {
			return TextNote.Load(this.Note);
		}

		public void UpdateGlyph() {
			FlowDocumentScrollViewer doc = this.TextNoteGlyph;
			doc.Document = this.Load();
		}

		public override void PositionGlyph() {
			FlowDocumentScrollViewer doc = this.TextNoteGlyph;
			Canvas.SetLeft(doc, Symbol.ScreenPoint(this.X));
			Canvas.SetTop(doc, Symbol.ScreenPoint(this.Y));
			doc.Width = Symbol.ScreenPoint(this.Width);
			doc.Height = Symbol.ScreenPoint(this.Height);
		}

		public override int Z { get { return 0; } }

		public GridPoint Point {
			get { return new GridPoint(this.X, this.Y); }
			set { this.X = value.X; this.Y = value.Y; }
		}

		public override void Shift(int dx, int dy) {
			this.X += dx;
			this.Y += dy;
		}

		public override void DeleteSymbol() {
			this.Delete();
		}

		public override Symbol CopyTo(LogicalCircuit target) {
			return target.CircuitProject.TextNoteSet.Copy(this, target);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static bool IsValidText(string text) {
			if(!string.IsNullOrEmpty(text)) {
				try {
					FlowDocument doc = TextNote.Load(text);
					return doc != null && 0 < (new TextRange(doc.ContentStart, doc.ContentEnd).Text.Trim().Length);
				} catch {
				}
			}
			return false;
		}

		public bool IsValid { get { return TextNote.IsValidText(this.Note); } }

		partial void OnTextNoteChanged() {
			this.PositionGlyph();
		}
	}

	public partial class TextNoteSet {
		public void Load(XmlNodeList list) {
			TextNoteData.Load(this.Table, list, rowId => this.Create(rowId));
		}

		public TextNote Create(LogicalCircuit logicalCircuit, GridPoint point, string note) {
			return this.CreateItem(Guid.NewGuid(), logicalCircuit, point.X, point.Y, TextNoteData.WidthField.Field.DefaultValue, TextNoteData.HeightField.Field.DefaultValue, note);
		}

		public TextNote Copy(TextNote other, LogicalCircuit target) {
			TextNoteData data;
			other.CircuitProject.TextNoteSet.Table.GetData(other.TextNoteRowId, out data);
			if(this.Find(data.TextNoteId) != null) {
				data.TextNoteId = Guid.NewGuid();
			}
			data.LogicalCircuitId = target.LogicalCircuitId;
			data.TextNote = null;
			return this.Create(this.Table.Insert(ref data));
		}
	}
}
