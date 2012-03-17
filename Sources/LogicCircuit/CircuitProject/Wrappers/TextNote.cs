﻿// This file is generated by ItemWrapper.Generator. Do not modify this file as it will be regenerated
namespace LogicCircuit {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using LogicCircuit.DataPersistent;

	// Defines the shape of the table TextNote
	internal partial struct TextNoteData {
		public Guid TextNoteId;
		public Guid LogicalCircuitId;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public string Note;
		public Rotation Rotation;
		internal TextNote TextNote;

		private interface IFieldSerializer {
			bool NeedToSave(ref TextNoteData data);
			string GetTextValue(ref TextNoteData data);
			void SetDefault(ref TextNoteData data);
			void SetTextValue(ref TextNoteData data, string text);
		}

		// Field accessors

		// Accessor of the TextNoteId field
		public sealed class TextNoteIdField : IField<TextNoteData, Guid>, IFieldSerializer {
			public static readonly TextNoteIdField Field = new TextNoteIdField();
			private TextNoteIdField() {}
			public string Name { get { return "TextNoteId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default(Guid); } }
			public Guid GetValue(ref TextNoteData record) {
				return record.TextNoteId;
			}
			public void SetValue(ref TextNoteData record, Guid value) {
				record.TextNoteId = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return l.TextNoteId.CompareTo(r.TextNoteId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.TextNoteId, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.TextNoteId);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.TextNoteId = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.TextNoteId = new Guid(text);
			}
		}

		// Accessor of the LogicalCircuitId field
		public sealed class LogicalCircuitIdField : IField<TextNoteData, Guid>, IFieldSerializer {
			public static readonly LogicalCircuitIdField Field = new LogicalCircuitIdField();
			private LogicalCircuitIdField() {}
			public string Name { get { return "LogicalCircuitId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default(Guid); } }
			public Guid GetValue(ref TextNoteData record) {
				return record.LogicalCircuitId;
			}
			public void SetValue(ref TextNoteData record, Guid value) {
				record.LogicalCircuitId = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return l.LogicalCircuitId.CompareTo(r.LogicalCircuitId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.LogicalCircuitId, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.LogicalCircuitId);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.LogicalCircuitId = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.LogicalCircuitId = new Guid(text);
			}
		}

		// Accessor of the X field
		public sealed class XField : IField<TextNoteData, int>, IFieldSerializer {
			public static readonly XField Field = new XField();
			private XField() {}
			public string Name { get { return "X"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return default(int); } }
			public int GetValue(ref TextNoteData record) {
				return record.X;
			}
			public void SetValue(ref TextNoteData record, int value) {
				record.X = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return Math.Sign((long)l.X - (long)r.X);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.X, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.X);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.X = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.X = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Y field
		public sealed class YField : IField<TextNoteData, int>, IFieldSerializer {
			public static readonly YField Field = new YField();
			private YField() {}
			public string Name { get { return "Y"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return default(int); } }
			public int GetValue(ref TextNoteData record) {
				return record.Y;
			}
			public void SetValue(ref TextNoteData record, int value) {
				record.Y = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return Math.Sign((long)l.Y - (long)r.Y);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.Y, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Y);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.Y = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.Y = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Width field
		public sealed class WidthField : IField<TextNoteData, int>, IFieldSerializer {
			public static readonly WidthField Field = new WidthField();
			private WidthField() {}
			public string Name { get { return "Width"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return 10; } }
			public int GetValue(ref TextNoteData record) {
				return record.Width;
			}
			public void SetValue(ref TextNoteData record, int value) {
				record.Width = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return Math.Sign((long)l.Width - (long)r.Width);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.Width, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Width);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.Width = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.Width = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Height field
		public sealed class HeightField : IField<TextNoteData, int>, IFieldSerializer {
			public static readonly HeightField Field = new HeightField();
			private HeightField() {}
			public string Name { get { return "Height"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return 10; } }
			public int GetValue(ref TextNoteData record) {
				return record.Height;
			}
			public void SetValue(ref TextNoteData record, int value) {
				record.Height = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return Math.Sign((long)l.Height - (long)r.Height);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.Height, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Height);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.Height = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.Height = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Note field
		public sealed class NoteField : IField<TextNoteData, string>, IFieldSerializer {
			public static readonly NoteField Field = new NoteField();
			private NoteField() {}
			public string Name { get { return "Note"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref TextNoteData record) {
				return record.Note;
			}
			public void SetValue(ref TextNoteData record, string value) {
				record.Note = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return StringComparer.Ordinal.Compare(l.Note, r.Note);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.Note, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Note);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.Note = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.Note = text;
			}
		}

		// Accessor of the Rotation field
		public sealed class RotationField : IField<TextNoteData, Rotation>, IFieldSerializer {
			public static readonly RotationField Field = new RotationField();
			private RotationField() {}
			public string Name { get { return "Rotation"; } }
			public int Order { get; set; }
			public Rotation DefaultValue { get { return Rotation.Up; } }
			public Rotation GetValue(ref TextNoteData record) {
				return record.Rotation;
			}
			public void SetValue(ref TextNoteData record, Rotation value) {
				record.Rotation = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return l.Rotation.CompareTo(r.Rotation);
			}
			public int Compare(Rotation l, Rotation r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref TextNoteData data) {
				return this.Compare(data.Rotation, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref TextNoteData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Rotation);
			}
			void IFieldSerializer.SetDefault(ref TextNoteData data) {
				data.Rotation = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref TextNoteData data, string text) {
				data.Rotation = (Rotation)Enum.Parse(typeof(Rotation), text, true);
			}
		}

		// Special field used to access items wrapper of this record from record.
		// This is used when no other universes is used
		internal sealed class TextNoteField : IField<TextNoteData, TextNote> {
			public static readonly TextNoteField Field = new TextNoteField();
			private TextNoteField() {}
			public string Name { get { return "TextNoteWrapper"; } }
			public int Order { get; set; }
			public TextNote DefaultValue { get { return null; } }
			public TextNote GetValue(ref TextNoteData record) {
				return record.TextNote;
			}
			public void SetValue(ref TextNoteData record, TextNote value) {
				record.TextNote = value;
			}
			public int Compare(ref TextNoteData l, ref TextNoteData r) {
				return this.Compare(l.TextNote, r.TextNote);
			}
			public int Compare(TextNote l, TextNote r) {
				if(object.ReferenceEquals(l, r)) return 0;
				if(l == null) return -1;
				if(r == null) return 1;
				return l.TextNoteRowId.CompareTo(r.TextNoteRowId);
			}
		}

		private static IField<TextNoteData>[] fields = {
			TextNoteIdField.Field,
			LogicalCircuitIdField.Field,
			XField.Field,
			YField.Field,
			WidthField.Field,
			HeightField.Field,
			NoteField.Field,
			RotationField.Field,
			TextNoteField.Field
		};

		// Creates table.
		public static TableSnapshot<TextNoteData> CreateTable(StoreSnapshot store) {
			TableSnapshot<TextNoteData> table = new TableSnapshot<TextNoteData>(store, "TextNote", TextNoteData.fields);
			// Create all but foreign keys of the table
			table.MakeUnique("PK_TextNote", TextNoteData.TextNoteIdField.Field , true);
			table.CreateIndex("IX_LogicalCircuit_TextNote", TextNoteData.LogicalCircuitIdField.Field );
			// Return created table
			return table;
		}

		// Creates all foreign keys of the table
		public static void CreateForeignKeys(StoreSnapshot store) {
			TableSnapshot<TextNoteData> table = (TableSnapshot<TextNoteData>)store.Table("TextNote");
			table.CreateForeignKey("FK_LogicalCircuit_TextNote", store.Table("LogicalCircuit"), TextNoteData.LogicalCircuitIdField.Field, ForeignKeyAction.Cascade, false);
		}

		// Serializer of the table
		public static void Save(TableSnapshot<TextNoteData> table, XmlWriter writer, string ns) {
			foreach(RowId rowId in table.Rows) {
				TextNoteData data;
				table.GetData(rowId, out data);
				writer.WriteStartElement(table.Name, ns);
				foreach(IField<TextNoteData> field in table.Fields) {
					IFieldSerializer serializer = field as IFieldSerializer;
					if(serializer != null && serializer.NeedToSave(ref data)) {
						writer.WriteStartElement(field.Name, ns);
						writer.WriteString(serializer.GetTextValue(ref data));
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
		}

		public static RowId Load(TableSnapshot<TextNoteData> table, XmlReader reader) {
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			Debug.Assert(reader.LocalName == table.Name);
			Debug.Assert(!reader.IsEmptyElement);

			TextNoteData data = new TextNoteData();
			// Initialize 'data' with default values: 
			for (int i = 0; i < TextNoteData.fields.Length; i ++) {
				IFieldSerializer serializer = TextNoteData.fields[i] as IFieldSerializer;
				if (serializer != null) {
					serializer.SetDefault(ref data);
				}
			}

			reader.Read();
			int fieldDepth = reader.Depth;
			object ns = reader.NamespaceURI;

			// Read through all fields of this record
			int hintIndex = 0;
			while (reader.Depth == fieldDepth) {
				if (reader.NodeType == XmlNodeType.Element && (object) reader.NamespaceURI == ns) {
					// We are position on field element
					string fieldName  = reader.LocalName;
					string fieldValue = ReadElementText(reader);     // reads the text and moved the reader to pass this element
					IFieldSerializer serializer = TextNoteData.FindField(fieldName, ref hintIndex);
					if (serializer != null) {
						serializer.SetTextValue(ref data, fieldValue);
					}
				}else {
					reader.Skip();     // skip everything else
				}
				Debug.Assert(reader.Depth == fieldDepth || reader.Depth == fieldDepth - 1, 
					"after reading the field we should be on fieldDepth or on fieldDepth - 1 if reach EndElement tag"
				);
			}
			// insert 'data' into the table
			return table.Insert(ref data);
		}

		private static string ReadElementText(XmlReader reader) {
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			string result;
			if (reader.IsEmptyElement) {
				result = string.Empty;
			} else {
				int fieldDepth = reader.Depth;
				reader.Read();                        // descend to the first child
				result = reader.ReadContentAsString();

				// Skip what ever we can meet here.
				while (fieldDepth < reader.Depth) {
					reader.Skip();
				}
				// Find ourselves on the EndElement tag.
				Debug.Assert(reader.Depth == fieldDepth);
				Debug.Assert(reader.NodeType == XmlNodeType.EndElement); 
			}
			
			// Skip EndElement or empty element.
			reader.Read();
			return result;
		}

		private static IFieldSerializer FindField(string name, ref int hint) {
			// We serialize/deserialize fields in the same order so result would always be at hint position or after it if hint is skipped during the serialization
			Debug.Assert(0 <= hint && hint <= TextNoteData.fields.Length);
			for (int i = hint; i < TextNoteData.fields.Length; i ++) {
				if (TextNoteData.fields[i].Name == name) {
					hint = i + 1;
					return TextNoteData.fields[i] as IFieldSerializer;
				}
			}

			// We don't find the field in expected place. Lets look the beginning of the list in case it is out of order
			for (int i = 0; i < hint; i ++) {
				if (TextNoteData.fields[i].Name == name) {
					return TextNoteData.fields[i] as IFieldSerializer;
				}
			}

			// Ups. Still don't find. 
			return null;
		}
	}


	// Class wrapper for a record.
	partial class TextNote : Symbol {

		// RowId of the wrapped record
		internal RowId TextNoteRowId { get; private set; }
		// Store this wrapper belongs to
		public CircuitProject CircuitProject { get; private set; }

		// Constructor
		public TextNote(CircuitProject store, RowId rowIdTextNote) {
			Debug.Assert(!rowIdTextNote.IsEmpty);
			this.CircuitProject = store;
			this.TextNoteRowId = rowIdTextNote;
			// Link back to record. Assuming that a transaction is started
			this.Table.SetField(this.TextNoteRowId, TextNoteData.TextNoteField.Field, this);
			this.InitializeTextNote();
		}

		partial void InitializeTextNote();

		// Gets table storing this item.
		private TableSnapshot<TextNoteData> Table { get { return this.CircuitProject.TextNoteSet.Table; } }

		// Deletes object
		public virtual void Delete() {
			this.Table.Delete(this.TextNoteRowId);
		}

		// Checks if the item is deleted
		public bool IsDeleted() {
			return this.Table.IsDeleted(this.TextNoteRowId);
		}

		//Properties of TextNote

		// Gets value of the TextNoteId field.
		public Guid TextNoteId {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.TextNoteIdField.Field); }
		}

		// Gets or sets the value referred by the foreign key on field LogicalCircuitId
		protected override LogicalCircuit SymbolLogicalCircuit {
			get { return this.CircuitProject.LogicalCircuitSet.FindByLogicalCircuitId(this.Table.GetField(this.TextNoteRowId, TextNoteData.LogicalCircuitIdField.Field)); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.LogicalCircuitIdField.Field, value.LogicalCircuitId); }
		}

		// Gets or sets value of the X field.
		public int X {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.XField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.XField.Field, value); }
		}

		// Gets or sets value of the Y field.
		public int Y {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.YField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.YField.Field, value); }
		}

		// Gets or sets value of the Width field.
		public int Width {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.WidthField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.WidthField.Field, value); }
		}

		// Gets or sets value of the Height field.
		public int Height {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.HeightField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.HeightField.Field, value); }
		}

		// Gets or sets value of the Note field.
		public string Note {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.NoteField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.NoteField.Field, value); }
		}

		// Gets or sets value of the Rotation field.
		public Rotation Rotation {
			get { return this.Table.GetField(this.TextNoteRowId, TextNoteData.RotationField.Field); }
			set { this.Table.SetField(this.TextNoteRowId, TextNoteData.RotationField.Field, value); }
		}


		internal void NotifyChanged(TableChange<TextNoteData> change) {
			if(this.HasListener) {
				TextNoteData oldData, newData;
				change.GetOldData(out oldData);
				change.GetNewData(out newData);
				if(TextNoteData.TextNoteIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("TextNoteId");
				}
				if(TextNoteData.LogicalCircuitIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("LogicalCircuit");
				}
				if(TextNoteData.XField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("X");
				}
				if(TextNoteData.YField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Y");
				}
				if(TextNoteData.WidthField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Width");
				}
				if(TextNoteData.HeightField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Height");
				}
				if(TextNoteData.NoteField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Note");
				}
				if(TextNoteData.RotationField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Rotation");
				}
			}
			this.OnTextNoteChanged();
		}

		partial void OnTextNoteChanged();
	}


	// Wrapper for table TextNote.
	partial class TextNoteSet : INotifyCollectionChanged, IEnumerable<TextNote> {

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		internal TableSnapshot<TextNoteData> Table { get; private set; }

		// Gets StoreSnapshot this set belongs to.
		public CircuitProject CircuitProject { get { return (CircuitProject)this.Table.StoreSnapshot; } }

		// Constructor
		public TextNoteSet(CircuitProject store) {
			ITableSnapshot table = store.Table("TextNote");
			if(table != null) {
				Debug.Assert(store.IsFrozen, "The store should be frozen");
				this.Table = (TableSnapshot<TextNoteData>)table;
			} else {
				Debug.Assert(!store.IsFrozen, "In order to create table, the store should not be frozen");
				this.Table = TextNoteData.CreateTable(store);
			}
			this.InitializeTextNoteSet();
		}

		partial void InitializeTextNoteSet();

		//internal void Register() {
		//	foreach(RowId rowId in this.Table.Rows) {
		//		this.FindOrCreate(rowId);
		//	}
		//}


		// gets items wrapper by RowId
		public TextNote Find(RowId rowId) {
			if(!rowId.IsEmpty) {
				return this.Table.GetField(rowId, TextNoteData.TextNoteField.Field);
			}
			return null;
		}

		private void Delete(RowId rowId) {
		}

		// gets items wrapper by RowId
		private IEnumerable<TextNote> Select(IEnumerable<RowId> rows) {
			foreach(RowId rowId in rows) {
				TextNote item = this.Find(rowId);
				Debug.Assert(item != null, "What is the reason for the item not to be found?");
				yield return item;
			}
		}

		// Create wrapper for the row and register it in the dictionary
		private TextNote Create(RowId rowId) {
			TextNote item = new TextNote(this.CircuitProject, rowId);
			return item;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal TextNote FindOrCreate(RowId rowId) {
			Debug.Assert(!rowId.IsEmpty && !this.Table.IsDeleted(rowId), "Bad RowId");
			TextNote item;
			if((item = this.Find(rowId)) != null) {
				Debug.Assert(!item.IsDeleted(), "Deleted item should not be present in the dictionary");
				return item;
			}

			return this.Create(rowId);
		}

		// Creates TextNote wrapper
		private TextNote CreateItem(
			// Fields of TextNote table
			Guid TextNoteId,
			LogicalCircuit LogicalCircuit,
			int X,
			int Y,
			int Width,
			int Height,
			string Note,
			Rotation Rotation
		) {
			TextNoteData dataTextNote = new TextNoteData() {
				TextNoteId = TextNoteId,
				LogicalCircuitId = (LogicalCircuit != null) ? LogicalCircuit.LogicalCircuitId : TextNoteData.LogicalCircuitIdField.Field.DefaultValue,
				X = X,
				Y = Y,
				Width = Width,
				Height = Height,
				Note = Note,
				Rotation = Rotation,
			};
			return this.Create(this.Table.Insert(ref dataTextNote));
		}

		// Search helpers

		// Finds TextNote by TextNoteId
		public TextNote Find(Guid textNoteId) {
			return this.Find(this.Table.Find(TextNoteData.TextNoteIdField.Field, textNoteId));
		}

		// Selects TextNote by LogicalCircuit
		public IEnumerable<TextNote> SelectByLogicalCircuit(LogicalCircuit logicalCircuit) {
			return this.Select(this.Table.Select(TextNoteData.LogicalCircuitIdField.Field, logicalCircuit.LogicalCircuitId));
		}

		public IEnumerator<TextNote> GetEnumerator() {
			return this.Select(this.Table.Rows).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs arg) {
			NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
			if(handler != null) {
				handler(this, arg);
			}
		}

		internal List<TextNote> UpdateSet(int oldVersion, int newVersion) {
			IEnumerator<TableChange<TextNoteData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<TextNote> del = (handlerAttached) ? new List<TextNote>() : null;
				while(change.MoveNext()) {
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						this.FindOrCreate(change.Current.RowId);
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						break;
					case SnapTableAction.Delete:
						if(handlerAttached) {
							TextNote item = change.Current.GetOldField(TextNoteData.TextNoteField.Field);
							Debug.Assert(item.IsDeleted());
							del.Add(item);
						}
						break;
					default:
						Debug.Assert(change.Current.Action == SnapTableAction.Update, "Unknown action");
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item does not exist during update?");
						break;
					}
				}
				change.Dispose();
				return del;
			}
			return null;
		}

		internal void NotifyVersionChanged(int oldVersion, int newVersion, List<TextNote> deleted) {
			IEnumerator<TableChange<TextNoteData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<TextNote> add = (handlerAttached) ? new List<TextNote>() : null;
				this.StartNotifyTextNoteSetChanged(oldVersion, newVersion);
				while(change.MoveNext()) {
					this.NotifyTextNoteSetChanged(change.Current);
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						if(handlerAttached) {
							add.Add(this.Find(change.Current.RowId));
						}
						break;
					case SnapTableAction.Delete:
						Debug.Assert(change.Current.GetOldField(TextNoteData.TextNoteField.Field).IsDeleted(), "Why the item still exists?");
						break;
					default:
						Debug.Assert(change.Current.Action == SnapTableAction.Update, "Unknown action");
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item does not exist during update?");
						this.Find(change.Current.RowId).NotifyChanged(change.Current);
						break;
					}
				}
				change.Dispose();
				if(handlerAttached) {
					if(deleted != null && 0 < deleted.Count) {
						this.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, deleted));
					}
					if(0 < add.Count) {
						this.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, add));
					}
				}
				this.EndNotifyTextNoteSetChanged();
			}
		}

		partial void StartNotifyTextNoteSetChanged(int oldVersion, int newVersion);
		partial void EndNotifyTextNoteSetChanged();
		partial void NotifyTextNoteSetChanged(TableChange<TextNoteData> change);

		internal void NotifyRolledBack(int version) {
			if(this.Table.WasAffected(version)) {
				IEnumerator<RowId> change = this.Table.GetRolledBackChanges(version);
				if(change != null) {
					while(change.MoveNext()) {
						RowId rowId = change.Current;
						if(this.Table.IsDeleted(rowId)) {
							this.Delete(rowId);
						} else {
							this.FindOrCreate(rowId);
						}
					}
					change.Dispose();
				}
			}
		}
	}

}
