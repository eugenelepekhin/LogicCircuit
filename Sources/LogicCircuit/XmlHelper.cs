using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LogicCircuit {
	internal static class XmlHelper {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static XmlDocument Transform(XmlDocument xml, string xsltText) {
			XslCompiledTransform xslt = new XslCompiledTransform();
			using(StringReader stringReader = new StringReader(xsltText)) {
				using(XmlTextReader xmlTextReader = new XmlTextReader(stringReader)) {
					xslt.Load(xmlTextReader);
				}
			}
			XmlDocument result = new XmlDocument();
			using(XmlNodeReader reader = new XmlNodeReader(xml.DocumentElement)) {
				using(StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
					using(XmlTextWriter writer = new XmlTextWriter(stringWriter)) {
						xslt.Transform(reader, writer);
					}
					result.LoadXml(stringWriter.ToString());
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static void Save(XmlDocument xml, string file) {
			if(!File.Exists(file)) {
				string dir = Path.GetDirectoryName(file);
				if(!Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}
			}
			using(XmlTextWriter writer = new XmlTextWriter(file, Encoding.UTF8)) {
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 1;
				writer.IndentChar = '\t';
				xml.Save(writer);
				writer.Close();
			}
		}
	}
}
