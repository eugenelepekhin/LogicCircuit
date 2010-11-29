using System;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ResourceWrapper.Generator {
	internal struct ResourceParser {

		private const string Prefix = "res";
		private const string ResourceXPath = "/root/data";
		private const string IncludeFileXPath = "type";
		private const string NameXPath = "name";
		private const string ValueXPath = "value";
		private const string CommentXPath = "comment";

		private string file;

		public bool Generate(string file, string code, string nameSpace, string className, string resourceName, bool isPublic) {
			CultureInfo culture = CultureInfo.InvariantCulture;
			Message.Write("Generating", code);
			StringBuilder text = new StringBuilder();
			text.Append(this.Header());
			text.AppendFormat(culture, TextMessage.ClassHeader, nameSpace, className, resourceName, isPublic ? "public" : "internal");

			this.file = file;
			if(!this.Generate(text, file)) {
				return false;
			}

			text.Append(TextMessage.ClassFooter);

			string content = text.ToString();
			string oldFileContent = null;
			if(File.Exists(code)) {
				oldFileContent = File.ReadAllText(code, Encoding.UTF8);
			}
			if(!StringComparer.Ordinal.Equals(oldFileContent, content)) {
				File.WriteAllText(code, content, Encoding.UTF8);
				Message.Flush();
			}
			Message.Clear();
			return true;
		}

		public bool Generate(StringBuilder text, string file) {
			XmlDocument resource = new XmlDocument();
			resource.Load(file);

			bool success = true;
			XmlNodeList nodeList = resource.SelectNodes(ResourceParser.ResourceXPath);
			if(nodeList != null && nodeList.Count > 0) {
				foreach(XmlNode node in nodeList) {
					XmlAttribute nodeName = node.Attributes[ResourceParser.NameXPath];
					if(nodeName == null) {
						return this.Error("Unknown Node", "NameMissing");
					}
					string name = nodeName.InnerText;

					XmlNode nodeValue = node.SelectSingleNode(ResourceParser.ValueXPath);
					if(nodeValue == null) {
						return this.Error(name, "ValueMissing");
					}
					string value = nodeValue.InnerText.Trim();

					XmlNode nodeComment = node.SelectSingleNode(ResourceParser.CommentXPath);
					string comment = (nodeComment != null) ? nodeComment.InnerText.Trim() : string.Empty;

					if(node.Attributes[ResourceParser.IncludeFileXPath] != null) {
						success = this.GenerateInclude(text, name, value, comment) && success;
					} else {
						success = this.GenerateString(text, name, value, comment) && success;
					}
				}
			}

			return success;
		}

		private bool GenerateInclude(StringBuilder text, string name, string value, string comment) {
			string[] list = value.Split(';');
			if(list.Length < 2) {
				return this.Error(name, "UnrecognizedStructure");
			}
			string file = list[0];
			list = list[1].Split(',');
			if(list.Length < 2) {
				return this.Error(name, "UnrecognizedStructure");
			}
			string type = list[0].Trim();
			if(type == "System.String") {
				return this.GenerateString(text, name, this.Format(TextMessage.FileLabel, file), comment);
			}
			text.AppendFormat(TextMessage.ResourceObjectProperty, type, name, file);
			return true;
		}

		private struct Parameter {
			public string type;
			public string name;
			public Parameter(string type, string name) {
				this.type = type;
				this.name = name;
			}
		}

		private bool GenerateString(StringBuilder text, string name, string value, string comment) {
			int parameterCount = this.ParameterCount(name, value);
			if(parameterCount < 0) {
				return false;
			} else if(parameterCount == 0) {
				return this.GenerateProperty(text, name, value);
			}
			List<Parameter> param = this.ParameterList(name, parameterCount, comment);
			if(param == null) {
				return false;
			}
			if(param.Count <= 0) {
				return this.GenerateProperty(text, name, value);
			}
			StringBuilder parameter = new StringBuilder();
			for(int i = 0; i < param.Count; i++) {
				if(i > 0) {
					parameter.Append(", ");
				}
				parameter.AppendFormat("{0} {1}", param[i].type, param[i].name);
			}
			string declare = parameter.ToString();
			parameter.Length = 0;
			for(int i = 0; i < param.Count; i++) {
				if(i > 0) {
					parameter.Append(", ");
				}
				parameter.Append(param[i].name);
			}
			string call = parameter.ToString();
			text.AppendFormat(TextMessage.ResourceStringMethod, name, declare, call, this.MakeComment(value));
			return true;
		}

		private bool GenerateProperty(StringBuilder text, string name, string value) {
			text.AppendFormat(TextMessage.ResourceStringProperty, name, this.MakeComment(value));
			return true;
		}

		private int ParameterCount(string nodeName, string value) {
			MatchCollection matchList = Regex.Matches(value, @"\{(?<param>\d(,[+-]?\d+)?(:[^}]+)?)}", RegexOptions.CultureInvariant);
			if(matchList != null && matchList.Count > 0) {
				bool[] param = new bool[10];
				foreach(Match match in matchList) {
					int index = 0;
					if(int.TryParse(match.Groups["param"].Value.Substring(0, 1), out index)) {
						param[index] = true;
					}
				}
				int max = 0;
				for(int i = 0; i < param.Length; i++) {
					if(param[i]) {
						if(max < i) {
							this.Error(nodeName, "ParameterNumberMissing", i);
							return -1;
						}
						max = i + 1;
					}
				}
				if(max <= 0) {
					this.Error(nodeName, "UnrecognizedStructure");
					return -1;
				}
				return max;
			}
			return 0;
		}

		private List<Parameter> empty;

		private List<Parameter> ParameterList(string name, int parameterCount, string comment) {
			Match match = Regex.Match(comment.Trim(), @"^(?<exclude>-?)\{(?<param>.+)}", RegexOptions.CultureInvariant);
			if(match != null && match.Success) {
				if(match.Groups["exclude"].Value == "-") {
					if(this.empty == null) {
						this.empty = new List<Parameter>(0);
					}
					return this.empty;
				}
				string[] list = match.Groups["param"].Value.Split(',');
				if(list.Length != parameterCount) {
					this.Error(name, "BadTypeDeclaration", name);
					return null;
				}
				List<Parameter> parameter = new List<Parameter>(parameterCount);
				Regex typeName = new Regex(@"^[A-Za-z_](\.?[A-Za-z_0-9]*)*$",
					RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled
				);
				Regex parameterName = new Regex(@"^[A-Za-z_][A-Za-z_0-9]*$",
					RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled
				);
				for(int i = 0; i < list.Length; i++) {
					string[] param = list[i].Trim().Split(' ');
					if(param == null || param.Length != 2) {
						this.Error(name, "BadParameter", i);
						return null;
					}
					string paramType = param[0].Trim();
					if(!typeName.IsMatch(paramType)) {
						this.Error(name, "BadParameterType", i);
						return null;
					}
					string paramName = param[1].Trim();
					if(!parameterName.IsMatch(paramName)) {
						this.Error(name, "BadParameterName", i);
						return null;
					}
					parameter.Add(new Parameter(paramType, paramName));
				}
				return parameter;
			} else {
				#if OptionalTypeList
					string[] typeList = new string[parameterCount];
					for(int i = 0; i < typeList.Length; i++) {
						typeList[i] = "object";
					}
					return typeList;
				#else
					this.Error(name, "TypeListMissing", name);
					return null;
				#endif
			}
		}

		private string MakeComment(string text) {
			string[] line = text.Split('\n', '\r');
			if(line != null && line.Length > 0) {
				if(line.Length > 1) {
					StringBuilder comment = new StringBuilder();
					comment.Append(line[0].Trim());
					int max = Math.Min(3, line.Length);
					string format = TextMessage.StringComment;
					for(int i = 1; i < line.Length && max > 0; i++) {
						string s = line[i].Trim();
						if(s.Length > 0) {
							comment.AppendFormat(CultureInfo.InvariantCulture, format, s);
							max--;
						}
					}
					return comment.ToString();
				}
				return line[0].Trim();
			}
			return string.Empty;
		}

		private bool Error(string nodeName, string errorCode, params object[] args) {
			//"C:\Projects\TestApp\TestApp\Subfolder\TextMessage.resx(10,1): error URW001: nodeName: my error"
			string message = string.Format(TextMessage.ResourceManager.GetString(errorCode), args);
			Message.Error("ErrorMessage", this.file, 1, 1, nodeName, message);
			return false;
		}

		public bool WasGenerated(string code) {
			string generatedHeader = this.Header();
			char[] codeHeader = new char[generatedHeader.Length];
			int length = 0;
			using(StreamReader reader = File.OpenText(code)) {
				length = reader.Read(codeHeader, 0, codeHeader.Length);
			}
			if(length == generatedHeader.Length) {
				string codeValue = new string(codeHeader);
				return StringComparer.Ordinal.Compare(generatedHeader, codeValue) == 0;
			}
			return false;
		}

		private string Header() {
			Assembly assembly = Assembly.GetExecutingAssembly();
			AssemblyName name = assembly.GetName();
			return this.Format(TextMessage.ResourceHeader, name.FullName);
		}

		private string Format(string format, params object[] args) {
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}
	}
}

/*
===ResourceHeader
//-----------------------------------------------------------------------------
//
//	This code was generated by a {0}.
//
//	Changes to this file may cause incorrect behavior and will be lost if
//	the code is regenerated.
//
//-----------------------------------------------------------------------------

===ClassHeader
namespace {0} {{
	using System;

	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	// This class was auto-generated.
	// To add or remove a member, edit your .ResX file then rerun MsBuild,
	// or rebuild your VS project.
	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	{3} static class {1} {{
		private static global::System.Resources.ResourceManager resourceManager;
		/// <summary>
		/// Returns the cached ResourceManager instance used by this class.
		/// </summary>
		[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
		internal static global::System.Resources.ResourceManager ResourceManager {{
			get {{
				if(resourceManager == null) {{
					resourceManager = new global::System.Resources.ResourceManager("{2}", typeof({1}).Assembly);
				}}
				return resourceManager;
			}}
		}}

		/// <summary>
		/// Overrides the current thread's CurrentUICulture property for all
		/// resource lookups using this strongly typed resource class.
		/// </summary>
		[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
		internal static global::System.Globalization.CultureInfo Culture {{ get; set; }}
===ClassFooter
	}
}
===ResourceObjectProperty

		/// <summary>
		/// Looks up a localized {0} similar to one in file: "{2}".
		/// </summary>
 		public static {0} {1} {{
			get {{ return ({0})ResourceManager.GetObject("{1}", Culture); }}
		}}
===ResourceStringProperty

		/// <summary>
		/// Looks up a localized string similar to {1}.
		/// </summary>
 		public static string {0} {{
			get {{ return ResourceManager.GetString("{0}", Culture); }}
		}}
===ResourceStringMethod

		/// <summary>
		/// Looks up a localized string similar to {3}.
		/// </summary>
 		public static string {0}({1}) {{
			return string.Format(Culture, ResourceManager.GetString("{0}", Culture), {2});
		}}
*/
