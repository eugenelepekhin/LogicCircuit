using System.Text;

namespace ResourceWrapper.Generator {
	partial class ResourcesWrapper {
		public string NameSpace { get; }
		public string ClassName { get; }
		public string ResourceName { get; }
		public bool IsPublic { get; }
		public bool Pseudo { get; }

		public bool FlowDirection { get; }

		public IEnumerable<ResourceItem> Items { get; }

		public ResourcesWrapper(
			string nameSpace,
			string className,
			string resourceName,
			bool isPublic,
			bool pseudo,
			bool flowDirection,
			IEnumerable<ResourceItem> items
		) {
			this.NameSpace = nameSpace;
			this.ClassName = className;
			this.ResourceName = resourceName;
			this.IsPublic = isPublic;
			this.Pseudo = pseudo;
			this.FlowDirection = flowDirection;
			this.Items = items;
		}

		public void Generate(string code) {
			string content = this.TransformText();
			string? oldFileContent = null;
			if(File.Exists(code)) {
				oldFileContent = File.ReadAllText(code, Encoding.UTF8);
			}
			if(!StringComparer.Ordinal.Equals(oldFileContent, content)) {
				File.WriteAllText(code, content, Encoding.UTF8);
			}
		}
	}
}
