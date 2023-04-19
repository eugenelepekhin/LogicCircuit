using System.Text;
using io = System.IO;

namespace ResourceWrapper.Generator {
	partial class ResourcesWrapper {
		public string File { get; }
		public string Code { get; }
		public string NameSpace { get; }
		public string ClassName { get; }
		public string ResourceName { get; }
		public bool IsPublic { get; }
		public bool Pseudo { get; }

		public bool EnforceParameterDeclaration { get; }
		public bool FlowDirection { get; }
		public IEnumerable<string> Satelites { get; }

		private IEnumerable<ResourceItem>? items;
		public IEnumerable<ResourceItem> Items => this.items ?? Enumerable.Empty<ResourceItem>();

		public ResourcesWrapper(
			string file,
			string code,
			string nameSpace,
			string className,
			string resourceName,
			bool isPublic,
			bool pseudo,
			bool enforceParameterDeclaration,
			bool flowDirection,
			IEnumerable<string> satelites
		) {
			this.File = file;
			this.Code = code;
			this.NameSpace = nameSpace;
			this.ClassName = className;
			this.ResourceName = resourceName;
			this.IsPublic = isPublic;
			this.Pseudo = pseudo;
			this.EnforceParameterDeclaration = enforceParameterDeclaration;
			this.FlowDirection = flowDirection;
			this.Satelites = satelites;
		}

		public bool Generate() {
			IEnumerable<ResourceItem>? list = ResourceParser.Parse(this.File, this.EnforceParameterDeclaration, this.Satelites);
			if(list != null) {
				this.items = list;
				string content = this.TransformText();
				string? oldFileContent = null;
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
