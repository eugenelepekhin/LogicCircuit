using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using io = System.IO;

namespace ResourceWrapper.Generator {
	partial class ResourcesWrapper {

		public string File { get; set; }
		public string Code { get; set; }
		public string NameSpace { get; set; }
		public string ClassName { get; set; }
		public string ResourceName { get; set; }
		public bool IsPublic { get; set; }
		public bool Pseudo { get; set; }

		public List<ResourceItem> Items { get; private set; }

		public bool Generate() {
			this.Items = new List<ResourceItem>();
			if(this.Pseudo) {
				Message.Write("Generating pseudo locale resources for \"{0}\"", this.File);
				Message.Flush();
			}
			Message.Write("Generating file \"{0}\"", this.Code);

			XmlDocument resource = new XmlDocument();
			resource.Load(this.File);

			bool success = true;
			XmlNodeList nodeList = resource.SelectNodes("/root/data");
			if(nodeList != null && nodeList.Count > 0) {
				foreach(XmlNode node in nodeList) {
					XmlAttribute nodeName = node.Attributes["name"];
					if(nodeName == null) {
						return this.Error("Unknown Node", "Resource name is missing");
					}
					string name = nodeName.InnerText;

					XmlNode nodeValue = node.SelectSingleNode("value");
					if(nodeValue == null) {
						return this.Error(name, "Value missing");
					}
					string value = nodeValue.InnerText.Trim();

					XmlNode nodeComment = node.SelectSingleNode("comment");
					string comment = (nodeComment != null) ? nodeComment.InnerText.Trim() : string.Empty;

					if(node.Attributes["type"] != null) {
						success = this.GenerateInclude(name, value, comment) && success;
					} else {
						success = this.GenerateString(name, value, comment) && success;
					}
				}
			}

			string content = this.TransformText();
			string oldFileContent = null;
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

		private bool GenerateInclude(string name, string value, string comment) {
			string[] list = value.Split(';');
			if(list.Length < 2) {
				return this.Corrupted(name);
			}
			string file = list[0];
			list = list[1].Split(',');
			if(list.Length < 2) {
				return this.Corrupted(name);
			}
			string type = list[0].Trim();
			if(type == "System.String") {
				return this.GenerateString(name, this.Format("content of the file: \"{0}\"", file), comment);
			}
			return this.GenerateObjectProperty(name, file, type);
		}

		private bool GenerateString(string name, string value, string comment) {
			bool pseudo;
			List<String> localizationVariants;
			this.GetLocalizationInfo(comment, out pseudo, out localizationVariants);

			int parameterCount = this.ParameterCount(name, value);
			if(parameterCount < 0) {
				return false;
			} else if(parameterCount == 0) {
				return this.GenerateStringProperty(name, value, pseudo, localizationVariants);
			}
			List<Parameter> param = this.ParameterList(name, parameterCount, comment);
			if(param == null) {
				return false;
			}
			if(param.Count <= 0) {
				return this.GenerateStringProperty(name, value, pseudo, localizationVariants);
			}
			return this.GenerateFunction(name, value, param, pseudo, localizationVariants);
		}

		private bool GenerateObjectProperty(string name, string file, string type) {
			this.Items.Add(new ResourceItem() {
				Name = name,
				Value = file,
				Type = type
			});
			return true;
		}

		private bool GenerateStringProperty(string name, string value, bool pseudo, List<String> localizationVariants) {
			this.Items.Add(new ResourceItem() {
				Name = name,
				Value = value,
				Type = "string",
				NonLocalizable = pseudo,
				LocalizationVariants = localizationVariants
			});
			return true;
		}

		private bool GenerateFunction(string name, string value, List<Parameter> param, bool pseudo, List<String> localizationVariants) {
			this.Items.Add(new ResourceItem() {
				Name = name,
				Value = value,
				Type = "string",
				Parameters = param,
				NonLocalizable = pseudo,
				LocalizationVariants = localizationVariants
			});
			return true;
		}

		private bool Error(string nodeName, string errorText, params object[] args) {
			//"C:\Projects\TestApp\TestApp\Subfolder\TextMessage.resx(10,1): error URW001: nodeName: my error"
			Message.Error("{0}(1,1): error URW001: {1}: {2}", this.File, nodeName, string.Format(errorText, args));
			return false;
		}

		private bool Corrupted(string nodeName) {
			return this.Error(nodeName, "Structure of the value node is corrupted");
		}

		private string Format(string format, params object[] args) {
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

		private void GetLocalizationInfo(string comment, out bool pseudo, out List<string> localizationVariants) {
			pseudo = this.Pseudo;
			localizationVariants = null;
			if(pseudo) {
				Match match = Regex.Match(comment, @"(?<exclude>!)(\((?<list>.*)\))?", RegexOptions.CultureInvariant);
				if(match != null && match.Success) {
					if(match.Groups["exclude"].Success) {
						pseudo = false;
						if(match.Groups["list"].Success) {
							string listText = match.Groups["list"].Value;
							string[] variants = listText.Split(',');
							List<string> list = new List<string>();
							foreach(string var in variants) {
								string text = var.Trim();
								if(0 < text.Length) {
									list.Add(text);
								}
							}
							localizationVariants = list;
						}
					}
				}
			}
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
							this.Error(nodeName, "parameter number {0} is missing in the string", i);
							return -1;
						}
						max = i + 1;
					}
				}
				if(max <= 0) {
					this.Corrupted(nodeName);
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
					this.Error(name, "number of parameters expected by value of the string {0} do not match to provided parameter list in comment", name);
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
						this.Error(name, "bad parameter declaration for parameter {0}", i);
						return null;
					}
					string paramType = param[0].Trim();
					if(!typeName.IsMatch(paramType)) {
						this.Error(name, "bad type provided for parameter {0}", i);
						return null;
					}
					string paramName = param[1].Trim();
					if(!parameterName.IsMatch(paramName)) {
						this.Error(name, "bad name provided for parameter {0}", i);
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
					this.Error(name, "Type list missing for string {0}. Please provide it in the comment field of the string resource in the form: {{type0, type1, ...}}", name);
					return null;
				#endif
			}
		}
	}
}
