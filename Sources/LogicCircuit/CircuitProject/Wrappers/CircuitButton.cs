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

	// Defines the shape of the table CircuitButton
	internal partial struct CircuitButtonData {
		public Guid CircuitButtonId;
		public string Notation;
		public bool IsToggle;
		public PinSide PinSide;
		public int Width;
		public int Height;
		public string Note;
		internal CircuitButton CircuitButton;
		// Field accessors
		// Accessor of the CircuitButtonId field
		public sealed class CircuitButtonIdField : IField<CircuitButtonData, Guid>, IFieldSerializer<CircuitButtonData> {
			public static readonly CircuitButtonIdField Field = new CircuitButtonIdField();
			private CircuitButtonIdField() {}
			public string Name { get { return "CircuitButtonId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default; } }
			public Guid GetValue(ref CircuitButtonData record) {
				return record.CircuitButtonId;
			}
			public void SetValue(ref CircuitButtonData record, Guid value) {
				record.CircuitButtonId = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return l.CircuitButtonId.CompareTo(r.CircuitButtonId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.CircuitButtonId, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.CircuitButtonId);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.CircuitButtonId = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.CircuitButtonId = new Guid(text);
			}
		}

		// Accessor of the Notation field
		public sealed class NotationField : IField<CircuitButtonData, string>, IFieldSerializer<CircuitButtonData> {
			public static readonly NotationField Field = new NotationField();
			private NotationField() {}
			public string Name { get { return "Notation"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref CircuitButtonData record) {
				return record.Notation;
			}
			public void SetValue(ref CircuitButtonData record, string value) {
				record.Notation = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return StringComparer.Ordinal.Compare(l.Notation, r.Notation);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.Notation, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Notation);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.Notation = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.Notation = text;
			}
		}

		// Accessor of the IsToggle field
		public sealed class IsToggleField : IField<CircuitButtonData, bool>, IFieldSerializer<CircuitButtonData> {
			public static readonly IsToggleField Field = new IsToggleField();
			private IsToggleField() {}
			public string Name { get { return "IsToggle"; } }
			public int Order { get; set; }
			public bool DefaultValue { get { return false; } }
			public bool GetValue(ref CircuitButtonData record) {
				return record.IsToggle;
			}
			public void SetValue(ref CircuitButtonData record, bool value) {
				record.IsToggle = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return l.IsToggle.CompareTo(r.IsToggle);
			}
			public int Compare(bool l, bool r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.IsToggle, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.IsToggle);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.IsToggle = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.IsToggle = bool.Parse(text);
			}
		}

		// Accessor of the PinSide field
		public sealed class PinSideField : IField<CircuitButtonData, PinSide>, IFieldSerializer<CircuitButtonData> {
			public static readonly PinSideField Field = new PinSideField();
			private PinSideField() {}
			public string Name { get { return "PinSide"; } }
			public int Order { get; set; }
			public PinSide DefaultValue { get { return PinSide.Right; } }
			public PinSide GetValue(ref CircuitButtonData record) {
				return record.PinSide;
			}
			public void SetValue(ref CircuitButtonData record, PinSide value) {
				record.PinSide = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return l.PinSide.CompareTo(r.PinSide);
			}
			public int Compare(PinSide l, PinSide r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.PinSide, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.PinSide);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.PinSide = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.PinSide = EnumHelper.Parse<PinSide>(text, this.DefaultValue);
			}
		}

		// Accessor of the Width field
		public sealed class WidthField : IField<CircuitButtonData, int>, IFieldSerializer<CircuitButtonData> {
			public static readonly WidthField Field = new WidthField();
			private WidthField() {}
			public string Name { get { return "Width"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return 2; } }
			public int GetValue(ref CircuitButtonData record) {
				return record.Width;
			}
			public void SetValue(ref CircuitButtonData record, int value) {
				record.Width = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return Math.Sign((long)l.Width - (long)r.Width);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.Width, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Width);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.Width = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.Width = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Height field
		public sealed class HeightField : IField<CircuitButtonData, int>, IFieldSerializer<CircuitButtonData> {
			public static readonly HeightField Field = new HeightField();
			private HeightField() {}
			public string Name { get { return "Height"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return 2; } }
			public int GetValue(ref CircuitButtonData record) {
				return record.Height;
			}
			public void SetValue(ref CircuitButtonData record, int value) {
				record.Height = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return Math.Sign((long)l.Height - (long)r.Height);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.Height, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Height);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.Height = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.Height = int.Parse(text, CultureInfo.InvariantCulture);
			}
		}

		// Accessor of the Note field
		public sealed class NoteField : IField<CircuitButtonData, string>, IFieldSerializer<CircuitButtonData> {
			public static readonly NoteField Field = new NoteField();
			private NoteField() {}
			public string Name { get { return "Note"; } }
			public int Order { get; set; }
			public string DefaultValue { get { return ""; } }
			public string GetValue(ref CircuitButtonData record) {
				return record.Note;
			}
			public void SetValue(ref CircuitButtonData record, string value) {
				record.Note = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return StringComparer.Ordinal.Compare(l.Note, r.Note);
			}
			public int Compare(string l, string r) {
				return StringComparer.Ordinal.Compare(l, r);
			}

			// Implementation of interface IFieldSerializer<CircuitButtonData>
			bool IFieldSerializer<CircuitButtonData>.NeedToSave(ref CircuitButtonData data) {
				return this.Compare(data.Note, this.DefaultValue) != 0;
			}
			string IFieldSerializer<CircuitButtonData>.GetTextValue(ref CircuitButtonData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Note);
			}
			void IFieldSerializer<CircuitButtonData>.SetDefault(ref CircuitButtonData data) {
				data.Note = this.DefaultValue;
			}
			void IFieldSerializer<CircuitButtonData>.SetTextValue(ref CircuitButtonData data, string text) {
				data.Note = text;
			}
		}

		// Special field used to access items wrapper of this record from record.
		// This is used when no other universes is used
		internal sealed class CircuitButtonField : IField<CircuitButtonData, CircuitButton> {
			public static readonly CircuitButtonField Field = new CircuitButtonField();
			private CircuitButtonField() {}
			public string Name { get { return "CircuitButtonWrapper"; } }
			public int Order { get; set; }
			public CircuitButton DefaultValue { get { return null; } }
			public CircuitButton GetValue(ref CircuitButtonData record) {
				return record.CircuitButton;
			}
			public void SetValue(ref CircuitButtonData record, CircuitButton value) {
				record.CircuitButton = value;
			}
			public int Compare(ref CircuitButtonData l, ref CircuitButtonData r) {
				return this.Compare(l.CircuitButton, r.CircuitButton);
			}
			public int Compare(CircuitButton l, CircuitButton r) {
				if(object.ReferenceEquals(l, r)) return 0;
				if(l == null) return -1;
				if(r == null) return 1;
				return l.CircuitButtonRowId.CompareTo(r.CircuitButtonRowId);
			}
		}

		private static readonly IField<CircuitButtonData>[] fields = {
			CircuitButtonIdField.Field,
			NotationField.Field,
			IsToggleField.Field,
			PinSideField.Field,
			WidthField.Field,
			HeightField.Field,
			NoteField.Field,
			CircuitButtonField.Field
		};

		// Creates table.
		public static TableSnapshot<CircuitButtonData> CreateTable(StoreSnapshot store) {
			TableSnapshot<CircuitButtonData> table = new TableSnapshot<CircuitButtonData>(store, "CircuitButton", CircuitButtonData.fields);
			// Create all but foreign keys of the table
			table.MakeUnique("PK_CircuitButton", CircuitButtonData.CircuitButtonIdField.Field , true);
			// Return created table
			return table;
		}

		// Creates all foreign keys of the table
		public static void CreateForeignKeys(StoreSnapshot store) {
			TableSnapshot<CircuitButtonData> table = (TableSnapshot<CircuitButtonData>)store.Table("CircuitButton");
			table.CreateForeignKey("PK_CircuitButton", store.Table("Circuit"), CircuitButtonData.CircuitButtonIdField.Field, ForeignKeyAction.Cascade, false);
		}
	}

	// Class wrapper for a record.
	partial class CircuitButton : Circuit {

		// RowId of the wrapped record
		internal RowId CircuitButtonRowId { get; private set; }

		// Constructor
		public CircuitButton(CircuitProject store, RowId rowIdCircuitButton, RowId rowIdCircuit) : base(store, rowIdCircuit) {
			Debug.Assert(!rowIdCircuitButton.IsEmpty);
			this.CircuitButtonRowId = rowIdCircuitButton;
			// Link back to record. Assuming that a transaction is started
			this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.CircuitButtonField.Field, this);
			this.InitializeCircuitButton();
		}

		partial void InitializeCircuitButton();

		// Gets table storing this item.
		private TableSnapshot<CircuitButtonData> Table { get { return this.CircuitProject.CircuitButtonSet.Table; } }


		//Properties of CircuitButton

		// Gets value of the CircuitButtonId field.
		public Guid CircuitButtonId {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.CircuitButtonIdField.Field); }
		}

		// Gets or sets value of the Notation field.
		public override string Notation {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.NotationField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.NotationField.Field, value); }
		}

		// Gets or sets value of the IsToggle field.
		public bool IsToggle {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.IsToggleField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.IsToggleField.Field, value); }
		}

		// Gets or sets value of the PinSide field.
		public PinSide PinSide {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.PinSideField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.PinSideField.Field, value); }
		}

		// Gets or sets value of the Width field.
		public int Width {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.WidthField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.WidthField.Field, value); }
		}

		// Gets or sets value of the Height field.
		public int Height {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.HeightField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.HeightField.Field, value); }
		}

		// Gets or sets value of the Note field.
		public override string Note {
			get { return this.Table.GetField(this.CircuitButtonRowId, CircuitButtonData.NoteField.Field); }
			set { this.Table.SetField(this.CircuitButtonRowId, CircuitButtonData.NoteField.Field, value); }
		}


		internal void NotifyChanged(TableChange<CircuitButtonData> change) {
			if(this.HasListener) {
				CircuitButtonData oldData, newData;
				change.GetOldData(out oldData);
				change.GetNewData(out newData);
				if(CircuitButtonData.CircuitButtonIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("CircuitButtonId");
				}
				if(CircuitButtonData.NotationField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Notation");
				}
				if(CircuitButtonData.IsToggleField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("IsToggle");
				}
				if(CircuitButtonData.PinSideField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("PinSide");
				}
				if(CircuitButtonData.WidthField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Width");
				}
				if(CircuitButtonData.HeightField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Height");
				}
				if(CircuitButtonData.NoteField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Note");
				}
			}
			this.OnCircuitButtonChanged();
		}

		partial void OnCircuitButtonChanged();
	}


	// Wrapper for table CircuitButton.
	partial class CircuitButtonSet : INotifyCollectionChanged, IEnumerable<CircuitButton> {

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		internal TableSnapshot<CircuitButtonData> Table { get; private set; }

		// Gets StoreSnapshot this set belongs to.
		public CircuitProject CircuitProject { get { return (CircuitProject)this.Table.StoreSnapshot; } }

		// Constructor
		public CircuitButtonSet(CircuitProject store) {
			ITableSnapshot table = store.Table("CircuitButton");
			if(table != null) {
				Debug.Assert(store.IsFrozen, "The store should be frozen");
				this.Table = (TableSnapshot<CircuitButtonData>)table;
			} else {
				Debug.Assert(!store.IsFrozen, "In order to create table, the store should not be frozen");
				this.Table = CircuitButtonData.CreateTable(store);
			}
			this.InitializeCircuitButtonSet();
		}

		partial void InitializeCircuitButtonSet();

		//internal void Register() {
		//	foreach(RowId rowId in this.Table.Rows) {
		//		this.FindOrCreate(rowId);
		//	}
		//}


		// gets items wrapper by RowId
		public CircuitButton Find(RowId rowId) {
			if(!rowId.IsEmpty) {
				return this.Table.GetField(rowId, CircuitButtonData.CircuitButtonField.Field);
			}
			return null;
		}


		// gets items wrapper by RowId
		private IEnumerable<CircuitButton> Select(IEnumerable<RowId> rows) {
			foreach(RowId rowId in rows) {
				CircuitButton item = this.Find(rowId);
				Debug.Assert(item != null, "What is the reason for the item not to be found?");
				yield return item;
			}
		}

		// Create wrapper for the row and register it in the dictionary
		private CircuitButton Create(RowId rowId, RowId CircuitRowId) {
			CircuitButton item = new CircuitButton(this.CircuitProject, rowId, CircuitRowId);
			return item;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal CircuitButton FindOrCreate(RowId rowId) {
			Debug.Assert(!rowId.IsEmpty && !this.Table.IsDeleted(rowId), "Bad RowId");
			CircuitButton item;
			if((item = this.Find(rowId)) != null) {
				Debug.Assert(!item.IsDeleted(), "Deleted item should not be present in the dictionary");
				return item;
			}
			Guid primaryKeyValue = this.Table.GetField(rowId, CircuitButtonData.CircuitButtonIdField.Field);


			TableSnapshot<CircuitData> tableCircuit = (TableSnapshot<CircuitData>)this.CircuitProject.Table("Circuit");
			return this.Create(rowId, tableCircuit.Find(CircuitData.CircuitIdField.Field, primaryKeyValue));
		}

		// Creates CircuitButton wrapper
		private CircuitButton CreateItem(
			// Fields of CircuitButton table
			Guid CircuitButtonId,
			string Notation,
			bool IsToggle,
			PinSide PinSide,
			int Width,
			int Height,
			string Note
			// Fields of Circuit table

		) {
			TableSnapshot<CircuitData> tableCircuit = (TableSnapshot<CircuitData>)this.CircuitProject.Table("Circuit");
			CircuitData dataCircuit = new CircuitData() {
				CircuitId = CircuitButtonId
			};
			RowId rowIdCircuit = tableCircuit.Insert(ref dataCircuit);

			CircuitButtonData dataCircuitButton = new CircuitButtonData() {
				CircuitButtonId = CircuitButtonId,
				Notation = Notation,
				IsToggle = IsToggle,
				PinSide = PinSide,
				Width = Width,
				Height = Height,
				Note = Note,
			};
			return this.Create(this.Table.Insert(ref dataCircuitButton), rowIdCircuit);
		}

		// Search helpers

		// Finds CircuitButton by CircuitButtonId
		public CircuitButton FindByCircuitButtonId(Guid circuitButtonId) {
			return this.Find(this.Table.Find(CircuitButtonData.CircuitButtonIdField.Field, circuitButtonId));
		}

		public IEnumerator<CircuitButton> GetEnumerator() {
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

		internal List<CircuitButton> UpdateSet(int oldVersion, int newVersion) {
			IEnumerator<TableChange<CircuitButtonData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<CircuitButton> del = (handlerAttached) ? new List<CircuitButton>() : null;
				while(change.MoveNext()) {
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						this.FindOrCreate(change.Current.RowId);
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						break;
					case SnapTableAction.Delete:
						if(handlerAttached) {
							CircuitButton item = change.Current.GetOldField(CircuitButtonData.CircuitButtonField.Field);
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

		internal void NotifyVersionChanged(int oldVersion, int newVersion, List<CircuitButton> deleted) {
			IEnumerator<TableChange<CircuitButtonData>> change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<CircuitButton> add = (handlerAttached) ? new List<CircuitButton>() : null;
				this.StartNotifyCircuitButtonSetChanged(oldVersion, newVersion);
				while(change.MoveNext()) {
					this.NotifyCircuitButtonSetChanged(change.Current);
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						if(handlerAttached) {
							add.Add(this.Find(change.Current.RowId));
						}
						break;
					case SnapTableAction.Delete:
						Debug.Assert(change.Current.GetOldField(CircuitButtonData.CircuitButtonField.Field).IsDeleted(), "Why the item still exists?");
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
				this.EndNotifyCircuitButtonSetChanged();
			}
		}

		partial void StartNotifyCircuitButtonSetChanged(int oldVersion, int newVersion);
		partial void EndNotifyCircuitButtonSetChanged();
		partial void NotifyCircuitButtonSetChanged(TableChange<CircuitButtonData> change);

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
