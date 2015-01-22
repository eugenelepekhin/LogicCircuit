using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ResourceWrapper.Generator {
	public class ResourceItem {
		public string Name { get; private set; }
		public string Value { get; private set; }
		public string Type { get; private set; }
		public List<Parameter> Parameters { get; set; }
		public List<string> LocalizationVariants { get; set; }
		public bool SuppressValidation { get; set; }

		public ResourceItem(string name, string value, string type) {
			this.Name = name;
			this.Value = value;
			this.Type = type;
		}

		public string Comment() {
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

		public string ParametersDeclaration() {
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

		public string ParametersInvocation() {
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

		public string GetStringExpression(bool pseudo) {
			if(pseudo && this.LocalizationVariants != null && 0 < this.LocalizationVariants.Count) {
				// In case of pseudo and localization variants
				StringBuilder text = new StringBuilder();
				foreach(string value in this.LocalizationVariants) {
					if(0 < text.Length) {
						text.Append(",");
					}
					text.AppendFormat("\"{0}\"", value.Replace("\"", "\\\"").Replace("\\", "\\\\"));
				}
				return "((PseudoResourceManager)ResourceManager).GetBaseString(new string[]{" + text.ToString() + "}, ";
			}
			return "ResourceManager.GetString(";
		}
	}
}
