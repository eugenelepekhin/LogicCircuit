using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class CircuitProject {
		private TableSnapshot<CircuitData>? circuitTable;
		internal TableSnapshot<CircuitData> CircuitTable {
			get {
				if(this.circuitTable == null) {
					this.circuitTable = (TableSnapshot<CircuitData>?)this.Table("Circuit");
				}
				Debug.Assert(this.circuitTable != null);
				return this.circuitTable;
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static CircuitProject Create(string? file) {
			try {
				#pragma warning disable CA1863 // Use 'CompositeFormat'
				XmlReader xmlReader = XmlHelper.CreateReader((file != null ?
					(TextReader)new StreamReader(file) :
					(TextReader)new StringReader(
						string.Format(CultureInfo.InvariantCulture, Schema.Empty,
							Guid.NewGuid(), //ProjectId
							Properties.Resources.CircuitProjectName,
							Guid.NewGuid(), //LogicalCircuitId
							Properties.Resources.LogicalCircuitMainName,
							Properties.Resources.LogicalCircuitMainNotation
						)
					)
				));
				#pragma warning restore CA1863 // Use 'CompositeFormat'
				return CircuitProject.CreateAndClose(xmlReader);
			} catch(XmlException xmlException) {
				throw new CircuitException(Cause.CorruptedFile, xmlException, Properties.Resources.ErrorFileCorrupted(file));
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

		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private CircuitProject(CircuitProject other) : base(other) {
			this.CreateSets();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public void Save(string file) {
			using (TextWriter textWriter = XmlHelper.FileWriter(file)) {
				this.Save(textWriter);
			}
		}

		public void SaveSnapshot(string file) {
			CircuitProject other = new CircuitProject(this);
			other.Save(file);
		}

		private void Save(TextWriter textWriter) {
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

			this.DistinctSymbol(this.CircuitProbeSet);
			this.DistinctSymbol(this.CircuitButtonSet);
			this.DistinctSymbol(this.ConstantSet);
			this.DistinctSymbol(this.MemorySet);
			this.DistinctSymbol(this.LedMatrixSet);
			this.DistinctSymbol(this.PinSet);
			this.DistinctSymbol(this.SplitterSet);
			this.DistinctSymbol(this.SensorSet);
			this.DistinctSymbol(this.SoundSet);
			this.DistinctSymbol(this.GraphicsArraySet);

			this.WireSet.Where(wire => wire.Point1 == wire.Point2).ToList().ForEach(wire => wire.Delete());
			this.TextNoteSet.Where(textNote => !textNote.IsValid).ToList().ForEach(textNote => textNote.Delete());

			this.ResetUndoRedo();
		}

		[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
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

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static bool CanPaste(string? text) {
			// To reduce number of exceptions from XML reader lets check there if some familiar text is in the string.
			// Lets not use namespace as it will prevent pasting of old versions of the XML.
			if (!string.IsNullOrEmpty(text) && text.Contains("<lc:CircuitProject", StringComparison.Ordinal)) {
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

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IEnumerable<Symbol> Paste(string? text) {
			List<Symbol> result = new List<Symbol>();
			if(!string.IsNullOrWhiteSpace(text)) {
				CircuitProject paste;
				try {
					paste = CircuitProject.CreateAndClose(XmlHelper.CreateReader(new StringReader(text)));
				} catch(XmlException xmlException) {
					throw new CircuitException(Cause.CorruptedFile, xmlException, Properties.Resources.ErrorClipboardCorrupted);
				}

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

					if(0 < result.Count) {
						while(this.NeedToShift(result)) {
							foreach(Symbol symbol in result) {
								symbol.Shift(2, 2);
							}
						}
					}
				});
			}
			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private bool NeedToShift(List<Symbol> list) {
			bool covered(int pos1, int size1, int pos2, int size2) => pos2 <= pos1 && pos1 + size1 <= pos2 + size2;

			HashSet<Symbol> exclude = new HashSet<Symbol>(list);
			bool need = false;
			LogicalCircuit target = this.ProjectSet.Project.LogicalCircuit;
			foreach(Symbol symbol in list) {
				if(symbol is CircuitSymbol circuitSymbol) {
					if(	Symbol.LogicalCircuitGridWidth <= circuitSymbol.X + circuitSymbol.Circuit.SymbolWidth ||
						Symbol.LogicalCircuitGridHeight <= circuitSymbol.Y + circuitSymbol.Circuit.SymbolHeight
					) {
						return false;
					}
					if(target.CircuitSymbols().Any(other =>
						!exclude.Contains(other) && circuitSymbol.Circuit.Similar(other.Circuit) &&
						covered(circuitSymbol.X, circuitSymbol.Circuit.SymbolWidth, other.X, other.Circuit.SymbolWidth) &&
						covered(circuitSymbol.Y, circuitSymbol.Circuit.SymbolHeight, other.Y, other.Circuit.SymbolHeight)
					)) {
						need = true;
					}
				} else if(symbol is TextNote textNote) {
					if(	Symbol.LogicalCircuitGridWidth <= textNote.X + textNote.Width ||
						Symbol.LogicalCircuitGridHeight <= textNote.Y + textNote.Height
					) {
						return false;
					}
					if(target.TextNotes().Any(other => !exclude.Contains(other) &&
						covered(textNote.X, textNote.Width, other.X, other.Width) &&
						covered(textNote.Y, textNote.Height, other.Y, other.Height)
					)) {
						need = true;
					}
				} else if(symbol is Wire wire) {
					if(	Symbol.LogicalCircuitGridWidth <= wire.X1 || Symbol.LogicalCircuitGridWidth <= wire.X2 ||
						Symbol.LogicalCircuitGridHeight <= wire.Y1 || Symbol.LogicalCircuitGridHeight <= wire.Y2
					) {
						return false;
					}
					if(target.Wires().Any(other => !exclude.Contains(other) && (
						wire.Point1 == other.Point1 || wire.Point1 == other.Point2 ||
						wire.Point2 == other.Point1 || wire.Point2 == other.Point2
					))) {
						need = true;
					}
				}
			}
			return need;
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

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[Conditional("DEBUG")]
		private void ValidateCircuitProject() {
			foreach(Circuit circuit in this.CircuitSet) {
				if(circuit is LogicalCircuit) {
					Tracer.Assert(this.LogicalCircuitSet.Table.Exists(LogicalCircuitData.LogicalCircuitIdField.Field, circuit.CircuitId));
				} else if(circuit is CircuitProbe) {
					Tracer.Assert(this.CircuitProbeSet.Table.Exists(CircuitProbeData.CircuitProbeIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is CircuitButton) {
					Tracer.Assert(this.CircuitButtonSet.Table.Exists(CircuitButtonData.CircuitButtonIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is Constant) {
					Tracer.Assert(this.ConstantSet.Table.Exists(ConstantData.ConstantIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is Sensor) {
					Tracer.Assert(this.SensorSet.Table.Exists(SensorData.SensorIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is DevicePin) {
					Tracer.Assert(this.DevicePinSet.Table.Exists(DevicePinData.PinIdField.Field, circuit.CircuitId));
					Tracer.Assert(!this.CircuitSymbolSet.SelectByCircuit(circuit).Any());
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
				} else if(circuit is Sound) {
					Tracer.Assert(this.SoundSet.Table.Exists(SoundData.SoundIdField.Field, circuit.CircuitId));
					Tracer.Assert(this.CircuitSymbolSet.SelectByCircuit(circuit).Count() == 1);
				} else if(circuit is GraphicsArray) {
					Tracer.Assert(this.GraphicsArraySet.Table.Exists(GraphicsArrayData.GraphicsArrayIdField.Field, circuit.CircuitId));
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
		/// XSLT string  -- when ns is for known previous version
		/// null         -- when ns is unknown
		/// </returns>
		private static string? FindTransformation(string ns) {
			StringComparer cmp = StringComparer.OrdinalIgnoreCase;

			if(cmp.Equals(ns, CircuitProject.PersistenceNamespace)) return string.Empty;

			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.10/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_10;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.9/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_9;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.8/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_8;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.7/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_7;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.6/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_6;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.5/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_5;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.4/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_4;
			if(cmp.Equals(ns, "http://LogicCircuit.net/2.0.0.3/CircuitProject.xsd")) return Schema.ConvertFrom_2_0_0_3;
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

				string? xslt = CircuitProject.FindTransformation(xmlReader.NamespaceURI);
				if (xslt == null) {
					throw new CircuitException(Cause.UnknownVersion, Properties.Resources.ErrorUnknownVersion);
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
