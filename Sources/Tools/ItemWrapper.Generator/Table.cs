using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ItemWrapper.Generator {
	public class Table {
		public Store Store { get; private set; }
		public string Name { get; set; }
		public ItemModifier ItemModifier { get; set; }
		public string ItemBaseClass { get; set; }
		public bool Persistent { get; set; }
		public List<Column> Columns { get; private set; }
		public List<Key> Keys { get; private set; }

		public Table() {
			this.ItemModifier = ItemModifier.None;
			this.Persistent = true;
			this.Columns = new List<Column>();
			this.Keys = new List<Key>();
		}

		public Key PrimaryKey() {
			return this.Keys.FirstOrDefault(k => k.IsPrimary());
		}

		public bool IsPrimary(Column column) {
			Key primary = this.PrimaryKey();
			if(primary != null) {
				return primary.Contains(column);
			}
			return false;
		}

		public bool IsSubclass() {
			Key key = this.PrimaryKey();
			return key != null && key.KeyType == KeyType.Subclass;
		}

		public string BaseName() {
			Key key = this.PrimaryKey();
			if(key != null && key.KeyType == KeyType.Subclass) {
				return key.ParentName;
			}
			return null;
		}

		public IEnumerable<Table> Ancestors(bool withSelf) {
			if(withSelf) {
				yield return this;
			}
			string baseName = this.BaseName();
			while(baseName != null) {
				Table parent = this.Store.Find(baseName);
				Debug.Assert(parent != null);
				yield return parent;
				baseName = parent.BaseName();
			}
		}

		public IEnumerable<Table> Subclasses() {
			foreach(Table table in this.Store) {
				if(table != this && table.IsSubclass() && table.Ancestors(false).First() == this) {
					yield return table;
				}
			}
		}

		public Key ForeignKey(Column column) {
			foreach(Key key in this.Keys) {
				if(key.IsForeign() && key.Contains(column)) {
					return key;
				}
			}
			return null;
		}

		public Key UniqueKey(Column column) {
			foreach(Key key in this.Keys) {
				if(key.IsUnique() && key.Count == 1 && key[0] == column) {
					return key;
				}
			}
			return null;
		}

		public Key UniqueKey(Column column0, Column column1) {
			foreach(Key key in this.Keys) {
				if(key.IsUnique() && key.Count == 2  && key[0] == column0  && key[1] == column1) {
					return key;
				}
			}
			return null;
		}

		public void Validate(Store store) {
			this.Store = store;
			if(this.Name == null) {
				throw new Error("Name is missing in table definition");
			}
			if(this.Columns.Count == 0) {
				throw new Error("Table {0} does not have any columns", this.Name);
			}
			foreach(Column column in this.Columns) {
				column.Validate(this);
			}
			foreach(Key key in this.Keys) {
				key.Validate(this);
			}
		}
	}
}
