using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace FindUnusedResources {
	/// <summary>
	/// TODO: write it down
	/// This utility is used for finding unused resource strings in the project.
	/// Currently you need to modify hard coded strings to provide path to the project
	/// Currently algorithm is fairly primitive so it will miss cases like: name versus names.
	/// </summary>
	[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
	internal static class Program {
		public static void Main(string[] args) {
			string? projectPath = null;
			if(args != null && 0 < args.Length && args[0] != null) {
				projectPath = args[0];
			} else {
				//from exe location (Sources\Tools\FindUnusedResources\bin\Debug) get up to solution folder and to main project file (Sources\LogicCircuit\LogicCircuit.csproj)
				projectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"..\..\..\..\..\LogicCircuit\LogicCircuit.csproj"));
			}
			Console.WriteLine("Checking project \"{0}\"", projectPath);

			List<string> resourceFiles = new List<string>();
			List<string> sourceFiles = new List<string>();

			// Register the most recent version of MSBuild
			MSBuildLocator.RegisterInstance(MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(instance => instance.Version).First());

			if(LoadProject(projectPath, resourceFiles, sourceFiles)) {
				HashSet<string> unused = new HashSet<string>();
				LoadResources(unused, resourceFiles);
				foreach(string file in sourceFiles) {
					string text = File.ReadAllText(file);
					HashSet<string> used = new HashSet<string>(unused.Where(r => text.Contains(r, StringComparison.Ordinal)));
					unused.RemoveWhere(r => used.Contains(r));
					if(unused.Count == 0) break;
				}
				// This is list of resources that accessed in a custom way. Beware it can mask a real problem
				List<string> exclude = new List<string>() {
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
			string root = Path.GetDirectoryName(projectFile)!;
			Project project = Project.FromFile(projectFile, new ProjectOptions());

			List<ProjectItem> resxList = project.GetItems("EmbeddedResource").Where(item =>
				item.EvaluatedInclude.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) &&
				!string.IsNullOrWhiteSpace(item.GetMetadataValue("Generator"))
			).ToList();
			resourceList.AddRange(resxList.Select(item => Path.Combine(root, item.EvaluatedInclude)));

			List<ProjectItem> csList = project.GetItems("Compile").ToList();
			sourceList.AddRange(csList.Select(item => Path.Combine(root, item.EvaluatedInclude)));

			List<ProjectItem> xamlList = project.GetItems("Page").ToList();
			sourceList.AddRange(xamlList.Select(item => Path.Combine(root, item.EvaluatedInclude)));

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
				nsmgr.AddNamespace(xml.DocumentElement!.Prefix, xml.DocumentElement.NamespaceURI);
				nsmgr.AddNamespace("r", xml.DocumentElement.NamespaceURI);

				XmlNodeList? list = xml.SelectNodes("/r:root/r:data", nsmgr);
				if(list != null && 0 < list.Count) {
					foreach(XmlNode node in list) {
						resources.Add(className + "." + node.Attributes!["name"]!.Value);
					}
				}
			}
		}
	}
}
