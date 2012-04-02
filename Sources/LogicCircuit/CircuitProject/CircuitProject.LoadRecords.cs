﻿using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using LogicCircuit.DataPersistent;
using System;

namespace LogicCircuit {
	public abstract class ARecordLoader {
		public abstract void LoadOneRecord(XmlReader reader);
	}

	public class RecordLoader<TRecord> : ARecordLoader where TRecord:struct {
		private readonly TableSnapshot<TRecord> table;
		private readonly Action<RowId> register;
		private Dictionary<string, IFieldSerializer<TRecord>> serializers = new Dictionary<string, IFieldSerializer<TRecord>>(XmlHelper.AtomComparier);

		public RecordLoader(XmlNameTable nameTable, TableSnapshot<TRecord> table, IEnumerable<IField<TRecord>> fields, Action<RowId> register) {
			Debug.Assert(nameTable != null);
			Debug.Assert(fields != null);
			Debug.Assert(register != null);

			this.table = table;
			this.register = register;
			foreach (IField<TRecord> field in fields) {
				IFieldSerializer<TRecord> serializer = field as IFieldSerializer<TRecord>;
				if (serializer != null) {
					string fieldName = nameTable.Add(field.Name);
					serializers.Add(fieldName, serializer);
				}
			}
		}

		public override void LoadOneRecord(XmlReader reader) {
			Debug.Assert(reader.NodeType == XmlNodeType.Element);

			TRecord data = new TRecord();
			// Initialize 'data' with default values:
			foreach(IFieldSerializer<TRecord> serializer in serializers.Values) {
				Debug.Assert(serializer != null);
				serializer.SetDefault(ref data);
			}

			if (! reader.IsEmptyElement) {
				string elementName = reader.LocalName;

				reader.Read();
				int fieldDepth = reader.Depth;
				string ns = reader.NamespaceURI;

				while (reader.Depth == fieldDepth) {
					if (reader.IsElement(ns)) {
						// The reader is positioned on a field element
						string fieldName  = reader.LocalName;
						string fieldValue = reader.ReadElementText();  // reads the text and moves the reader beyond this element
						IFieldSerializer<TRecord> serializer;
						if (serializers.TryGetValue(fieldName, out serializer)) {
							Debug.Assert(serializer != null);
							serializer.SetTextValue(ref data, fieldValue);
						}
					} else {
						reader.Skip();  // skip everything else
					}
				}
				Debug.Assert(reader.IsEndElement(ns, elementName));
				Debug.Assert(reader.Depth == fieldDepth - 1);
			}

			// insert 'data' into the table
			RowId rowId = table.Insert(ref data);

			register(rowId);
		}
	}

	public partial class CircuitProject {
		private void LoadRecords(XmlReader xmlReader) {
			XmlNameTable nameTable = xmlReader.NameTable;
			string ns = nameTable.Add(CircuitProject.PersistenceNamespace);
			string rootName = nameTable.Add("CircuitProject");

			Dictionary<string, ARecordLoader> loaders = new Dictionary<string, ARecordLoader>(16, XmlHelper.AtomComparier);
			loaders.Add(nameTable.Add(this.ProjectSet          .Table.Name), this.ProjectSet          .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CollapsedCategorySet.Table.Name), this.CollapsedCategorySet.CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.LogicalCircuitSet   .Table.Name), this.LogicalCircuitSet   .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.PinSet              .Table.Name), this.PinSet              .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.ConstantSet         .Table.Name), this.ConstantSet         .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CircuitButtonSet    .Table.Name), this.CircuitButtonSet    .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.MemorySet           .Table.Name), this.MemorySet           .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.LedMatrixSet        .Table.Name), this.LedMatrixSet        .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.SplitterSet         .Table.Name), this.SplitterSet         .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CircuitSymbolSet    .Table.Name), this.CircuitSymbolSet    .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.WireSet             .Table.Name), this.WireSet             .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.TextNoteSet         .Table.Name), this.TextNoteSet         .CreateRecordLoader(nameTable));

			// skip to the first element
			while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()) ;
			Debug.Assert(xmlReader.Depth == 0);
			if (xmlReader.IsElement(ns, rootName)) {
				bool isEmpty = xmlReader.IsEmptyElement;
				xmlReader.Read();
				if (! isEmpty) {
					Debug.Assert(xmlReader.Depth == 1);
					while (xmlReader.Depth == 1) {
						if (xmlReader.IsElement(ns)) {
							ARecordLoader loader;
							if (loaders.TryGetValue(xmlReader.LocalName, out loader)) {
								Debug.Assert(loader != null);
								loader.LoadOneRecord(xmlReader);
								continue;
							}
						}
						xmlReader.Skip();
					}
					Debug.Assert(xmlReader.Depth == 0);
					#if DEBUG
						Debug.Assert(xmlReader.IsEndElement(ns, rootName));
					#endif
				}
			}
		}
	}
}