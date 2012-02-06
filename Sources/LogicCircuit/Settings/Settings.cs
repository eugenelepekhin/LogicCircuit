using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace LogicCircuit {
	public class Settings {
		public const string Prefix = "lcs";
		public const string NamespaceUri = "http://LogicCircuit.net/Settings/Data.xsd";
		private const string OldNamespaceUri = "http://LogicCircuit.net/SettingsData.xsd";

		private static readonly UserSettings user = new UserSettings();
		private static readonly Settings session = new Settings();

		public static UserSettings User { get { return Settings.user; } }
		public static Settings Session { get { return Settings.session; } }

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

		protected virtual void Load(XmlDocument xml, XmlNamespaceManager namespaceManager) {
			foreach(XmlElement node in xml.SelectNodes(string.Format(CultureInfo.InvariantCulture, "/{0}:settings/{0}:property", Settings.Prefix), namespaceManager)) {
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
				XmlElement element = xml.CreateElement(Settings.Prefix, "property", Settings.NamespaceUri);
				XmlAttribute name = xml.CreateAttribute("name");
				name.Value = kv.Key;
				element.Attributes.Append(name);
				element.AppendChild(xml.CreateTextNode(kv.Value));
				root.AppendChild(element);
			}
		}
	}

	public sealed class UserSettings : Settings, IDisposable, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsIntegerCache maxRecentFileCount;
		private SettingsBoolCache loadLastFileOnStartup;
		private SettingsEnumCache<GateShape> gateShape;

		private Dictionary<string, DateTime> recentFile = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		private FileSystemWatcher fileWatcher;
		public bool IsFirstRun { get; private set; }

		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public UserSettings() {
			this.fileWatcher = new FileSystemWatcher();
			this.fileWatcher.EnableRaisingEvents = false;
			string file = UserSettings.FileName();
			if(!File.Exists(file)) {
				this.IsFirstRun = true;
				this.Save();
			}
			this.Load(file);
			this.fileWatcher.Path = Path.GetDirectoryName(file);
			this.fileWatcher.Filter = Path.GetFileName(file);
			this.fileWatcher.Changed += new FileSystemEventHandler(this.fileChanged);
			this.fileWatcher.EnableRaisingEvents = true;
			this.maxRecentFileCount = new SettingsIntegerCache(this, "Settings.MaxRecentFileCount", 1, 24, 4);
			this.loadLastFileOnStartup = new SettingsBoolCache(this, "Settings.LoadLastFileOnStartup", true);
			this.gateShape = new SettingsEnumCache<GateShape>(this, "Settings.GateShape", GateShape.Rectangular);
			this.TruncateRecentFile();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void fileChanged(object sender, FileSystemEventArgs e) {
			try {
				if(e.ChangeType == WatcherChangeTypes.Changed) {
					this.Merge();
				}
				this.NotifyRecentFilesChanged();
			} catch(Exception exception) {
				Tracer.Report("UserSettings.fileChanged", exception);
				//swallow all exceptions here
			}
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void Save() {
			bool enabled = this.fileWatcher.EnableRaisingEvents;
			try {
				this.fileWatcher.EnableRaisingEvents = false;
				this.Save(UserSettings.FileName());
			} finally {
				this.fileWatcher.EnableRaisingEvents = enabled;
			}
		}

		private static string FileName() {
			Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(Settings).Assembly;
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				assembly.GetName().Name,
				"Settings.xml"
			);
		}

		protected override void Load(XmlDocument xml, XmlNamespaceManager namespaceManager) {
			base.Load(xml, namespaceManager);
			foreach(XmlElement node in xml.SelectNodes(string.Format(CultureInfo.InvariantCulture, "/{0}:settings/{0}:file", Settings.Prefix), namespaceManager)) {
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
			set {
				this.maxRecentFileCount.Value = value;
				this.TruncateRecentFile();
				this.NotifyRecentFilesChanged();
			}
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
			this.NotifyRecentFilesChanged();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public void DeleteRecentFile(string file) {
			this.recentFile.Remove(file);
			this.NotifyRecentFilesChanged();
		}

		/// <summary>
		/// Gets list of recent files ordered by descendent date
		/// </summary>
		public IEnumerable<string> RecentFiles { get { return this.AllRecentFiles().Select(p => p.Key); } }

		/// <summary>
		/// Get number of recently opened files currently known.
		/// </summary>
		public int RecentFilesCount { get { return this.recentFile.Count; } }

		private void NotifyRecentFilesChanged() {
			this.NotifyPropertyChanged("RecentFilesCount");
			this.NotifyPropertyChanged("RecentFiles");
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
			using(UserSettings other = new UserSettings()) {
				foreach(KeyValuePair<string, DateTime> kv in other.recentFile) {
					DateTime dateTime;
					if(!this.recentFile.TryGetValue(kv.Key, out dateTime)) {
						dateTime = kv.Value;
					}
					this.recentFile[kv.Key] = (dateTime < kv.Value) ? kv.Value : dateTime;
				}
				this.TruncateRecentFile();
			}
		}

		private void TruncateRecentFile() {
			if(this.MaxRecentFileCount < this.recentFile.Count) {
				List<KeyValuePair<string, DateTime>> list = this.AllRecentFiles();
				for(int i = this.MaxRecentFileCount; i < list.Count; i++) {
					this.recentFile.Remove(list[i].Key);
				}
			}
		}

		public void Dispose() {
			if(this.fileWatcher != null) {
				this.fileWatcher.Dispose();
				this.fileWatcher = null;
			}
		}
	}
}
