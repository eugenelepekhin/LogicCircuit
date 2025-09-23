﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LogicCircuit {
	internal static class XmlHelper {
		private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings() {
			CloseInput = true,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			IgnoreWhitespace = true,
			DtdProcessing = DtdProcessing.Prohibit, // we don't use DTD. Let's prohibit it for better security
			XmlResolver = null // no external resources are allowed
		};

		private static readonly XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() {
			CloseOutput = true,
			Indent = true,
			IndentChars = "\t"
		};

		public static XmlReader CreateReader(TextReader textReader) {
			return XmlReader.Create(textReader, XmlHelper.xmlReaderSettings);
		}

		public static XmlDocument Create() {
			return new XmlDocument() { XmlResolver = XmlResolver.ThrowingResolver };
		}

		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static XmlDocument LoadXml(string text) {
			using(StringReader reader = new StringReader(text)) {
				using(XmlReader xmlReader = XmlHelper.CreateReader(reader)) {
					XmlDocument xml = XmlHelper.Create();
					xml.Load(xmlReader);
					return xml;
				}
			}
		}

		public static TextWriter FileWriter(string fileName) {
			if(!File.Exists(fileName)) {
				string? dir = Path.GetDirectoryName(fileName);
				if(!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}
			}
			return new StreamWriter(fileName, false, Encoding.UTF8);
		}

		public static XmlWriter CreateWriter(TextWriter textWriter) {
			return XmlWriter.Create(textWriter, XmlHelper.xmlWriterSettings);
		}

		// Transform will close input reader and replace it with new one.
		// To emphasize this we pass xmlReader by ref.
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static void Transform(string xsltText, ref XmlReader inputXml) {
			XslCompiledTransform xslt = new XslCompiledTransform();
			using(StringReader stringReader = new StringReader(xsltText)) {
				using(XmlReader xmlTextReader = XmlHelper.CreateReader(stringReader)) {
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

		public static bool IsElement(this XmlReader xmlReader, string ns, string? localName = null) {
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
				reader.Read(); // descend to the first child
				result = string.Empty;

				// Read and concatenate all text nodes. Skip inner elements and their ends.
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
			public static bool IsEndElement(this XmlReader xmlReader, string ns, string? localName = null) {
				return (
					xmlReader.NodeType == XmlNodeType.EndElement &&
					XmlHelper.AreEqualAtoms(xmlReader.NamespaceURI, ns) &&
					(localName == null || XmlHelper.AreEqualAtoms(xmlReader.LocalName, localName))
				);
			}
		#endif

		public static bool AreEqualAtoms(string? x, string? y) {
			Debug.Assert(x != y || object.ReferenceEquals(x, y),
				"Atomization problem. You forgot to atomize string: '" + x + "'"
			);
			return object.ReferenceEquals(x, y);
		}

		private sealed class AtomEqualityComparer : IEqualityComparer<string> {
			public bool Equals(string? x, string? y) {
				return XmlHelper.AreEqualAtoms(x, y);
			}

			public int GetHashCode(string obj) {
				return obj.GetHashCode(StringComparison.Ordinal);
			}
		}

		public static readonly IEqualityComparer<string> AtomComparer = new AtomEqualityComparer();
	}
}
