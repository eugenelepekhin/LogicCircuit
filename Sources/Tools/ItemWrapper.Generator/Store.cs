namespace ItemWrapper.Generator {
	public class Store : List<Table> {
		public string Namespace { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string PersistenceNamespace { get; set; } = string.Empty;
		public string PersistencePrefix { get; set; } = string.Empty;

		public Store() {
		}

		public void Validate() {
			foreach(Table table in this) {
				table.Validate(this);
			}
		}

		public Table? Find(string tableName) {
			return this.FirstOrDefault(t => t.Name == tableName);
		}
	}
}
