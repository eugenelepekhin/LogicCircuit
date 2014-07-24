using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using LogicCircuit.DataPersistent;
using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	public interface IRecordLoader {
		void LoadRecord(XmlReader reader);
	}

	public class RecordLoader<TRecord> : IRecordLoader where TRecord:struct {
		private readonly TableSnapshot<TRecord> table;
		private readonly Action<RowId> register;
		private Dictionary<string, IFieldSerializer<TRecord>> serializers;

		public RecordLoader(XmlNameTable nameTable, TableSnapshot<TRecord> table, Action<RowId> register) {
			Debug.Assert(nameTable != null);
			Debug.Assert(register != null);

			this.table = table;
			this.register = register;
			this.serializers = new Dictionary<string, IFieldSerializer<TRecord>>(XmlHelper.AtomComparer);
			foreach (IField<TRecord> field in this.table.Fields) {
				IFieldSerializer<TRecord> serializer = field as IFieldSerializer<TRecord>;
				if (serializer != null) {
					string fieldName = nameTable.Add(field.Name);
					this.serializers.Add(fieldName, serializer);
				}
			}
		}

		public void LoadRecord(XmlReader reader) {
			Debug.Assert(reader.IsElement(reader.NamespaceURI, this.table.Name));

			TRecord data = new TRecord();
			// Initialize 'data' with default values:
			foreach(IFieldSerializer<TRecord> serializer in this.serializers.Values) {
				Debug.Assert(serializer != null);
				serializer.SetDefault(ref data);
			}

			if (! reader.IsEmptyElement) {
				reader.Read();
				int fieldDepth = reader.Depth;
				string ns = reader.NamespaceURI;

				while (reader.Depth == fieldDepth) {
					if (reader.IsElement(ns)) {
						// The reader is positioned on a field element
						string fieldName  = reader.LocalName;
						string fieldValue = reader.ReadElementText();  // reads the text and moves the reader beyond this element
						IFieldSerializer<TRecord> serializer;
						if (this.serializers.TryGetValue(fieldName, out serializer)) {
							Debug.Assert(serializer != null);
							serializer.SetTextValue(ref data, fieldValue);
						}
					} else {
						reader.Skip();  // skip everything else
					}
				}
				#if DEBUG
					Debug.Assert(reader.IsEndElement(ns, this.table.Name));
				#endif
				Debug.Assert(reader.Depth == fieldDepth - 1);
			}

			// insert 'data' into the table
			RowId rowId = this.table.Insert(ref data);

			this.register(rowId);
		}
	}

	public partial class CircuitProject {
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void LoadRecords(XmlReader xmlReader) {
			XmlNameTable nameTable = xmlReader.NameTable;
			string ns = nameTable.Add(CircuitProject.PersistenceNamespace);
			string rootName = nameTable.Add("CircuitProject");

			Dictionary<string, IRecordLoader> loaders = new Dictionary<string, IRecordLoader>(16, XmlHelper.AtomComparer);
			loaders.Add(nameTable.Add(this.ProjectSet          .Table.Name), this.ProjectSet          .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CollapsedCategorySet.Table.Name), this.CollapsedCategorySet.CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.LogicalCircuitSet   .Table.Name), this.LogicalCircuitSet   .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.PinSet              .Table.Name), this.PinSet              .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CircuitProbeSet     .Table.Name), this.CircuitProbeSet     .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.ConstantSet         .Table.Name), this.ConstantSet         .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CircuitButtonSet    .Table.Name), this.CircuitButtonSet    .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.MemorySet           .Table.Name), this.MemorySet           .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.LedMatrixSet        .Table.Name), this.LedMatrixSet        .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.SplitterSet         .Table.Name), this.SplitterSet         .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.SensorSet           .Table.Name), this.SensorSet           .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.CircuitSymbolSet    .Table.Name), this.CircuitSymbolSet    .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.WireSet             .Table.Name), this.WireSet             .CreateRecordLoader(nameTable));
			loaders.Add(nameTable.Add(this.TextNoteSet         .Table.Name), this.TextNoteSet         .CreateRecordLoader(nameTable));

			// skip to the first element
			while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read());
			Debug.Assert(xmlReader.Depth == 0);
			if (xmlReader.IsElement(ns, rootName)) {
				bool isEmpty = xmlReader.IsEmptyElement;
				xmlReader.Read();
				if (! isEmpty) {
					Debug.Assert(xmlReader.Depth == 1);
					while (xmlReader.Depth == 1) {
						if (xmlReader.IsElement(ns)) {
							IRecordLoader loader;
							if (loaders.TryGetValue(xmlReader.LocalName, out loader)) {
								Debug.Assert(loader != null);
								loader.LoadRecord(xmlReader);
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

		private void SaveRecords(XmlWriter xmlWriter) {
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement(CircuitProject.PersistencePrefix, "CircuitProject", CircuitProject.PersistenceNamespace);
			CircuitProject.SaveRecords(xmlWriter, this.ProjectSet.Table          );
			CircuitProject.SaveRecords(xmlWriter, this.CollapsedCategorySet.Table);
			CircuitProject.SaveRecords(xmlWriter, this.LogicalCircuitSet.Table   );
			CircuitProject.SaveRecords(xmlWriter, this.PinSet.Table              );
			CircuitProject.SaveRecords(xmlWriter, this.CircuitProbeSet.Table     );
			CircuitProject.SaveRecords(xmlWriter, this.ConstantSet.Table         );
			CircuitProject.SaveRecords(xmlWriter, this.CircuitButtonSet.Table    );
			CircuitProject.SaveRecords(xmlWriter, this.MemorySet.Table           );
			CircuitProject.SaveRecords(xmlWriter, this.LedMatrixSet.Table        );
			CircuitProject.SaveRecords(xmlWriter, this.SplitterSet.Table         );
			CircuitProject.SaveRecords(xmlWriter, this.SensorSet.Table           );
			CircuitProject.SaveRecords(xmlWriter, this.CircuitSymbolSet.Table    );
			CircuitProject.SaveRecords(xmlWriter, this.WireSet.Table             );
			CircuitProject.SaveRecords(xmlWriter, this.TextNoteSet.Table         );
			xmlWriter.WriteEndElement();
		}

		// Saves the table
		private static void SaveRecords<TRecord>(XmlWriter writer, TableSnapshot<TRecord> table) where TRecord:struct {
			foreach(RowId rowId in table.Rows) {
				TRecord data;
				table.GetData(rowId, out data);
				writer.WriteStartElement(table.Name, CircuitProject.PersistenceNamespace);
				foreach(IField<TRecord> field in table.Fields) {
					IFieldSerializer<TRecord> serializer = field as IFieldSerializer<TRecord>;
					if(serializer != null && serializer.NeedToSave(ref data)) {
						writer.WriteStartElement(field.Name, CircuitProject.PersistenceNamespace);
						writer.WriteString(serializer.GetTextValue(ref data));
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
		}
	}
}