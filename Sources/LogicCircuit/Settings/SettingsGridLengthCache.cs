using System;
using System.Windows;

namespace LogicCircuit {
	public class SettingsGridLengthCache {
		private Settings settings;
		private string key;

		private GridLength cache;
		public GridLength Value {
			get { return this.cache; }
			set {
				if(0.0001 <= Math.Abs(this.cache.Value - value.Value)) {
					this.cache = value;
					this.settings[this.key] = this.cache.ToString();
				}
			}
		}

		public SettingsGridLengthCache(
			Settings settings,
			string key,
			string defaultValue
		) {
			this.settings = settings;
			this.key = key;
			string text = this.settings[this.key];
			if(string.IsNullOrWhiteSpace(text)) {
				text = defaultValue;
			}
			GridLengthConverter converter = new GridLengthConverter();
			GridLength original = (GridLength)converter.ConvertFromInvariantString(defaultValue);
			bool success = false;
			try {
				this.cache = (GridLength)converter.ConvertFromInvariantString(text);
				if(this.cache.GridUnitType != original.GridUnitType) {
					this.cache = original;
				}
				success = true;
			} catch(NotSupportedException) {
			} catch(FormatException) {
			}
			if(!success) {
				this.cache = original;
			}
		}
	}
}
