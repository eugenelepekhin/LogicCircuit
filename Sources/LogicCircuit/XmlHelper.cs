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
		private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings() {
			CloseInput = true,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			IgnoreWhitespace = true,
			DtdProcessing = DtdProcessing.Prohibit,       // we don't use DTD. Let's prohibit it for better security
		};

		private static readonly XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() {
			CloseOutput = true,
			Indent = true,
			IndentChars = "\t"
		};

		public static XmlReader CreateReader(TextReader textReader) {
			return XmlReader.Create(textReader, XmlHelper.xmlReaderSettings);
		}

		public static XmlWriter CreateWriter(TextWriter textWriter) {
			return XmlWriter.Create(textWriter, XmlHelper.xmlWriterSettings);
		}

		// Transform will close input reader and replace it with new one.
		// To emphasize this we pass xmlReader by ref.
		public static void Transform(string xsltText, ref XmlReader inputXml) {
			XslCompiledTransform xslt = new XslCompiledTransform();
			using(StringReader stringReader = new StringReader(xsltText)) {
				using(XmlTextReader xmlTextReader = new XmlTextReader(stringReader)) {
					xslt.Load(xmlTextReader);
				}
			}

			// To get the results of XSLT transformation in form of XmlReader we are writing the output to string
			// and then creating XmlReader to parse this string.
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
				XmlHelper.AreEqualAtoms(xmlReader.NamespaceURI, ns) &&
				(localName == null || XmlHelper.AreEqualAtoms(xmlReader.LocalName, localName))
			);
		}

		public static string ReadElementText(this XmlReader reader) {
			Debug.Assert(reader.NodeType == XmlNodeType.Element);
			string result;
			if (reader.IsEmptyElement) {
				result = string.Empty;
			} else {
				int fieldDepth = reader.Depth;
				reader.Read();                        // descend to the first child
				result = "";

				// Read and concatenate all text nodes. Skip inner elements and there ends.
				while (fieldDepth < reader.Depth) {
					if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement) {
						reader.Read();
					}
					result += reader.ReadContentAsString();
				}
				// Find ourselves on the EndElement tag.
				Debug.Assert(reader.Depth == fieldDepth);
				Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
			}

			// Skip EndElement or empty element.
			reader.Read();
			return result;
		}

		#if DEBUG
			public static bool IsEndElement(this XmlReader xmlReader, string ns, string localName = null) {
				return (
					xmlReader.NodeType == XmlNodeType.EndElement &&
					XmlHelper.AreEqualAtoms(xmlReader.NamespaceURI, ns) &&
					(localName == null || XmlHelper.AreEqualAtoms(xmlReader.LocalName, localName))
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
