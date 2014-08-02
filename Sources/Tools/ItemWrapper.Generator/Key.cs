using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ItemWrapper.Generator {

	public enum KeyType {
		Auto,
		Primary,
		Subclass,
		Unique,
		Index,
		Foreign
	}

	public enum Action {
		Restrict,
		Cascade,
		SetDefault
	}

	public class Key : List<Column> {
		public Table Table { get; private set; }
		public string Name { get; set; }
		public KeyType KeyType { get; set; }
		public string ParentName { get; set; }
		public Action Action { get; set; }
		public bool AllowsDefault { get; set; }
		public string PropertyType { get; set; }

		public Key() {
			this.KeyType = KeyType.Index;
			this.Action = Action.Restrict;
		}

		public bool IsPrimary() {
			return this.KeyType == KeyType.Auto || this.KeyType == KeyType.Primary || this.KeyType == KeyType.Subclass;
		}

		public bool IsForeign() {
			return this.KeyType == KeyType.Foreign || this.KeyType == KeyType.Subclass;
		}

		public bool IsUnique() {
			return this.IsPrimary() || this.KeyType == KeyType.Unique;
		}

		public bool IsIndex() {
			return this.KeyType == KeyType.Index;
		}

		public string RoleName() {
			Debug.Assert(this.IsForeign(), "Foreign key expected");
			string name = this[0].Name;
			if(name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) {
				return name.Substring(0, name.Length - 2);
			}
			return name;
		}

		public Table Parent() {
			if(this.IsForeign()) {
				return this.Table.Store.Find(this.ParentName);
			}
			return null;
		}

		public void Validate(Table table) {
			this.Table = table;
			List<Column> list = new List<Column>(this.Count);
			foreach(Column column in this) {
				Column actual = this.Table.Columns.FirstOrDefault(c => c.Name == column.Name);
				if(actual == null) {
					throw new Error("Key {0}.{1} referring unknown column {2}", this.Table.Name, this.Name, column.Name);
				}
				list.Add(actual);
			}
			this.Clear();
			this.InsertRange(0, list);
			if(this.KeyType == KeyType.Auto) {
				if(0 < this.Count) {
					throw new Error("Key {0} defined as Auto and cannot contain any columns", this.Name);
				}
			} else if(this.Count < 1 || 2 < this.Count) {
				throw new Error("Key {0}.{1} should have 1 or 2 columns", this.Table.Name, this.Name);
			}
			if(this.IsForeign() && this.Count != 1) {
				throw new Error("Foreign key {0}.{1} should have only one column", this.Table.Name, this.Name);
			}
			if(this.IsPrimary() && this.Table.Keys.Any(k => k != this && k.IsPrimary())) {
				throw new Error("Table {0} defines more then one primary key", this.Table.Name);
			}
			if(this.IsForeign()) {
				if(string.IsNullOrEmpty(this.ParentName)) {
					throw new Error("Foreign key {0}.{1} expecting ParentName property", this.Table.Name, this.Name);
				}
				Table parent = this.Table.Store.Find(this.ParentName);
				if(parent == null) {
					throw new Error("Foreign key {0}.{1} referencing unknown table {2}", this.Table.Name, this.Name, this.ParentName);
				}
				Key pk = parent.PrimaryKey();
				if(pk == null || (pk.KeyType != KeyType.Auto && pk.Count != 1)) {
					throw new Error("Foreign key {0}.{1} referencing incompatible primary key", this.Table.Name, this.Name);
				}
			}
		}
	}
}
