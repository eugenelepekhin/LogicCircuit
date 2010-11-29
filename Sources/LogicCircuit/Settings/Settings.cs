using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LogicCircuit {
	public class Settings {
		public const string Prefix = "lcs";
		public const string NamespaceUri = "http://LogicCircuit.net/Settings/Data.xsd";
		private const string OldNamespaceUri = "http://LogicCircuit.net/SettingsData.xsd";

		public static UserSettings User = new UserSettings();
		public static Settings Session = new Settings();

		private Dictionary<string, string> property = new Dictionary<string, string>();

		public string this[string propertyName] {
			get {
				string value;
				if(this.property.TryGetValue(propertyName, out value) && value != null) {
					return value;
				}
				return null;
			}
			set {
				if(value == null) {
					this.property.Remove(propertyName);
				} else {
					this.property[propertyName] = value.Trim();
				}
			}
		}

		protected void Load(string file) {
			if(File.Exists(file)) {
				XmlDocument xml = new XmlDocument();
				xml.Load(file);
				if(StringComparer.OrdinalIgnoreCase.Compare(xml.DocumentElement.NamespaceURI, Settings.OldNamespaceUri) == 0) {
					xml = XmlHelper.Transform(xml, Schema.ConvertSettings);
				}
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
				nsmgr.AddNamespace(Settings.Prefix, Settings.NamespaceUri);
				this.Load(xml, nsmgr);
			}
		}

		protected virtual void Load(XmlDocument xml, XmlNamespaceManager nsmgr) {
			foreach(XmlElement node in xml.SelectNodes(string.Format(CultureInfo.InvariantCulture, "/{0}:settings/{0}:property", Settings.Prefix), nsmgr)) {
				string key = node.GetAttribute("name");
				if(!string.IsNullOrEmpty(key)) {
					this[key] = node.InnerText.Trim();
				}
			}
		}

		protected void Save(string file) {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(string.Format(CultureInfo.InvariantCulture, "<{0}:settings xmlns:{0}=\"{1}\"/>", Settings.Prefix, Settings.NamespaceUri));
			this.Save(xml);
			XmlHelper.Save(xml, file);
		}

		protected virtual void Save(XmlDocument xml) {
			XmlElement root = xml.DocumentElement;
			foreach(KeyValuePair<string, string> kv in this.property.OrderBy(kv => kv.Key)) {
				XmlElement property = xml.CreateElement(Settings.Prefix, "property", Settings.NamespaceUri);
				XmlAttribute name = xml.CreateAttribute("name");
				name.Value = kv.Key;
				property.Attributes.Append(name);
				property.AppendChild(xml.CreateTextNode(kv.Value));
				root.AppendChild(property);
			}
		}
	}

	public class UserSettings : Settings {
		private SettingsIntegerCache maxRecentFileCount;
		private SettingsBoolCache loadLastFileOnStartup;
		private SettingsEnumCache<GateShape> gateShape;

		private Dictionary<string, DateTime> recentFile = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		private FileSystemWatcher fileWatcher;
		public bool IsFirstRun { get; private set; }

		public UserSettings() {
			this.fileWatcher = new FileSystemWatcher();
			this.fileWatcher.EnableRaisingEvents = false;
			string file = this.FileName();
			if(!File.Exists(file)) {
				this.IsFirstRun = true;
				this.Save();
			}
			this.Load(file);
			this.fileWatcher.Path = Path.GetDirectoryName(file);
			this.fileWatcher.Filter = Path.GetFileName(file);
			this.fileWatcher.Changed += new FileSystemEventHandler(this.fileChanged);
			this.fileWatcher.EnableRaisingEvents = true;
			this.maxRecentFileCount = new SettingsIntegerCache(this, "Settings.MaxRecentFileCount", 1, 32, 4);
			this.loadLastFileOnStartup = new SettingsBoolCache(this, "Settings.LoadLastFileOnStartup", true);
			this.gateShape = new SettingsEnumCache<GateShape>(this, "Settings.GateShape", GateShape.Rectangular);
			this.TruncateRecentFile();
		}

		private void fileChanged(object sender, FileSystemEventArgs e) {
			if(e.ChangeType == WatcherChangeTypes.Changed) {
				this.Merge();
			}
		}

		public void Save() {
			bool enabled = this.fileWatcher.EnableRaisingEvents;
			try {
				this.fileWatcher.EnableRaisingEvents = false;
				this.Save(this.FileName());
			} finally {
				this.fileWatcher.EnableRaisingEvents = enabled;
			}
		}

		private string FileName() {
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				@"LogicCircuit\Settings2.xml"
			);
		}

		protected override void Load(XmlDocument xml, XmlNamespaceManager nsmgr) {
			base.Load(xml, nsmgr);
			foreach(XmlElement node in xml.SelectNodes(string.Format(CultureInfo.InvariantCulture, "/{0}:settings/{0}:file", Settings.Prefix), nsmgr)) {
				string file = node.GetAttribute("name");
				if(!string.IsNullOrEmpty(file)) {
					string text = node.GetAttribute("date");
					DateTime date;
					if(!string.IsNullOrEmpty(text) && DateTime.TryParse(text, out date)) {
						this.recentFile[file] = date;
					}
				}
			}
			this.IsFirstRun = false;
		}

		protected override void Save(XmlDocument xml) {
			base.Save(xml);
			XmlElement root = xml.DocumentElement;
			foreach(KeyValuePair<string, DateTime> kv in this.recentFile.OrderByDescending(kv => kv.Value)) {
				XmlElement file = xml.CreateElement(Settings.Prefix, "file", Settings.NamespaceUri);
				XmlAttribute name = xml.CreateAttribute("name");
				name.Value = kv.Key;
				file.Attributes.Append(name);
				XmlAttribute date = xml.CreateAttribute("date");
				date.Value = kv.Value.ToString("s", DateTimeFormatInfo.InvariantInfo);
				file.Attributes.Append(date);
				root.AppendChild(file);
			}
		}

		public int MaxRecentFileCount {
			get { return this.maxRecentFileCount.Value; }
			set { this.maxRecentFileCount.Value = value; }
		}

		public bool LoadLastFileOnStartup {
			get { return this.loadLastFileOnStartup.Value; }
			set { this.loadLastFileOnStartup.Value = value; }
		}

		public GateShape GateShape {
			get { return this.gateShape.Value; }
			set { this.gateShape.Value = value; }
		}

		/// <summary>
		/// Adds the file name to the list of recently opened files.
		/// </summary>
		/// <param name="file">File name to be added</param>
		public void AddRecentFile(string file) {
			this.recentFile[file.Trim()] = DateTime.UtcNow;
			this.TruncateRecentFile();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public void DeleteRecentFile(string file) {
			this.recentFile.Remove(file);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private List<KeyValuePair<string, DateTime>> AllRecentFiles() {
			List<KeyValuePair<string, DateTime>> list = new List<KeyValuePair<string, DateTime>>(this.recentFile);
			list.Sort((x, y) => y.Value.CompareTo(x.Value));
			return list;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string RecentFile() {
			List<KeyValuePair<string, DateTime>> list = this.AllRecentFiles();
			if(0 < list.Count) {
				return list[0].Key;
			}
			return null;
		}

		private void Merge() {
			UserSettings other = new UserSettings();
			this.recentFile.Union(other.recentFile);
			this.TruncateRecentFile();
		}

		private void TruncateRecentFile() {
			if(this.MaxRecentFileCount < this.recentFile.Count) {
				List<KeyValuePair<string, DateTime>> list = this.AllRecentFiles();
				for(int i = this.MaxRecentFileCount; i < list.Count; i++) {
					this.recentFile.Remove(list[i].Key);
				}
			}
		}
	}
}
