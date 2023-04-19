using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ResourceWrapper.Generator {
	public class Parser {
		public string ProjectFolder { get; set; } = string.Empty;
		public string NameSpace { get; set; } = string.Empty;
		public string Generator { get; set; } = string.Empty;
		public string ResxFile { get; set; } = string.Empty;
		public string Output { get; set; } = string.Empty;
		public string WrapperNamespace { get; set; } = string.Empty;
		public string AllItems { get; set; } = string.Empty;

		public bool Pseudo { get; set; }
		public bool OptionalParameterDeclaration { get; set; }
		public bool FlowDirection { get; set; }
		public static bool Verbose { get; set; }

		private bool IsPublic => Parser.Equals(this.Generator, "PublicResXFileCodeGenerator");
		private bool IsInternal => Parser.Equals(this.Generator, "ResXFileCodeGenerator");

		private string ResxFolder => Path.GetDirectoryName(this.ResxFile)!;
		private string ResxName => Path.GetFileNameWithoutExtension(this.ResxFile);

		private void LogInputs() {
			Parser.Log(">>> Generating resources");
			Parser.Log($"ProjectFolder={this.ProjectFolder}");
			Parser.Log($"NameSpace={this.NameSpace}");
			Parser.Log($"Generator={this.Generator}");
			Parser.Log($"ResxFile={this.ResxFile}");
			Parser.Log($"Output={this.Output}");
			Parser.Log($"WrapperNamespace={this.WrapperNamespace}");
			Parser.Log($"AllItems={this.AllItems}");

			Parser.Log($"Pseudo={this.Pseudo}");
			Parser.Log($"OptionalParameterDeclaration={this.OptionalParameterDeclaration}");
			Parser.Log($"FlowDirection={this.FlowDirection}");
			Parser.Log($"Verbose={Parser.Verbose}");
		}

		public bool Parse() {
			this.LogInputs();
			Parser.LogAll($"Generating resource wrappers for {Path.Combine(this.ProjectFolder, this.ResxFile)}");

			if(!this.IsPublic && !this.IsInternal) {
				LogAll($"Unknown Generator: {this.Generator}. Specify PublicResXFileCodeGenerator or ResXFileCodeGenerator");
				return false;
			}
			if(string.IsNullOrEmpty(this.Output)) {
				LogAll($"Missing Output.");
				return false;
			}

			string nameSpace = (
				!string.IsNullOrEmpty(this.WrapperNamespace)
				? this.WrapperNamespace
				: (
					!string.IsNullOrEmpty(this.ResxFolder)
					? Parser.Format("{0}.{1}", this.NameSpace, this.ResxFolder.Replace('\\', '.'))
					: this.NameSpace
				)
			);

			Regex regex = new Regex(@"^\s*" + Regex.Escape(Path.ChangeExtension(this.ResxFile, null)) + @"\.[a-zA-Z\-]{2,20}\.resx\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			IEnumerable<string> satelites = this.AllItems.Split(';').Where(i => !string.IsNullOrWhiteSpace(i) && regex.IsMatch(i)).Select(i => Path.Combine(this.ProjectFolder, i.Trim())).ToArray();

			string resourceName = (
				!string.IsNullOrEmpty(this.ResxFolder)
				? Parser.Format("{0}.{1}.{2}", this.NameSpace, this.ResxFolder.Replace('\\', '.'), this.ResxName)
				: Parser.Format("{0}.{1}", this.NameSpace, this.ResxName)
			);

			ResourcesWrapper resourcesWrapper = new ResourcesWrapper(
				file: Path.Combine(this.ProjectFolder, this.ResxFile),
				code: Path.Combine(this.ProjectFolder, this.ResxFolder, this.Output),
				nameSpace: nameSpace,
				className: this.ResxName.Replace('.', '_'),
				resourceName: resourceName,
				isPublic: this.IsPublic,
				pseudo: this.Pseudo,
				enforceParameterDeclaration: !this.OptionalParameterDeclaration,
				flowDirection: this.FlowDirection,
				satelites: satelites
			);

			return resourcesWrapper.Generate();
		}

		public static bool Equals(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);

		public static void LogAll(string text) {
			Debug.WriteLine(text);
			Console.WriteLine(text);
		}

		public static void Log(string text) {
			if(Parser.Verbose) {
				LogAll(text);
			}
		}

		public static string Format(string format, params string[] args) => string.Format(CultureInfo.InvariantCulture, format, args);
	}
}
