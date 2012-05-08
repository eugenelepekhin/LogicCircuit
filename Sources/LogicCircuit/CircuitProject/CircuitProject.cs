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
			try {
				XmlReader xmlReader = XmlHelper.CreateReader((file != null ?
					(TextReader) new StreamReader(file) :
					(TextReader) new StringReader(
						string.Format(CultureInfo.InvariantCulture, Schema.Empty,
							Guid.NewGuid(), //ProjectId
							Resources.CircuitProjectName,
							Guid.NewGuid(), //LogicalCircuitId
							Resources.LogicalCircuitMainName,
							Resources.LogicalCircuitMainNotation
						)
					)
				));
				return CircuitProject.CreateAndClose(xmlReader);
			} catch(XmlException xmlException) {
				throw new CircuitException(Cause.CorruptedFile, xmlException, Resources.ErrorFileCorrupted(file));
			}
		}

		private static CircuitProject CreateAndClose(XmlReader xmlReader) {
			try {
				CircuitProject.Transform(ref xmlReader);    // may close original xmlReader and return another one instead.

				CircuitProject circuitProject = new CircuitProject();
				circuitProject.InTransaction(() => circuitProject.Load(xmlReader));
				circuitProject.CircuitSymbolSet.ValidateAll();
				return circuitProject;
			} finally {
				// Don't use using here. Transform may close original XmlReader and open new one.
				xmlReader.Close();
			}
		}

		public void Save(TextWriter textWriter) {
			using(XmlWriter writer = XmlHelper.CreateWriter(textWriter)) {
				this.SaveRecords(writer);
			}
		}

		private void Load(XmlReader xmlReader) {
			// Load all records from the XML file
			this.LoadRecords(xmlReader);

			// Post process loaded records
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
			this.DistinctSymbol(this.LedMatrixSet);
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
			CircuitProject copy = new CircuitProject();
			bool started = copy.StartTransaction();
			Tracer.Assert(started);
			copy.ProjectSet.Copy(this.ProjectSet.Project);
			LogicalCircuit target = copy.LogicalCircuitSet.Copy(this.ProjectSet.Project.LogicalCircuit, false);
			foreach(Symbol s in symbol) {
				s.CopyTo(target);
			}
			StringBuilder sb = new StringBuilder();
			using (TextWriter textWriter = new StringWriter(sb, CultureInfo.InvariantCulture)) {
				copy.Save(textWriter);
			}
			return sb.ToString();
		}

		public static bool CanPaste(string text) {
			// To reduce number of exceptions from XML reader lets check there if some familiar text is in the string.
			// Lets not use namespace as it will prevent pasting of old versions of the XML.
			if (!string.IsNullOrEmpty(text) && text.Contains("<lc:CircuitProject")) {
				try {
					using (XmlReader xmlReader = XmlHelper.CreateReader(new StringReader(text))) {
						string rootName = xmlReader.NameTable.Add("CircuitProject");
						while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()); // skip to the first element
						return (
							XmlHelper.AreEqualAtoms(rootName, xmlReader.LocalName) &&          // We are at CircuitProject element
							CircuitProject.FindTransformation(xmlReader.NamespaceURI) != null  // and it is in known namespace
						);
					}
				} catch {}
			}
			return false;
		}

		public IEnumerable<Symbol> Paste(string text) {
			CircuitProject paste = null;
			try {
				paste = CircuitProject.CreateAndClose(XmlHelper.CreateReader(new StringReader(text)));
			} catch(XmlException xmlException) {
				throw new CircuitException(Cause.CorruptedFile, xmlException, Resources.ErrorClipboardCorrupted);
			}

			List<Symbol> result = new List<Symbol>();
			this.InTransaction(() => {
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
			});
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
				} else if(circuit is LedMatrix) {
					Tracer.Assert(this.LedMatrixSet.Table.Exists(LedMatrixData.LedMatrixIdField.Field, circuit.CircuitId));
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
			foreach(LedMatrix ledMatrix in this.LedMatrixSet) {
				int count = this.DevicePinSet.SelectByCircuit(ledMatrix).Count();
				if(ledMatrix.MatrixType == LedMatrixType.Individual) {
					Tracer.Assert(ledMatrix.Rows == count);
				} else {
					Tracer.Assert((ledMatrix.Rows + ledMatrix.Columns) == count);
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

		/// <summary>
		/// By giving XML namespace finds the XSLT that transforms CurcuitProject of given version to the next version
		/// </summary>
		/// <param name="ns">XML namespace that defines the project version</param>
		/// <returns>
		/// String.Empty -- when ns is for current version and no transformation is required
		/// XSLT string  -- when ns is for know previous version
		/// null         -- when ns is unknown
		/// </returns>
		private static string FindTransformation(string ns) {
			StringComparer cmp = StringComparer.OrdinalIgnoreCase;

			if(cmp.Equals(ns, CircuitProject.PersistenceNamespace)) return string.Empty;

			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.2/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_2;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.1/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_1;
			if(cmp.Equals(ns, "http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd")) return Schema.ConvertFrom_1_0_0_3;
			if(cmp.Equals(ns, "http://LogicCircuit.net/1.0.0.2/CircuitProject.xsd")) return Schema.ConvertFrom_1_0_0_2;

			return null;
		}

		// Transform may close input reader and replace it with a new one.
		// To emphasize this we pass xmlReader by ref.s
		private static void Transform(ref XmlReader xmlReader) {
			for(;;) {
				while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()); // skip to the first element

				string xslt = CircuitProject.FindTransformation(xmlReader.NamespaceURI);
				if (xslt == null) {
					throw new CircuitException(Cause.UnknownVersion, Resources.ErrorUnknownVersion);
				}
				if (xslt.Length == 0) {
					// No transform needed. We are at the current version.
					return;
				}

				XmlHelper.Transform(xslt, ref xmlReader);
			}
		}
	}
}
