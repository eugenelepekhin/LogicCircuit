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

	// Defines the shape of the table Splitter
	internal partial struct SplitterData {
		public Guid SplitterId;
		private int fieldBitWidth;
		public int BitWidth {
			get { return this.fieldBitWidth; }
			set { this.fieldBitWidth = BasePin.CheckBitWidth(value); }
		}
		private int fieldPinCount;
		public int PinCount {
			get { return this.fieldPinCount; }
			set { this.fieldPinCount = BasePin.CheckBitWidth(value); }
		}
		public bool Clockwise;
		internal Splitter? Splitter;
		// Field accessors
		// Accessor of the SplitterId field
		public sealed class SplitterIdField : IField<SplitterData, Guid>, IFieldSerializer<SplitterData> {
			public static readonly SplitterIdField Field = new SplitterIdField();
			private SplitterIdField() {}
			public string Name { get { return "SplitterId"; } }
			public int Order { get; set; }
			public Guid DefaultValue { get { return default; } }
			public Guid GetValue(ref SplitterData record) {
				return record.SplitterId;
			}
			public void SetValue(ref SplitterData record, Guid value) {
				record.SplitterId = value;
			}
			public int Compare(ref SplitterData l, ref SplitterData r) {
				return l.SplitterId.CompareTo(r.SplitterId);
			}
			public int Compare(Guid l, Guid r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<SplitterData>
			bool IFieldSerializer<SplitterData>.NeedToSave(ref SplitterData data) {
				return this.Compare(data.SplitterId, this.DefaultValue) != 0;
			}
			string IFieldSerializer<SplitterData>.GetTextValue(ref SplitterData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.SplitterId);
			}
			void IFieldSerializer<SplitterData>.SetDefault(ref SplitterData data) {
				data.SplitterId = this.DefaultValue;
			}
			void IFieldSerializer<SplitterData>.SetTextValue(ref SplitterData data, string text) {
				data.SplitterId = new Guid(text);
			}
			bool IFieldSerializer<SplitterData>.WasWritten { get; set; }

		}

		// Accessor of the BitWidth field
		public sealed class BitWidthField : IField<SplitterData, int>, IFieldSerializer<SplitterData> {
			public static readonly BitWidthField Field = new BitWidthField();
			private BitWidthField() {}
			public string Name { get { return "BitWidth"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return default; } }
			public int GetValue(ref SplitterData record) {
				return record.BitWidth;
			}
			public void SetValue(ref SplitterData record, int value) {
				record.BitWidth = value;
			}
			public int Compare(ref SplitterData l, ref SplitterData r) {
				return Math.Sign((long)l.BitWidth - (long)r.BitWidth);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer<SplitterData>
			bool IFieldSerializer<SplitterData>.NeedToSave(ref SplitterData data) {
				return this.Compare(data.BitWidth, this.DefaultValue) != 0;
			}
			string IFieldSerializer<SplitterData>.GetTextValue(ref SplitterData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.BitWidth);
			}
			void IFieldSerializer<SplitterData>.SetDefault(ref SplitterData data) {
				data.BitWidth = this.DefaultValue;
			}
			void IFieldSerializer<SplitterData>.SetTextValue(ref SplitterData data, string text) {
				data.BitWidth = int.Parse(text, CultureInfo.InvariantCulture);
			}
			bool IFieldSerializer<SplitterData>.WasWritten { get; set; }

		}

		// Accessor of the PinCount field
		public sealed class PinCountField : IField<SplitterData, int>, IFieldSerializer<SplitterData> {
			public static readonly PinCountField Field = new PinCountField();
			private PinCountField() {}
			public string Name { get { return "PinCount"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return default; } }
			public int GetValue(ref SplitterData record) {
				return record.PinCount;
			}
			public void SetValue(ref SplitterData record, int value) {
				record.PinCount = value;
			}
			public int Compare(ref SplitterData l, ref SplitterData r) {
				return Math.Sign((long)l.PinCount - (long)r.PinCount);
			}
			public int Compare(int l, int r) {
				return Math.Sign((long)l - (long)r);
			}

			// Implementation of interface IFieldSerializer<SplitterData>
			bool IFieldSerializer<SplitterData>.NeedToSave(ref SplitterData data) {
				return this.Compare(data.PinCount, this.DefaultValue) != 0;
			}
			string IFieldSerializer<SplitterData>.GetTextValue(ref SplitterData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.PinCount);
			}
			void IFieldSerializer<SplitterData>.SetDefault(ref SplitterData data) {
				data.PinCount = this.DefaultValue;
			}
			void IFieldSerializer<SplitterData>.SetTextValue(ref SplitterData data, string text) {
				data.PinCount = int.Parse(text, CultureInfo.InvariantCulture);
			}
			bool IFieldSerializer<SplitterData>.WasWritten { get; set; }

		}

		// Accessor of the Clockwise field
		public sealed class ClockwiseField : IField<SplitterData, bool>, IFieldSerializer<SplitterData> {
			public static readonly ClockwiseField Field = new ClockwiseField();
			private ClockwiseField() {}
			public string Name { get { return "Clockwise"; } }
			public int Order { get; set; }
			public bool DefaultValue { get { return default; } }
			public bool GetValue(ref SplitterData record) {
				return record.Clockwise;
			}
			public void SetValue(ref SplitterData record, bool value) {
				record.Clockwise = value;
			}
			public int Compare(ref SplitterData l, ref SplitterData r) {
				return l.Clockwise.CompareTo(r.Clockwise);
			}
			public int Compare(bool l, bool r) {
				return l.CompareTo(r);
			}

			// Implementation of interface IFieldSerializer<SplitterData>
			bool IFieldSerializer<SplitterData>.NeedToSave(ref SplitterData data) {
				return this.Compare(data.Clockwise, this.DefaultValue) != 0;
			}
			string IFieldSerializer<SplitterData>.GetTextValue(ref SplitterData data) {
				return string.Format(CultureInfo.InvariantCulture, "{0}", data.Clockwise);
			}
			void IFieldSerializer<SplitterData>.SetDefault(ref SplitterData data) {
				data.Clockwise = this.DefaultValue;
			}
			void IFieldSerializer<SplitterData>.SetTextValue(ref SplitterData data, string text) {
				data.Clockwise = bool.Parse(text);
			}
			bool IFieldSerializer<SplitterData>.WasWritten { get; set; }

		}

		// Special field used to access items wrapper of this record from record.
		// This is used when no other universes is used
		internal sealed class SplitterField : IField<SplitterData, Splitter> {
			public static readonly SplitterField Field = new SplitterField();
			private SplitterField() {}
			public string Name { get { return "SplitterWrapper"; } }
			public int Order { get; set; }
			public Splitter DefaultValue { get { return null!; } }
			public Splitter GetValue(ref SplitterData record) {
				return record.Splitter!;
			}
			public void SetValue(ref SplitterData record, Splitter value) {
				record.Splitter = value;
			}
			public int Compare(ref SplitterData l, ref SplitterData r) {
				return this.Compare(l.Splitter, r.Splitter);
			}
			public int Compare(Splitter? l, Splitter? r) {
				if(object.ReferenceEquals(l, r)) return 0;
				if(l == null) return -1;
				if(r == null) return 1;
				return l.SplitterRowId.CompareTo(r.SplitterRowId);
			}
		}

		private static readonly IField<SplitterData>[] fields = {
			SplitterIdField.Field,
			BitWidthField.Field,
			PinCountField.Field,
			ClockwiseField.Field,
			SplitterField.Field
		};

		// Creates table.
		public static TableSnapshot<SplitterData> CreateTable(StoreSnapshot store) {
			TableSnapshot<SplitterData> table = new TableSnapshot<SplitterData>(store, "Splitter", SplitterData.fields);
			// Create all but foreign keys of the table
			table.MakeUnique("PK_Splitter", SplitterData.SplitterIdField.Field , true);
			// Return created table
			return table;
		}

		// Creates all foreign keys of the table
		public static void CreateForeignKeys(StoreSnapshot store) {
			TableSnapshot<SplitterData>? table = (TableSnapshot<SplitterData>?)store.Table("Splitter");
			Debug.Assert(table != null);
			table.CreateForeignKey("PK_Splitter", store.Table("Circuit"), SplitterData.SplitterIdField.Field, ForeignKeyAction.Cascade, false);
		}
	}

	// Class wrapper for a record.
	partial class Splitter : Circuit {

		// RowId of the wrapped record
		internal RowId SplitterRowId { get; private set; }

		// Constructor
		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public Splitter(CircuitProject store, RowId rowIdSplitter, RowId rowIdCircuit) : base(store, rowIdCircuit) {
			Debug.Assert(!rowIdSplitter.IsEmpty);
			this.SplitterRowId = rowIdSplitter;
			// Link back to record. Assuming that a transaction is started
			this.Table.SetField(this.SplitterRowId, SplitterData.SplitterField.Field, this);
			this.InitializeSplitter();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		partial void InitializeSplitter();

		// Gets table storing this item.
		private TableSnapshot<SplitterData> Table { get { return this.CircuitProject.SplitterSet.Table; } }


		//Properties of Splitter

		// Gets value of the SplitterId field.
		public Guid SplitterId {
			get { return this.Table.GetField(this.SplitterRowId, SplitterData.SplitterIdField.Field); }
		}

		// Gets or sets value of the BitWidth field.
		public int BitWidth {
			get { return this.Table.GetField(this.SplitterRowId, SplitterData.BitWidthField.Field); }
			set { this.Table.SetField(this.SplitterRowId, SplitterData.BitWidthField.Field, value); }
		}

		// Gets or sets value of the PinCount field.
		public int PinCount {
			get { return this.Table.GetField(this.SplitterRowId, SplitterData.PinCountField.Field); }
			set { this.Table.SetField(this.SplitterRowId, SplitterData.PinCountField.Field, value); }
		}

		// Gets or sets value of the Clockwise field.
		public bool Clockwise {
			get { return this.Table.GetField(this.SplitterRowId, SplitterData.ClockwiseField.Field); }
			set { this.Table.SetField(this.SplitterRowId, SplitterData.ClockwiseField.Field, value); }
		}


		internal void NotifyChanged(TableChange<SplitterData> change) {
			if(this.HasListener) {
				SplitterData oldData, newData;
				change.GetOldData(out oldData);
				change.GetNewData(out newData);
				if(SplitterData.SplitterIdField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("SplitterId");
				}
				if(SplitterData.BitWidthField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("BitWidth");
				}
				if(SplitterData.PinCountField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("PinCount");
				}
				if(SplitterData.ClockwiseField.Field.Compare(ref oldData, ref newData) != 0) {
					this.NotifyPropertyChanged("Clockwise");
				}
			}
			this.OnSplitterChanged();
		}

		partial void OnSplitterChanged();
	}


	// Wrapper for table Splitter.
	partial class SplitterSet : INotifyCollectionChanged, IEnumerable<Splitter> {

		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		internal TableSnapshot<SplitterData> Table { get; private set; }

		// Gets StoreSnapshot this set belongs to.
		public CircuitProject CircuitProject { get { return (CircuitProject)this.Table.StoreSnapshot; } }

		// Constructor
		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public SplitterSet(CircuitProject store) {
			ITableSnapshot? table = store.Table("Splitter");
			if(table != null) {
				Debug.Assert(store.IsFrozen, "The store should be frozen");
				this.Table = (TableSnapshot<SplitterData>)table;
			} else {
				Debug.Assert(!store.IsFrozen, "In order to create table, the store should not be frozen");
				this.Table = SplitterData.CreateTable(store);
			}
			this.InitializeSplitterSet();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		partial void InitializeSplitterSet();

		//internal void Register() {
		//	foreach(RowId rowId in this.Table.Rows) {
		//		this.FindOrCreate(rowId);
		//	}
		//}


		// gets items wrapper by RowId
		public Splitter? Find(RowId rowId) {
			if(!rowId.IsEmpty) {
				return this.Table.GetField(rowId, SplitterData.SplitterField.Field);
			}
			return null;
		}


		// gets items wrapper by RowId
		private IEnumerable<Splitter> Select(IEnumerable<RowId> rows) {
			foreach(RowId rowId in rows) {
				Splitter? item = this.Find(rowId);
				Debug.Assert(item != null, "What is the reason for the item not to be found?");
				yield return item;
			}
		}

		// Create wrapper for the row and register it in the dictionary
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
		private Splitter Create(RowId rowId, RowId CircuitRowId) {
			Splitter item = new Splitter(this.CircuitProject, rowId, CircuitRowId);
			return item;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal Splitter FindOrCreate(RowId rowId) {
			Debug.Assert(!rowId.IsEmpty && !this.Table.IsDeleted(rowId), "Bad RowId");
			Splitter? item;
			if((item = this.Find(rowId)) != null) {
				Debug.Assert(!item.IsDeleted(), "Deleted item should not be present in the dictionary");
				return item;
			}
			Guid primaryKeyValue = this.Table.GetField(rowId, SplitterData.SplitterIdField.Field);


			TableSnapshot<CircuitData>? tableCircuit = (TableSnapshot<CircuitData>?)this.CircuitProject.Table("Circuit");
			Debug.Assert(tableCircuit != null);
			return this.Create(rowId, tableCircuit.Find(CircuitData.CircuitIdField.Field, primaryKeyValue));
		}

		// Creates Splitter wrapper
		private Splitter CreateItem(
			// Fields of Splitter table
			Guid SplitterId,
			int BitWidth,
			int PinCount,
			bool Clockwise
			// Fields of Circuit table

		) {
			TableSnapshot<CircuitData>? tableCircuit = (TableSnapshot<CircuitData>?)this.CircuitProject.Table("Circuit");
			Debug.Assert(tableCircuit != null);
			CircuitData dataCircuit = new CircuitData() {
				CircuitId = SplitterId
			};
			RowId rowIdCircuit = tableCircuit.Insert(ref dataCircuit);

			SplitterData dataSplitter = new SplitterData() {
				SplitterId = SplitterId,
				BitWidth = BitWidth,
				PinCount = PinCount,
				Clockwise = Clockwise,
			};
			return this.Create(this.Table.Insert(ref dataSplitter), rowIdCircuit);
		}

		// Search helpers

		// Finds Splitter by SplitterId
		public Splitter? FindBySplitterId(Guid splitterId) {
			return this.Find(this.Table.Find(SplitterData.SplitterIdField.Field, splitterId));
		}

		public IEnumerator<Splitter> GetEnumerator() {
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

		internal List<Splitter>? UpdateSet(int oldVersion, int newVersion) {
			IEnumerator<TableChange<SplitterData>>? change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<Splitter>? del = (handlerAttached) ? new List<Splitter>() : null;
				while(change.MoveNext()) {
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						this.FindOrCreate(change.Current.RowId);
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						break;
					case SnapTableAction.Delete:
						if(handlerAttached) {
							Splitter item = change.Current.GetOldField(SplitterData.SplitterField.Field);
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

		internal void NotifyVersionChanged(int oldVersion, int newVersion, List<Splitter>? deleted) {
			IEnumerator<TableChange<SplitterData>>? change = this.Table.GetVersionChangeChanges(oldVersion, newVersion);
			if(change != null) {
				bool handlerAttached = (this.CollectionChanged != null);
				List<Splitter>? add = (handlerAttached) ? new List<Splitter>() : null;
				this.StartNotifySplitterSetChanged(oldVersion, newVersion);
				while(change.MoveNext()) {
					this.NotifySplitterSetChanged(change.Current);
					switch(change.Current.Action) {
					case SnapTableAction.Insert:
						Debug.Assert(this.Find(change.Current.RowId) != null, "Why the item was not created?");
						if(handlerAttached) {
							add!.Add(this.Find(change.Current.RowId)!);
						}
						break;
					case SnapTableAction.Delete:
						Debug.Assert(change.Current.GetOldField(SplitterData.SplitterField.Field).IsDeleted(), "Why the item still exists?");
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
				this.EndNotifySplitterSetChanged();
			}
		}

		partial void StartNotifySplitterSetChanged(int oldVersion, int newVersion);
		partial void EndNotifySplitterSetChanged();
		partial void NotifySplitterSetChanged(TableChange<SplitterData> change);

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
