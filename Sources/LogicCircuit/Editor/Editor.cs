using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LogicCircuit {
	public partial class Editor : EditorDiagram, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public string File { get; private set; }
		private int savedVersion;

		public CircuitDescriptorList CircuitDescriptorList { get; private set; }
		private Switcher switcher;

		public bool HasChanges { get { return this.savedVersion != this.CircuitProject.Version; } }
		public string Caption { get { return Resources.MainFrameCaption(this.File); } }

		// use process specific id in order to prevent dragging and dropping between processes.
		private const double DragStartProximity = 3;
		private Point dragStart;
		private FrameworkElement dragSource;

		// TODO: implement it correctly
		private bool power = false;
		public bool Power {
			get { return this.power; }
			set {
				if(this.power != value) {
					this.power = value;
					this.NotifyPropertyChanged("Power");
				}
			}
		}

		// TODO: implement it correctly
		public override bool InEditMode { get { return true; } }

		public Editor(Mainframe mainframe, string file) : base(mainframe, CircuitProject.Create(file)) {
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.CircuitDescriptorList = new CircuitDescriptorList(this.CircuitProject);
			this.switcher = new Switcher(this);
		}

		public void Save(string file) {
			this.CircuitProject.Save(file);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.NotifyPropertyChanged("File");
		}

		protected override void OnProjectPropertyChanged(string propertyName) {
			switch(propertyName) {
			case "Zoom":
			case "Frequency":
			case "IsMaximumSpeed":
				this.NotifyPropertyChanged(propertyName);
				break;
			}
			base.OnProjectPropertyChanged(propertyName);
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.Mainframe.NotifyPropertyChanged(this.PropertyChanged, this, propertyName);
		}

		public double Zoom {
			get { return this.Project.Zoom; }
			set {
				if(this.Zoom != value) {
					try {
						this.CircuitProject.InTransaction(() => this.Project.Zoom = value);
					} catch(Exception exception) {
						this.Mainframe.ReportException(exception);
					}
				}
			}
		}

		public int Frequency {
			get { return this.Project.Frequency; }
			set {
				if(this.Frequency != value) {
					try {
						this.CircuitProject.InTransaction(() => this.Project.Frequency = value);
					} catch(Exception exception) {
						this.Mainframe.ReportException(exception);
					}
				}
			}
		}

		public bool IsMaximumSpeed {
			get { return this.Project.IsMaximumSpeed; }
			set {
				if(this.IsMaximumSpeed != value) {
					try {
						this.CircuitProject.InTransaction(() => this.Project.IsMaximumSpeed = value);
					} catch(Exception exception) {
						this.Mainframe.ReportException(exception);
					}
				}
			}
		}

		//--- Edit Operation

		public void Undo() {
			if(this.CanUndo()) {
				this.CancelMove();
				this.ClearSelection();
				this.CircuitProject.Undo();
			}
		}

		public void Redo() {
			if(this.CanRedo()) {
				this.CancelMove();
				this.ClearSelection();
				this.CircuitProject.Redo();
			}
		}

		public bool CanUndo() {
			return this.InEditMode && this.CircuitProject.CanUndo;
		}

		public bool CanRedo() {
			return this.InEditMode && this.CircuitProject.CanRedo;
		}

		public void OpenLogicalCircuit(LogicalCircuit logicalCircuit) {
			this.CancelMove();
			this.ClearSelection();
			if(logicalCircuit != this.Project.LogicalCircuit) {
				bool success = false;
				bool started = false;
				try {
					if(!this.CircuitProject.IsEditor) {
						started = this.CircuitProject.StartTransaction();
					}
					if(this.CircuitProject.IsEditor) {
						this.Project.LogicalCircuit = logicalCircuit;
						if(started) {
							this.CircuitProject.PrepareCommit();
						}
						success = true;
					}
				} finally {
					if(started) {
						if(success) {
							this.CircuitProject.Commit();
						} else {
							this.CircuitProject.Rollback();
						}
					}
				}
			}
		}

		public void DeleteLogicalCircuit() {
			if(1 < this.CircuitProject.LogicalCircuitSet.Count()) {
				this.CancelMove();
				this.ClearSelection();
				LogicalCircuit current = this.Project.LogicalCircuit;
				LogicalCircuit other = this.switcher.SuggestNext();
				Tracer.Assert(other != null && other != current);
				this.CircuitProject.InTransaction(() => {
					this.Project.LogicalCircuit = other;
					current.Delete();
				});
			}
		}

		public void Copy() {
			//this.CancelMove();
			//if(0 < this.SelectionCount) {
			//    XmlDocument xml = this.ProjectManager.CircuitProject.Copy(this.selection.Keys);
			//    StringBuilder text = new StringBuilder();
			//    using(StringWriter stringWriter = new StringWriter(text, CultureInfo.InvariantCulture)) {
			//        using(XmlTextWriter writer = new XmlTextWriter(stringWriter)) {
			//            writer.Formatting = Formatting.None;
			//            xml.WriteTo(writer);
			//        }
			//    }
			//    Clipboard.SetDataObject(text.ToString(), false);
			//}
		}

		public static bool CanPaste() {
			return CircuitProject.CanPaste(Clipboard.GetText());
		}

		public void Paste() {
			//this.CancelMove();
			//this.ClearSelection();
			//string text = Clipboard.GetText();
			//if(this.CircuitProject.CanPaste(text)) {
			//    XmlDocument xml = new XmlDocument();
			//    xml.LoadXml(text);
			//    List<Symbol> result = null;
			//    this.CircuitProject.InTransaction(() => {
			//        result = this.ProjectManager.CircuitProject.Paste(xml);
			//    });
			//    Tracer.Assert(result.All(symbol => symbol.LogicalCircuit == this.CircuitProject.ProjectSet.Project.LogicalCircuit));
			//    this.Select(result);
			//}
		}

		public void Delete() {
			this.CancelMove();
			if(0 < this.SelectionCount) {
				IEnumerable<Symbol> selection = this.Selection();
				this.ClearSelection();
				this.CircuitProject.InTransaction(() => {
					foreach(Symbol symbol in selection) {
						CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
						if(circuitSymbol != null) {
							circuitSymbol.DeleteSymbol();
						} else {
							Wire wire = symbol as Wire;
							if(wire != null) {
								wire.Delete();
							}
						}
					}
				});
			}
		}

		public void Cut() {
			//this.CancelMove();
			//if(0 < this.SelectionCount) {
			//    this.Copy();
			//    this.Delete();
			//}
		}

		public void Edit(Project project) {
			Tracer.Assert(project == this.Project);
			DialogProject dialog = new DialogProject(project);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		public override void Edit(LogicalCircuit logicalCircuit) {
			DialogCircuit dialog = new DialogCircuit(logicalCircuit);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		private void Edit(CircuitButton button) {
			DialogButton dialog = new DialogButton(button);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		private void Edit(Constant constant) {
			DialogConstant dialog = new DialogConstant(constant);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		private void Edit(Memory memory) {
			Window dialog = memory.Writable ? (Window)new DialogRAM(memory) : (Window)new DialogROM(memory);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		private void Edit(Pin pin) {
			DialogPin dialog = new DialogPin(pin);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		protected override void Edit(Symbol symbol) {
			if(this.InEditMode) {
				CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
				if(circuitSymbol != null) {
					LogicalCircuit lc = circuitSymbol.Circuit as LogicalCircuit;
					if(lc != null) {
						this.OpenLogicalCircuit(lc);
						return;
					}
					CircuitButton cb = circuitSymbol.Circuit as CircuitButton;
					if(cb != null) {
						this.Edit(cb);
						return;
					}
					Constant ct = circuitSymbol.Circuit as Constant;
					if(ct != null) {
						this.Edit(ct);
						return;
					}
					Memory m = circuitSymbol.Circuit as Memory;
					if(m != null) {
						this.Edit(m);
						return;
					}
					Pin pin = circuitSymbol.Circuit as Pin;
					if(pin != null) {
						this.Edit(pin);
						return;
					}
				}
			}
		}

		public void SelectAll() {
			if(this.InEditMode) {
				LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
				foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
					this.Select(symbol);
				}
				foreach(Wire wire in logicalCircuit.Wires()) {
					this.Select(wire);
				}
			}
		}

		public void SelectAllWires() {
			if(this.InEditMode) {
				foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
					this.Select(wire);
				}
			}
		}

		public int SelectFreeWires() {
			if(this.InEditMode) {
				LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
				Dictionary<GridPoint, int> pointCount = new Dictionary<GridPoint, int>();
				Dictionary<GridPoint, Wire> firstWire = new Dictionary<GridPoint, Wire>();
				foreach(Wire wire in logicalCircuit.Wires()) {
					Tracer.Assert(wire.Point1 != wire.Point2);
					int count;
					if(pointCount.TryGetValue(wire.Point1, out count)) {
						if(count < 2) {
							pointCount[wire.Point1] = count + 1;
						}
					} else {
						pointCount.Add(wire.Point1, 1);
						firstWire.Add(wire.Point1, wire);
					}
					if(pointCount.TryGetValue(wire.Point2, out count)) {
						if(count < 2) {
							pointCount[wire.Point2] = count + 1;
						}
					} else {
						pointCount.Add(wire.Point2, 1);
						firstWire.Add(wire.Point2, wire);
					}
				}
				foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
					foreach(Jam jam in symbol.Jams()) {
						int count;
						if(pointCount.TryGetValue(jam.AbsolutePoint, out count) && count < 2) {
							pointCount[jam.AbsolutePoint] = count + 1;
						}
					}
				}
				int freeWireCount = 0;
				foreach(KeyValuePair<GridPoint, int> pair in pointCount) {
					if(pair.Value < 2) {
						this.Select(firstWire[pair.Key]);
						freeWireCount++;
					}
				}
				return freeWireCount;
			}
			return 0;
		}

		public int SelectFloatingSymbols() {
			if(this.InEditMode) {
				LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
				HashSet<GridPoint> wirePoint = new HashSet<GridPoint>();
				foreach(Wire wire in logicalCircuit.Wires()) {
					wirePoint.Add(wire.Point1);
					wirePoint.Add(wire.Point2);
				}
				int count = 0;
				foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
					foreach(Jam jam in symbol.Jams()) {
						if(!wirePoint.Contains(jam.AbsolutePoint)) {
							this.Select(symbol);
							count++;
							break;
						}
					}
				}
				return count;
			}
			return 0;
		}

		public void SelectAllButWires() {
			if(this.InEditMode) {
				foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
					this.Select(symbol);
				}
			}
		}

		public void UnselectAllWires() {
			if(this.InEditMode) {
				foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
					this.Unselect(wire);
				}
			}
		}

		public void UnselectAllButWires() {
			if(this.InEditMode) {
				foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
					this.Unselect(symbol);
				}
			}
		}

		public void SelectAllProbes(bool withWire) {
			if(this.InEditMode) {
				LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
				foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
					Gate gate = symbol.Circuit as Gate;
					if(gate != null && gate.GateType == GateType.Probe) {
						this.Select(symbol);
						if(withWire) {
							Tracer.Assert(symbol.Jams().Count() == 1);
							GridPoint point = symbol.Jams().First().AbsolutePoint;
							foreach(Wire wire in logicalCircuit.Wires()) {
								if(wire.Point1 == point || wire.Point2 == point) {
									this.Select(wire);
								}
							}
						}
					}
				}
			}
		}

		//--- Event Handling ---

		public void DiagramLostFocus() {
			this.CancelMove();
		}

		public void DiagramKeyDown(KeyEventArgs e) {
			if(this.InEditMode) {
				if(e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
					this.switcher.OnControlDown();
					this.Mainframe.Status = Resources.TipOnCtrlDown;
					e.Handled = true;
				} else if(e.Key == Key.Tab) {
					this.switcher.OnTabDown(
						(Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None,
						(Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None
					);
					e.Handled = true;
				} else if(e.Key == Key.Escape) {
					this.CancelMove();
				}
			}
		}

		public void DiagramKeyUp(KeyEventArgs e) {
			if(this.InEditMode && (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)) {
				this.switcher.OnControlUp();
				e.Handled = true;
			}
		}

		public void DescriptorMouseDown(FrameworkElement sender, MouseButtonEventArgs e) {
			if(e.ChangedButton == MouseButton.Left && this.InEditMode) {
				ICircuitDescriptor descriptor = sender.DataContext as ICircuitDescriptor;
				if(descriptor != null) {
					if(1 < e.ClickCount) {
						LogicalCircuitDescriptor logicalCircuitDescriptor = descriptor as LogicalCircuitDescriptor;
						if(logicalCircuitDescriptor != null && !logicalCircuitDescriptor.IsCurrent) {
							this.OpenLogicalCircuit(logicalCircuitDescriptor.Circuit);
						}
					} else {
						this.dragStart = e.GetPosition(sender);
						this.dragSource = sender;
					}
				}
			}
		}

		public void DescriptorMouseUp() {
			this.dragSource = null;
		}

		public void DescriptorMouseMove(FrameworkElement sender, MouseEventArgs e) {
			if(this.InEditMode && this.dragSource != null) {
				Point point = e.GetPosition(this.dragSource);
				double x = point.X - this.dragStart.X;
				double y = point.Y - this.dragStart.Y;
				if(Editor.DragStartProximity < x * x + y * y) {
					this.dragSource = null;
					DragDrop.DoDragDrop(sender,
						new DataObject(EditorDiagram.CircuitDescriptorDataFormat, sender.DataContext),
						DragDropEffects.Copy | DragDropEffects.Scroll
					);
				}
			}
		}
	}
}
