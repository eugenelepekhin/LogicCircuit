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
	using LogicCircuit.DataPersistent;

	// Defines the shape of the table CircuitProbe
	internal partial struct CircuitProbeData {
		public Guid CircuitProbeId;
		private string fieldName;
		public string Name {
			get { return this.fieldName; }
			set { this.fieldName = this.CheckName(value); }
		}
		public PinSide PinSide;
		public string Note;
		internal CircuitProbe? CircuitProbe;
		// Field accessors
		// Accessor of the CircuitProbeId field
		public sealed class CircuitProbeIdField : IField<CircuitProbeData, Guid>, IFieldSerializer<CircuitProbeData> {
			public static readonly CircuitProbeIdField Field = new CircuitProbeIdField();
			private CircuitProbeIdField() {}
			public string Name { get { return "CircuitProbeId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default; } }
			public Guid GetValue(ref CircuitProbeData record) {
				return record.CircuitProbeId;
			}
			public void SetValue(ref CircuitProbeData record, Guid value) {
				record.CircuitProbeId = value;
			}
			public int Compare(ref CircuitProbeData l, ref CircuitProbeData r) {
				return l.CircuitProbeId.CompareTo(r.CircuitProbeId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<CircuitProbeData>
			bool IFieldSerializer<CircuitProbeData>.NeedToSave(ref CircuitProbeData data) {
				return this.Compare(data.CircuitProbeId, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitProbeData>.GetTextValue(ref CircuitProbeData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.CircuitProbeId);
			}
			void IFieldSerializer<CircuitProbeData>.SetDefault(ref CircuitProbeData data) {
				data.CircuitProbeId = this.DefaultValue;
			}
			void IFieldSerializer<CircuitProbeData>.SetTextValue(ref CircuitProbeData data, string text) {
				data.CircuitProbeId = new Guid(text);
			}
			bool IFieldSerializer<CircuitProbeData>.WasWritten { get; set; }

		}

		// Accessor of the Name field
		public sealed class NameField : IField<CircuitProbeData, string>, IFieldSerializer<CircuitProbeData> {
			public static readonly NameField Field = new NameField();
			private NameField() {}
			public string Name { get { return "Name"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref CircuitProbeData record) {
				return record.Name;
			}
			public void SetValue(ref CircuitProbeData record, string value) {
				record.Name = value;
			}
			public int Compare(ref CircuitProbeData l, ref CircuitProbeData r) {
				return StringComparer.Ordinal.Compare(l.Name, r.Name);
			}
			public int Compare(string? l, string? r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer<CircuitProbeData>
			bool IFieldSerializer<CircuitProbeData>.NeedToSave(ref CircuitProbeData data) {
				return this.Compare(data.Name, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitProbeData>.GetTextValue(ref CircuitProbeData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Name);
			}
			void IFieldSerializer<CircuitProbeData>.SetDefault(ref CircuitProbeData data) {
				data.Name = this.DefaultValue;
			}
			void IFieldSerializer<CircuitProbeData>.SetTextValue(ref CircuitProbeData data, string text) {
				data.Name = text;
			}
			bool IFieldSerializer<CircuitProbeData>.WasWritten { get; set; }

		}

		// Accessor of the PinSide field
		public sealed class PinSideField : IField<CircuitProbeData, PinSide>, IFieldSerializer<CircuitProbeData> {
			public static readonly PinSideField Field = new PinSideField();
			private PinSideField() {}
			public string Name { get { return "PinSide"; } }
			public int Order { get; set; }
			public PinSide DefaultValue { get { return PinSide.Left; } }
			public PinSide GetValue(ref CircuitProbeData record) {
				return record.PinSide;
			}
			public void SetValue(ref CircuitProbeData record, PinSide value) {
				record.PinSide = value;
			}
			public int Compare(ref CircuitProbeData l, ref CircuitProbeData r) {
				return l.PinSide.CompareTo(r.PinSide);
			}
			public int Compare(PinSide l, PinSide r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<CircuitProbeData>
			bool IFieldSerializer<CircuitProbeData>.NeedToSave(ref CircuitProbeData data) {
				return this.Compare(data.PinSide, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitProbeData>.GetTextValue(ref CircuitProbeData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.PinSide);
			}
			void IFieldSerializer<CircuitProbeData>.SetDefault(ref CircuitProbeData data) {
				data.PinSide = this.DefaultValue;
			}
			void IFieldSerializer<CircuitProbeData>.SetTextValue(ref CircuitProbeData data, string text) {
				data.PinSide = EnumHelper.Parse<PinSide>(text, this.DefaultValue);
			}
			bool IFieldSerializer<CircuitProbeData>.WasWritten { get; set; }

		}

		// Accessor of the Note field
		public sealed class NoteField : IField<CircuitProbeData, string>, IFieldSerializer<CircuitProbeData> {
			public static readonly NoteField Field = new NoteField();
			private NoteField() {}
			public string Name { get { return "Note"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref CircuitProbeData record) {
				return record.Note;
			}
			public void SetValue(ref CircuitProbeData record, string value) {
				record.Note = value;
			}
			public int Compare(ref CircuitProbeData l, ref CircuitProbeData r) {
				return StringComparer.Ordinal.Compare(l.Note, r.Note);
			}
			public int Compare(string? l, string? r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer<CircuitProbeData>
			bool IFieldSerializer<CircuitProbeData>.NeedToSave(ref CircuitProbeData data) {
				return this.Compare(data.Note, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitProbeData>.GetTextValue(ref CircuitProbeData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Note);
			}
			void IFieldSerializer<CircuitProbeData>.SetDefault(ref CircuitProbeData data) {
				data.Note = this.DefaultValue;
			}
			void IFieldSerializer<CircuitProbeData>.SetTextValue(ref CircuitProbeData data, string text) {
				data.Note = text;
			}
			bool IFieldSerializer<CircuitProbeData>.WasWritten { get; set; }

		}

		// Special field used to access items wrapper of this record from record.
		// This is used when no other universes is used
		internal sealed class CircuitProbeField : IField<CircuitProbeData, CircuitProbe> {
			public static readonly CircuitProbeField Field = new CircuitProbeField();
			private CircuitProbeField() {}
			public string Name { get { return "CircuitProbeWrapper"; } }
			public int Order { get; set; }
			public CircuitProbe DefaultValue { get { return null!; } }
			public CircuitProbe GetValue(ref CircuitProbeData record) {
				return record.CircuitProbe!;
			}
			public void SetValue(ref CircuitProbeData record, CircuitProbe value) {
				record.CircuitProbe = value;
			}
			public int Compare(ref CircuitProbeData l, ref CircuitProbeData r) {
				return this.Compare(l.CircuitProbe, r.CircuitProbe);
			}
			public int Compare(CircuitProbe? l, CircuitProbe? r) {
				if(object.ReferenceEquals(l, r)) return 0;
				if(l == null) return -1;
				if(r == null) return 1;
				return l.CircuitProbeRowId.CompareTo(r.CircuitProbeRowId);
			}
		}

		private static readonly IField<CircuitProbeData>[] fields = {
			CircuitProbeIdField.Field,
			NameField.Field,
			PinSideField.Field,
			NoteField.Field,
			CircuitProbeField.Field
		};

		// Creates table.
		public static TableSnapshot<CircuitProbeData> CreateTable(StoreSnapshot store) {
			TableSnapshot<CircuitProbeData> table = new TableSnapshot<CircuitProbeData>(store, "CircuitProbe", CircuitProbeData.fields);
			// Create all but foreign keys of the table
			table.MakeUnique("PK_CircuitProbe", CircuitProbeData.CircuitProbeIdField.Field , true);
			table.MakeUnique("AK_CircuitProbe_Name", CircuitProbeData.NameField.Field , false);
			// Return created table
			return table;
		}

		// Creates all foreign keys of the table
		public static void CreateForeignKeys(StoreSnapshot store) {
			TableSnapshot<CircuitProbeData>? table = (TableSnapshot<CircuitProbeData>?)store.Table("CircuitProbe");
			Debug.Assert(table != null);
			table.CreateForeignKey("PK_CircuitProbe", store.Table("Circuit"), CircuitProbeData.CircuitProbeIdField.Field, ForeignKeyAction.Cascade, false);
		}
	}

	// Class wrapper for a record.
	partial class CircuitProbe : Circuit {

		// RowId of the wrapped record
		internal RowId CircuitProbeRowId { get; private set; }

		// Constructor
		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public CircuitProbe(CircuitProject store, RowId rowIdCircuitProbe, RowId rowIdCircuit) : base(store, rowIdCircuit) {
			Debug.Assert(!rowIdCircuitProbe.IsEmpty);
			this.CircuitProbeRowId = rowIdCircuitProbe;
			// Link back to record. Assuming that a transaction is started
			this.Table.SetField(this.CircuitProbeRowId, CircuitProbeData.CircuitProbeField.Field, this);
			this.InitializeCircuitProbe();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		partial void InitializeCircuitProbe();

		// Gets table storing this item.
		private TableSnapshot<CircuitProbeData> Table { get { return this.CircuitProject.CircuitProbeSet.Table; } }


		//Properties of CircuitProbe

		// Gets value of the CircuitProbeId field.
		public Guid CircuitProbeId {
			get { return this.Table.GetField(this.CircuitProbeRowId, CircuitProbeData.CircuitProbeIdField.Field); }
		}

		// Gets or sets value of the Name field.
		public override string Name {
			get { return this.Table.GetField(this.CircuitProbeRowId, CircuitProbeData.NameField.Field); }
			set { this.Table.SetField(this.CircuitProbeRowId, CircuitProbeData.NameField.Field, value); }
		}

		// Gets or sets value of the PinSide field.
		public PinSide PinSide {
			get { return this.Table.GetField(this.CircuitProbeRowId, CircuitProbeData.PinSideField.Field); }
			set { this.Table.SetField(this.CircuitProbeRowId, CircuitProbeData.PinSideField.Field, value); }
		}

		// Gets or sets value of the Note field.
		public override string Note {
			get { return this.Table.GetField(this.CircuitProbeRowId, CircuitProbeData.NoteField.Field); }
			set { this.Table.SetField(this.CircuitProbeRowId, CircuitProbeData.NoteField.Field, value); }
		}


		internal void NotifyChanged(TableChange<CircuitProbeData> change) {
			if(this.HasListener) {
				CircuitProbeData oldData, newData;
				change.GetOldData(out oldData);
				change.GetNewData(out newData);
				if(CircuitProbeData.CircuitProbeIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("CircuitProbeId");
				}
				if(CircuitProbeData.NameField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Name");
				}
				if(CircuitProbeData.PinSideField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("PinSide");
				}
				if(CircuitProbeData.NoteField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Note");
				}
			}
			this.OnCircuitProbeChanged();
		}

		partial void OnCircuitProbeChanged();
	}


	// Wrapper for table CircuitProbe.
	partial class CircuitProbeSet : INotifyCollectionChanged, IEnumerable<CircuitProbe> {

		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		internal TableSnapshot<CircuitProbeData> Table { get; private set; }

		// Gets StoreSnapshot this set belongs to.
		public CircuitProject CircuitProject { get { return (CircuitProject)this.Table.StoreSnapshot; } }

		// Constructor
		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public CircuitProbeSet(CircuitProject store) {
			ITableSnapshot? table = store.Table("CircuitProbe");
			if(table != null) {
				Debug.Assert(store.IsFrozen, "The store should be frozen");
				this.Table = (TableSnapshot<CircuitProbeData>)table;
			} else {
				Debug.Assert(!store.IsFrozen, "In order to create table, the store should not be frozen");
				this.Table = CircuitProbeData.CreateTable(store);
			}
			this.InitializeCircuitProbeSet();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		partial void InitializeCircuitProbeSet();

		//internal void Register() {
		//	foreach(RowId rowId in this.Table.Rows) {
		//		this.FindOrCreate(rowId);
		//	}
		//}


		// gets items wrapper by RowId
		public CircuitProbe? Find(RowId rowId) {
			if(!rowId.IsEmpty) {
				return this.Table.GetField(rowId, CircuitProbeData.CircuitProbeField.Field);
			}
			return null;
		}


		// gets items wrapper by RowId
		private IEnumerable<CircuitProbe> Select(IEnumerable<RowId> rows) {
			foreach(RowId rowId in rows) {
				CircuitProbe? item = this.Find(rowId);
				Debug.Assert(item != null, "What is the reason for the item not to be found?");
				yield return item;
			}
		}

		// Create wrapper for the row and register it in the dictionary
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
		private CircuitProbe Create(RowId rowId, RowId CircuitRowId) {
			CircuitProbe item = new CircuitProbe(this.CircuitProject, rowId, CircuitRowId);
			return item;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal CircuitProbe FindOrCreate(RowId rowId) {
			Debug.Assert(!rowId.IsEmpty && !this.Table.IsDeleted(rowId), "Bad RowId");
			CircuitProbe? item;
			if((item = this.Find(rowId)) != null) {
				Debug.Assert(!item.IsDeleted(), "Deleted item should not be present in the dictionary");
				return item;
			}
			Guid primaryKeyValue = this.Table.GetField(rowId, CircuitProbeData.CircuitProbeIdField.Field);


			TableSnapshot<CircuitData>? tableCircuit = (TableSnapshot<CircuitData>?)this.CircuitProject.Table("Circuit");
			Debug.Assert(tableCircuit != null);
			return this.Create(rowId, tableCircuit.Find(CircuitData.CircuitIdField.Field, primaryKeyValue));
		}

		// Creates CircuitProbe wrapper
		private CircuitProbe CreateItem(
			// Fields of CircuitProbe table
			Guid CircuitProbeId,
			string Name,
			PinSide PinSide,
			string Note
			// Fields of Circuit table

		) {
			TableSnapshot<CircuitData>? tableCircuit = (TableSnapshot<CircuitData>?)this.CircuitProject.Table("Circuit");
			Debug.Assert(tableCircuit != null);
			CircuitData dataCircuit = new CircuitData() {
				CircuitId = CircuitProbeId
			};
			RowId rowIdCircuit = tableCircuit.Insert(ref dataCircuit);

			CircuitProbeData dataCircuitProbe = new CircuitProbeData() {
				CircuitProbeId = CircuitProbeId,
				Name = Name,
				PinSide = PinSide,
				Note = Note,
			};
			return this.Create(this.Table.Insert(ref dataCircuitProbe), rowIdCircuit);
		}

		// Search helpers

		// Finds CircuitProbe by CircuitProbeId
		public CircuitProbe? FindByCircuitProbeId(Guid circuitProbeId) {
			return this.Find(this.Table.Find(CircuitProbeData.CircuitProbeIdField.Field, circuitProbeId));
		}

		// Finds CircuitProbe by Name
		public CircuitProbe? FindByName(string name) {
			return this.Find(this.Table.Find(CircuitProbeData.NameField.Field, name));
		}

		public IEnumerator<CircuitProbe> GetEnumerator() {
			return this.Select(this.Table.Rows).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs arg) {
			NotifyCollectionChangedEventHandler? handler = this.CollectionChanged;
			if(handler != null) {
				handler(this, arg);
			}
		}

		internal List<CircuitProbe>? UpdateSet(int oldVersion, int newVersion) {
			IEnumerator<TableChange<CircuitProbeData>>? change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<CircuitProbe>? del = (handlerAttached) ? new List<CircuitProbe>() : null;
				while(change.MoveNext()) {
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						this.FindOrCreate(change.Current.RowId);
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						break;
					case SnapTableAction.Delete:
						if(handlerAttached) {
							CircuitProbe item = change.Current.GetOldField(CircuitProbeData.CircuitProbeField.Field);
							Debug.Assert(item.IsDeleted());
							del!.Add(item);
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

		internal void NotifyVersionChanged(int oldVersion, int newVersion, List<CircuitProbe>? deleted) {
			IEnumerator<TableChange<CircuitProbeData>>? change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<CircuitProbe>? add = (handlerAttached) ? new List<CircuitProbe>() : null;
				this.StartNotifyCircuitProbeSetChanged(oldVersion, newVersion);
				while(change.MoveNext()) {
					this.NotifyCircuitProbeSetChanged(change.Current);
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						if(handlerAttached) {
							add!.Add(this.Find(change.Current.RowId)!);
						}
						break;
					case SnapTableAction.Delete:
						Debug.Assert(change.Current.GetOldField(CircuitProbeData.CircuitProbeField.Field).IsDeleted(), "Why the item still exists?");
						break;
					default:
						Debug.Assert(change.Current.Action == SnapTableAction.Update, "Unknown action");
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item does not exist during update?");
						this.Find(change.Current.RowId)!.NotifyChanged(change.Current);
						break;
					}
				}
				change.Dispose();
				if(handlerAttached) {
					if(deleted != null && 0 < deleted.Count) {
						this.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, deleted));
					}
					if(0 < add!.Count) {
						this.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, add));
					}
				}
				this.EndNotifyCircuitProbeSetChanged();
			}
		}

		partial void StartNotifyCircuitProbeSetChanged(int oldVersion, int newVersion);
		partial void EndNotifyCircuitProbeSetChanged();
		partial void NotifyCircuitProbeSetChanged(TableChange<CircuitProbeData> change);

		internal void NotifyRolledBack(int version) {
			if(this.Table.WasAffected(version)) {
				IEnumerator<RowId> change = this.Table.GetRolledBackChanges(version);
				if(change != null) {
					while(change.MoveNext()) {
						RowId rowId = change.Current;
						if(this.Table.IsDeleted(rowId)) {
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
