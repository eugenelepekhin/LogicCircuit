using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LogicCircuit.DataPersistent;

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

		public static CircuitProject Create() {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(CircuitProject.ChangeGuid(CircuitProject.ChangeGuid(Schema.Empty, "ProjectId"), "LogicalCircuitId"));
			CircuitProject circuitProject = new CircuitProject();
			circuitProject.InTransaction(() => circuitProject.LoadXml(xml));
			return circuitProject;
		}

		public static CircuitProject Load(string file) {
			XmlDocument xml = new XmlDocument();
			xml.Load(file);
			return CircuitProject.Load(xml);
		}

		public void Save(string file) {
			XmlDocument xml = this.Save();
			XmlHelper.Save(xml, file);
		}

		private static CircuitProject Load(XmlDocument xml) {
			CircuitProject circuitProject = new CircuitProject();
			circuitProject.InTransaction(() => circuitProject.LoadXml(xml));
			return circuitProject;
		}

		private void LoadXml(XmlDocument xml) {
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
			}

			this.GateSet.Generate();

			List<Wire> list = new List<Wire>();
			foreach(Wire wire in this.WireSet) {
				if(wire.Point1 == wire.Point2) {
					list.Add(wire);
				}
			}
			if(0 < list.Count) {
				foreach(Wire wire in list) {
					wire.Delete();
				}
			}
			this.ResetUndoRedo();
		}

		private static string ChangeGuid(string text, string nodeName) {
			string s = Regex.Replace(text,
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>\{{?[0-9a-fA-F]{{8}}-([0-9a-fA-F]{{4}}-){{3}}[0-9a-fA-F]{{12}}\}}?</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName
				),
				string.Format(CultureInfo.InvariantCulture,
					@"<{0}:{1}>{2}</{0}:{1}>", CircuitProject.PersistencePrefix, nodeName, Guid.NewGuid()
				),
				RegexOptions.CultureInvariant | RegexOptions.Singleline
			);
			return s;
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
			return xml;
		}

		public XmlDocument Copy(IEnumerable<Symbol> symbol) {
			CircuitProject copy = new CircuitProject();
			bool started = copy.StartTransaction();
			Tracer.Assert(started);
			copy.ProjectSet.Copy(this.ProjectSet.Project);
			copy.LogicalCircuitSet.Copy(this.ProjectSet.Project.LogicalCircuit, false);
			foreach(Symbol s in symbol) {
				s.CopyTo(copy);
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
			paste.LoadXml(xml);

			List<Symbol> result = new List<Symbol>();

			LogicalCircuit logicalCircuit = this.ProjectSet.Project.LogicalCircuit;
			foreach(CircuitSymbol symbol in paste.ProjectSet.Project.LogicalCircuit.CircuitSymbols()) {
				symbol.CircuitSymbolId = Guid.NewGuid();
				symbol.LogicalCircuit = logicalCircuit;
				Tracer.Assert(this.CircuitSymbolSet.Find(symbol.CircuitSymbolId) == null);
				symbol.CopyTo(this);
				CircuitSymbol copy = this.CircuitSymbolSet.Find(symbol.CircuitSymbolId);
				Tracer.Assert(copy != null);
				result.Add(copy);
			}
			foreach(Wire wire in paste.ProjectSet.Project.LogicalCircuit.Wires()) {
				wire.WireId = Guid.NewGuid();
				wire.LogicalCircuit = logicalCircuit;
				Tracer.Assert(this.WireSet.Find(wire.WireId) == null);
				wire.CopyTo(this);
				Wire copy = this.WireSet.Find(wire.WireId);
				Tracer.Assert(copy != null);
				result.Add(copy);
			}

			if(0 < result.Count && paste.ProjectSet.Project.LogicalCircuit.LogicalCircuitId == logicalCircuit.LogicalCircuitId) {
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
				if(comparer.Compare("http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd", xml.DocumentElement.NamespaceURI) == 0) {
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
