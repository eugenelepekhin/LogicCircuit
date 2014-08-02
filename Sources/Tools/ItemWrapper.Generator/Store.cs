using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemWrapper.Generator {
	public class Store : List<Table> {
		public string Namespace { get; set; }
		public string Name { get; set; }
		public string PersistenceNamespace { get; set; }
		public string PersistencePrefix { get; set; }

		public Store() {
		}

		public void Validate() {
			foreach(Table table in this) {
				table.Validate(this);
			}
		}

		public Table Find(string tableName) {
			return this.FirstOrDefault(t => t.Name == tableName);
		}
	}
}
