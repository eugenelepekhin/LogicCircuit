using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Evaluation;

namespace ResourceWrapper.Generator {
	internal struct ProjectParser {
		public bool Generate(string projectPath, IEnumerable<string> pseudo, bool enforceParameterDeclaration, bool flowDirection) {
			List<Resource> list = this.Parse(projectPath);
			bool success = true;
			foreach(Resource resource in list) {
				success &= new ResourcesWrapper() {
					File = resource.file,
					Code = resource.code,
					NameSpace = resource.nameSpace,
					ClassName = resource.className,
					ResourceName = resource.name,
					IsPublic = resource.isPublic,
					Pseudo = pseudo.Contains(resource.name, StringComparer.OrdinalIgnoreCase),
					EnforceParameterDeclaration = enforceParameterDeclaration,
					FlowDirection = flowDirection,
					Satelites = resource.sateliteList
				}.Generate();
			}
			return success;
		}

		private List<Resource> Parse(string projectPath) {
			List<Resource> list = new List<Resource>();
			Predicate<ProjectItem> isResource = item => item.ItemType == "EmbeddedResource";
			Predicate<string> isPublic = generator => generator == "PublicResXFileCodeGenerator";
			Predicate<string> isInternal = generator => generator == "ResXFileCodeGenerator";
			Predicate<string> isGenerated = generator => isPublic(generator) || isInternal(generator);
			Predicate<ProjectItem> isGeneratedItem = item => isGenerated(item.GetMetadataValue("Generator")) && !string.IsNullOrWhiteSpace(item.GetMetadataValue("LastGenOutput"));

			Project project = new Project(projectPath);
			string projectFolder = project.DirectoryPath;
			string projectNamespace = project.GetPropertyValue("RootNamespace");

			foreach(ProjectItem item in project.Items.Where(i => isResource(i) && isGeneratedItem(i))) {
				string resourcePath = item.EvaluatedInclude;
				string resourceRoot = Path.GetDirectoryName(resourcePath);
				string resourceFile = Path.GetFileNameWithoutExtension(resourcePath);
				string resourceName = !string.IsNullOrEmpty(resourceRoot)
					? string.Format("{0}.{1}.{2}", projectNamespace, resourceRoot.Replace('\\', '.') , resourceFile)
					: string.Format("{0}.{1}", projectNamespace, resourceFile)
				;
				string nameSpace = item.GetMetadataValue("CustomToolNamespace");
				if(string.IsNullOrWhiteSpace(nameSpace)) {
					nameSpace =  !string.IsNullOrEmpty(resourceRoot)
						? string.Format("{0}.{1}", projectNamespace, resourceRoot.Replace('\\', '.'))
						: projectNamespace
					;
				}

				Regex regex = new Regex(Regex.Escape(Path.ChangeExtension(resourcePath, null)) + @"\.[a-zA-Z\-]{2,20}\.resx", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				List<string> sateliteList = new List<string>();
				foreach(ProjectItem satelite in project.Items.Where(i => isResource(i) && !isGeneratedItem(i) && regex.IsMatch(i.EvaluatedInclude))) {
					sateliteList.Add(Path.Combine(projectFolder, satelite.EvaluatedInclude));
				}
				
				Resource resource = new Resource() {
					file = Path.Combine(projectFolder, resourcePath),
					code = Path.Combine(projectFolder, resourceRoot, item.GetMetadataValue("LastGenOutput")),
					name = resourceName,
					nameSpace = nameSpace,
					className = resourceFile.Replace('.', '_'),
					isPublic = isPublic(item.GetMetadataValue("Generator")),
					sateliteList = sateliteList
				};
				list.Add(resource);
			}
			return list;
		}

		private struct Resource {
			public string file;
			public string code;
			public string name;
			public string nameSpace;
			public string className;
			public bool isPublic;
			public List<string> sateliteList;
		}
	}
}
