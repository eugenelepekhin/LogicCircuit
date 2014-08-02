using System;

namespace ItemWrapper.Generator {
	public class Column {
		public Table Table { get; private set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string Default { get; set; }
		public bool IgnoreCase { get; set; }

		public string Check { get; set; }
		public bool ReadOnly { get; set; }
		public AccessModifier AccessModifier { get; set; }
		public bool PropertyOverrides { get; set; }
		public string PropertyNamePrefix { get; set; }

		public Column() {
			this.AccessModifier = AccessModifier.Public;
			this.PropertyNamePrefix = string.Empty;
		}

		public string AccessModifierName() {
			return this.AccessModifier.ToString().ToLower();
		}

		public bool IsFirst() {
			return this == this.Table.Columns[0];
		}

		public bool IsLast() {
			return this == this.Table.Columns[this.Table.Columns.Count - 1];
		}

		public void Validate(Table table) {
			this.Table = table;
			if(string.IsNullOrEmpty(this.Name)) {
				throw new Error("Column name is missing");
			}
			if(string.IsNullOrEmpty(this.Type)) {
				throw new Error("Column {0} is missing its type", this.Name);
			}
		}
	}
}
