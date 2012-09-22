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
using System.Xml.XPath;

namespace LogicCircuit {
	public class Settings {
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

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected void Load(string file) {
			if(File.Exists(file)) {
				XmlReader xmlReader = XmlHelper.CreateReader(new StreamReader(file));

				try {
					// skip to the first element
					while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read());

					if(StringComparer.OrdinalIgnoreCase.Compare(xmlReader.NamespaceURI, Settings.OldNamespaceUri) == 0) {
						XmlHelper.Transform(Schema.ConvertSettings, ref xmlReader);
					}

					this.Load(new XPathDocument(xmlReader).CreateNavigator());
				} finally {
					// Don't use using here. Transform may close original XmlReader and open new one.
					xmlReader.Close();
				}
			}
		}

		protected virtual void Load(XPathNavigator navigator) {
			XmlNamespaceManager nsManager = new XmlNamespaceManager(navigator.NameTable);
			nsManager.AddNamespace("p", Settings.NamespaceUri);
			XPathExpression exp = XPathExpression.Compile("/p:settings/p:property", nsManager);

			foreach(XPathNavigator node in navigator.Select(exp)) {
				string key = node.GetAttribute("name", string.Empty);
				if(!string.IsNullOrEmpty(key)) {
					this[key] = node.Value.Trim();
				}
			}
		}

		protected void Save(string file) {
			using (XmlWriter writer = XmlHelper.CreateWriter(XmlHelper.FileWriter(file))) {
				writer.WriteStartDocument();
				writer.WriteStartElement("lcs", "settings", Settings.NamespaceUri);
				this.Save(writer);
				writer.WriteEndElement();
			}
		}

		protected virtual void Save(XmlWriter writer) {
			foreach(KeyValuePair<string, string> kv in this.property.OrderBy(kv => kv.Key)) {
				writer.WriteStartElement("property", Settings.NamespaceUri);
				writer.WriteAttributeString("name", kv.Key);
				writer.WriteValue(kv.Value);
				writer.WriteEndElement();
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

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public UserSettings() {
			this.fileWatcher = new FileSystemWatcher();
			this.fileWatcher.EnableRaisingEvents = false;
			string file = UserSettings.FileName();
			if(!File.Exists(file)) {
				this.IsFirstRun = true;
				try {
					this.Save();
				} catch(Exception exception) {
					Tracer.Report("UserSettings.ctor", exception);
				}
			}
			this.Load(file);
			this.fileWatcher.Path = Path.GetDirectoryName(file);
			this.fileWatcher.Filter = Path.GetFileName(file);
			this.fileWatcher.Changed += new FileSystemEventHandler(this.fileChanged);
			this.fileWatcher.EnableRaisingEvents = true;
			this.maxRecentFileCount = new SettingsIntegerCache(this, "Settings.MaxRecentFileCount", 1, 24, 4);
			this.loadLastFileOnStartup = new SettingsBoolCache(this, "Settings.LoadLastFileOnStartup", true);
			this.gateShape = new SettingsEnumCache<GateShape>(this, "Settings.GateShape",
				SettingsEnumCache<GateShape>.Parse(Properties.Resources.DefaultGateShape, GateShape.Rectangular)
			);
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

		protected override void Load(XPathNavigator navigator) {
			base.Load(navigator);
			XmlNamespaceManager nsManager = new XmlNamespaceManager(navigator.NameTable);
			nsManager.AddNamespace("p", Settings.NamespaceUri);
			XPathExpression exp = XPathExpression.Compile("/p:settings/p:file", nsManager);

			foreach(XPathNavigator node in navigator.Select(exp)) {
				string file = node.GetAttribute("name", "");
				if(!string.IsNullOrEmpty(file)) {
					string text = node.GetAttribute("date", "");
					DateTime date;
					if(!string.IsNullOrEmpty(text) && DateTime.TryParse(text, out date)) {
						this.recentFile[file] = date;
					}
				}
			}
			this.IsFirstRun = false;
		}

		protected override void Save(XmlWriter writer) {
			base.Save(writer);
			foreach(KeyValuePair<string, DateTime> kv in this.recentFile.OrderByDescending(kv => kv.Value)) {
				writer.WriteStartElement("file", Settings.NamespaceUri);
				writer.WriteAttributeString("name", kv.Key);
				writer.WriteAttributeString("date", kv.Value.ToString("s", DateTimeFormatInfo.InvariantInfo));
				writer.WriteEndElement();
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
