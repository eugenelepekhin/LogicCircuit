using System;
using System.Globalization;

namespace LogicCircuit {
	public class SettingsIntegerCache {
		private Settings settings;
		private string key;
		private int minimum;
		private int maximum;

		private int cache;
		public int Value {
			get { return this.cache; }
			set {
				this.cache = Math.Max(this.minimum, Math.Min(value, this.maximum));
				this.settings[this.key] = this.cache.ToString(CultureInfo.InvariantCulture);
			}
		}

		public SettingsIntegerCache(
			Settings settings,
			string key,
			int minimum,
			int maximum,
			int defaultValue
		) {
			this.settings = settings;
			this.key = key;
			this.minimum = minimum;
			this.maximum = maximum;
			string text = this.settings[this.key];
			int value;
			if(string.IsNullOrEmpty(text) || !int.TryParse(text, out value)) {
				value = defaultValue;
			}
			this.cache = Math.Max(this.minimum, Math.Min(value, this.maximum));
		}
	}
}
