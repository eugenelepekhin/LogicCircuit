using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml;

namespace ExportResx {
	internal class Exporter {
		private const string Extension = ".resx";
		private readonly string resxFolder;
		private readonly string fileName;
		private readonly List<string> cultures;

		public Exporter(string resxFolder, string fileName, List<string> cultures) {
			this.resxFolder = resxFolder;
			this.fileName = fileName;
			this.cultures = cultures;
		}

		public bool Export(string outputFile) {
			List<string> files = new List<string>();
			files.Add(Path.Combine(this.resxFolder, this.fileName + Exporter.Extension));
			foreach(string code in this.cultures) {
				files.Add(Path.Combine(this.resxFolder, string.Format("{0}.{1}{2}", this.fileName, code, Exporter.Extension)));
			}
			List<Dictionary<string, Value>> list = new List<Dictionary<string, Value>>();
			foreach(string file in files) {
				if(!File.Exists(file)) {
					Program.Log("File {0} not found", file);
					return false;
				}
				Dictionary<string, Value> content = this.Read(file);
				if(content == null) {
					return false;
				}
				list.Add(content);
			}
			XmlDocument xml = new XmlDocument();
			XmlElement root = xml.CreateElement(this.fileName);
			xml.AppendChild(root);
			Dictionary<string, Value> main = list[0];
			foreach(KeyValuePair<string, Value> pair in main) {
				XmlElement res = xml.CreateElement("res");
				root.AppendChild(res);
				XmlElement name = xml.CreateElement("name");
				res.AppendChild(name);
				name.InnerText = pair.Key;
				XmlElement text = xml.CreateElement("text");
				res.AppendChild(text);
				text.InnerText = pair.Value.Text;
				for(int i = 1; i < list.Count; i++) {
					XmlElement lang = xml.CreateElement(this.cultures[i - 1]);
					res.AppendChild(lang);
					if(list[i].TryGetValue(pair.Key, out Value value)) {
						lang.InnerText = value.Text;
					}
				}
				XmlElement note = xml.CreateElement("comment");
				res.AppendChild(note);
				note.InnerText = pair.Value.Note ?? string.Empty;
			}
			xml.Save(outputFile);
			return true;
		}

		private Dictionary<string, Value> Read(string file) {
			Dictionary<string, Value> list = new Dictionary<string, Value>();
			using(ResXResourceReader reader = new ResXResourceReader(file)) {
				reader.UseResXDataNodes = true;
				foreach(DictionaryEntry item in reader) {
					ResXDataNode node = (ResXDataNode)item.Value;
					string name = node.Name;
					object obj = node.GetValue((ITypeResolutionService)null);
					if(obj is string text) {
						Value value = new Value(name, text, node.Comment);
						list.Add(value.Name, value);
					} else {
						Program.Log("File {0} contains unsupported resource {1}", file, name);
						return null;
                    }
				}
			}
			return list;
		}
	}
}
