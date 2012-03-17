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

		private static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() {
			CloseOutput = true,
			Indent = true
		};

		public static XmlReader CreateReader(TextReader textReader) {			
			return XmlReader.Create(textReader, xmlReaderSettings);
		}

		public static XmlWriter CreateWriter(TextWriter textWriter) {
			return XmlWriter.Create(textWriter, xmlWriterSettings);
		}

		// Transform may close input reader and replace it with new one.
		// To emphasize this we pass xmlReader by ref.
		public static void Transform(string xsltText, ref XmlReader inputXml) {
			XslCompiledTransform xslt = new XslCompiledTransform();
			using(StringReader stringReader = new StringReader(xsltText)) {
				using(XmlTextReader xmlTextReader = new XmlTextReader(stringReader)) {
					xslt.Load(xmlTextReader);
				}
			}

			// To get the results if XSLT transformation in form of XmlReader we are writing the output to string 
			// and them creating XmlReader to parse this string.
			StringBuilder sb = new StringBuilder();
			using(XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb, CultureInfo.InvariantCulture))) {
				xslt.Transform(inputXml, writer);
			}
			// closing this reader and create another one to use instead
			inputXml.Close();                   
			inputXml = XmlHelper.CreateReader(new StringReader(sb.ToString()));
		}

		public static bool IsElement(this XmlReader xmlReader, string ns, string localName = null) {
			return (
				xmlReader.NodeType == XmlNodeType.Element && 
				AreEqualAtoms(xmlReader.NamespaceURI, ns) && 
				(localName == null || AreEqualAtoms(xmlReader.LocalName, localName))
			);
		}

		#if DEBUG
			public static bool IsEndElement(this XmlReader xmlReader, string ns, string localName = null) {
				return (
					xmlReader.NodeType == XmlNodeType.EndElement && 
					AreEqualAtoms(xmlReader.NamespaceURI, ns) && 
					(localName == null || AreEqualAtoms(xmlReader.LocalName, localName))
				);
			}
		#endif

		public static bool AreEqualAtoms(string x, string y) {
			Debug.Assert(x != y || object.ReferenceEquals(x, y),
				"Atomization problem. You forgot to atomize string: '" + x + "'"
			);
			return object.ReferenceEquals(x, y);
		}
	}
}
