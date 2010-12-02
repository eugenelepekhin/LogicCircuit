using System;

namespace LogicCircuit {
	//internal class SettingsStringCache {
	//    private Settings settings;
	//    private string key;
	//    private string[] constraint;
	//    private string defaultValue;

	//    private string cache;
	//    public string Value {
	//        get { return this.cache; }
	//        set {
	//            this.cache = this.Normalize(value);
	//            this.settings[this.key] = this.cache;
	//        }
	//    }

	//    public SettingsStringCache(
	//        Settings settings,
	//        string key,
	//        string defaultValue,
	//        params string[] constraint
	//    ) {
	//        this.settings = settings;
	//        this.key = key;
	//        this.constraint = (constraint == null || constraint.Length == 0) ? null : constraint;
	//        this.defaultValue = string.Empty;
	//        this.defaultValue = this.Normalize(defaultValue);
	//        this.cache = this.Normalize(this.settings[this.key]);
	//    }

	//    private string Normalize(string value) {
	//        string text = string.IsNullOrEmpty(value) ? null : value.Trim();
	//        if(this.constraint != null) {
	//            if(Array.IndexOf(this.constraint, text) < 0) {
	//                text = null;
	//            }
	//        }
	//        return string.IsNullOrEmpty(text) ? this.defaultValue : text;
	//    }
	//}
}
