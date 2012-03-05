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

	// Defines the shape of the table LogicalCircuit
	internal partial struct LogicalCircuitData {
		public Guid LogicalCircuitId;
		public string Name;
		public string Notation;
		public string Description;
		public string Category;
		internal LogicalCircuit LogicalCircuit;

		private interface IFieldSerializer {
			bool NeedToSave(ref LogicalCircuitData data);
			string GetTextValue(ref LogicalCircuitData data);
			void SetDefault(ref LogicalCircuitData data);
			void SetTextValue(ref LogicalCircuitData data, string text);
		}

		// Field accessors

		// Accessor of the LogicalCircuitId field
		public sealed class LogicalCircuitIdField : IField<LogicalCircuitData, Guid>, IFieldSerializer {
			public static readonly LogicalCircuitIdField Field = new LogicalCircuitIdField();
			private LogicalCircuitIdField() {}
			public string Name { get { return "LogicalCircuitId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default(Guid); } }
			public Guid GetValue(ref LogicalCircuitData record) {
				return record.LogicalCircuitId;
			}
			public void SetValue(ref LogicalCircuitData record, Guid value) {
				record.LogicalCircuitId = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return l.LogicalCircuitId.CompareTo(r.LogicalCircuitId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref LogicalCircuitData data) {
				return this.Compare(data.LogicalCircuitId, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref LogicalCircuitData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.LogicalCircuitId);
			}
			void IFieldSerializer.SetDefault(ref LogicalCircuitData data) {
				data.LogicalCircuitId = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref LogicalCircuitData data, string text) {
				data.LogicalCircuitId = new Guid(text);
			}
		}

		// Accessor of the Name field
		public sealed class NameField : IField<LogicalCircuitData, string>, IFieldSerializer {
			public static readonly NameField Field = new NameField();
			private NameField() {}
			public string Name { get { return "Name"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return "Main"; } }
			public string GetValue(ref LogicalCircuitData record) {
				return record.Name;
			}
			public void SetValue(ref LogicalCircuitData record, string value) {
				record.Name = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return StringComparer.Ordinal.Compare(l.Name, r.Name);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref LogicalCircuitData data) {
				return this.Compare(data.Name, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref LogicalCircuitData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Name);
			}
			void IFieldSerializer.SetDefault(ref LogicalCircuitData data) {
				data.Name = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref LogicalCircuitData data, string text) {
				data.Name = text;
			}
		}

		// Accessor of the Notation field
		public sealed class NotationField : IField<LogicalCircuitData, string>, IFieldSerializer {
			public static readonly NotationField Field = new NotationField();
			private NotationField() {}
			public string Name { get { return "Notation"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref LogicalCircuitData record) {
				return record.Notation;
			}
			public void SetValue(ref LogicalCircuitData record, string value) {
				record.Notation = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return StringComparer.Ordinal.Compare(l.Notation, r.Notation);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref LogicalCircuitData data) {
				return this.Compare(data.Notation, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref LogicalCircuitData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Notation);
			}
			void IFieldSerializer.SetDefault(ref LogicalCircuitData data) {
				data.Notation = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref LogicalCircuitData data, string text) {
				data.Notation = text;
			}
		}

		// Accessor of the Description field
		public sealed class DescriptionField : IField<LogicalCircuitData, string>, IFieldSerializer {
			public static readonly DescriptionField Field = new DescriptionField();
			private DescriptionField() {}
			public string Name { get { return "Description"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref LogicalCircuitData record) {
				return record.Description;
			}
			public void SetValue(ref LogicalCircuitData record, string value) {
				record.Description = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return StringComparer.Ordinal.Compare(l.Description, r.Description);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref LogicalCircuitData data) {
				return this.Compare(data.Description, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref LogicalCircuitData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Description);
			}
			void IFieldSerializer.SetDefault(ref LogicalCircuitData data) {
				data.Description = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref LogicalCircuitData data, string text) {
				data.Description = text;
			}
		}

		// Accessor of the Category field
		public sealed class CategoryField : IField<LogicalCircuitData, string>, IFieldSerializer {
			public static readonly CategoryField Field = new CategoryField();
			private CategoryField() {}
			public string Name { get { return "Category"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref LogicalCircuitData record) {
				return record.Category;
			}
			public void SetValue(ref LogicalCircuitData record, string value) {
				record.Category = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return StringComparer.Ordinal.Compare(l.Category, r.Category);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer
			bool IFieldSerializer.NeedToSave(ref LogicalCircuitData data) {
				return this.Compare(data.Category, this.DefaultValue) != 0;
			}
			string IFieldSerializer.GetTextValue(ref LogicalCircuitData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Category);
			}
			void IFieldSerializer.SetDefault(ref LogicalCircuitData data) {
				data.Category = this.DefaultValue;
			}
			void IFieldSerializer.SetTextValue(ref LogicalCircuitData data, string text) {
				data.Category = text;
			}
		}

		// Special field used to access items wrapper of this record from record.
		// This is used when no other universes is used
		internal sealed class LogicalCircuitField : IField<LogicalCircuitData, LogicalCircuit> {
			public static readonly LogicalCircuitField Field = new LogicalCircuitField();
			private LogicalCircuitField() {}
			public string Name { get { return "LogicalCircuitWrapper"; } }
			public int Order { get; set; }
			public LogicalCircuit DefaultValue { get { return null; } }
			public LogicalCircuit GetValue(ref LogicalCircuitData record) {
				return record.LogicalCircuit;
			}
			public void SetValue(ref LogicalCircuitData record, LogicalCircuit value) {
				record.LogicalCircuit = value;
			}
			public int Compare(ref LogicalCircuitData l, ref LogicalCircuitData r) {
				return this.Compare(l.LogicalCircuit, r.LogicalCircuit);
			}
			public int Compare(LogicalCircuit l, LogicalCircuit r) {
				if(object.ReferenceEquals(l, r)) return 0;
				if(l == null) return -1;
				if(r == null) return 1;
				return l.LogicalCircuitRowId.CompareTo(r.LogicalCircuitRowId);
			}
		}

		private static IField<LogicalCircuitData>[] fields = {
			LogicalCircuitIdField.Field,
			NameField.Field,
			NotationField.Field,
			DescriptionField.Field,
			CategoryField.Field,
			LogicalCircuitField.Field
		};

		// Creates table.
		public static TableSnapshot<LogicalCircuitData> CreateTable(StoreSnapshot store) {
			TableSnapshot<LogicalCircuitData> table = new TableSnapshot<LogicalCircuitData>(store, "LogicalCircuit", LogicalCircuitData.fields);
			// Create all but foreign keys of the table
			table.MakeUnique("PK_LogicalCircuit", LogicalCircuitData.LogicalCircuitIdField.Field , true);
			table.MakeUnique("AK_LogicalCircuit_Name", LogicalCircuitData.NameField.Field , false);
			// Return created table
			return table;
		}

		// Creates all foreign keys of the table
		public static void CreateForeignKeys(StoreSnapshot store) {
			TableSnapshot<LogicalCircuitData> table = (TableSnapshot<LogicalCircuitData>)store.Table("LogicalCircuit");
			table.CreateForeignKey("PK_LogicalCircuit", store.Table("Circuit"), LogicalCircuitData.LogicalCircuitIdField.Field, ForeignKeyAction.Cascade, false);
		}

		// Serializer of the table
		public static void Save(TableSnapshot<LogicalCircuitData> table, XmlElement root) {
			XmlDocument xml = root.OwnerDocument;
			foreach(RowId rowId in table.Rows) {
				LogicalCircuitData data;
				table.GetData(rowId, out data);
				XmlElement node = xml.CreateElement(root.Prefix, table.Name, root.NamespaceURI);
				root.AppendChild(node);
				foreach(IField<LogicalCircuitData> field in table.Fields) {
					IFieldSerializer serializer = field as IFieldSerializer;
					if(serializer != null && serializer.NeedToSave(ref data)) {
						XmlElement e = xml.CreateElement(root.Prefix, field.Name, root.NamespaceURI);
						node.AppendChild(e);
						e.AppendChild(xml.CreateTextNode(serializer.GetTextValue(ref data)));
					}
				}
			}
		}

		public static void Load(TableSnapshot<LogicalCircuitData> table, XmlNodeList list, Action<RowId> register) {
			foreach(XmlElement node in list) {
				Debug.Assert(node.LocalName == table.Name);
				LogicalCircuitData data = new LogicalCircuitData();
				// Initialize 'data' with default values: 
				for (int i = 0; i < LogicalCircuitData.fields.Length; i ++) {
					IFieldSerializer serializer = LogicalCircuitData.fields[i] as IFieldSerializer;
					if (serializer != null) {
						serializer.SetDefault(ref data);
					}
				}
				// For each child of 'node' deserialize the field of the 'data' record
				int hintIndex = 0;
				foreach(XmlNode child in node.ChildNodes) {
					XmlElement c = child as XmlElement;
					if(c != null && c.NamespaceURI == node.NamespaceURI) {
						IFieldSerializer serializer = LogicalCircuitData.FindField(c.LocalName, ref hintIndex);
						if (serializer != null) {
							serializer.SetTextValue(ref data, c.InnerText);
						}
					}
				}
				// insert 'data' into the table
				RowId rowId = table.Insert(ref data);
				// 'register' it (create realm object)
				if(register != null) {
					register(rowId);
				}
			}
		}

		private static IFieldSerializer FindField(string name, ref int hint) {
			// We serialize/deserialize fields in the same order so result would always be at hint position or after it if hint is skipped during the serialization
			Debug.Assert(0 <= hint && hint <= LogicalCircuitData.fields.Length);
			for (int i = hint; i < LogicalCircuitData.fields.Length; i ++) {
				if (LogicalCircuitData.fields[i].Name == name) {
					hint = i + 1;
					return LogicalCircuitData.fields[i] as IFieldSerializer;
				}
			}

			// We don't find the field in expected place. Lets look the beginning of the list in case it is out of order
			for (int i = 0; i < hint; i ++) {
				if (LogicalCircuitData.fields[i].Name == name) {
					return LogicalCircuitData.fields[i] as IFieldSerializer;
				}
			}

			// Ups. Still don't find. 
			return null;
		}
	}


	// Class wrapper for a record.
	partial class LogicalCircuit : Circuit {

		// RowId of the wrapped record
		internal RowId LogicalCircuitRowId { get; private set; }

		// Constructor
		public LogicalCircuit(CircuitProject store, RowId rowIdLogicalCircuit, RowId rowIdCircuit) : base(store, rowIdCircuit) {
			Debug.Assert(!rowIdLogicalCircuit.IsEmpty);
			this.LogicalCircuitRowId = rowIdLogicalCircuit;
			// Link back to record. Assuming that a transaction is started
			this.Table.SetField(this.LogicalCircuitRowId, LogicalCircuitData.LogicalCircuitField.Field, this);
			this.InitializeLogicalCircuit();
		}

		partial void InitializeLogicalCircuit();

		// Gets table storing this item.
		private TableSnapshot<LogicalCircuitData> Table { get { return this.CircuitProject.LogicalCircuitSet.Table; } }


		//Properties of LogicalCircuit

		// Gets value of the LogicalCircuitId field.
		public Guid LogicalCircuitId {
			get { return this.Table.GetField(this.LogicalCircuitRowId, LogicalCircuitData.LogicalCircuitIdField.Field); }
		}

		// Gets or sets value of the Name field.
		public override string Name {
			get { return this.Table.GetField(this.LogicalCircuitRowId, LogicalCircuitData.NameField.Field); }
			set { this.Table.SetField(this.LogicalCircuitRowId, LogicalCircuitData.NameField.Field, value); }
		}

		// Gets or sets value of the Notation field.
		public override string Notation {
			get { return this.Table.GetField(this.LogicalCircuitRowId, LogicalCircuitData.NotationField.Field); }
			set { this.Table.SetField(this.LogicalCircuitRowId, LogicalCircuitData.NotationField.Field, value); }
		}

		// Gets or sets value of the Description field.
		public string Description {
			get { return this.Table.GetField(this.LogicalCircuitRowId, LogicalCircuitData.DescriptionField.Field); }
			set { this.Table.SetField(this.LogicalCircuitRowId, LogicalCircuitData.DescriptionField.Field, value); }
		}

		// Gets or sets value of the Category field.
		public override string Category {
			get { return this.Table.GetField(this.LogicalCircuitRowId, LogicalCircuitData.CategoryField.Field); }
			set { this.Table.SetField(this.LogicalCircuitRowId, LogicalCircuitData.CategoryField.Field, value); }
		}


		internal void NotifyChanged(TableChange<LogicalCircuitData> change) {
			if(this.HasListener) {
				LogicalCircuitData oldData, newData;
				change.GetOldData(out oldData);
				change.GetNewData(out newData);
				if(LogicalCircuitData.LogicalCircuitIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("LogicalCircuitId");
				}
				if(LogicalCircuitData.NameField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Name");
				}
				if(LogicalCircuitData.NotationField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Notation");
				}
				if(LogicalCircuitData.DescriptionField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Description");
				}
				if(LogicalCircuitData.CategoryField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Category");
				}
			}
			this.OnLogicalCircuitChanged();
		}

		partial void OnLogicalCircuitChanged();
	}


	// Wrapper for table LogicalCircuit.
	partial class LogicalCircuitSet : INotifyCollectionChanged, IEnumerable<LogicalCircuit> {

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		internal TableSnapshot<LogicalCircuitData> Table { get; private set; }

		// Gets StoreSnapshot this set belongs to.
		public CircuitProject CircuitProject { get { return (CircuitProject)this.Table.StoreSnapshot; } }

		// Constructor
		public LogicalCircuitSet(CircuitProject store) {
			ITableSnapshot table = store.Table("LogicalCircuit");
			if(table != null) {
				Debug.Assert(store.IsFrozen, "The store should be frozen");
				this.Table = (TableSnapshot<LogicalCircuitData>)table;
			} else {
				Debug.Assert(!store.IsFrozen, "In order to create table, the store should not be frozen");
				this.Table = LogicalCircuitData.CreateTable(store);
			}
			this.InitializeLogicalCircuitSet();
		}

		partial void InitializeLogicalCircuitSet();

		//internal void Register() {
		//	foreach(RowId rowId in this.Table.Rows) {
		//		this.FindOrCreate(rowId);
		//	}
		//}


		// gets items wrapper by RowId
		public LogicalCircuit Find(RowId rowId) {
			if(!rowId.IsEmpty) {
				return this.Table.GetField(rowId, LogicalCircuitData.LogicalCircuitField.Field);
			}
			return null;
		}

		private void Delete(RowId rowId) {
		}

		// gets items wrapper by RowId
		private IEnumerable<LogicalCircuit> Select(IEnumerable<RowId> rows) {
			foreach(RowId rowId in rows) {
				LogicalCircuit item = this.Find(rowId);
				Debug.Assert(item != null, "What is the reason for the item not to be found?");
				yield return item;
			}
		}

		// Create wrapper for the row and register it in the dictionary
		private LogicalCircuit Create(RowId rowId, RowId CircuitRowId) {
			LogicalCircuit item = new LogicalCircuit(this.CircuitProject, rowId, CircuitRowId);
			return item;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal LogicalCircuit FindOrCreate(RowId rowId) {
			Debug.Assert(!rowId.IsEmpty && !this.Table.IsDeleted(rowId), "Bad RowId");
			LogicalCircuit item;
			if((item = this.Find(rowId)) != null) {
				Debug.Assert(!item.IsDeleted(), "Deleted item should not be present in the dictionary");
				return item;
			}
			Guid primaryKeyValue = this.Table.GetField(rowId, LogicalCircuitData.LogicalCircuitIdField.Field);


			TableSnapshot<CircuitData> tableCircuit = (TableSnapshot<CircuitData>)this.CircuitProject.Table("Circuit");
			return this.Create(rowId, tableCircuit.Find(CircuitData.CircuitIdField.Field, primaryKeyValue));
		}

		// Creates LogicalCircuit wrapper
		private LogicalCircuit CreateItem(
			// Fields of LogicalCircuit table
			Guid LogicalCircuitId,
			string Name,
			string Notation,
			string Description,
			string Category
			// Fields of Circuit table

		) {
			TableSnapshot<CircuitData> tableCircuit = (TableSnapshot<CircuitData>)this.CircuitProject.Table("Circuit");
			CircuitData dataCircuit = new CircuitData() {
				CircuitId = LogicalCircuitId
			};
			RowId rowIdCircuit = tableCircuit.Insert(ref dataCircuit);

			LogicalCircuitData dataLogicalCircuit = new LogicalCircuitData() {
				LogicalCircuitId = LogicalCircuitId,
				Name = Name,
				Notation = Notation,
				Description = Description,
				Category = Category,
			};
			return this.Create(this.Table.Insert(ref dataLogicalCircuit), rowIdCircuit);
		}

		// Search helpers

		// Finds LogicalCircuit by LogicalCircuitId
		public LogicalCircuit FindByLogicalCircuitId(Guid logicalCircuitId) {
			return this.Find(this.Table.Find(LogicalCircuitData.LogicalCircuitIdField.Field, logicalCircuitId));
		}

		// Finds LogicalCircuit by Name
		public LogicalCircuit FindByName(string name) {
			return this.Find(this.Table.Find(LogicalCircuitData.NameField.Field, name));
		}

		public IEnumerator<LogicalCircuit> GetEnumerator() {
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

		internal List<LogicalCircuit> UpdateSet(int oldVersion, int newVersion) {
			IEnumerator<TableChange<LogicalCircuitData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<LogicalCircuit> del = (handlerAttached) ? new List<LogicalCircuit>() : null;
				while(change.MoveNext()) {
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						this.FindOrCreate(change.Current.RowId);
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						break;
					case SnapTableAction.Delete:
						if(handlerAttached) {
							LogicalCircuit item = change.Current.GetOldField(LogicalCircuitData.LogicalCircuitField.Field);
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

		internal void NotifyVersionChanged(int oldVersion, int newVersion, List<LogicalCircuit> deleted) {
			IEnumerator<TableChange<LogicalCircuitData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<LogicalCircuit> add = (handlerAttached) ? new List<LogicalCircuit>() : null;
				this.StartNotifyLogicalCircuitSetChanged(oldVersion, newVersion);
				while(change.MoveNext()) {
					this.NotifyLogicalCircuitSetChanged(change.Current);
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						if(handlerAttached) {
							add.Add(this.Find(change.Current.RowId));
						}
						break;
					case SnapTableAction.Delete:
						Debug.Assert(change.Current.GetOldField(LogicalCircuitData.LogicalCircuitField.Field).IsDeleted(), "Why the item still exists?");
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
				this.EndNotifyLogicalCircuitSetChanged();
			}
		}

		partial void StartNotifyLogicalCircuitSetChanged(int oldVersion, int newVersion);
		partial void EndNotifyLogicalCircuitSetChanged();
		partial void NotifyLogicalCircuitSetChanged(TableChange<LogicalCircuitData> change);

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
