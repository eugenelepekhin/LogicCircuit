using System;
using System.Globalization;

namespace LogicCircuit {
	internal class SettingsBoolCache {
		private Settings settings;
		private string key;

		private bool cache;
		public bool Value {
			get { return this.cache; }
			set {
				if(this.cache != value) {
					this.cache = value;
					this.settings[this.key] = this.cache.ToString(CultureInfo.InvariantCulture);
				}
			}
		}

		public SettingsBoolCache(
			Settings settings,
			string key,
			bool defaultValue
		) {
			this.settings = settings;
			this.key = key;
			string text = this.settings[this.key];
			if(string.IsNullOrEmpty(text) || !bool.TryParse(text, out this.cache)) {
				this.cache = defaultValue;
			}
		}
	}
}
