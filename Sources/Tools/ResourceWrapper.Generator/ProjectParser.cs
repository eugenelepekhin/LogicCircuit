using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ResourceWrapper.Generator {
	internal struct ProjectParser {

		//ProjectPath=C:\Projects\TestApp\TestApp\TestApp.csproj ProjectDir=C:\Projects\TestApp\TestApp\

		private const string Prefix = "prj";
		private const string RootNamespaceXPath = "/prj:Project/prj:PropertyGroup/prj:RootNamespace";
		private const string ResourceXPath = "/prj:Project/prj:ItemGroup/prj:EmbeddedResource[prj:Generator='ResXFileCodeGenerator' or prj:Generator='PublicResXFileCodeGenerator']";
		private const string ResourceFileXPath = "Include"; //attribute names come without prefix
		private const string GeneratorXPath = "prj:Generator";
		private const string WrapperFileXPath = "prj:LastGenOutput";
		private const string NamespaceXPath = "prj:CustomToolNamespace";

		public bool Generate(string projectPath, string projectRoot) {
			XmlDocument project = new XmlDocument();
			project.Load(projectPath);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(project.NameTable);
			nsmgr.AddNamespace(project.DocumentElement.Prefix, project.DocumentElement.NamespaceURI);
			nsmgr.AddNamespace(ProjectParser.Prefix, project.DocumentElement.NamespaceURI);

			XmlNode node = project.SelectSingleNode(ProjectParser.RootNamespaceXPath, nsmgr);
			if(node == null) {
				return this.BadProject(projectPath);
			}
			string rootNamespace = node.InnerText;

			List<Resource> list = new List<Resource>();
			XmlNodeList nodeList = project.SelectNodes(ProjectParser.ResourceXPath, nsmgr);
			if(nodeList != null && nodeList.Count > 0) {
				foreach(XmlNode resourceNode in nodeList) {
					XmlAttribute attribute = resourceNode.Attributes[ProjectParser.ResourceFileXPath];
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
					node = resourceNode.SelectSingleNode(ProjectParser.GeneratorXPath, nsmgr);
					if(node == null) {
						return this.BadProject(projectPath);
					}
					string generator = node.InnerText;
					bool isPublic = StringComparer.OrdinalIgnoreCase.Compare(generator.Trim(), "PublicResXFileCodeGenerator") == 0;
					node = resourceNode.SelectSingleNode(ProjectParser.WrapperFileXPath, nsmgr);
					if(node == null) {
						return this.BadProject(projectPath);
					}
					string code = node.InnerText;
					node = resourceNode.SelectSingleNode(ProjectParser.NamespaceXPath, nsmgr);
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
			foreach(Resource resource in list) {
				if(!this.Generate(projectRoot, resource)) {
					return false;
				}
			}
			return true;
		}

		private bool Generate(string projectRoot, Resource resource) {
			string file = this.Combine(projectRoot, resource.root, resource.file, "resx");
			string code = this.Combine(projectRoot, resource.root, resource.code);
			return new ResourceParser().Generate(file, code, resource.nameSpace, resource.file, resource.name, resource.isPublic);
		}

		private string Combine(string project, string folder, string file) {
			return Path.Combine(Path.Combine(project, folder), file);
		}
		private string Combine(string project, string folder, string file, string ext) {
			return this.Combine(project, folder, string.Format("{0}.{1}", file, ext));
		}

		private bool BadProject(string file) {
			Message.Error("BadProject", file);
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
