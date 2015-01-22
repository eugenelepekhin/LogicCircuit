using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace ResourceWrapper.Generator {
	/// <summary>
	/// Parses and creates list of Items for generator to produce code.
	/// Expect the following syntax on comment:
	/// - minus in the first position of the comment turn off any parsing and property will be generated.
	/// !(value1, value2, ... valueN) list of allowed items. The value of the resource is expected to be one of the value1-valueN
	/// If there is no formating parameters than comment is ignored
	/// If there are formating parameters comment should declare parameters of formating function: {type1 parameter1, type2 parameter2, ... typeM parameterM}
	/// </summary>
	internal class ResourceParser {
		public static IEnumerable<ResourceItem> Parse(string file, bool enforceParameterDeclaration, IEnumerable<string> satelites) {
			XmlDocument resource = new XmlDocument();
			resource.Load(file);
			XmlNodeList nodeList = ResourceParser.SelectResources(resource);
			if(nodeList != null && 0 < nodeList.Count) {
				ResourceParser parser = new ResourceParser(file, enforceParameterDeclaration, satelites);
				List<ResourceItem> list = new List<ResourceItem>();
				Action<ResourceItem> assign = item => { if(item != null) { list.Add(item); } };
				parser.Parse(nodeList,
					(string name, string value, string comment) => assign(parser.GenerateInclude(name, value, comment)),
					(string name, string value, string comment) => assign(parser.GenerateString(name, value, comment))
				);
				if(parser.errorCount == 0 && parser.satelites != null) {
					parser.VerifySatelites(list);
				}
				if(parser.errorCount == 0) {
					return list;
				} else {
					return null;
				}
			}
			return Enumerable.Empty<ResourceItem>();
		}

		private static XmlNodeList SelectResources(XmlDocument resource) {
			return resource.SelectNodes("/root/data");
		}

		private string fileName;
		private readonly bool enforceParameterDeclaration;
		private readonly IEnumerable<string> satelites;

		private const RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline;
		private readonly Regex variantList = new Regex(@"^!\((?<list>.*)\)", regexOptions);
		// {int index, string message} hello, world {System.Int32 param} comment {} {MyType value1, Other value2, OneMore last}
		private readonly Regex functionParameters = new Regex(@"\{(?<param>[^}]+)\}", regexOptions);
		// a.b.c.d a, int i, string text, System.Int32 index
		private readonly Regex parameterDeclaration = new Regex(@"^(?<type>[A-Za-z_][A-Za-z_0-9]*(\s*\.\s*[A-Za-z_][A-Za-z_0-9]*)*)\s+(?<name>[A-Za-z_][A-Za-z_0-9]*)$", regexOptions);

		private int errorCount;
		private int warningCount;

		private ResourceParser(string file, bool enforceParameterDeclaration, IEnumerable<string> satelites) {
			this.fileName = file;
			this.enforceParameterDeclaration = enforceParameterDeclaration;
			this.satelites = satelites;
			this.errorCount = 0;
			this.warningCount = 0;
		}

		private void Parse(XmlNodeList nodeList, Action<string, string, string> generateInclude, Action<string, string, string> generateString) {
			foreach(XmlNode node in nodeList) {
				XmlAttribute nodeName = node.Attributes["name"];
				if(nodeName == null) {
					this.Error("Unknown Node", "Resource name is missing");
					continue;
				}
				string name = nodeName.InnerText.Trim();

				XmlNode nodeValue = node.SelectSingleNode("value");
				if(nodeValue == null) {
					this.Error(name, "Value missing");
					continue;
				}
				string value = nodeValue.InnerText.Trim();

				XmlNode nodeComment = node.SelectSingleNode("comment");
				string comment = (nodeComment != null) ? nodeComment.InnerText.Trim() : string.Empty;

				if(node.Attributes["type"] != null) {
					generateInclude(name, value, comment);
				} else {
					generateString(name, value, comment);
				}
			}
		}

		private void VerifySatelites(List<ResourceItem> itemList) {
			Dictionary<string, ResourceItem> items = new Dictionary<string,ResourceItem>(itemList.Count);
			itemList.ForEach(i => items.Add(i.Name, i));
			string mainFile = this.fileName;
			foreach(string file in this.satelites) {
				XmlDocument resource = new XmlDocument();
				resource.Load(file);
				XmlNodeList nodeList = ResourceParser.SelectResources(resource);
				if(nodeList != null && 0 < nodeList.Count) {
					this.fileName = file;
					this.Parse(nodeList, (a, b, c) => {},
						(string name, string value, string comment) => {
							ResourceItem item;
							if(items.TryGetValue(name, out item)) {
								if(!item.SuppressValidation) {
									int count = this.ValidateFormatItems(name, value, false);
									if(item.Parameters != null) {
										if(count != item.Parameters.Count) {
											this.Warning(name, "number of parameters is different from the same resource in the main resource file \"{0}\"", mainFile);
										}
									} else if(item.LocalizationVariants != null) {
										if(!item.LocalizationVariants.Contains(value)) {
											this.Error(name, "provided value is not in variant list defined in main resource file: \"{0}\"", mainFile);
										}
									}
								}
							} else {
								this.Warning(name, "resource does not exist in the main resource file \"{0}\"", mainFile);
							}
						}
					);
				}
			}
			this.fileName = mainFile;
		}

		private bool Error(string nodeName, string errorText, params object[] args) {
			//"C:\Projects\TestApp\TestApp\Subfolder\TextMessage.resx(10,1): error URW001: nodeName: my error"
			Message.Error("{0}(1,1): error URW001: {1}: {2}", this.fileName, nodeName, this.Format(errorText, args));
			this.errorCount++;
			return false;
		}

		private void Warning(string nodeName, string errorText, params object[] args) {
			Message.Warning("{0}(1,1): warning: {1}: {2}", this.fileName, nodeName, this.Format(errorText, args));
			this.warningCount++;
			Message.Flush();
		}

		private bool Corrupted(string nodeName) {
			return this.Error(nodeName, "Structure of the value node is corrupted");
		}

		private string Format(string format, params object[] args) {
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

		private ResourceItem GenerateInclude(string name, string value, string comment) {
			string[] list = value.Split(';');
			if(list.Length < 2) {
				this.Corrupted(name);
				return null;
			}
			string file = list[0];
			list = list[1].Split(',');
			if(list.Length < 2) {
				this.Corrupted(name);
				return null;
			}
			string type = list[0].Trim();
			if(type == "System.String") {
				return this.GenerateString(name, this.Format("content of the file: \"{0}\"", file), comment);
			}
			return this.GenerateObjectProperty(name, file, type);
		}

		private ResourceItem GenerateObjectProperty(string name, string file, string type) {
			return (0 == this.errorCount) ? new ResourceItem(name, file, type) : null;
		}

		private ResourceItem GenerateString(string name, string value, string comment) {
			ResourceItem item = new ResourceItem(name, value, "string");

			if(!comment.StartsWith("-")) {
				if(!this.IsVariantList(item, value, comment)) {
					this.ParseFormatList(item, value, comment);
				}
			} else {
				item.SuppressValidation = true;
			}

			return (0 == this.errorCount) ? item : null;
		}

		private bool IsVariantList(ResourceItem item, string value, string comment) {
			Match match = this.variantList.Match(comment);
			if(match.Success) {
				string listText = match.Groups["list"].Value;
				string[] variants = listText.Split(',');
				List<string> list = new List<string>();
				foreach(string var in variants) {
					string text = var.Trim();
					if(0 < text.Length) {
						list.Add(text);
					}
				}
				item.LocalizationVariants = list;
				if(!list.Contains(value)) {
					this.Error(item.Name, "Localization variants does not contain provided value: {0}", value);
				}
			}
			return match.Success;
		}

		private void ParseFormatList(ResourceItem item, string value, string comment) {
			int count = this.ValidateFormatItems(item.Name, value, true);
			if(0 < count) {
				Match paramsMatch = this.functionParameters.Match(comment);
				if(paramsMatch.Success) {
					string[] list = paramsMatch.Groups["param"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					List<Parameter> parameterList = new List<Parameter>(list.Length);
					foreach(string text in list) {
						if(!string.IsNullOrWhiteSpace(text)) {
							Match parameterMatch = this.parameterDeclaration.Match(text.Trim());
							if(parameterMatch.Success) {
								parameterList.Add(new Parameter(parameterMatch.Groups["type"].Value, parameterMatch.Groups["name"].Value));
							} else {
								this.Error(item.Name, "bad parameter declaration: {0}", text.Trim());
							}
						}
					}
					if(parameterList.Count != count) {
						this.Error(item.Name, "number of parameters expected by value of the string do not match to provided parameter list in comment");
					}
					item.Parameters = parameterList;
				} else {
					string error = "string value contains formating placeholders, but function parameters declaration is missing in comment.";
					if(this.enforceParameterDeclaration) {
						this.Error(item.Name, error);
					} else {
						this.Warning(item.Name, error);
					}
				}
			}
		}

		private int ValidateFormatItems(string name, string value, bool requareAllParameters) {
			Func<int> error = () => {
				this.Error(name, "Invalid formating item.");
				return -1;
			};
			HashSet<int> indexes = new HashSet<int>();
			for(int i = 0; i < value.Length; i++) {
				if('}' == value[i]) {
					i++;
					if(!(i < value.Length && '}' == value[i])) {
						this.Error(name, "Input string is not in correct format");
						return -1;
					}
				} else if('{' == value[i]) {
					i++;
					if(i < value.Length && '{' == value[i]) {
						continue; // skip escaped {
					}
					// Formating item is started
					bool isNumber = false;
					int index = 0;
					while(i < value.Length && '0' <= value[i] && value[i] <= '9' && index < 1000000) {
						index = index * 10 + value[i] - '0';
						isNumber = true;
						i++;
					}
					if(!isNumber || 1000000 <= index) {
						return error();
					}
					indexes.Add(index);
					while(i < value.Length && ' ' == value[i]) {
						i++;
					}
					if(i < value.Length && ',' == value[i]) {
						i++;
						while(i < value.Length && ' ' == value[i]) {
							i++;
						}
						if(i < value.Length && '-' == value[i]) {
							i++; //skip sign
						}
						isNumber = false;
						index = 0;
						while(i < value.Length && '0' <= value[i] && value[i] <= '9' && index < 1000000) {
							isNumber = true;
							i++;
						}
						if(!isNumber || 1000000 <= index) {
							return error();
						}
					}
					while(i < value.Length && ' ' == value[i]) {
						i++;
					}
					if(i < value.Length && ':' == value[i]) {
						// Inside format string. It is allowed to have escaped open and closed braces, so skip them until single }
						for(;;) {
							i++;
							while(i < value.Length && '}' != value[i]) {
								if('{' == value[i]) {
									i++;
									if(!(i < value.Length && '{' == value[i])) {
										return error();
									}
								}
								i++;
							}
							if(i + 1 < value.Length && '}' == value[i + 1]) {
								i++;
							} else {
								break;
							}
						}
					}
					if(!(i < value.Length && '}' == value[i])) {
						return error();
					}
				}
			}
			int current = 0;
			foreach(int index in indexes.OrderBy(i => i)) {
				if(index != current++) {
					if(requareAllParameters) {
						this.Error(name, "parameter number {0} is missing in the string", current - 1);
					} else {
						this.Warning(name, "parameter number {0} is missing in the string", current - 1);
						break;
					}
					return -1; // report just one missing parameter number
				}
			}
			return indexes.Count;
		}
	}
}
