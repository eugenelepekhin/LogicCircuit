using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Diagnostics;

namespace LogicCircuit {
	internal static class XmlHelper {
		private static XmlReaderSettings xmlReaderSettings = new XmlReaderSettings() {
			CloseInput = true,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			IgnoreWhitespace = true,
			DtdProcessing = DtdProcessing.Prohibit,       // we don't use DTD. Let's prohibit it for better security
		};

		public static XmlReader ReadFromString(string xmlText) {			
			return XmlReader.Create(new StringReader(xmlText), xmlReaderSettings);
		}

		public static XmlReader ReadFromFile(string fileName) {
			return XmlReader.Create(fileName, xmlReaderSettings);
		}

		public static void Transform(string xsltText, ref XmlReader inputXml) {
			XslCompiledTransform xslt = new XslCompiledTransform();
			using(StringReader stringReader = new StringReader(xsltText)) {
				using(XmlTextReader xmlTextReader = new XmlTextReader(stringReader)) {
					xslt.Load(xmlTextReader);
				}
			}

			// To get the results if XSLT transformation in form of XmlReader we are writing the output to string 
			// and them creating XmlReader to parse this string.
			using(StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
				using(XmlTextWriter writer = new XmlTextWriter(stringWriter)) {
					xslt.Transform(inputXml, writer);
				}
				// closing this reader and create another one to use instead
				inputXml.Close();                   
				inputXml = ReadFromString(stringWriter.ToString());
			}
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

		public static bool IsElement(this XmlReader xmlReader, string ns, string localName = null) {
			return (
				xmlReader.NodeType == XmlNodeType.Element && 
				AreEqualAtoms(xmlReader.NamespaceURI, ns) && 
				(localName == null || AreEqualAtoms(xmlReader.LocalName, localName))
			);
		}

		public static bool IsEndElement(this XmlReader xmlReader, string ns, string localName = null) {
			return (
				xmlReader.NodeType == XmlNodeType.EndElement && 
				AreEqualAtoms(xmlReader.NamespaceURI, ns) && 
				(localName == null || AreEqualAtoms(xmlReader.LocalName, localName))
			);
		}

		public static bool AreEqualAtoms(string x, string y) {
			Debug.Assert(x != y || object.ReferenceEquals(x, y),
				"Atomization problem. You forgot to atomize string: '" + x + "'"
			);
			return object.ReferenceEquals(x, y);
		}
	}
}
