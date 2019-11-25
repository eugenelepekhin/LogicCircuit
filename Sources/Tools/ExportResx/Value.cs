using System;

namespace ExportResx {
	internal class Value {
		public string Name { get; }
		public string Text { get; }
		public string Note { get; }

		public Value(string name, string text, string note) {
			this.Name = name;
			this.Text = text;
			this.Note = note;
		}
	}
}
