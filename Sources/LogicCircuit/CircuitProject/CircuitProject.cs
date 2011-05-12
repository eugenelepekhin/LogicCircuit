using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LogicCircuit.DataPersistent;
using System.Diagnostics;

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
			XmlDocument xml = new XmlDocument();
			bool rename = false;
			if(file != null) {
				xml.Load(file);
			} else {
				xml.LoadXml(CircuitProject.ChangeGuid(CircuitProject.ChangeGuid(Schema.Empty, "ProjectId"), "LogicalCircuitId"));
				rename = true;
			}
			CircuitProject circuitProject = new CircuitProject();
			circuitProject.InTransaction(() => {
				circuitProject.Load(xml);
				if(rename) {
					Project project = circuitProject.ProjectSet.Project;
					project.Name = Resources.CircuitProjectName;
					LogicalCircuit logicalCircuit = project.LogicalCircuit;
					logicalCircuit.Name = logicalCircuit.Notation = Resources.LogicalCircuitMainName;
				}
			});
			circuitProject.CircuitSymbolSet.ValidateAll();
			return circuitProject;
		}

		public void Save(string file) {
			XmlDocument xml = this.Save();
			XmlHelper.Save(xml, file);
		}

		private void Load(XmlDocument xml) {
			xml = CircuitProject.Transform(xml);

			XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
			nsmgr.AddNamespace("lc", CircuitProject.PersistenceNamespace);

			XmlElement root = xml.SelectSingleNode("/lc:CircuitProject", nsmgr) as XmlElement;
			if(root != null) {
				this.ProjectSet.Load(root.SelectNodes("lc:Project", nsmgr));
				this.LogicalCircuitSet.Load(root.SelectNodes("lc:LogicalCircuit", nsmgr));
				this.PinSet.Load(root.SelectNodes("lc:Pin", nsmgr));
				this.ConstantSet.Load(root.SelectNodes("lc:Constant", nsmgr));
				this.CircuitButtonSet.Load(root.SelectNodes("lc:CircuitButton", nsmgr));
				this.MemorySet.Load(root.SelectNodes("lc:Memory", nsmgr));
				this.SplitterSet.Load(root.SelectNodes("lc:Splitter", nsmgr));
				this.CircuitSymbolSet.Load(root.SelectNodes("lc:CircuitSymbol", nsmgr));
				this.WireSet.Load(root.SelectNodes("lc:Wire", nsmgr));
				this.TextNoteSet.Load(root.SelectNodes("lc:TextNote", nsmgr));
			}

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

		private XmlDocument Save() {
			XmlDocument xml = new XmlDocument();
			XmlElement root = xml.CreateElement(CircuitProject.PersistencePrefix, "CircuitProject", CircuitProject.PersistenceNamespace);
			xml.AppendChild(root);
			ProjectData.Save(this.ProjectSet.Table, root);
			LogicalCircuitData.Save(this.LogicalCircuitSet.Table, root);
			PinData.Save(this.PinSet.Table, root);
			ConstantData.Save(this.ConstantSet.Table, root);
			CircuitButtonData.Save(this.CircuitButtonSet.Table, root);
			MemoryData.Save(this.MemorySet.Table, root);
			SplitterData.Save(this.SplitterSet.Table, root);
			CircuitSymbolData.Save(this.CircuitSymbolSet.Table, root);
			WireData.Save(this.WireSet.Table, root);
			TextNoteData.Save(this.TextNoteSet.Table, root);
			return xml;
		}

		private static string ChangeGuid(string text, string nodeName) {
			return Regex.Replace(text,
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>\{{?[0-9a-fA-F]{{8}}-([0-9a-fA-F]{{4}}-){{3}}[0-9a-fA-F]{{12}}\}}?</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName
				),
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>{2}</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName, Guid.NewGuid()
				),
				RegexOptions.CultureInvariant | RegexOptions.Singleline
			);
		}

		public XmlDocument Copy(IEnumerable<Symbol> symbol) {
			CircuitProject copy = new CircuitProject();
			bool started = copy.StartTransaction();
			Tracer.Assert(started);
			copy.ProjectSet.Copy(this.ProjectSet.Project);
			LogicalCircuit target = copy.LogicalCircuitSet.Copy(this.ProjectSet.Project.LogicalCircuit, false);
			foreach(Symbol s in symbol) {
				s.CopyTo(target);
			}
			return copy.Save();
		}

		public static bool CanPaste(string text) {
			return (!string.IsNullOrEmpty(text) &&
				Regex.IsMatch(text, Regex.Escape("<lc:CircuitProject xmlns:lc=\"" + CircuitProject.PersistenceNamespace + "\">"))
			);
		}

		public IEnumerable<Symbol> Paste(XmlDocument xml) {
			CircuitProject paste = new CircuitProject();
			bool started = paste.StartTransaction();
			Tracer.Assert(started);
			paste.Load(xml);

			List<Symbol> result = new List<Symbol>();

			LogicalCircuit target = this.ProjectSet.Project.LogicalCircuit;
			foreach(CircuitSymbol symbol in paste.ProjectSet.Project.LogicalCircuit.CircuitSymbols()) {
				result.Add(symbol.CopyTo(target));
			}
			foreach(Wire wire in paste.ProjectSet.Project.LogicalCircuit.Wires()) {
				result.Add(wire.CopyTo(target));
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
			//TODO: Add check of many wires hidden by eachother.
		}

		public void InTransaction(Action action) {
			this.InTransaction(action, true);
		}

		public void InOmitTransaction(Action action) {
			this.InTransaction(action, false);
		}

		private static XmlDocument Transform(XmlDocument xml) {
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;
			while(comparer.Compare(CircuitProject.PersistenceNamespace, xml.DocumentElement.NamespaceURI) != 0) {
				string xslt;
				//TODO: 1.0.0.4 - is just a transition version used only in the development. Remove this after 2.0.0.1 catch up with old version before ship.
				if(       comparer.Compare("http://LogicCircuit.net/1.0.0.4/CircuitProject.xsd", xml.DocumentElement.NamespaceURI) == 0) {
					xslt = Schema.ConvertFrom_1_0_0_4;
				} else if(comparer.Compare("http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd", xml.DocumentElement.NamespaceURI) == 0) {
					xslt = Schema.ConvertFrom_1_0_0_3;
				} else if(comparer.Compare("http://LogicCircuit.net/1.0.0.2/CircuitProject.xsd", xml.DocumentElement.NamespaceURI) == 0) {
					xslt = Schema.ConvertFrom_1_0_0_2;
				} else {
					throw new CircuitException(Cause.UnknownVersion, Resources.ErrorUnknownVersion);
				}
				xml = XmlHelper.Transform(xml, xslt);
			}
			return xml;
		}
	}
}
