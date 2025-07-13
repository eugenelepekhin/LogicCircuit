using System.Diagnostics;

namespace ItemWrapper.Generator {
	public class Column {
		public Table? Table { get; private set; }
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string? Default { get; set; }
		public bool IgnoreCase { get; set; }

		public string? Check { get; set; }
		public bool ReadOnly { get; set; }
		public AccessModifier AccessModifier { get; set; }
		public bool PropertyOverrides { get; set; }
		public string PropertyNamePrefix { get; set; } = string.Empty;

		public Column() {
			this.AccessModifier = AccessModifier.Public;
		}

		public string AccessModifierName() {
			switch(this.AccessModifier) {
			case AccessModifier.Public:		return "public";
			case AccessModifier.Internal:	return "internal";
			case AccessModifier.Protected:	return "protected";
			case AccessModifier.Private:	return "private";
			default:
				throw new GeneratorException("Unknown access modifier: {0}", this.AccessModifier);
			}
		}

		public bool IsFirst() {
			Debug.Assert(this.Table != null, "Table should be set before calling IsFirst()");
			return this == this.Table.Columns[0];
		}

		public bool IsLast() {
			Debug.Assert(this.Table != null, "Table should be set before calling IsLast()");
			return this == this.Table.Columns[this.Table.Columns.Count - 1];
		}

		public void Validate(Table table) {
			this.Table = table;
			if(string.IsNullOrEmpty(this.Name)) {
				throw new GeneratorException("Column name is missing");
			}
			if(string.IsNullOrEmpty(this.Type)) {
				throw new GeneratorException("Column {0} is missing its type", this.Name);
			}
		}
	}
}
