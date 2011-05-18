using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;

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
			doc.Document = TextNote.Load(this.Note);
			Panel.SetZIndex(doc, this.Z);
			return doc;
		}

		public void UpdateGlyph() {
			FlowDocumentScrollViewer doc = this.TextNoteGlyph;
			doc.Document = TextNote.Load(this.Note);
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

		public static bool IsValidText(string text) {
			FlowDocument doc = TextNote.Load(text);
			return doc != null && 0 < (new TextRange(doc.ContentStart, doc.ContentEnd).Text.Trim().Length);
		}

		public bool IsValid { get { return TextNote.IsValidText(this.Note); } }

		partial void OnTextNoteChanged() {
			this.PositionGlyph();
		}

		public static string Save(FlowDocument document) {
			using(MemoryStream stream = new MemoryStream()) {
				TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
				range.Save(stream, DataFormats.XamlPackage);
				stream.Flush();
				return Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.InsertLineBreaks);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static FlowDocument Load(string text) {
			if(!string.IsNullOrEmpty(text)) {
				try {
					using(MemoryStream stream = new MemoryStream(Convert.FromBase64String(text))) {
						FlowDocument document = new FlowDocument();
						TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
						range.Load(stream, DataFormats.XamlPackage);
						return document;
					}
				} catch(Exception exception) {
					Tracer.Report("TextNote.Load", exception);
				}
			}
			return null;
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
