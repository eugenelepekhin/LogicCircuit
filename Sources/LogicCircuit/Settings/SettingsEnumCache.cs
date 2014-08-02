using System;

namespace LogicCircuit {
	internal class SettingsEnumCache<T> where T:struct {
		private Settings settings;
		private string key;
		private T defaultValue;

		private T cache;
		public T Value {
			get { return this.cache; }
			set {
				T t = EnumHelper.IsValid(value) ? value : this.defaultValue;
				if(!this.cache.Equals(t)) {
					this.cache = t;
					this.settings[this.key] = this.cache.ToString();
				}
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
