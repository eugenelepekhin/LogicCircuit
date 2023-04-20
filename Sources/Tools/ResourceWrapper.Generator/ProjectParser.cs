using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace ResourceWrapper.Generator {
	public class ProjectParser {
		private sealed class ResourceGroup {
			public string ResxPath { get; }
			public string CodePath { get; }
			public string Name { get; }
			public string Namespace { get; }
			public string ClassName { get; }
			public bool IsPublic { get; }

			private HashSet<string> satelites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			public IEnumerable<string> Satelites => this.satelites;

			public ResourceGroup(string resxPath, string codePath, string name, string nameSpace, string className, bool isPublic) {
				this.ResxPath = resxPath;
				this.CodePath = codePath;
				this.Name = name;
				this.Namespace = nameSpace;
				this.ClassName = className;
				this.IsPublic = isPublic;
			}

			public void AddSatelite(string path) {
				this.satelites.Add(path);
			}

			public bool IsMainResource(string path) => StringComparer.OrdinalIgnoreCase.Equals(this.ResxPath, path);
		}

		public static void InitMsBuild() {
			// Register the most recent version of MSBuild
			MSBuildLocator.RegisterInstance(MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(instance => instance.Version).First());
		}

		public string ProjectFile { get; }
		public bool Pseudo { get; }
		public bool OptionalParameters { get; }
		public bool FlowDirection { get; }
		public bool Verbose { get; }

		private int errorCount;
		private int warningCount;

		public ProjectParser(
			string fileName,
			bool pseudo,
			bool optionalParameters,
			bool flowDirection,
			bool verbose
		) {
			this.ProjectFile = fileName;
			this.Pseudo = pseudo;
			this.OptionalParameters = optionalParameters;
			this.FlowDirection = flowDirection;
			this.Verbose = verbose;
		}

		public void Error(string message) {
			this.errorCount++;
			Program.Error(message);
		}

		public void Warning(string message) {
			this.warningCount++;
			Program.Warning(message);
		}

		public void Message(string message) {
			if(this.Verbose) {
				Program.Message(message);
			}
		}

		public bool Parse() {
			List<ResourceGroup> groups = this.ParseProject();
			foreach(ResourceGroup group in groups) {
				int errors = 0;
				int warnings = 0;
				IEnumerable<ResourceItem> items = ResourceParser.Parse(group.ResxPath, !this.OptionalParameters, group.Satelites, out errors, out warnings);
				this.errorCount += errors;
				this.warningCount += warnings;
				if(errors == 0) {
					ResourcesWrapper wrapper = new ResourcesWrapper(group.Namespace, group.ClassName, group.Name, group.IsPublic, this.Pseudo, this.FlowDirection, items);
					wrapper.Generate(group.CodePath);
				}
			}
			return this.errorCount == 0;
		}

		private List<ResourceGroup> ParseProject() {
			List<ResourceGroup> groups = new List<ResourceGroup>();
			Project project = Project.FromFile(this.ProjectFile, new ProjectOptions());
			List<ProjectItem> list = project.GetItems("EmbeddedResource").Where(i => i.EvaluatedInclude.EndsWith(".resx", StringComparison.OrdinalIgnoreCase)).ToList();
			if(list.Count > 0) {
				string projectFolder = project.DirectoryPath;
				string projectNamespace = project.GetPropertyValue("RootNamespace");
				foreach (ProjectItem item in list) {
					string generator = item.GetMetadataValue("Generator");
					bool isPublic = generator == "PublicResXFileCodeGenerator";
					bool isInternal = generator == "ResXFileCodeGenerator";
					if (isPublic || isInternal) {
						string resourcePath = item.EvaluatedInclude;
						string resourceRoot = Path.GetDirectoryName(resourcePath) ?? string.Empty;
						string resourceFile = Path.GetFileNameWithoutExtension(resourcePath);
						string resourceName = !string.IsNullOrEmpty(resourceRoot)
							? string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", projectNamespace, resourceRoot.Replace('\\', '.') , resourceFile)
							: string.Format(CultureInfo.InvariantCulture, "{0}.{1}", projectNamespace, resourceFile)
						;
						string nameSpace = item.GetMetadataValue("CustomToolNamespace");
						if(string.IsNullOrWhiteSpace(nameSpace)) {
							nameSpace =  !string.IsNullOrEmpty(resourceRoot)
								? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", projectNamespace, resourceRoot.Replace('\\', '.'))
								: projectNamespace
							;
						}

						ResourceGroup group = new ResourceGroup(
							resxPath: Path.Combine(projectFolder,  resourcePath),
							codePath: Path.Combine(projectFolder, resourceRoot, item.GetMetadataValue("LastGenOutput")),
							name: resourceName,
							nameSpace: nameSpace,
							className: resourceFile.Replace('.', '_'),
							isPublic: isPublic
						);
						groups.Add(group);

						Regex regex = new Regex(Regex.Escape(Path.ChangeExtension(resourcePath, null)) + @"\.[a-zA-Z\-]{2,20}\.resx", RegexOptions.Compiled | RegexOptions.IgnoreCase);
						foreach(ProjectItem satelite in list) {
							string path = satelite.EvaluatedInclude;
							if(regex.IsMatch(path) && !group.IsMainResource(path)) {
								group.AddSatelite(Path.Combine(projectFolder,  path));
							}
						}
					}
				}
			}
			return groups;
		}
	}
}
