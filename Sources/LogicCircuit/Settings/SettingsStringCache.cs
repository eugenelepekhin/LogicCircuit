using System;

namespace LogicCircuit {
	internal sealed class SettingsStringCache {
		private readonly Settings settings;
		private readonly string key;
		private readonly string[]? constraint;
		private readonly string defaultValue;

		private string cache;
		public string Value {
			get { return this.cache; }
			set {
				string text = this.Normalize(value);
				if(!StringComparer.Ordinal.Equals(this.cache, text)) {
					this.cache = text;
					this.settings[this.key] = this.cache;
				}
			}
		}

		public SettingsStringCache(
			Settings settings,
			string key,
			string? defaultValue,
			params string[] constraint
		) {
			this.settings = settings;
			this.key = key;
			this.constraint = (constraint.Length == 0) ? null : constraint;
			this.defaultValue = string.Empty;
			this.defaultValue = this.Normalize(defaultValue);
			this.cache = this.Normalize(this.settings[this.key]);
		}

		private string Normalize(string? value) {
			string? text = string.IsNullOrEmpty(value) ? null : value.Trim();
			if(this.constraint != null) {
				if(Array.IndexOf(this.constraint, text) < 0) {
					text = null;
				}
			}
			return string.IsNullOrEmpty(text) ? this.defaultValue : text;
		}
	}
}
