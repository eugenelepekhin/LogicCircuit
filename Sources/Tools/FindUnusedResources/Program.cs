using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;

namespace FindUnusedResources {
    /// <summary>
	/// TODO: write it down
    /// This utility is used for finding unused resource strings in the project.
    /// Currently you need to modify hard coded strings to provide path to the project
    /// Currently algorithm is fairly primitive so it will miss cases like: name versus names.
    /// </summary>
	internal static class Program {
		public static void Main(string[] args) {
            string projectPath = null;
			if(args != null && 0 < args.Length && args[0] != null) {
				projectPath = args[0];
			} else {
				//from exe location (Sources\Tools\FindUnusedResources\bin\Debug) get up to solution folder and to main project file (Sources\LogicCircuit\LogicCircuit.csproj)
				projectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\..\LogicCircuit\LogicCircuit.csproj"));
			}
			Console.WriteLine("Checking project \"{0}\"", projectPath);

			List<string> resourceFiles = new List<string>();
			List<string> sourceFiles = new List<string>();

			if(LoadProject(projectPath, resourceFiles, sourceFiles)) {
				HashSet<string> unused = new HashSet<string>();
				LoadResources(unused, resourceFiles);
				foreach(string file in sourceFiles) {
					string text = File.ReadAllText(file);
					HashSet<string> used = new HashSet<string>(unused.Where(r => text.Contains(r)));
					unused.RemoveWhere(r => used.Contains(r));
					if(unused.Count == 0) break;
				}
				// This is list of resources that accessed in a custom way. Beware it can mask a real problem
				List<string> exclude = new List<string>() {
					"Resources.CommandCircuitDelete",
					"Resources.CommandCircuitNew",
					"Resources.CommandCircuitPower",
					"Resources.CommandCircuitUsage",
					"Resources.CommandEditRotateLeft",
					"Resources.CommandEditRotateRight",
					"Resources.CommandEditSelectAllButWires",
					"Resources.CommandEditSelectAllProbes",
					"Resources.CommandEditSelectAllProbesWithWire",
					"Resources.CommandEditSelectAllWires",
					"Resources.CommandEditSelectFloatingSymbols",
					"Resources.CommandEditSelectFreeWires",
					"Resources.CommandEditUnselectAllButWires",
					"Resources.CommandEditUnselectAllWires",
					"Resources.CommandFileExportImage",
					"Resources.CommandFileFileImport",
					"Resources.CommandHelpAbout",
					"Resources.CommandToolsOptions",
					"Resources.CommandToolsOscilloscope",
					"Resources.ErrorNotUniqueIndex",
					"Resources.Led7Pin1",
					"Resources.Led7Pin2",
					"Resources.Led7Pin3",
					"Resources.Led7Pin4",
					"Resources.Led7Pin5",
					"Resources.Led7Pin6",
					"Resources.Led7Pin7",
					"Resources.Led7Pin8",
				};
				exclude.ForEach(s => unused.Remove(s));

				if(0 < unused.Count) {
					Console.WriteLine("Unused resources:");
					List<string> list = unused.ToList();
					list.Sort();
					list.ForEach(r => Console.WriteLine(r));
				}
			}
		}

		private static bool LoadProject(string projectFile, List<string> resourceList, List<string> sourceList) {
			string root = Path.GetDirectoryName(projectFile);

			XmlDocument project = new XmlDocument();
			project.Load(projectFile);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(project.NameTable);
			nsmgr.AddNamespace(project.DocumentElement.Prefix, project.DocumentElement.NamespaceURI);
			nsmgr.AddNamespace("p", project.DocumentElement.NamespaceURI);

			return Include(root, resourceList, project.SelectNodes("/p:Project/p:ItemGroup/p:EmbeddedResource[p:Generator='ResXFileCodeGenerator' or p:Generator='PublicResXFileCodeGenerator']", nsmgr))
				&& Include(root, sourceList, project.SelectNodes("/p:Project/p:ItemGroup/p:Compile", nsmgr))
				&& Include(root, sourceList, project.SelectNodes("/p:Project/p:ItemGroup/p:Page", nsmgr))
			;
		}

		private static bool Include(string root, List<string> list, XmlNodeList nodeList) {
			if(nodeList != null && 0 < nodeList.Count) {
				foreach(XmlNode node in nodeList) {
					XmlAttribute attribute = node.Attributes["Include"];
					if(attribute == null) {
						Console.WriteLine("Bad project");
						return false;
					}
					string value = attribute.Value.Trim();
					list.Add(Path.Combine(root, value));
				}
			}
			return true;
		}

		private static void LoadResources(HashSet<string> resources, List<string> files) {
			foreach(string resx in files) {
				if(!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(resx), ".resx")) {
					Console.WriteLine("Bad resource file: {0}", resx);

				}
				string className = Path.GetFileNameWithoutExtension(resx);
				XmlDocument xml = new XmlDocument();
				xml.Load(resx);
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
				nsmgr.AddNamespace(xml.DocumentElement.Prefix, xml.DocumentElement.NamespaceURI);
				nsmgr.AddNamespace("r", xml.DocumentElement.NamespaceURI);

				XmlNodeList list = xml.SelectNodes("/r:root/r:data", nsmgr);
				if(list != null && 0 < list.Count) {
					foreach(XmlNode node in list) {
						resources.Add(className + "." + node.Attributes["name"].Value);
					}
				}
			}
		}
	}
}
