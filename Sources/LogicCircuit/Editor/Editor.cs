using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;

namespace LogicCircuit {
	public partial class Editor : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public Mainframe Mainframe { get; private set; }
		public string File { get; private set; }
		public CircuitProject CircuitProject { get; private set; }
		private int savedVersion;
		public CircuitDescriptorList CircuitDescriptorList { get; private set; }
		private readonly Dictionary<GridPoint, int> wirePoint = new Dictionary<GridPoint, int>();
		private Switcher switcher;
		private LogicalCircuit currentLogicalCircuit;
		private Dictionary<Symbol, Marker> selection = new Dictionary<Symbol, Marker>();
		private Canvas selectionLayer;

		private Dispatcher Dispatcher { get { return this.Mainframe.Dispatcher; } }
		private Canvas Diagram { get { return this.Mainframe.Diagram; } }

		public bool HasChanges { get { return this.savedVersion != this.CircuitProject.Version; } }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }
		public string Caption { get { return Resources.MainFrameCaption(this.File); } }

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
		public bool InEditMode { get { return true; } }

		public Editor(Mainframe mainframe, string file) {
			this.Mainframe = mainframe;
			this.File = file;
			this.CircuitProject = CircuitProject.Create(this.File);
			this.savedVersion = this.CircuitProject.Version;
			this.CircuitDescriptorList = new CircuitDescriptorList(this.CircuitProject);
			this.switcher = new Switcher(this);
			this.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
		}

		public void Save(string file) {
			this.CircuitProject.Save(file);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.NotifyPropertyChanged("File");
		}

		private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
			case "Zoom":
			case "Frequency":
			case "IsMaximumSpeed":
				this.NotifyPropertyChanged(e.PropertyName);
				break;
			case "LogicalCircuit":
				this.CancelMove();
				this.ClearSelection();
				if(this.currentLogicalCircuit != this.Project.LogicalCircuit) {
					// TODO: this is not very good way to get scroll control as this assumes canvas is sitting on scroll viewer.
					// What if this get changed? For now just do it in hackky way
					ScrollViewer scrollViewer = this.Diagram.Parent as ScrollViewer;
					if(scrollViewer != null) {
						if(this.currentLogicalCircuit != null && !this.currentLogicalCircuit.IsDeleted()) {
							this.currentLogicalCircuit.ScrollOffset = new Point(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
						}
						this.currentLogicalCircuit = this.Project.LogicalCircuit;
						scrollViewer.ScrollToHorizontalOffset(this.currentLogicalCircuit.ScrollOffset.X);
						scrollViewer.ScrollToVerticalOffset(this.currentLogicalCircuit.ScrollOffset.Y);
					}
				}
				break;
			}
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

		//--- Drawing on Diagram --

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void RedrawDiagram() {
			Canvas diagram = this.Diagram;
			diagram.Children.Clear();
			this.wirePoint.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				Line line = wire.WireGlyph;
				Point p = Symbol.ScreenPoint(wire.Point1);
				line.X1 = p.X;
				line.Y1 = p.Y;
				p = Symbol.ScreenPoint(wire.Point2);
				line.X2 = p.X;
				line.Y2 = p.Y;
				diagram.Children.Add(line);
				this.AddWirePoint(wire.Point1);
				this.AddWirePoint(wire.Point2);
			}
			foreach(KeyValuePair<GridPoint, int> solder in this.wirePoint) {
				if(2 < solder.Value) {
					Ellipse ellipse = new Ellipse();
					Panel.SetZIndex(ellipse, 0);
					ellipse.Width = ellipse.Height = 2 * Symbol.PinRadius;
					Canvas.SetLeft(ellipse, Symbol.ScreenPoint(solder.Key.X) - Symbol.PinRadius);
					Canvas.SetTop(ellipse, Symbol.ScreenPoint(solder.Key.Y) - Symbol.PinRadius);
					ellipse.Fill = Symbol.JamDirectFill;
					diagram.Children.Add(ellipse);
				}
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Point point = Symbol.ScreenPoint(symbol.Point);
				Canvas.SetLeft(symbol.Glyph, point.X);
				Canvas.SetTop(symbol.Glyph, point.Y);
				diagram.Children.Add(symbol.Glyph);
			}
		}

		private void AddWirePoint(GridPoint point) {
			int count;
			if(!this.wirePoint.TryGetValue(point, out count)) {
				count = 0;
			}
			this.wirePoint[point] = count + 1;
		}

		//--- Edit Operation

		public void Undo() {
			if(this.CanUndo()) {
				this.CancelMove();
				this.ClearSelection();
				this.CircuitProject.Undo();
				this.Refresh();
			}
		}

		public void Redo() {
			if(this.CanRedo()) {
				this.CancelMove();
				this.ClearSelection();
				this.CircuitProject.Redo();
				this.Refresh();
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
				LogicalCircuit oldDiagram = this.Project.LogicalCircuit;
				bool success = false;
				bool started = false;
				CircuitProject circuitProject = this.CircuitProject;
				try {
					if(!circuitProject.IsEditor) {
						started = circuitProject.StartTransaction();
					}
					if(circuitProject.IsEditor) {
						this.Project.LogicalCircuit = logicalCircuit;
						if(started) {
							circuitProject.PrepareCommit();
						}
						success = true;
					}
				} finally {
					if(started) {
						if(success) {
							circuitProject.Commit();
						} else {
							circuitProject.Rollback();
						}
					}
					this.Refresh();
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
					this.OpenLogicalCircuit(other);
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

		public bool CanPaste() {
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
			//this.CancelMove();
			//if(0 < this.SelectionCount) {
			//    IEnumerable<Symbol> selection = this.Selection();
			//    this.ClearSelection();
			//    this.CircuitProject.InTransaction(() => {
			//        foreach(Symbol symbol in selection) {
			//            CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
			//            if(circuitSymbol != null) {
			//                if(circuitSymbol.Circuit is Gate || circuitSymbol.Circuit is LogicalCircuit) {
			//                    circuitSymbol.Delete();
			//                } else {
			//                    circuitSymbol.Circuit.Delete();
			//                }
			//            } else {
			//                Wire wire = symbol as Wire;
			//                if(wire != null) {
			//                    wire.Delete();
			//                }
			//            }
			//        }
			//    });
			//}
		}

		public void Cut() {
			//this.CancelMove();
			//if(0 < this.SelectionCount) {
			//    this.Copy();
			//    this.Delete();
			//}
		}

		public void Edit(Project project) {
			//Tracer.Assert(project == this.Project);
			//DialogProject dialog = new DialogProject(project);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		public void Edit(LogicalCircuit logicalCircuit) {
			//DialogCircuit dialog = new DialogCircuit(logicalCircuit);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		private void Edit(CircuitButton button) {
			//DialogButton dialog = new DialogButton(button);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		private void Edit(Constant constant) {
			//DialogConstant dialog = new DialogConstant(constant);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		private void Edit(Memory memory) {
			//Window dialog = memory.Writable ? (Window)new DialogRAM(memory) : (Window)new DialogROM(memory);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		private void Edit(Pin pin) {
			//DialogPin dialog = new DialogPin(pin);
			//dialog.Owner = this.MainFrame;
			//dialog.ShowDialog();
		}

		//--- Selection ---

		public int SelectionCount { get { return this.selection.Count; } }

		public IEnumerable<Symbol> Selection() {
			return new List<Symbol>(this.selection.Keys);
		}

		public void ClearSelection() {
			this.selection.Clear();
			if(this.selectionLayer != null) {
				this.selectionLayer.Children.Clear();
			}
		}

		private Marker CreateMarker(Symbol symbol) {
			CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
			if(circuitSymbol != null) {
				return new CircuitSymbolMarker(circuitSymbol);
			}
			Wire wire = symbol as Wire;
			if(wire != null) {
				return new WireMarker(wire);
			}
			throw new InvalidOperationException();
		}

		private Marker FindMarker(Symbol symbol) {
			Marker marker;
			if(this.selection.TryGetValue(symbol, out marker)) {
				return marker;
			}
			return null;
		}

		private Marker SelectSymbol(Symbol symbol) {
			Tracer.Assert(symbol.LogicalCircuit == this.Project.LogicalCircuit);
			Marker marker = this.FindMarker(symbol);
			if(marker == null) {
				marker = this.CreateMarker(symbol);
				this.selection.Add(symbol, marker);
				if(this.selectionLayer == null) {
					this.selectionLayer = new Canvas();
					Panel.SetZIndex(this.selectionLayer, int.MaxValue);
				}
				if(this.selectionLayer.Parent != this.Diagram) {
					this.Diagram.Children.Add(this.selectionLayer);
				}
				this.selectionLayer.Children.Add(marker.Glyph);
			}
			return marker;
		}

		public void Select(Symbol symbol) {
			this.SelectSymbol(symbol);
		}

		private void Unselect(Marker marker) {
			this.selection.Remove(marker.Symbol);
			this.selectionLayer.Children.Remove(marker.Glyph);
		}

		public void Unselect(Symbol symbol) {
			Marker marker = this.FindMarker(symbol);
			if(marker != null) {
				this.Unselect(marker);
			}
		}

		public void Select(IEnumerable<Symbol> symbol) {
			foreach(Symbol s in symbol) {
				this.Select(s);
			}
		}

		private void Select(Rect area) {
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Rect item = new Rect(Symbol.ScreenPoint(symbol.Point),
					new Size(Symbol.ScreenPoint(symbol.Circuit.SymbolWidth), Symbol.ScreenPoint(symbol.Circuit.SymbolHeight))
				);
				if(area.Contains(item)) {
					this.Select(symbol);
				}
			}
			foreach(Wire wire in logicalCircuit.Wires()) {
				if(area.Contains(Symbol.ScreenPoint(wire.Point1)) && area.Contains(Symbol.ScreenPoint(wire.Point2))) {
					this.Select(wire);
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

		private void SelectConductor(Wire wire) {
			Tracer.Assert(wire.LogicalCircuit == this.Project.LogicalCircuit);
			ConductorMap map = new ConductorMap(this.Project.LogicalCircuit);
			Conductor conductor;
			if(map.TryGetValue(wire.Point1, out conductor)) {
				this.Select(conductor.Wires);
			}
		}

		private void UnselectConductor(Wire wire) {
			Tracer.Assert(wire.LogicalCircuit == this.Project.LogicalCircuit);
			ConductorMap map = new ConductorMap(this.Project.LogicalCircuit);
			Conductor conductor;
			if(map.TryGetValue(wire.Point1, out conductor)) {
				foreach(Wire w in conductor.Wires) {
					this.Unselect(w);
				}
			}
		}

		private void CancelMove() {
			//if(this.movingMarker != null) {
			//    Mouse.Capture(null);
			//    if(this.movingMarker is WirePledge) {
			//        this.SymbolList.Remove(this.movingMarker);
			//    }
			//    this.movingMarker = null;
			//    this.MovingVector = new Vector();
			//}
		}
	}
}
