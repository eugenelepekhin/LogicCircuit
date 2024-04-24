﻿namespace LogicCircuit {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using LogicCircuit.DataPersistent;

	partial class CircuitProject : StoreSnapshot, INotifyPropertyChanged {
		private const string PersistenceNamespace = "http://LogicCircuit.net/2.0.0.12/CircuitProject.xsd";
		private const string PersistencePrefix = "lc";

		public event PropertyChangedEventHandler? PropertyChanged;

		public ProjectSet ProjectSet { get; private set; }
		public CollapsedCategorySet CollapsedCategorySet { get; private set; }
		public CircuitSet CircuitSet { get; private set; }
		public DevicePinSet DevicePinSet { get; private set; }
		public GateSet GateSet { get; private set; }
		public LogicalCircuitSet LogicalCircuitSet { get; private set; }
		public PinSet PinSet { get; private set; }
		public CircuitProbeSet CircuitProbeSet { get; private set; }
		public ConstantSet ConstantSet { get; private set; }
		public CircuitButtonSet CircuitButtonSet { get; private set; }
		public MemorySet MemorySet { get; private set; }
		public LedMatrixSet LedMatrixSet { get; private set; }
		public SplitterSet SplitterSet { get; private set; }
		public SensorSet SensorSet { get; private set; }
		public SoundSet SoundSet { get; private set; }
		public GraphicsArraySet GraphicsArraySet { get; private set; }
		public CircuitSymbolSet CircuitSymbolSet { get; private set; }
		public WireSet WireSet { get; private set; }
		public TextNoteSet TextNoteSet { get; private set; }

		public bool UpdateInProgress { get; private set; }

		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public CircuitProject() : base() {
			// Create all sets
			this.CreateSets();

			// Create foreign keys
			ProjectData.CreateForeignKeys(this);
			CollapsedCategoryData.CreateForeignKeys(this);
			CircuitData.CreateForeignKeys(this);
			DevicePinData.CreateForeignKeys(this);
			GateData.CreateForeignKeys(this);
			LogicalCircuitData.CreateForeignKeys(this);
			PinData.CreateForeignKeys(this);
			CircuitProbeData.CreateForeignKeys(this);
			ConstantData.CreateForeignKeys(this);
			CircuitButtonData.CreateForeignKeys(this);
			MemoryData.CreateForeignKeys(this);
			LedMatrixData.CreateForeignKeys(this);
			SplitterData.CreateForeignKeys(this);
			SensorData.CreateForeignKeys(this);
			SoundData.CreateForeignKeys(this);
			GraphicsArrayData.CreateForeignKeys(this);
			CircuitSymbolData.CreateForeignKeys(this);
			WireData.CreateForeignKeys(this);
			TextNoteData.CreateForeignKeys(this);

			this.FreezeShape();
			this.Init();
		}
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		private void CreateSets() {
			this.ProjectSet = new ProjectSet(this);
			this.CollapsedCategorySet = new CollapsedCategorySet(this);
			this.CircuitSet = new CircuitSet(this);
			this.DevicePinSet = new DevicePinSet(this);
			this.GateSet = new GateSet(this);
			this.LogicalCircuitSet = new LogicalCircuitSet(this);
			this.PinSet = new PinSet(this);
			this.CircuitProbeSet = new CircuitProbeSet(this);
			this.ConstantSet = new ConstantSet(this);
			this.CircuitButtonSet = new CircuitButtonSet(this);
			this.MemorySet = new MemorySet(this);
			this.LedMatrixSet = new LedMatrixSet(this);
			this.SplitterSet = new SplitterSet(this);
			this.SensorSet = new SensorSet(this);
			this.SoundSet = new SoundSet(this);
			this.GraphicsArraySet = new GraphicsArraySet(this);
			this.CircuitSymbolSet = new CircuitSymbolSet(this);
			this.WireSet = new WireSet(this);
			this.TextNoteSet = new TextNoteSet(this);
		}

		private void Init() {
			this.VersionChanged += new EventHandler<VersionChangeEventArgs>(this.StoreVersionChanged);
			this.LatestVersionChanged += new EventHandler(this.StoreLatestVersionChanged);
			this.RolledBack += new EventHandler<RolledBackEventArgs>(this.StoreRolledBack);
		}

		private void NotifyPropertyChanged(string name) {
			PropertyChangedEventHandler? handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private void StoreVersionChanged(object? sender, VersionChangeEventArgs e) {
			try {
				this.UpdateInProgress = true;
				int oldVersion = e.OldVersion;
				int newVersion = e.NewVersion;
				List<Project>? deletedProject = this.ProjectSet.UpdateSet(oldVersion, newVersion);
				List<CollapsedCategory>? deletedCollapsedCategory = this.CollapsedCategorySet.UpdateSet(oldVersion, newVersion);
				List<Circuit>? deletedCircuit = this.CircuitSet.UpdateSet(oldVersion, newVersion);
				List<DevicePin>? deletedDevicePin = this.DevicePinSet.UpdateSet(oldVersion, newVersion);
				List<Gate>? deletedGate = this.GateSet.UpdateSet(oldVersion, newVersion);
				List<LogicalCircuit>? deletedLogicalCircuit = this.LogicalCircuitSet.UpdateSet(oldVersion, newVersion);
				List<Pin>? deletedPin = this.PinSet.UpdateSet(oldVersion, newVersion);
				List<CircuitProbe>? deletedCircuitProbe = this.CircuitProbeSet.UpdateSet(oldVersion, newVersion);
				List<Constant>? deletedConstant = this.ConstantSet.UpdateSet(oldVersion, newVersion);
				List<CircuitButton>? deletedCircuitButton = this.CircuitButtonSet.UpdateSet(oldVersion, newVersion);
				List<Memory>? deletedMemory = this.MemorySet.UpdateSet(oldVersion, newVersion);
				List<LedMatrix>? deletedLedMatrix = this.LedMatrixSet.UpdateSet(oldVersion, newVersion);
				List<Splitter>? deletedSplitter = this.SplitterSet.UpdateSet(oldVersion, newVersion);
				List<Sensor>? deletedSensor = this.SensorSet.UpdateSet(oldVersion, newVersion);
				List<Sound>? deletedSound = this.SoundSet.UpdateSet(oldVersion, newVersion);
				List<GraphicsArray>? deletedGraphicsArray = this.GraphicsArraySet.UpdateSet(oldVersion, newVersion);
				List<CircuitSymbol>? deletedCircuitSymbol = this.CircuitSymbolSet.UpdateSet(oldVersion, newVersion);
				List<Wire>? deletedWire = this.WireSet.UpdateSet(oldVersion, newVersion);
				List<TextNote>? deletedTextNote = this.TextNoteSet.UpdateSet(oldVersion, newVersion);

				this.ProjectSet.NotifyVersionChanged(oldVersion, newVersion, deletedProject);
				this.CollapsedCategorySet.NotifyVersionChanged(oldVersion, newVersion, deletedCollapsedCategory);
				this.CircuitSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuit);
				this.DevicePinSet.NotifyVersionChanged(oldVersion, newVersion, deletedDevicePin);
				this.GateSet.NotifyVersionChanged(oldVersion, newVersion, deletedGate);
				this.LogicalCircuitSet.NotifyVersionChanged(oldVersion, newVersion, deletedLogicalCircuit);
				this.PinSet.NotifyVersionChanged(oldVersion, newVersion, deletedPin);
				this.CircuitProbeSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuitProbe);
				this.ConstantSet.NotifyVersionChanged(oldVersion, newVersion, deletedConstant);
				this.CircuitButtonSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuitButton);
				this.MemorySet.NotifyVersionChanged(oldVersion, newVersion, deletedMemory);
				this.LedMatrixSet.NotifyVersionChanged(oldVersion, newVersion, deletedLedMatrix);
				this.SplitterSet.NotifyVersionChanged(oldVersion, newVersion, deletedSplitter);
				this.SensorSet.NotifyVersionChanged(oldVersion, newVersion, deletedSensor);
				this.SoundSet.NotifyVersionChanged(oldVersion, newVersion, deletedSound);
				this.GraphicsArraySet.NotifyVersionChanged(oldVersion, newVersion, deletedGraphicsArray);
				this.CircuitSymbolSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuitSymbol);
				this.WireSet.NotifyVersionChanged(oldVersion, newVersion, deletedWire);
				this.TextNoteSet.NotifyVersionChanged(oldVersion, newVersion, deletedTextNote);

				this.NotifyPropertyChanged("Version");
			} finally {
				this.UpdateInProgress = false;
			}
		}

		private void StoreLatestVersionChanged(object? sender, EventArgs e) {
			this.NotifyPropertyChanged("LatestAvailableVersion");
		}

		private void StoreRolledBack(object? sender, RolledBackEventArgs e) {
			int version = e.Version;
			this.ProjectSet.NotifyRolledBack(version);
			this.CollapsedCategorySet.NotifyRolledBack(version);
			this.CircuitSet.NotifyRolledBack(version);
			this.DevicePinSet.NotifyRolledBack(version);
			this.GateSet.NotifyRolledBack(version);
			this.LogicalCircuitSet.NotifyRolledBack(version);
			this.PinSet.NotifyRolledBack(version);
			this.CircuitProbeSet.NotifyRolledBack(version);
			this.ConstantSet.NotifyRolledBack(version);
			this.CircuitButtonSet.NotifyRolledBack(version);
			this.MemorySet.NotifyRolledBack(version);
			this.LedMatrixSet.NotifyRolledBack(version);
			this.SplitterSet.NotifyRolledBack(version);
			this.SensorSet.NotifyRolledBack(version);
			this.SoundSet.NotifyRolledBack(version);
			this.GraphicsArraySet.NotifyRolledBack(version);
			this.CircuitSymbolSet.NotifyRolledBack(version);
			this.WireSet.NotifyRolledBack(version);
			this.TextNoteSet.NotifyRolledBack(version);
		}
	}
}
