namespace LogicCircuit {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using LogicCircuit.DataPersistent;

	partial class CircuitProject : StoreSnapshot, INotifyPropertyChanged {
		public const string PersistenceNamespace = "http://LogicCircuit.net/1.0.0.4/CircuitProject.xsd";
		public const string PersistencePrefix = "lc";

		public event PropertyChangedEventHandler PropertyChanged;

		public ProjectSet ProjectSet { get; private set; }
		public CircuitSet CircuitSet { get; private set; }
		public DevicePinSet DevicePinSet { get; private set; }
		public GateSet GateSet { get; private set; }
		public LogicalCircuitSet LogicalCircuitSet { get; private set; }
		public PinSet PinSet { get; private set; }
		public ConstantSet ConstantSet { get; private set; }
		public CircuitButtonSet CircuitButtonSet { get; private set; }
		public MemorySet MemorySet { get; private set; }
		public SplitterSet SplitterSet { get; private set; }
		public CircuitSymbolSet CircuitSymbolSet { get; private set; }
		public WireSet WireSet { get; private set; }

		public bool UpdateInProgress { get; private set; }

		public CircuitProject() : base() {
			// Create all sets
			this.CreateSets();

			// Create foreign keys
			ProjectData.CreateForeignKeys(this);
			CircuitData.CreateForeignKeys(this);
			DevicePinData.CreateForeignKeys(this);
			GateData.CreateForeignKeys(this);
			LogicalCircuitData.CreateForeignKeys(this);
			PinData.CreateForeignKeys(this);
			ConstantData.CreateForeignKeys(this);
			CircuitButtonData.CreateForeignKeys(this);
			MemoryData.CreateForeignKeys(this);
			SplitterData.CreateForeignKeys(this);
			CircuitSymbolData.CreateForeignKeys(this);
			WireData.CreateForeignKeys(this);

			this.FreezeShape();
			this.Init();
		}

		private void CreateSets() {
			this.ProjectSet = new ProjectSet(this);
			this.CircuitSet = new CircuitSet(this);
			this.DevicePinSet = new DevicePinSet(this);
			this.GateSet = new GateSet(this);
			this.LogicalCircuitSet = new LogicalCircuitSet(this);
			this.PinSet = new PinSet(this);
			this.ConstantSet = new ConstantSet(this);
			this.CircuitButtonSet = new CircuitButtonSet(this);
			this.MemorySet = new MemorySet(this);
			this.SplitterSet = new SplitterSet(this);
			this.CircuitSymbolSet = new CircuitSymbolSet(this);
			this.WireSet = new WireSet(this);
		}

		private void Init() {
			this.VersionChanged += new EventHandler<VersionChangeEventArgs>(this.StoreVersionChanged);
			this.LatestVersionChanged += new EventHandler(this.StoreLatestVersionChanged);
			this.RolledBack += new EventHandler<RolledBackEventArgs>(this.StoreRolledBack);
		}

		private void NotifyPropertyChanged(string name) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private void StoreVersionChanged(object sender, VersionChangeEventArgs e) {
			try {
				this.UpdateInProgress = true;
				int oldVersion = e.OldVersion;
				int newVersion = e.NewVersion;
				List<Project> deletedProject = this.ProjectSet.UpdateSet(oldVersion, newVersion);
				List<Circuit> deletedCircuit = this.CircuitSet.UpdateSet(oldVersion, newVersion);
				List<DevicePin> deletedDevicePin = this.DevicePinSet.UpdateSet(oldVersion, newVersion);
				List<Gate> deletedGate = this.GateSet.UpdateSet(oldVersion, newVersion);
				List<LogicalCircuit> deletedLogicalCircuit = this.LogicalCircuitSet.UpdateSet(oldVersion, newVersion);
				List<Pin> deletedPin = this.PinSet.UpdateSet(oldVersion, newVersion);
				List<Constant> deletedConstant = this.ConstantSet.UpdateSet(oldVersion, newVersion);
				List<CircuitButton> deletedCircuitButton = this.CircuitButtonSet.UpdateSet(oldVersion, newVersion);
				List<Memory> deletedMemory = this.MemorySet.UpdateSet(oldVersion, newVersion);
				List<Splitter> deletedSplitter = this.SplitterSet.UpdateSet(oldVersion, newVersion);
				List<CircuitSymbol> deletedCircuitSymbol = this.CircuitSymbolSet.UpdateSet(oldVersion, newVersion);
				List<Wire> deletedWire = this.WireSet.UpdateSet(oldVersion, newVersion);

				this.ProjectSet.NotifyVersionChanged(oldVersion, newVersion, deletedProject);
				this.CircuitSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuit);
				this.DevicePinSet.NotifyVersionChanged(oldVersion, newVersion, deletedDevicePin);
				this.GateSet.NotifyVersionChanged(oldVersion, newVersion, deletedGate);
				this.LogicalCircuitSet.NotifyVersionChanged(oldVersion, newVersion, deletedLogicalCircuit);
				this.PinSet.NotifyVersionChanged(oldVersion, newVersion, deletedPin);
				this.ConstantSet.NotifyVersionChanged(oldVersion, newVersion, deletedConstant);
				this.CircuitButtonSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuitButton);
				this.MemorySet.NotifyVersionChanged(oldVersion, newVersion, deletedMemory);
				this.SplitterSet.NotifyVersionChanged(oldVersion, newVersion, deletedSplitter);
				this.CircuitSymbolSet.NotifyVersionChanged(oldVersion, newVersion, deletedCircuitSymbol);
				this.WireSet.NotifyVersionChanged(oldVersion, newVersion, deletedWire);

				this.NotifyPropertyChanged("Version");
			} finally {
				this.UpdateInProgress = false;
			}
		}

		private void StoreLatestVersionChanged(object sender, EventArgs e) {
			this.NotifyPropertyChanged("LatestAvailableVersion");
		}

		private void StoreRolledBack(object sender, RolledBackEventArgs e) {
			int version = e.Version;
			this.ProjectSet.NotifyRolledBack(version);
			this.CircuitSet.NotifyRolledBack(version);
			this.DevicePinSet.NotifyRolledBack(version);
			this.GateSet.NotifyRolledBack(version);
			this.LogicalCircuitSet.NotifyRolledBack(version);
			this.PinSet.NotifyRolledBack(version);
			this.ConstantSet.NotifyRolledBack(version);
			this.CircuitButtonSet.NotifyRolledBack(version);
			this.MemorySet.NotifyRolledBack(version);
			this.SplitterSet.NotifyRolledBack(version);
			this.CircuitSymbolSet.NotifyRolledBack(version);
			this.WireSet.NotifyRolledBack(version);
		}
	}
}
