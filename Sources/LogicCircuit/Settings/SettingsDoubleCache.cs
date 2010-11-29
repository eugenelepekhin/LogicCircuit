using System;
using System.Globalization;

namespace LogicCircuit {
	internal class SettingsDoubleCache {
		private Settings settings;
		private string key;
		private double minimum;
		private double maximum;

		private double cache;
		public double Value {
			get { return this.cache; }
			set {
				this.cache = Math.Max(this.minimum, Math.Min(value, this.maximum));
				this.settings[this.key] = this.cache.ToString(CultureInfo.InvariantCulture);
			}
		}

		public SettingsDoubleCache(
			Settings settings,
			string key,
			double minimum,
			double maximum,
			double defaultValue
		) {
			this.settings = settings;
			this.key = key;
			this.minimum = minimum;
			this.maximum = maximum;
			string text = this.settings[this.key];
			if(string.IsNullOrEmpty(text) || !double.TryParse(text, out this.cache)) {
				this.cache = Math.Max(this.minimum, Math.Min(defaultValue, this.maximum));;
			}
		}
	}
}
