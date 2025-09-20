﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Xml;
using DataPersistent;

namespace LogicCircuit {
	public partial class LogicalCircuit {
		public const int MaxTruthTableFilters = 5;
		public int PinVersion { get; private set; }

		public Point ScrollOffset { get; set; }

		private ConductorMap? conductorMap;

		public void UpdateConductorMap() {
			this.conductorMap = new ConductorMap(this);
		}

		public ConductorMap ConductorMap() {
			if(this.conductorMap == null) {
				this.UpdateConductorMap();
			}
			return this.conductorMap!;
		}

		public override void Delete() {
			this.CircuitSymbols().ToList().ForEach(symbol => symbol.DeleteSymbol());
			base.Delete();
		}

		public bool InvertIsDisplay { get; set; }

		public override bool IsDisplay => this.InvertIsDisplay ^ (this.CircuitShape == CircuitShape.Display);

		public IEnumerable<Pin> LogicalPins {
			get { return this.CircuitProject.PinSet.SelectByCircuit(this); }
		}

		public override IEnumerable<BasePin> Pins {
			get { return (IEnumerable<BasePin>)this.LogicalPins; }
		}

		public override string ToolTip { get { return Circuit.BuildToolTip(this.Name, this.Note); } }

		public IEnumerable<CircuitSymbol> CircuitSymbols() {
			return this.CircuitProject.CircuitSymbolSet.SelectByLogicalCircuit(this);
		}

		public IEnumerable<Wire> Wires() {
			return this.CircuitProject.WireSet.SelectByLogicalCircuit(this);
		}

		public IEnumerable<TextNote> TextNotes() {
			return this.CircuitProject.TextNoteSet.SelectByLogicalCircuit(this);
		}

		public bool IsEmpty() {
			return !this.CircuitSymbols().Any() && !this.Wires().Any() && !this.TextNotes().Any();
		}

		public override Circuit CopyTo(LogicalCircuit target) {
			return target.CircuitProject.LogicalCircuitSet.Copy(this, true);
		}

		public void Rename(string name) {
			if(LogicalCircuitData.NameField.Field.Compare(this.Name, name) != 0) {
				this.Name = this.CircuitProject.LogicalCircuitSet.UniqueName(name);
			}
		}

		private bool HasDisplayLoop(HashSet<LogicalCircuit> parents) {
			if(parents.Add(this)) {
				foreach(CircuitSymbol symbol in this.CircuitSymbols()) {
					if(symbol.Circuit is LogicalCircuit lc && lc.IsDisplay && lc.HasDisplayLoop(parents)) {
						return true;
					}
				}
				parents.Remove(this);
				return false;
			}
			return true;
		}
		
		public bool ContainsDisplays() {
			return !this.HasDisplayLoop(new HashSet<LogicalCircuit>()) && this.CircuitSymbols().Any(symbol => symbol.Circuit.IsValidDisplay());
		}

		public override bool IsValidDisplay() {
			return this.IsDisplay && this.ContainsDisplays();
		}

		#if DEBUG
			public IList<CircuitSymbol> ValidDisplayList => this.CircuitSymbols().Where(s => s.Circuit.IsValidDisplay()).ToList();
		#endif

		private Rect DisplayBounds() {
			return this.CircuitSymbols().Where(s => s.Circuit.IsValidDisplay()).Select(s => s.Bounds()).Aggregate((r1, r2) => Rect.Union(r1, r2));
		}

		protected override int CircuitSymbolWidth(int defaultWidth) {
			if(this.IsValidDisplay()) {
				return Math.Max(defaultWidth, Symbol.GridPoint(this.DisplayBounds().Width));
			}
			return Math.Max(3, defaultWidth);
		}

		protected override int CircuitSymbolHeight(int defaultHeight) {
			if(this.IsValidDisplay()) {
				return Math.Max(defaultHeight, Symbol.GridPoint(this.DisplayBounds().Height));
			}
			return Math.Max(4, defaultHeight);
		}

		public override FrameworkElement CreateGlyph(CircuitGlyph symbol) {
			Tracer.Assert(this == symbol.Circuit);
			if(this.IsValidDisplay()) {
				return symbol.CreateDisplayGlyph(symbol);
			} else {
				switch(this.CircuitShape) {
				case CircuitShape.Mux:		return symbol.CreateMuxGlyph();
				case CircuitShape.Demux:	return symbol.CreateDemuxGlyph();
				case CircuitShape.Alu:		return symbol.CreateAluGlyph();
				case CircuitShape.FlipFlop:	return symbol.CreateFlipFlopGlyph();
				}
				return symbol.CreateRectangularGlyph();
			}
		}

		public override FrameworkElement CreateDisplay(CircuitGlyph symbol, CircuitGlyph mainSymbol) {
			Tracer.Assert(this == symbol.Circuit);
			if(this.IsValidDisplay()) {
				return symbol.CreateDisplayGlyph(mainSymbol);
			}
			return base.CreateDisplay(symbol, mainSymbol);
		}

		public override void ResetPins() {
			base.ResetPins();
			this.PinVersion = this.CircuitProject.Version;
		}

		partial void OnLogicalCircuitChanged() {
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(this)) {
				symbol.Invalidate();
			}
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class LogicalCircuitSet : NamedItemSet {
		public event EventHandler? LogicalCircuitSetChanged;

		private HashSet<LogicalCircuit> invalid = new HashSet<LogicalCircuit>();
		public IEnumerable<LogicalCircuit> Invalid { get { return this.invalid; } }
		public void Invalidate(LogicalCircuit circuit) {
			if(!circuit.IsDeleted()) {
				this.invalid.Add(circuit);
			}
		}
		public void ValidateAll() {
			this.invalid.Clear();
		}

		private LogicalCircuit Register(RowId rowId) {
			CircuitData data = new CircuitData() {
				CircuitId = this.Table.GetField(rowId, LogicalCircuitData.LogicalCircuitIdField.Field)
			};
			LogicalCircuit circuit = this.Create(rowId, this.CircuitProject.CircuitTable.Insert(ref data));
			circuit.PropertyChanged += new PropertyChangedEventHandler(this.CircuitPropertyChanged);
			return circuit;
		}

		private void CircuitPropertyChanged(object? sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == "CircuitShape") {
				this.Invalidate((LogicalCircuit)sender!);
			}
		}

		protected override bool Exists(string name) {
			return this.FindByName(name) != null;
		}

		protected override bool Exists(string name, Circuit group) {
			throw new NotSupportedException();
		}

		public IEnumerable<LogicalCircuit> SelectByCategory(string category) {
			return this.Select(this.Table.Select(LogicalCircuitData.CategoryField.Field, category));
		}

		public LogicalCircuit Create() {
			string name = this.UniqueName(Properties.Resources.LogicalCircuitName);
			LogicalCircuit circuit = this.CreateItem(Guid.NewGuid(),
				name,
				name,
				LogicalCircuitData.NoteField.Field.DefaultValue,
				LogicalCircuitData.CategoryField.Field.DefaultValue,
				LogicalCircuitData.CircuitShapeField.Field.DefaultValue,
				LogicalCircuitData.ValidatorsField.Field.DefaultValue
			);
			circuit.PropertyChanged += new PropertyChangedEventHandler(this.CircuitPropertyChanged);
			return circuit;
		}

		public LogicalCircuit Copy(LogicalCircuit other, bool deepCopy) {
			LogicalCircuit? logicalCircuit = this.FindByLogicalCircuitId(other.LogicalCircuitId);
			Tracer.Assert(deepCopy || logicalCircuit == null);
			if(logicalCircuit == null) {
				LogicalCircuitData data;
				other.CircuitProject.LogicalCircuitSet.Table.GetData(other.LogicalCircuitRowId, out data);
				data.Name = this.UniqueName(data.Name);
				data.LogicalCircuit = null;
				logicalCircuit = this.Register(this.Table.Insert(ref data));
				if(deepCopy) {
					foreach(CircuitSymbol symbol in other.CircuitSymbols()) {
						symbol.CopyTo(logicalCircuit);
					}
					foreach(Wire wire in other.Wires()) {
						wire.CopyTo(logicalCircuit);
					}
					foreach(TextNote symbol in other.TextNotes()) {
						symbol.CopyTo(logicalCircuit);
					}
				}
			}
			return logicalCircuit;
		}

		partial void EndNotifyLogicalCircuitSetChanged() {
			//TODO: this is only used for update list of descriptors. Consider removing it.
			this.LogicalCircuitSetChanged?.Invoke(this, EventArgs.Empty);
		}

		public void UpdateConductorMaps() {
			foreach(LogicalCircuit circuit in this) {
				circuit.UpdateConductorMap();
			}
		}

		public IRecordLoader CreateRecordLoader(XmlNameTable nameTable) {
			return new RecordLoader<LogicalCircuitData>(nameTable, this.Table, rowId => this.Register(rowId));
		}
	}
}
