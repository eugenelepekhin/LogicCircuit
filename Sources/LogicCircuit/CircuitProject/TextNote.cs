using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace LogicCircuit {
	public partial class TextNote : IRotatable {
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
			doc.RenderTransform = new RotateTransform();
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

			doc.RenderTransformOrigin = Symbol.RotationCenter(this.Width, this.Height);
			RotateTransform rotation = (RotateTransform)doc.RenderTransform;
			rotation.Angle = Symbol.Angle(this.Rotation);
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

		public override Rect Bounds() {
			Rect bounds = new Rect(Symbol.ScreenPoint(this.Point),
				new Size(Symbol.ScreenPoint(this.Width), Symbol.ScreenPoint(this.Height))
			);
			if(this.Rotation != Rotation.Up) {
				bounds = Symbol.Transform(bounds, Symbol.RotationTransform(this.Rotation, this.X, this.Y, this.Width, this.Height));
			}
			return bounds;
		}

		public override void DeleteSymbol() {
			this.Delete();
		}

		public override Symbol CopyTo(LogicalCircuit target) {
			return target.CircuitProject.TextNoteSet.Copy(this, target);
		}

		public static bool IsValidText(string text) {
			FlowDocument doc = TextNote.Load(text);
			return doc != null && (
				!string.IsNullOrWhiteSpace(new TextRange(doc.ContentStart, doc.ContentEnd).Text) ||
				!text.StartsWith("<FlowDocument", StringComparison.OrdinalIgnoreCase)
			);
		}

		public bool IsValid { get { return TextNote.IsValidText(this.Note); } }

		partial void OnTextNoteChanged() {
			this.UpdateGlyph();
			this.PositionGlyph();
		}

		private static string SavePackage(FlowDocument document) {
			using(MemoryStream stream = new MemoryStream()) {
				TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
				range.Save(stream, DataFormats.XamlPackage, true);
				stream.Flush();
				return Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.InsertLineBreaks);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static FlowDocument LoadPackage(string text) {
			if(!string.IsNullOrEmpty(text)) {
				try {
					using(MemoryStream stream = new MemoryStream(Convert.FromBase64String(text))) {
						FlowDocument document = new FlowDocument();
						TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
						range.Load(stream, DataFormats.XamlPackage);
						return document;
					}
				} catch(Exception exception) {
					Tracer.Report("TextNote.LoadPackage", exception);
				}
			}
			return null;
		}

		private static string SaveXaml(FlowDocument document) {
			return XamlWriter.Save(document);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static FlowDocument LoadXaml(string text) {
			try {
				return XamlReader.Parse(text) as FlowDocument;
			} catch(Exception exception) {
				Tracer.Report("TextNote.SaveXaml", exception);
			}
			return null;
		}

		private static bool ContainsAny(string text, params string[] sample) {
			return sample.Any(s => 0 <= text.IndexOf(s, StringComparison.OrdinalIgnoreCase));
		}

		public static string Save(FlowDocument document) {
			string text = TextNote.SaveXaml(document);
			if(!TextNote.ContainsAny(text, "<Image", "<BitmapImage")) {
				return text;
			}
			return TextNote.SavePackage(document);
		}

		public static FlowDocument Load(string text) {
			if(text.StartsWith("<FlowDocument", StringComparison.OrdinalIgnoreCase)) {
				FlowDocument document = TextNote.LoadXaml(text);
				if(document != null) {
					return document;
				}
			}

			return TextNote.LoadPackage(text);
		}

		public bool Match(Regex regex) {
			FlowDocument doc = TextNote.Load(this.Note);
			TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
			string text = range.Text;
			return !string.IsNullOrEmpty(text) && regex.IsMatch(text);
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class TextNoteSet {
		public TextNote Create(LogicalCircuit logicalCircuit, GridPoint point, string note) {
			return this.CreateItem(Guid.NewGuid(), logicalCircuit, point.X, point.Y,
				TextNoteData.WidthField.Field.DefaultValue, TextNoteData.HeightField.Field.DefaultValue, note, TextNoteData.RotationField.Field.DefaultValue
			);
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

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<TextNoteData>(nameTable, this.Table, rowId => this.Create(rowId));
		}
	}
}
