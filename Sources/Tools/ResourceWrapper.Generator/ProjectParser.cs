using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResourceWrapper.Generator {
	internal struct ProjectParser {

		public bool Generate(string projectPath, IEnumerable<string> pseudo) {
			XmlDocument project = new XmlDocument();
			project.Load(projectPath);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(project.NameTable);
			nsmgr.AddNamespace(project.DocumentElement.Prefix, project.DocumentElement.NamespaceURI);
			nsmgr.AddNamespace("prj", project.DocumentElement.NamespaceURI);

			XmlNode node = project.SelectSingleNode("/prj:Project/prj:PropertyGroup/prj:RootNamespace", nsmgr);
			if(node == null) {
				return this.BadProject(projectPath);
			}
			string rootNamespace = node.InnerText;

			List<Resource> list = new List<Resource>();
			XmlNodeList nodeList = project.SelectNodes("/prj:Project/prj:ItemGroup/prj:EmbeddedResource[prj:Generator='ResXFileCodeGenerator' or prj:Generator='PublicResXFileCodeGenerator']", nsmgr);
			if(nodeList != null && nodeList.Count > 0) {
				foreach(XmlNode resourceNode in nodeList) {
					XmlAttribute attribute = resourceNode.Attributes["Include"];
					if(attribute == null) {
						return this.BadProject(projectPath);
					}
					string value = attribute.Value.Trim();
					string root = Path.GetDirectoryName(value);
					string file = Path.GetFileNameWithoutExtension(value);
					string name = ((root.Length > 0)
						? string.Format("{0}.{1}.{2}", rootNamespace, root.Replace('\\', '.') , file)
						: string.Format("{0}.{1}", rootNamespace, file)
					);
					node = resourceNode.SelectSingleNode("prj:Generator", nsmgr);
					if(node == null) {
						return this.BadProject(projectPath);
					}
					string generator = node.InnerText;
					bool isPublic = StringComparer.OrdinalIgnoreCase.Compare(generator.Trim(), "PublicResXFileCodeGenerator") == 0;
					node = resourceNode.SelectSingleNode("prj:LastGenOutput", nsmgr);
					if(node == null) {
						return this.BadProject(projectPath);
					}
					string code = node.InnerText;
					node = resourceNode.SelectSingleNode("prj:CustomToolNamespace", nsmgr);
					string nameSpace = string.Empty;
					if(node != null) {
						nameSpace = node.InnerText;
					} else {
						if(root.Length > 0) {
							nameSpace = string.Format("{0}.{1}", rootNamespace, root.Replace('\\', '.'));
						} else {
							nameSpace = rootNamespace;
						}
					}
					list.Add(new Resource(root, file, code, name, nameSpace, isPublic));
				}
			}
			string projectRoot = Path.GetDirectoryName(projectPath);
			foreach(Resource resource in list) {
				if(!this.Generate(projectRoot, resource, pseudo)) {
					return false;
				}
			}
			return true;
		}

		private bool Generate(string projectRoot, Resource resource, IEnumerable<string> pseudo) {
			return new ResourcesWrapper() {
				File = this.Combine(projectRoot, resource.root, resource.file, "resx"),
				Code = this.Combine(projectRoot, resource.root, resource.code),
				NameSpace = resource.nameSpace,
				ClassName = resource.file,
				ResourceName = resource.name,
				IsPublic = resource.isPublic,
				Pseudo = pseudo.Contains(resource.name, StringComparer.OrdinalIgnoreCase)
			}.Generate();
		}

		private string Combine(string project, string folder, string file) {
			return Path.Combine(Path.Combine(project, folder), file);
		}
		private string Combine(string project, string folder, string file, string ext) {
			return this.Combine(project, folder, string.Format("{0}.{1}", file, ext));
		}

		private bool BadProject(string file) {
			Message.Error("Project file \"{0}\" is corrupted", file);
			return false;
		}
		private struct Resource {
			public string root;
			public string file;
			public string code;
			public string name;
			public string nameSpace;
			public bool isPublic;
			public Resource(string root, string file, string code, string name, string nameSpace, bool isPublic) {
				this.root = root;
				this.file = file;
				this.code = code;
				this.name = name;
				this.nameSpace = nameSpace;
				this.isPublic = isPublic;
			}
		}
	}
}
