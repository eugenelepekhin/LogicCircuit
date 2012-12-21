using System;
using System.Globalization;

namespace LogicCircuit {
	internal class SettingsDateTimeCache {
		private Settings settings;
		private string key;

		private DateTime cache;
		public DateTime Value {
			get { return this.cache; }
			set {
				if(this.cache != value) {
					this.cache = value;
					this.settings[this.key] = this.cache.ToString("s", CultureInfo.InvariantCulture);
				}
			}
		}

		public SettingsDateTimeCache(
			Settings settings,
			string key,
			DateTime defaultValue
		) {
			this.settings = settings;
			this.key = key;
			string text = this.settings[this.key];
			DateTime value;
			if( string.IsNullOrEmpty(text) ||
				!DateTime.TryParseExact(text, "s", CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal | DateTimeStyles.NoCurrentDateDefault,
					out value
				)
			) {
				value = defaultValue;
			}
			this.cache = value;
		}
	}
}
