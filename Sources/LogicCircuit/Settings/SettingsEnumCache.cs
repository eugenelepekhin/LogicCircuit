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
				this.cache = EnumHelper.IsValid(value) ? value : this.defaultValue;
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
			this.cache = EnumHelper.Parse(this.settings[this.key], this.defaultValue);
		}
	}
}
