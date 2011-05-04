using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using LogicCircuit.DataPersistent;

namespace LogicCircuit {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public abstract partial class EditorDiagram {

		private const int ClickProximity = 2 * Symbol.PinRadius;
		protected static readonly string CircuitDescriptorDataFormat = "LogicCircuit.CircuitDescriptor." + Process.GetCurrentProcess().Id;

		private struct Connect {
			public int Count;
			public Ellipse Solder;
		}

		public Mainframe Mainframe { get; private set; }
		private Dispatcher Dispatcher { get { return this.Mainframe.Dispatcher; } }
		private Canvas Diagram { get { return this.Mainframe.Diagram; } }

		public CircuitProject CircuitProject { get; private set; }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }

		private bool refreshPending;

		public abstract bool InEditMode { get; }

		private LogicalCircuit currentLogicalCircuit;
		private readonly Dictionary<GridPoint, Connect> wirePoint = new Dictionary<GridPoint, Connect>();

		private readonly Dictionary<Symbol, Marker> selection = new Dictionary<Symbol, Marker>();
		private Canvas selectionLayer;

		private Marker movingMarker;
		private Point moveStart;
		private TranslateTransform moveVector;

		protected EditorDiagram(Mainframe mainframe, CircuitProject circuitProject) {
			this.Mainframe = mainframe;
			this.CircuitProject = circuitProject;
			this.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
			this.CircuitProject.CircuitSymbolSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.CircuitSymbolSetCollectionChanged);
			this.CircuitProject.WireSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.WireSetCollectionChanged);
			this.CircuitProject.WireSet.WireSetChanged += new EventHandler(this.WireSetChanged);
			this.CircuitProject.VersionChanged += new EventHandler<DataPersistent.VersionChangeEventArgs>(this.CircuitProjectVersionChanged);
			this.Refresh();
		}

		//--- Handling model (CircuitProject) changes ---

		protected abstract void UpdateGlyph(LogicalCircuit logicalCircuit);

		private void CircuitProjectVersionChanged(object sender, VersionChangeEventArgs e) {
			if(this.refreshPending) {
				this.refreshPending = false;
				this.Refresh();
			} else {
				foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.Invalid) {
					if(symbol.LogicalCircuit == this.Project.LogicalCircuit) {
						this.Diagram.Children.Remove(symbol.Glyph);
						symbol.Reset();
						this.Add(symbol);
						CircuitSymbolMarker marker = (CircuitSymbolMarker)this.FindMarker(symbol);
						if(marker != null) {
							marker.Invalidate();
						}
					} else {
						symbol.Reset();
					}
				}
			}
			this.CircuitProject.CircuitSymbolSet.ValidateAll();
			if(this.CircuitProject.Version == this.Project.LogicalCircuit.PinVersion) {
				this.UpdateGlyph(this.Project.LogicalCircuit);
			}
		}

		private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
			this.OnProjectPropertyChanged(e.PropertyName);
		}

		protected virtual void OnProjectPropertyChanged(string propertyName) {
			if(propertyName == "LogicalCircuit") {
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
					this.refreshPending = true;
				}
			}
		}

		private void CircuitSymbolSetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if(!this.refreshPending) {
				if(e.OldItems != null && 0 < e.OldItems.Count) {
					foreach(CircuitSymbol symbol in e.OldItems) {
						if(symbol.HasCreatedGlyph) {
							this.Unselect(symbol);
							FrameworkElement glyph = symbol.Glyph;
							if(glyph.Parent == this.Diagram) {
								this.Diagram.Children.Remove(glyph);
							}
							Tracer.Assert(glyph.Parent == null);
						}
					}
				}
				if(e.NewItems != null && 0 < e.NewItems.Count) {
					LogicalCircuit current = this.Project.LogicalCircuit;
					foreach(CircuitSymbol symbol in e.NewItems) {
						if(symbol.LogicalCircuit == current) {
							this.Add(symbol);
						}
					}
				}
			}
		}

		private void WireSetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if(!this.refreshPending) {
				if(e.OldItems != null && 0 < e.OldItems.Count) {
					foreach(Wire wire in e.OldItems) {
						if(wire.HasCreatedGlyph) {
							this.Unselect(wire);
							Line line = wire.WireGlyph;
							if(line.Parent == this.Diagram) {
								this.Diagram.Children.Remove(line);
							}
							Tracer.Assert(line.Parent == null);
						}
					}
				}
				if(e.NewItems != null && 0 < e.NewItems.Count) {
					LogicalCircuit current = this.Project.LogicalCircuit;
					foreach(Wire wire in e.NewItems) {
						if(wire.LogicalCircuit == current) {
							this.Add(wire);
						}
					}
				}
			}
		}

		private void WireSetChanged(object sender, EventArgs e) {
			if(!this.refreshPending) {
				this.UpdateSolders();
			}
		}

		//--- Utils ---

		private void ShowStatus(CircuitSymbol symbol) {
			this.Mainframe.Status = symbol.Circuit.Notation + symbol.Point.ToString();
		}

		//--- Drawing ---

		protected abstract void ButtonIsPressedChanged(CircuitSymbol symbol, bool isPressed);

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void Add(Wire wire) {
			wire.PositionGlyph();
			this.Diagram.Children.Add(wire.WireGlyph);
		}

		private void Add(CircuitSymbol symbol) {
			symbol.PositionGlyph();
			this.Diagram.Children.Add(symbol.Glyph);
			ButtonControl button = symbol.ProbeView as ButtonControl;
			if(button != null) {
				button.ButtonPressed = this.ButtonIsPressedChanged;
			}
		}

		private void AddWirePoint(GridPoint point) {
			Connect connect;
			if(!this.wirePoint.TryGetValue(point, out connect)) {
				connect = new Connect();
			}
			connect.Count++;
			if(2 < connect.Count && connect.Solder == null) {
				connect.Solder = new Ellipse();
				Panel.SetZIndex(connect.Solder, 0);
				connect.Solder.Width = connect.Solder.Height = 2 * Symbol.PinRadius;
				Canvas.SetLeft(connect.Solder, Symbol.ScreenPoint(point.X) - Symbol.PinRadius);
				Canvas.SetTop(connect.Solder, Symbol.ScreenPoint(point.Y) - Symbol.PinRadius);
				connect.Solder.Fill = Symbol.JamDirectFill;
				this.Diagram.Children.Add(connect.Solder);
			}
			this.wirePoint[point] = connect;
		}

		private void UpdateSolders() {
			foreach(Connect connect in this.wirePoint.Values) {
				if(connect.Solder != null) {
					this.Diagram.Children.Remove(connect.Solder);
				}
			}
			this.wirePoint.Clear();
			foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
				this.AddWirePoint(wire.Point1);
				this.AddWirePoint(wire.Point2);
			}
		}

		private void RedrawDiagram() {
			this.Diagram.Children.Clear();
			this.wirePoint.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				this.Add(wire);
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				this.Add(symbol);
			}
			this.UpdateSolders();
		}

		//--- Diagram Primitives ---

		protected abstract void Edit(Symbol symbol);
		public abstract void Edit(LogicalCircuit logicalCircuit);

		private static bool IsPinClose(Point p1, Point p2) {
			return Point.Subtract(p1, p2).LengthSquared <= Symbol.PinRadius * Symbol.PinRadius * 5;
		}

		private Wire FindWireNear(Point point) {
			Rect rect = new Rect(
				point.X - EditorDiagram.ClickProximity, point.Y - EditorDiagram.ClickProximity,
				2 * EditorDiagram.ClickProximity, 2 * EditorDiagram.ClickProximity
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
			}
			return wire;
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

		//--- Selection ---

		public int SelectionCount { get { return this.selection.Count; } }

		public IEnumerable<Symbol> SelectedSymbols { get { return this.selection.Keys; } }

		public IEnumerable<Symbol> Selection() {
			return new List<Symbol>(this.selection.Keys);
		}

		public void ClearSelection() {
			this.selection.Clear();
			if(this.selectionLayer != null) {
				this.selectionLayer.Children.Clear();
			}
		}

		private static Marker CreateMarker(Symbol symbol) {
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
				marker = EditorDiagram.CreateMarker(symbol);
				this.selection.Add(symbol, marker);
				this.AddMarkerGlyph(marker);
			}
			return marker;
		}

		public void Select(Symbol symbol) {
			this.SelectSymbol(symbol);
		}

		public void Unselect(Symbol symbol) {
			Marker marker = this.FindMarker(symbol);
			if(marker != null) {
				this.selection.Remove(marker.Symbol);
				this.selectionLayer.Children.Remove(marker.Glyph);
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

		private void StartMove(Marker marker, Point startPoint, string tip) {
			Tracer.Assert(this.movingMarker == null);
			Tracer.Assert(marker != null);
			Mouse.Capture(this.Diagram, CaptureMode.Element);
			this.movingMarker = marker;
			this.moveStart = startPoint;
			this.Mainframe.Status = tip;
		}

		private void StartMove(Marker marker, Point startPoint) {
			this.StartMove(marker, startPoint, Resources.TipOnStartMove);
		}

		private void MoveSelection(Point point) {
			this.moveVector.X = point.X - this.moveStart.X;
			this.moveVector.Y = point.Y - this.moveStart.Y;
		}

		protected void CancelMove() {
			if(this.movingMarker != null) {
				Mouse.Capture(null);
				if(this.movingMarker is WirePledge || this.movingMarker is AreaMarker) {
					this.selectionLayer.Children.Remove(this.movingMarker.Glyph);
				}
				this.movingMarker = null;
				this.moveVector.X = this.moveVector.Y = 0;
			}
		}

		private void StartWire(Point point) {
			this.ClearSelection();
			this.CancelMove();
			WirePledge wirePledge = new WirePledge(point);
			this.AddMarkerGlyph(wirePledge);
			this.StartMove(wirePledge, point, Resources.TipOnStartWire);
		}

		private void StartAreaSelection(Point point) {
			this.CancelMove();
			AreaMarker marker = new AreaMarker(point);
			this.AddMarkerGlyph(marker);
			this.StartMove(marker, point, Resources.TipOnAwaitingArea(this.Project.LogicalCircuit.Name, Symbol.GridPoint(point)));
		}

		private void FinishMove(Point position, bool withWires) {
			this.movingMarker.Commit(this, position, withWires);
			this.CancelMove();
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

		//--- User Input Event Handling ---

		public void DiagramDragOver(DragEventArgs e) {
			if(this.InEditMode && e.Data.GetDataPresent(EditorDiagram.CircuitDescriptorDataFormat, false)) {
				e.Effects = DragDropEffects.Copy | DragDropEffects.Scroll;
			} else {
				e.Effects = DragDropEffects.None;
			}
			e.Handled = true;
		}

		public void DiagramDrop(DragEventArgs e) {
			if(this.InEditMode) {
				ICircuitDescriptor descriptor = e.Data.GetData(EditorDiagram.CircuitDescriptorDataFormat, false) as ICircuitDescriptor;
				if(descriptor != null) {
					GridPoint point = Symbol.GridPoint(e.GetPosition(this.Diagram));
					this.CircuitProject.InTransaction(() => {
						this.CircuitProject.CircuitSymbolSet.Create(descriptor.GetCircuitToDrop(this.CircuitProject), this.Project.LogicalCircuit, point.X, point.Y);
					});
				}
			}
			e.Handled = true;
		}

		public void DiagramMouseDown(MouseButtonEventArgs e) {
			FrameworkElement element = e.OriginalSource as FrameworkElement;
			Tracer.Assert(element != null);
			Marker marker = null;
			Symbol symbol = null;
			Jam jam = null;
			if(element != this.Diagram) { // something on the diagram was clicked
				marker = element.DataContext as Marker;
				if(marker == null) {
					jam = element.DataContext as Jam;
					if(jam == null) {
						symbol = element.DataContext as Symbol;
						if(symbol == null) {
							FrameworkElement root = element;
							while(root != null && !(root.DataContext is Symbol)) {
								root = (root.Parent ?? root.TemplatedParent) as FrameworkElement;
							}
							if(root != null) {
								symbol = root.DataContext as Symbol;
							}
						}
						Tracer.Assert(symbol != null);
					} else {
						if(!(element is Ellipse)) { // Jam's notations - text on circuit symbol was clicked. Treat this as symbol click
							symbol = jam.CircuitSymbol;
						}
					}
				}
			} else { // click on the empty space of the diagram
				Point point = e.GetPosition(this.Diagram);
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
					this.MarkerMouseDown(marker, e);
				} else {
					this.MarkerDoubleClick(marker);
				}
			} else if(symbol != null) {
				if(e.ClickCount < 2) {
					this.SymbolMouseDown(symbol, e);
				} else {
					this.Edit(symbol);
				}
			} else if(jam != null) {
				if(e.ClickCount < 2) {
					this.JamMouseDown(e);
				} else {
					this.Edit(jam.CircuitSymbol);
				}
			} else if(this.InEditMode) { // Nothing was clicked on the diagram
				if(e.ClickCount < 2) {
					if(Keyboard.Modifiers != ModifierKeys.Shift) {
						this.ClearSelection();
					}
					this.StartAreaSelection(e.GetPosition(this.Diagram));
				} else {
					this.ClearSelection();
					this.Edit(this.Project.LogicalCircuit);
				}
			}
		}

		public void DiagramMouseUp(MouseButtonEventArgs e) {
			if(e.ChangedButton == MouseButton.Left && this.InEditMode) {
				if(this.movingMarker != null) {
					this.FinishMove(e.GetPosition(this.Diagram), (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
				}
			}
		}

		public void DiagramMouseMove(MouseEventArgs e) {
			if(e.LeftButton == MouseButtonState.Pressed && this.InEditMode && this.movingMarker != null) {
				this.movingMarker.Move(this, e.GetPosition(this.Diagram));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private void SymbolMouseDown(Symbol symbol, MouseButtonEventArgs e) {
			if(this.InEditMode) {
				if(e.ChangedButton == MouseButton.Left) {
					Wire wire = symbol as Wire;
					if(wire != null) {
						if(Keyboard.Modifiers == ModifierKeys.Control) {
							this.StartMove(this.SelectSymbol(wire), e.GetPosition(this.Diagram));
						} else if(Keyboard.Modifiers == ModifierKeys.Shift) {
							this.ClearSelection();
							this.SelectConductor(wire);
							this.StartMove(this.FindMarker(wire), e.GetPosition(this.Diagram));
						} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
							this.SelectConductor(wire);
							this.StartMove(this.FindMarker(wire), e.GetPosition(this.Diagram));
						} else {
							Point point = e.GetPosition(this.Diagram);
							if(Editor.IsPinClose(point, Symbol.ScreenPoint(wire.Point1)) || Editor.IsPinClose(point, Symbol.ScreenPoint(wire.Point2))) {
								this.StartWire(point);
								return;
							}
							this.ClearSelection();
							this.StartMove(this.SelectSymbol(wire), e.GetPosition(this.Diagram));
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
							this.StartMove(this.SelectSymbol(symbol), e.GetPosition(this.Diagram));
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

		private void MarkerMouseDown(Marker marker, MouseButtonEventArgs e) {
			if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
				this.Unselect(marker.Symbol);
				return;
			}

			WireMarker wireMarker = marker as WireMarker;
			if(wireMarker != null) {
				Wire wire = wireMarker.Wire;
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					this.ClearSelection();
					this.SelectConductor(wire);
					this.StartMove(this.FindMarker(wire), e.GetPosition(this.Diagram));
					return;
				} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
					this.UnselectConductor(wire);
					return;
				} else if(Keyboard.Modifiers == ModifierKeys.Alt) {
					this.Split(wire, Symbol.GridPoint(e.GetPosition(this.Diagram)));
					return;
				}
			}

			WirePointMarker wirePointMarker = marker as WirePointMarker;
			if(wirePointMarker != null) {
				Wire wire = wirePointMarker.Parent.Wire;
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					this.ClearSelection();
					this.SelectConductor(wire);
					this.StartMove(this.FindMarker(wire), e.GetPosition(this.Diagram));
					return;
				} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
					this.UnselectConductor(wire);
					return;
				} else if(Keyboard.Modifiers == ModifierKeys.Alt && this.SelectionCount == 2) {
					GridPoint point = wirePointMarker.WirePoint();
					foreach(Symbol symbol in this.selection.Keys) {
						Wire wire2 = symbol as Wire;
						if(wire2 != null && wire2 != wire && (wire2.Point1 == point || wire2.Point2 == point)) {
							this.Merge(wire, wire2);
							break;
						}
					}
					return;
				}
			}

			this.StartMove(marker, e.GetPosition(this.Diagram));
		}

		private void MarkerDoubleClick(Marker marker) {
			if(this.InEditMode) {
				CircuitSymbolMarker circuitSymbolMarker = marker as CircuitSymbolMarker;
				if(circuitSymbolMarker != null) {
					this.Edit(circuitSymbolMarker.CircuitSymbol);
				}
			}
		}

		private void JamMouseDown(MouseButtonEventArgs e) {
			if(this.InEditMode && e.ChangedButton == MouseButton.Left) {
				this.StartWire(e.GetPosition(this.Diagram));
			}
		}
	}
}
