using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LogicCircuit {
	public partial class Editor : INotifyPropertyChanged {
		private const int ClickProximity = 2 * Symbol.PinRadius;

		public event PropertyChangedEventHandler PropertyChanged;

		public Mainframe Mainframe { get; private set; }
		public string File { get; private set; }
		public CircuitProject CircuitProject { get; private set; }
		private int savedVersion;

		public CircuitDescriptorList CircuitDescriptorList { get; private set; }
		private Switcher switcher;
		private LogicalCircuit currentLogicalCircuit;
		private readonly Dictionary<GridPoint, int> wirePoint = new Dictionary<GridPoint, int>();

		private Dictionary<Symbol, Marker> selection = new Dictionary<Symbol, Marker>();
		private Canvas selectionLayer;

		private Marker movingMarker;
		private Point moveStart;
		private TranslateTransform moveVector;

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

		//--- Drawing on Diagram --

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void Add(Canvas diagram, Wire wire) {
			wire.PositionGlyph();
			diagram.Children.Add(wire.WireGlyph);
			this.AddWirePoint(wire.Point1);
			this.AddWirePoint(wire.Point2);
		}

		private void RedrawDiagram() {
			Canvas diagram = this.Diagram;
			diagram.Children.Clear();
			this.wirePoint.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				this.Add(diagram, wire);
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

		private void DeleteEmptyWires() {
			foreach(Wire wire in this.Project.LogicalCircuit.Wires().Where(wire => wire.Point1 == wire.Point2).ToList()) {
				this.Unselect(wire);
				wire.Delete();
			}
		}

		private Wire CreateWire(GridPoint point1, GridPoint point2) {
			Wire wire = null;
			if(point1 != point2) {
				this.CircuitProject.InTransaction(() => wire = this.CircuitProject.WireSet.Create(this.Project.LogicalCircuit, point1, point2));
				this.Add(this.Diagram, wire);
			}
			return wire;
		}

		private void CommitMove(Point point, bool withWires) {
			int dx = Symbol.GridPoint(point.X - this.moveStart.X);
			int dy = Symbol.GridPoint(point.Y - this.moveStart.Y);
			if(dx != 0 || dy != 0) {
				HashSet<GridPoint> movedPoints = null;
				if(withWires) {
					movedPoints = new HashSet<GridPoint>();
					foreach(Marker marker in this.selection.Values) {
						CircuitSymbol symbol = marker.Symbol as CircuitSymbol;
						if(symbol != null) {
							foreach(Jam jam in symbol.Jams()) {
								movedPoints.Add(jam.AbsolutePoint);
							}
						} else {
							Wire wire = marker.Symbol as Wire;
							if(wire != null) {
								movedPoints.Add(wire.Point1);
								movedPoints.Add(wire.Point2);
							}
						}
					}
				}
				this.CircuitProject.InTransaction(() => {
					foreach(Marker marker in this.selection.Values) {
						marker.Shift(dx, dy);
					}
					if(withWires) {
						foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
							if(!this.selection.ContainsKey(wire)) {
								if(movedPoints.Contains(wire.Point1)) {
									wire.X1 += dx;
									wire.Y1 += dy;
									wire.PositionGlyph();
								}
								if(movedPoints.Contains(wire.Point2)) {
									wire.X2 += dx;
									wire.Y2 += dy;
									wire.PositionGlyph();
								}
							}
						}
					}
					this.DeleteEmptyWires();
				});
			}
		}

		private void CommitMove(Point point, bool withWires, WirePointMarker marker) {
			int dx = Symbol.GridPoint(point.X - this.moveStart.X);
			int dy = Symbol.GridPoint(point.Y - this.moveStart.Y);
			if(dx != 0 || dy != 0) {
				this.CircuitProject.InTransaction(() => {
					GridPoint originalPoint = marker.WirePoint();
					marker.Shift(dx, dy);
					if(withWires && this.JamAt(originalPoint) == null) {
						foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
							if(!this.selection.ContainsKey(wire)) {
								if(originalPoint == wire.Point1) {
									wire.X1 += dx;
									wire.Y1 += dy;
									wire.PositionGlyph();
								}
								if(originalPoint == wire.Point2) {
									wire.X2 += dx;
									wire.Y2 += dy;
									wire.PositionGlyph();
								}
							}
						}
					}
					this.DeleteEmptyWires();
				});
			}
		}

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
			Tracer.Assert(project == this.Project);
			DialogProject dialog = new DialogProject(project);
			dialog.Owner = this.Mainframe;
			dialog.ShowDialog();
		}

		public void Edit(LogicalCircuit logicalCircuit) {
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

		private void AddMarkerGlyph(Marker marker) {
			if(this.selectionLayer == null) {
				this.selectionLayer = new Canvas() {
					RenderTransform = this.moveVector = new TranslateTransform()
				};
				Panel.SetZIndex(this.selectionLayer, int.MaxValue);
			}
			if(this.selectionLayer.Parent != this.Diagram) {
				this.Diagram.Children.Add(this.selectionLayer);
			}
			this.selectionLayer.Children.Add(marker.Glyph);
		}

		private Marker SelectSymbol(Symbol symbol) {
			Tracer.Assert(symbol.LogicalCircuit == this.Project.LogicalCircuit);
			Marker marker = this.FindMarker(symbol);
			if(marker == null) {
				marker = this.CreateMarker(symbol);
				this.selection.Add(symbol, marker);
				this.AddMarkerGlyph(marker);
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

		//--- Moving markers ---

		private void StartMove(Canvas diagram, Marker marker, Point startPoint, string tip) {
			Tracer.Assert(this.movingMarker == null);
			Mouse.Capture(diagram, CaptureMode.Element);
			this.movingMarker = marker;
			this.moveStart = startPoint;
			this.Mainframe.Status = tip;
		}

		private void StartMove(Canvas diagram, Marker marker, Point startPoint) {
			this.StartMove(diagram, marker, startPoint, Resources.TipOnStartMove);
		}

		private void MoveSelection(Point point) {
			this.moveVector.X = point.X - this.moveStart.X;
			this.moveVector.Y = point.Y - this.moveStart.Y;
		}

		private void CancelMove() {
			if(this.movingMarker != null) {
				Mouse.Capture(null);
				if(this.movingMarker is WirePledge || this.movingMarker is AreaMarker) {
					this.selectionLayer.Children.Remove(this.movingMarker.Glyph);
				}
				this.movingMarker = null;
				this.moveVector.X = this.moveVector.Y = 0;
			}
		}

		private void StartWire(Canvas diagram, Point point) {
			this.ClearSelection();
			this.CancelMove();
			WirePledge wirePledge = new WirePledge(point);
			this.AddMarkerGlyph(wirePledge);
			this.StartMove(diagram, wirePledge, point, Resources.TipOnStartWire);
		}

		private void StartAreaSelection(Canvas diagram, Point point) {
			this.CancelMove();
			AreaMarker marker = new AreaMarker(point);
			this.AddMarkerGlyph(marker);
			this.StartMove(diagram, marker, point, Resources.TipOnAwaitingArea(this.Project.LogicalCircuit.Name, Symbol.GridPoint(point)));
		}

		private void FinishMove(Point position, bool withWires) {
			this.movingMarker.Commit(this, position, withWires);
			this.CancelMove();
		}

		private static bool IsPinClose(Point p1, Point p2) {
			return Point.Subtract(p1, p2).LengthSquared <= Symbol.PinRadius * Symbol.PinRadius * 5;
		}

		private Wire FindWireNear(Point point) {
			Rect rect = new Rect(
				point.X - Editor.ClickProximity, point.Y - Editor.ClickProximity,
				2 * Editor.ClickProximity, 2 * Editor.ClickProximity
			);
			foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
				Point p1 = Symbol.ScreenPoint(wire.Point1);
				Point p2 = Symbol.ScreenPoint(wire.Point2);
				if(Symbol.Intersected(p1, p2, rect)) {
					return wire;
				}
			}
			return null;
		}

		private Jam JamAt(GridPoint point) {
			foreach(CircuitSymbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
				if(symbol.X <= point.X && point.X <= symbol.X + symbol.Circuit.SymbolWidth && symbol.Y <= point.Y && point.Y <= symbol.Y + symbol.Circuit.SymbolHeight) {
					foreach(Jam jam in symbol.Jams()) {
						if(jam.AbsolutePoint == point) {
							return jam;
						}
					}
				}
			}
			return null;
		}

		private Jam JamNear(Point point) {
			GridPoint gridPoint = Symbol.GridPoint(point);
			return Editor.IsPinClose(point, Symbol.ScreenPoint(gridPoint)) ? this.JamAt(gridPoint) : null;
		}

		private bool Split(Wire wire, GridPoint point) {
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			Tracer.Assert(wire.LogicalCircuit == logicalCircuit);
			if(wire.Point1 != point && wire.Point2 != point) {
				Wire wire2 = null;
				this.ClearSelection();
				this.CircuitProject.InTransaction(() => {
					wire2 = this.CircuitProject.WireSet.Create(logicalCircuit, point, wire.Point2);
					wire.Point2 = point;
				});
				wire.PositionGlyph();
				this.Add(this.Diagram, wire2);
				this.Select(wire);
				this.Select(wire2);
				return true;
			}
			return false;
		}

		private bool Merge(Wire wire1, Wire wire2) {
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			Tracer.Assert(wire1.LogicalCircuit == logicalCircuit && wire2.LogicalCircuit == logicalCircuit);
			GridPoint point1, point2;
			if(wire1.Point1 == wire2.Point1) {
				point1 = wire1.Point2;
				point2 = wire2.Point2;
			} else if(wire1.Point1 == wire2.Point2) {
				point1 = wire1.Point2;
				point2 = wire2.Point1;
			} else if(wire1.Point2 == wire2.Point1) {
				point1 = wire1.Point1;
				point2 = wire2.Point2;
			} else if(wire1.Point2 == wire2.Point2) {
				point1 = wire1.Point1;
				point2 = wire2.Point1;
			} else {
				return false;
			}
			this.ClearSelection();
			this.CircuitProject.InTransaction(() => {
				wire2.Delete();
				wire1.Point1 = point1;
				wire1.Point2 = point2;
			});
			this.Select(wire1);
			return true;
		}

		private void ShowStatus(CircuitSymbol symbol) {
			this.Mainframe.Status = symbol.Circuit.Notation + symbol.Point.ToString();
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

		public void DiagramMouseDown(Canvas diagram, MouseButtonEventArgs e) {
			FrameworkElement element = e.OriginalSource as FrameworkElement;
			Tracer.Assert(element != null);
			Marker marker = null;
			Symbol symbol = null;
			Jam jam = null;
			if(element != diagram) { // something on the diagram was clicked
				marker = element.DataContext as Marker;
				if(marker == null) {
					jam = element.DataContext as Jam;
					if(jam == null) {
						symbol = element.DataContext as Symbol;
						Tracer.Assert(symbol != null);
					} else {
						if(!(element is Ellipse)) { // Jam's notations - text on circuit symbol was clicked. Treat this as symbol click
							symbol = jam.CircuitSymbol;
						}
					}
				}
			} else { // click on the empty space of the diagram
				Point point = e.GetPosition(diagram);
				Wire wire = this.FindWireNear(point);
				if(wire != null) {
					marker = this.FindMarker(wire);
					if(marker == null) {
						symbol = wire;
					}
				} else {
					jam = this.JamNear(point);
				}
			}
			if(marker != null) {
				if(e.ClickCount < 2) {
					this.MarkerMouseDown(marker, diagram, e);
				} else {
					this.MarkerDoubleClick(marker);
				}
			} else if(symbol != null) {
				if(e.ClickCount < 2) {
					this.SymbolMouseDown(symbol, diagram, e);
				} else {
					this.SymbolDoubleClick(symbol);
				}
			} else if(jam != null) {
				if(e.ClickCount < 2) {
					this.JamMouseDown(jam, diagram, e);
				} else {
					this.SymbolDoubleClick(jam.CircuitSymbol);
				}
			} else if(this.InEditMode) { // Nothing was clicked on the diagram
				if(e.ClickCount < 2) {
					if(Keyboard.Modifiers != ModifierKeys.Shift) {
						this.ClearSelection();
					}
					this.StartAreaSelection(diagram, e.GetPosition(diagram));
				} else {
					this.ClearSelection();
					this.Edit(this.Project.LogicalCircuit);
				}
			}
		}

		public void DiagramMouseUp(Canvas diagram, MouseButtonEventArgs e) {
			if(e.ChangedButton == MouseButton.Left && this.InEditMode) {
				if(this.movingMarker != null) {
					this.FinishMove(e.GetPosition(diagram), (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
				}
			}
		}

		public void DiagramMouseMove(Canvas diagram, MouseEventArgs e) {
			if(e.LeftButton == MouseButtonState.Pressed && this.InEditMode && this.movingMarker != null) {
				this.movingMarker.Move(this, e.GetPosition(diagram));
			}
		}

		private void SymbolMouseDown(Symbol symbol, Canvas diagram, MouseButtonEventArgs e) {
			if(this.InEditMode) {
				if(e.ChangedButton == MouseButton.Left) {
					Wire wire = symbol as Wire;
					if(wire != null) {
						if(Keyboard.Modifiers == ModifierKeys.Control) {
							this.StartMove(diagram, this.SelectSymbol(wire), e.GetPosition(diagram));
						} else if(Keyboard.Modifiers == ModifierKeys.Shift) {
							this.ClearSelection();
							this.SelectConductor(wire);
							this.StartMove(diagram, this.FindMarker(wire), e.GetPosition(diagram));
						} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
							this.SelectConductor(wire);
							this.StartMove(diagram, this.FindMarker(wire), e.GetPosition(diagram));
						} else {
							this.ClearSelection();
							this.StartMove(diagram, this.SelectSymbol(wire), e.GetPosition(diagram));
						}
						this.Mainframe.Status = Resources.TipOnWireSelect;
						return;
					}

					CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
					if(circuitSymbol != null) {
						if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
							this.Select(symbol);
						} else {
							this.ClearSelection();
							this.StartMove(diagram, this.SelectSymbol(symbol), e.GetPosition(diagram));
						}
						this.ShowStatus(circuitSymbol);
						return;
					}
				}
			} else {
				CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
				if(circuitSymbol != null) {
					this.ShowStatus(circuitSymbol);
					return;
				}
			}
		}

		private void MarkerMouseDown(Marker marker, Canvas diagram, MouseButtonEventArgs e) {
			if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
				this.Unselect(marker);
				return;
			}
			WireMarker wireMarker = marker as WireMarker;
			if(wireMarker != null) {
				Wire wire = wireMarker.Wire;
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					this.ClearSelection();
					this.SelectConductor(wire);
					this.StartMove(diagram, this.FindMarker(wire), e.GetPosition(diagram));
					return;
				} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
					this.UnselectConductor(wire);
					return;
				} else if(Keyboard.Modifiers == ModifierKeys.Alt) {
					this.Split(wire, Symbol.GridPoint(e.GetPosition(diagram)));
					return;
				}
			}
			this.StartMove(diagram, marker, e.GetPosition(diagram));
			
			//} else if(Keyboard.Modifiers == ModifierKeys.Shift && marker is WirePointMarker) {
			//    Wire w = ((WirePointMarker)marker).WireMarker.Wire;
			//    this.ClearSelection();
			//    this.SelectConductor(w);
			//    this.StartMove(diagram, this.Marker(w), e.GetPosition(diagram));
			//} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control) && marker is WirePointMarker) {
			//    this.UnselectConductor(((WirePointMarker)marker).WireMarker.Wire);
			//} else if(Keyboard.Modifiers == ModifierKeys.Alt && marker is WirePointMarker && this.SelectionCount == 2) {
			//    WirePointMarker pointMarker = (WirePointMarker)marker;
			//    Wire wire1 = pointMarker.WireMarker.Wire;
			//    GridPoint point = Plotter.GridPoint(pointMarker.ScreenPoint);
			//    foreach(Symbol s in this.selection.Keys) {
			//        Wire wire2 = s as Wire;
			//        if(wire2 != null && wire2 != wire1 && (wire2.Point1 == point || wire2.Point2 == point)) {
			//            this.Merge(wire1, wire2);
			//            break;
			//        }
			//    }
		}

		private void JamMouseDown(Jam jam, Canvas diagram, MouseButtonEventArgs e) {
			if(this.InEditMode && e.ChangedButton == MouseButton.Left) {
				this.StartWire(diagram, e.GetPosition(diagram));
			}
		}

		private void SymbolDoubleClick(Symbol symbol) {
			/*if(this.InEditMode) {
				Marker marker = symbol as Marker;
				if(marker != null) {
					CircuitSymbolMarker circuitSymbolMarker = marker as CircuitSymbolMarker;
					if(circuitSymbolMarker != null) {
						LogicalCircuit lc = circuitSymbolMarker.CircuitSymbol.Circuit as LogicalCircuit;
						if(lc != null) {
							this.OpenLogicalCircuit(lc);
							return;
						}
						CircuitButton cb = circuitSymbolMarker.CircuitSymbol.Circuit as CircuitButton;
						if(cb != null) {
							this.Edit(cb);
							return;
						}
						Constant ct = circuitSymbolMarker.CircuitSymbol.Circuit as Constant;
						if(ct != null) {
							this.Edit(ct);
							return;
						}
						Memory m = circuitSymbolMarker.CircuitSymbol.Circuit as Memory;
						if(m != null) {
							this.Edit(m);
							return;
						}
						Pin pin = circuitSymbolMarker.CircuitSymbol.Circuit as Pin;
						if(pin != null) {
							this.Edit(pin);
							return;
						}
					}
				}
			} else {
				
			}*/
		}
		private void MarkerDoubleClick(Marker marker) {
			if(this.InEditMode) {
				CircuitSymbolMarker circuitSymbolMarker = marker as CircuitSymbolMarker;
				if(circuitSymbolMarker != null) {
					LogicalCircuit lc = circuitSymbolMarker.CircuitSymbol.Circuit as LogicalCircuit;
					if(lc != null) {
						this.OpenLogicalCircuit(lc);
						return;
					}
					CircuitButton cb = circuitSymbolMarker.CircuitSymbol.Circuit as CircuitButton;
					if(cb != null) {
						this.Edit(cb);
						return;
					}
					Constant ct = circuitSymbolMarker.CircuitSymbol.Circuit as Constant;
					if(ct != null) {
						this.Edit(ct);
						return;
					}
					Memory m = circuitSymbolMarker.CircuitSymbol.Circuit as Memory;
					if(m != null) {
						this.Edit(m);
						return;
					}
					Pin pin = circuitSymbolMarker.CircuitSymbol.Circuit as Pin;
					if(pin != null) {
						this.Edit(pin);
						return;
					}
				}
			}
		}
	}
}
