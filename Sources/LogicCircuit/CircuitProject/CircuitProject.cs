using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LogicCircuit.DataPersistent;
using System.Diagnostics;
using System.IO;

namespace LogicCircuit {
	public partial class CircuitProject {
		private TableSnapshot<CircuitData> circuitTable;
		internal TableSnapshot<CircuitData> CircuitTable {
			get {
				if(this.circuitTable == null) {
					this.circuitTable = (TableSnapshot<CircuitData>)this.Table("Circuit");
				}
				return this.circuitTable;
			}
		}

		public static CircuitProject Create(string file) {
			XmlReader xmlReader;
			if(file != null) {
				xmlReader = XmlHelper.ReadFromFile(file);
			} else {
				string newEmptyProject = string.Format(CultureInfo.InvariantCulture, Schema.Empty,
					Guid.NewGuid(), //ProjectId
					Resources.CircuitProjectName,
					Guid.NewGuid(), //LogicalCircuitId
					Resources.LogicalCircuitMainName,
					Resources.LogicalCircuitMainNotation
				);
				xmlReader = XmlHelper.ReadFromString(newEmptyProject);
			}

			try {
				xmlReader = CircuitProject.Transform(xmlReader);

				CircuitProject circuitProject = new CircuitProject();
				circuitProject.InTransaction(() => circuitProject.Load(xmlReader));
				circuitProject.CircuitSymbolSet.ValidateAll();
				return circuitProject;
			} finally {
				xmlReader.Close();
			}
		}

		public void Save(TextWriter textWriter) {
			using(XmlWriter writer = XmlHelper.CreateWriter(textWriter)) {
				writer.WriteStartDocument();
				writer.WriteStartElement(CircuitProject.PersistencePrefix, "CircuitProject", CircuitProject.PersistenceNamespace);
				ProjectData.Save(this.ProjectSet.Table, writer, CircuitProject.PersistenceNamespace);
				CollapsedCategoryData.Save(this.CollapsedCategorySet.Table, writer, CircuitProject.PersistenceNamespace);
				LogicalCircuitData.Save(this.LogicalCircuitSet.Table, writer, CircuitProject.PersistenceNamespace);
				PinData.Save(this.PinSet.Table, writer, CircuitProject.PersistenceNamespace);
				ConstantData.Save(this.ConstantSet.Table, writer, CircuitProject.PersistenceNamespace);
				CircuitButtonData.Save(this.CircuitButtonSet.Table, writer, CircuitProject.PersistenceNamespace);
				MemoryData.Save(this.MemorySet.Table, writer, CircuitProject.PersistenceNamespace);
				SplitterData.Save(this.SplitterSet.Table, writer, CircuitProject.PersistenceNamespace);
				CircuitSymbolData.Save(this.CircuitSymbolSet.Table, writer, CircuitProject.PersistenceNamespace);
				WireData.Save(this.WireSet.Table, writer, CircuitProject.PersistenceNamespace);
				TextNoteData.Save(this.TextNoteSet.Table, writer, CircuitProject.PersistenceNamespace);
				writer.WriteEndElement();
			}
		}

		private class AtomizedComparator : IEqualityComparer<string> {
			public bool Equals(string x, string y) {
				return XmlHelper.AreEqualAtoms(x, y);
			}

			public int GetHashCode(string obj) {
				return obj.GetHashCode();
			}
		}

		private void Load(XmlReader xmlReader) {
			XmlNameTable nameTable = xmlReader.NameTable;
			string ns = nameTable.Add(CircuitProject.PersistenceNamespace);
			string rootName = nameTable.Add("CircuitProject");

			Dictionary<string, IRecordLoader> loaders = new Dictionary<string, IRecordLoader>(12, new AtomizedComparator());
			loaders.Add(nameTable.Add(this.ProjectSet          .Table.Name), (IRecordLoader) this.ProjectSet          );
			loaders.Add(nameTable.Add(this.CollapsedCategorySet.Table.Name), (IRecordLoader) this.CollapsedCategorySet);
			loaders.Add(nameTable.Add(this.LogicalCircuitSet   .Table.Name), (IRecordLoader) this.LogicalCircuitSet   );
			loaders.Add(nameTable.Add(this.PinSet              .Table.Name), (IRecordLoader) this.PinSet              );
			loaders.Add(nameTable.Add(this.ConstantSet         .Table.Name), (IRecordLoader) this.ConstantSet         );
			loaders.Add(nameTable.Add(this.CircuitButtonSet    .Table.Name), (IRecordLoader) this.CircuitButtonSet    );
			loaders.Add(nameTable.Add(this.MemorySet           .Table.Name), (IRecordLoader) this.MemorySet           );
			loaders.Add(nameTable.Add(this.SplitterSet         .Table.Name), (IRecordLoader) this.SplitterSet         );
			loaders.Add(nameTable.Add(this.CircuitSymbolSet    .Table.Name), (IRecordLoader) this.CircuitSymbolSet    );
			loaders.Add(nameTable.Add(this.WireSet             .Table.Name), (IRecordLoader) this.WireSet             );
			loaders.Add(nameTable.Add(this.TextNoteSet         .Table.Name), (IRecordLoader) this.TextNoteSet         );

			// skip to the first element
			while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()) ;
			Debug.Assert(xmlReader.Depth == 0);
			if (xmlReader.IsElement(ns, rootName)) {
				bool isEmpty = xmlReader.IsEmptyElement;
				xmlReader.Read();
				if (! isEmpty) {
					Debug.Assert(xmlReader.Depth == 1);
					while (xmlReader.Depth == 1) {
						if (xmlReader.IsElement(ns) && ! xmlReader.IsEmptyElement) {
							IRecordLoader loader;
							if (loaders.TryGetValue(xmlReader.LocalName, out loader)) {
								loader.Load(xmlReader); 
								continue; 
							}
						}
						xmlReader.Skip();
					}
					Debug.Assert(xmlReader.Depth == 0);
					Debug.Assert(xmlReader.IsEndElement(ns, rootName));
				}
			}

			this.CollapsedCategorySet.Purge();

			foreach(CircuitSymbol symbol in this.CircuitSymbolSet) {
				Guid circuitId = this.CircuitSymbolSet.Table.GetField(symbol.CircuitSymbolRowId, CircuitSymbolData.CircuitIdField.Field);
				if(this.CircuitSet.Find(circuitId) == null) {
					this.GateSet.Gate(circuitId);
				}
			}

			this.DistinctSymbol(this.CircuitButtonSet);
			this.DistinctSymbol(this.ConstantSet);
			this.DistinctSymbol(this.MemorySet);
			this.DistinctSymbol(this.PinSet);
			this.DistinctSymbol(this.SplitterSet);

			this.WireSet.Where(wire => wire.Point1 == wire.Point2).ToList().ForEach(wire => wire.Delete());
			this.TextNoteSet.Where(textNote => !textNote.IsValid).ToList().ForEach(textNote => textNote.Delete());

			this.ResetUndoRedo();
		}

		private void DistinctSymbol(IEnumerable<Circuit> set) {
			set.Where(circuit => !this.CircuitSymbolSet.SelectByCircuit(circuit).Any()).ToList().ForEach(circuit => circuit.Delete());
			foreach(Circuit circuit in set) {
				List<CircuitSymbol> list = this.CircuitSymbolSet.SelectByCircuit(circuit).ToList();
				for(int i = 1; i < list.Count; i++) {
					list[i].Delete();
				}
			}
		}

		public string WriteToString(IEnumerable<Symbol> symbol) {
			StringBuilder sb = new StringBuilder();
			using (TextWriter textWriter = new StringWriter(sb)) {
				CircuitProject copy = new CircuitProject();
				bool started = copy.StartTransaction();
				Tracer.Assert(started);
				copy.ProjectSet.Copy(this.ProjectSet.Project);
				LogicalCircuit target = copy.LogicalCircuitSet.Copy(this.ProjectSet.Project.LogicalCircuit, false);
				foreach(Symbol s in symbol) {
					s.CopyTo(target);
				}
				copy.Save(textWriter);
			}
			return sb.ToString();
		}

		public static bool CanPaste(string text) {
			if (string.IsNullOrEmpty(text)) {
				return true;
			}
			try {
				using (XmlReader xmlReader = XmlHelper.ReadFromString(text)) {
					string rootName = xmlReader.NameTable.Add("CircuitProject");
					string ns       = xmlReader.NameTable.Add(CircuitProject.PersistenceNamespace);
					while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()) ;        // skip to the first element
					return xmlReader.IsElement(ns, rootName);
				}
			} catch {}
			return false;
		}

		public IEnumerable<Symbol> Paste(XmlReader xmlReader) {
			CircuitProject paste = new CircuitProject();
			bool started = paste.StartTransaction();
			Tracer.Assert(started);
			paste.Load(xmlReader);

			List<Symbol> result = new List<Symbol>();

			LogicalCircuit target = this.ProjectSet.Project.LogicalCircuit;
			foreach(CircuitSymbol symbol in paste.ProjectSet.Project.LogicalCircuit.CircuitSymbols()) {
				result.Add(symbol.CopyTo(target));
			}
			foreach(Wire wire in paste.ProjectSet.Project.LogicalCircuit.Wires()) {
				result.Add(wire.CopyTo(target));
			}
			foreach(TextNote symbol in paste.ProjectSet.Project.LogicalCircuit.TextNotes()) {
				if(symbol.IsValid) {
					result.Add(symbol.CopyTo(target));
				}
			}

			if(0 < result.Count && paste.ProjectSet.Project.LogicalCircuit.LogicalCircuitId == target.LogicalCircuitId) {
				foreach(Symbol symbol in result) {
					symbol.Shift(2, 2);
				}
			}
			return result;
		}

		private void InTransaction(Action action, bool commit) {
			bool success = false;
			try {
				if(this.StartTransaction()) {
					action();
					this.PrepareCommit();
					this.ValidateCircuitProject();
					success = true;
				}
			} finally {
				if(this.IsEditor) {
					if(success) {
						if(commit && this.Tables.Any(t => t.WasAffected(this.Version))) {
							this.Commit();
						} else {
							this.Omit();
						}
					} else {
						this.Rollback();
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[Conditional("DEBUG")]
		private void ValidateCircuitProject() {
			foreach(Circuit circuit in this.CircuitSet) {
				if(circuit is LogicalCircuit) {
					Tracer.Assert(this.LogicalCircuitSet.Table.Exists(LogicalCircuitData.LogicalCircuitIdField.Field, circuit.CircuitId));
				} else if(circuit is CircuitButton) {
					Tracer.Assert(this.CircuitButtonSet.Table.Exists(CircuitButtonData.CircuitButtonIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is Constant) {
					Tracer.Assert(this.ConstantSet.Table.Exists(ConstantData.ConstantIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is DevicePin) {
					Tracer.Assert(this.DevicePinSet.Table.Exists(DevicePinData.PinIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 0);
				} else if(circuit is Gate) {
					Tracer.Assert(this.GateSet.Table.Exists(GateData.GateIdField.Field, circuit.CircuitId));
				} else if(circuit is Memory) {
					Tracer.Assert(this.MemorySet.Table.Exists(MemoryData.MemoryIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is Pin) {
					Tracer.Assert(this.PinSet.Table.Exists(PinData.PinIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
					Pin pin = (Pin)circuit;
					Tracer.Assert(pin.LogicalCircuit == this.CircuitSymbolSet.SelectByCircuit(circuit).First().LogicalCircuit);
				} else if(circuit is Splitter) {
					Tracer.Assert(this.SplitterSet.Table.Exists(SplitterData.SplitterIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				}
			}
			foreach(Wire wire in this.WireSet) {
				Tracer.Assert(wire.Point1 != wire.Point2);
			}
			//TODO: Add check of many wires hidden by each other.
		}

		public void InTransaction(Action action) {
			this.InTransaction(action, true);
		}

		public void InOmitTransaction(Action action) {
			this.InTransaction(action, false);
		}

		private static XmlReader Transform(XmlReader xmlReader) {
			StringComparer cmp = StringComparer.OrdinalIgnoreCase;
			do {				
				while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()) ;        // skip to the first element
				string ns = xmlReader.NamespaceURI;

				string xslt;
				if (cmp.Compare(ns, CircuitProject.PersistenceNamespace                 ) == 0) { return xmlReader;                  } else 
				if (cmp.Compare(ns, "http://LogicCircuit.net/2.0.0.2/CircuitProject.xsd") == 0) { xslt = Schema.ConvertFrom_2_0_0_2; } else 
				if (cmp.Compare(ns, "http://LogicCircuit.net/2.0.0.1/CircuitProject.xsd") == 0) { xslt = Schema.ConvertFrom_2_0_0_1; } else 
				if (cmp.Compare(ns, "http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd") == 0) { xslt = Schema.ConvertFrom_1_0_0_3; } else 
				if (cmp.Compare(ns, "http://LogicCircuit.net/1.0.0.2/CircuitProject.xsd") == 0) { xslt = Schema.ConvertFrom_1_0_0_2; } else 
				{ 
					throw new CircuitException(Cause.UnknownVersion, Resources.ErrorUnknownVersion); 
				}
				XmlHelper.Transform(xslt, ref xmlReader);
			} while (true);
		}
	}
}
