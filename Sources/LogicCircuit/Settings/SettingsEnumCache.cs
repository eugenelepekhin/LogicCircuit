using System;
using System.Linq;

namespace LogicCircuit {
	internal class SettingsEnumCache<T> where T:struct {
		private Settings settings;
		private string key;
		private T defaultValue;

		private T cache;
		public T Value {
			get { return this.cache; }
			set {
				this.cache = Enum.IsDefined(typeof(T), value) ? value : this.defaultValue;
				this.settings[this.key] = this.cache.ToString();
			}
		}

		public SettingsEnumCache(
			Settings settings,
			string key,
			T defaultValue
		) {
			this.settings = settings;
			this.key = key;
			this.defaultValue = defaultValue;
			this.cache = Parse(this.settings[this.key], this.defaultValue);
		}

		public static T Parse(string text, T defaultValue) {
			T value;
			if(string.IsNullOrWhiteSpace(text) || !Enum.TryParse<T>(text, true, out value) || !IsValid(value)) {
				return defaultValue;
			}
			return value;
		}

		private static bool IsValid(T value) {
			string[] name = Enum.GetNames(typeof(T));
			string text = value.ToString();
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;
			if(!name.Contains(text, comparer)) {
				string[] part = text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				return part.All(p => name.Contains(p, comparer));
			}
			return true;
		}
	}
}
