using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ResourceWrapper.Generator {
	public class ResourceItem {
		public string Name { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }
		public List<Parameter> Parameters { get; set; }
		public bool NonLocalizable { get; set; }
		public List<string> LocalizationVariants { get; set; }

		public string Comment {
			get {
				string[] line = this.Value.Split('\n', '\r');
				if(line != null && line.Length > 0) {
					if(line.Length > 1) {
						StringBuilder comment = new StringBuilder();
						comment.Append(line[0].Trim());
						int max = Math.Min(3, line.Length);
						string format = "\t\t/// {0}";
						for(int i = 1; i < line.Length && max > 0; i++) {
							string s = line[i].Trim();
							if(s.Length > 0) {
								comment.AppendLine();
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
		}

		public string ParametersDeclaration {
			get {
				Debug.Assert(this.Parameters != null, "There are no parameters");
				StringBuilder parameter = new StringBuilder();
				for(int i = 0; i < this.Parameters.Count; i++) {
					if(i > 0) {
						parameter.Append(", ");
					}
					parameter.AppendFormat("{0} {1}", this.Parameters[i].Type, this.Parameters[i].Name);
				}
				return parameter.ToString();
			}
		}

		public string ParametersInvocation {
			get {
				Debug.Assert(this.Parameters != null, "There are no parameters");
				StringBuilder parameter = new StringBuilder();
				for(int i = 0; i < this.Parameters.Count; i++) {
					if(i > 0) {
						parameter.Append(", ");
					}
					parameter.Append(this.Parameters[i].Name);
				}
				return parameter.ToString();
			}
		}

		public string GetStringExpression(bool pseudo) {
			if(pseudo && !this.NonLocalizable) {
				string values = string.Empty;
				if(this.LocalizationVariants != null && 0 < this.LocalizationVariants.Count) {
					StringBuilder text = new StringBuilder();
					foreach(string value in this.LocalizationVariants) {
						if(0 < text.Length) {
							text.Append(",");
						}
						text.AppendFormat("\"{0}\"", value.Replace("\"", "\\\"").Replace("\\", "\\\\"));
					}
					values = "new string[]{" + text.ToString() + "}, ";
				}
				return "((PseudoResourceManager)ResourceManager).GetBaseString(" + values;
			}
			return "ResourceManager.GetString(";
		}
	}
}
