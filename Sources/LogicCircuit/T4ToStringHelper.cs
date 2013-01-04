using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	/// <summary>
	/// Utility class to produce culture-oriented representation of an object as a string.
	/// </summary>
	public class T4ToStringHelper {
		private IFormatProvider formatProvider = Properties.Resources.Culture;

		/// <summary>
		/// Gets or sets format provider to be used by ToStringWithCulture method.
		/// </summary>
		public IFormatProvider FormatProvider {
			get { return this.formatProvider; }
			set { this.formatProvider = value ?? Properties.Resources.Culture; }
		}

		public bool EscapeXmlText { get; set; }

		/// <summary>
		/// This is called from the compile/run appdomain to convert objects within an expression block to a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string ToStringWithCulture(object value) {
			if(value == null) {
				throw new ArgumentNullException("value");
			}

			Type type = value.GetType();
			MethodInfo method = type.GetMethod("ToString", new Type[] { typeof(IFormatProvider) });
			if(method != null) {
				return this.EscapeXml((string)method.Invoke(value, new object[] { this.FormatProvider }));
			} else {
				return this.EscapeXml(value.ToString());
			}
		}

		public string ToStringWithCulture(int value) {
			return this.EscapeXml(value.ToString("D", this.FormatProvider));
		}

		public string ToStringWithCulture(string value) {
			return this.EscapeXml(value);
		}

		private string EscapeXml(string text) {
			if(this.EscapeXmlText) {
				return text
					.Replace("&", "&amp;")
					.Replace("<", "&lt;")
					.Replace(">", "&gt;")
				;
			}
			return text;
		}
	}
}
