using System;
using System.Collections.Generic;
using System.Text;
using io = System.IO;

namespace ResourceWrapper.Generator {
	partial class ResourcesWrapper {
		public string File { get; set; }
		public string Code { get; set; }
		public string NameSpace { get; set; }
		public string ClassName { get; set; }
		public string ResourceName { get; set; }
		public bool IsPublic { get; set; }
		public bool Pseudo { get; set; }

		public bool EnforceParameterDeclaration { get; set; }
		public bool FlowDirection { get; set; }
		public IEnumerable<string> Satelites { get; set; }

		public IEnumerable<ResourceItem> Items { get; private set; }

		public bool Generate() {
			IEnumerable<ResourceItem> list = ResourceParser.Parse(this.File, this.EnforceParameterDeclaration, this.Satelites);
			if(list != null) {
				this.Items = list;
				string content = this.TransformText();
				string oldFileContent = null;
				if(io.File.Exists(this.Code)) {
					oldFileContent = io.File.ReadAllText(this.Code, Encoding.UTF8);
				}
				if(!StringComparer.Ordinal.Equals(oldFileContent, content)) {
					io.File.WriteAllText(this.Code, content, Encoding.UTF8);
					Message.Flush();
				}
				Message.Clear();
				return true;
			}
			return false;
		}
	}
}
