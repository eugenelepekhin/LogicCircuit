﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public abstract partial class EditorDiagram {

		private const int ClickProximity = 2 * Symbol.PinRadius;
		protected static readonly string CircuitDescriptorDataFormat = "LogicCircuit.CircuitDescriptor." + Environment.ProcessId;

		public Mainframe Mainframe { get; private set; }
		private Dispatcher Dispatcher { get { return this.Mainframe.Dispatcher; } }
		protected Canvas Diagram { get { return this.Mainframe.Diagram; } }

		public CircuitProject CircuitProject { get; private set; }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }

		private bool refreshPending;
		private readonly WireValidator wireValidator;

		public abstract bool InEditMode { get; }

		private LogicalCircuit? currentLogicalCircuit;
		private readonly Dictionary<GridPoint, Ellipse> wireJunctionPoint = new Dictionary<GridPoint, Ellipse>();

		private readonly Dictionary<Symbol, Marker> selection = new Dictionary<Symbol, Marker>();
		private Canvas? selectionLayer;

		private Marker? movingMarker;
		private Point moveStart;
		private Point panStart;
		private bool panning;
		private TranslateTransform? moveVector;
		private Point maxMove;

		protected EditorDiagram(Mainframe mainframe, CircuitProject circuitProject) {
			this.Mainframe = mainframe;
			this.CircuitProject = circuitProject;
			this.wireValidator = new WireValidator(this);
			this.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
			this.CircuitProject.LogicalCircuitSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.LogicalCircuitSetCollectionChanged);
			this.CircuitProject.CircuitSymbolSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.CircuitSymbolSetCollectionChanged);
			this.CircuitProject.WireSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.WireSetCollectionChanged);
			this.CircuitProject.WireSet.WireSetChanged += new EventHandler(this.WireSetChanged);
			this.CircuitProject.TextNoteSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.TextNoteSetCollectionChanged);
			this.CircuitProject.VersionChanged += new EventHandler<DataPersistent.VersionChangeEventArgs>(this.CircuitProjectVersionChanged);
			this.Refresh();
		}

		//--- Handling model (CircuitProject) changes ---

		protected abstract void UpdateGlyph(LogicalCircuit logicalCircuit);

		private void CircuitProjectVersionChanged(object? sender, VersionChangeEventArgs e) {
			this.UpdateAllDisplay();
			if(this.refreshPending) {
				this.refreshPending = false;
				this.Refresh();
			} else {
				foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.Invalid) {
					if(symbol.LogicalCircuit == this.Project.LogicalCircuit) {
						this.Diagram.Children.Remove(symbol.Glyph);
						symbol.Reset();
						this.Add(symbol);
						Marker? marker = this.FindMarker(symbol);
						if(marker != null) {
							marker.Refresh();
						}
					} else {
						symbol.Reset();
					}
				}
				this.wireValidator.Update();
			}
			this.CircuitProject.CircuitSymbolSet.ValidateAll();
			if(this.CircuitProject.Version == this.Project.LogicalCircuit.PinVersion) {
				this.UpdateGlyph(this.Project.LogicalCircuit);
			}
		}

		private void UpdateAllDisplay() {
			if(this.InEditMode) {
				HashSet<LogicalCircuit> updated = new HashSet<LogicalCircuit>();
				foreach(LogicalCircuit circuit in this.CircuitProject.LogicalCircuitSet.Invalid) {
					circuit.ResetPins();
				}
				foreach(LogicalCircuit circuit in this.CircuitProject.LogicalCircuitSet.Invalid) {
					this.UpdateDisplay(circuit, updated);
				}
				this.CircuitProject.LogicalCircuitSet.ValidateAll();
			}
		}

		private void UpdateDisplay(LogicalCircuit display, HashSet<LogicalCircuit> updated) {
			if(updated.Add(display)) {
				display.ResetPins();
				this.UpdateGlyph(display);
				foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet.SelectByCircuit(display)) {
					symbol.ResetJams();
					symbol.Invalidate();
					if(symbol.LogicalCircuit.IsDisplay) {
						this.UpdateDisplay(symbol.LogicalCircuit, updated);
					}
				}
			}
		}

		private void ProjectPropertyChanged(object? sender, PropertyChangedEventArgs e) {
			this.OnProjectPropertyChanged(e.PropertyName);
		}

		protected virtual void OnProjectPropertyChanged(string? propertyName) {
			if(propertyName == nameof(this.Project.LogicalCircuit)) {
				this.CancelMove();
				this.ClearSelection();
				if(this.currentLogicalCircuit != this.Project.LogicalCircuit) {
					if(this.currentLogicalCircuit != null && !this.currentLogicalCircuit.IsDeleted()) {
						this.currentLogicalCircuit.ScrollOffset = this.Mainframe.ScrollOffset;
					}
					this.currentLogicalCircuit = this.Project.LogicalCircuit;
					this.Mainframe.ScrollOffset = this.currentLogicalCircuit.ScrollOffset;
					this.refreshPending = true;
				}
			}
		}

		private void LogicalCircuitSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			// As it is not possible to check if deleted was display or not, lets invalidate all displays.
			foreach(LogicalCircuit circuit in this.CircuitProject.LogicalCircuitSet.Where(c => c.IsDisplay)) {
				this.CircuitProject.LogicalCircuitSet.Invalidate(circuit);
			}
		}

		private void CircuitSymbolSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			bool invalidateAllDisplays = false;
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
						LogicalCircuit? circuit = symbol.CachedLogicCircuit;
						if(circuit != null && !circuit.IsDeleted() && circuit.IsDisplay) {
							invalidateAllDisplays = true;
						} else if(circuit == null) {
							invalidateAllDisplays = true;
						}
					}
				}
				if(e.NewItems != null && 0 < e.NewItems.Count) {
					LogicalCircuit current = this.Project.LogicalCircuit;
					foreach(CircuitSymbol symbol in e.NewItems) {
						if(symbol.LogicalCircuit == current) {
							this.Add(symbol);
						}
						if(symbol.LogicalCircuit.IsDisplay && (symbol.Circuit.IsDisplay || symbol.Circuit is Pin)) {
							this.CircuitProject.LogicalCircuitSet.Invalidate(symbol.LogicalCircuit);
						}
					}
				}
			}
			if(invalidateAllDisplays) {
				foreach(LogicalCircuit circuit in this.CircuitProject.LogicalCircuitSet.Where(c => c.IsDisplay)) {
					this.CircuitProject.LogicalCircuitSet.Invalidate(circuit);
				}
			}
		}

		private void WireSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
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

		private void WireSetChanged(object? sender, EventArgs e) {
			if(!this.refreshPending) {
				this.UpdateSolders();
			}
		}

		private void TextNoteSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
			if(!this.refreshPending) {
				if(e.OldItems != null && 0 < e.OldItems.Count) {
					foreach(TextNote symbol in e.OldItems) {
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
					foreach(TextNote symbol in e.NewItems) {
						if(symbol.LogicalCircuit == current) {
							this.Add(symbol);
						}
					}
				}
			}
		}

		//--- Utilities ---

		private void ShowStatus(CircuitSymbol symbol) {
			this.Mainframe.Status = symbol.Circuit.ToolTip + symbol.Point.ToString();
		}

		//--- Drawing ---

		public void Refresh() {
			this.wireValidator.Reset();
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void Add(TextNote textNote) {
			textNote.PositionGlyph();
			this.Diagram.Children.Add(textNote.TextNoteGlyph);
		}

		private void Add(Wire wire) {
			wire.PositionGlyph();
			this.Diagram.Children.Add(wire.WireGlyph);
		}

		private void Add(CircuitSymbol symbol) {
			symbol.PositionGlyph();
			this.Diagram.Children.Add(symbol.Glyph);
		}

		private void UpdateSolders() {
			ConductorMap map = this.Project.LogicalCircuit.ConductorMap();
			List<GridPoint> deleted = new List<GridPoint>();
			foreach(KeyValuePair<GridPoint, Ellipse> pair in this.wireJunctionPoint) {
				if(map.JunctionCount(pair.Key) < 3) {
					this.Diagram.Children.Remove(pair.Value);
					deleted.Add(pair.Key);
				}
			}
			foreach(GridPoint point in deleted) {
				this.wireJunctionPoint.Remove(point);
			}
			foreach(GridPoint point in map.JunctionPoints(3)) {
				if(!this.wireJunctionPoint.ContainsKey(point)) {
					Ellipse solder = new Ellipse();
					Panel.SetZIndex(solder, 0);
					solder.Width = solder.Height = 2 * Symbol.PinRadius;
					Canvas.SetLeft(solder, Symbol.ScreenPoint(point.X) - Symbol.PinRadius);
					Canvas.SetTop(solder, Symbol.ScreenPoint(point.Y) - Symbol.PinRadius);
					solder.Fill = Symbol.JamDirectFill;
					solder.DataContext = this.wireJunctionPoint;
					this.Diagram.Children.Add(solder);

					this.wireJunctionPoint.Add(point, solder);
				}
			}
		}

		private void RedrawDiagram() {
			this.Diagram.Children.Clear();
			this.wireJunctionPoint.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(TextNote symbol in logicalCircuit.TextNotes()) {
				this.Add(symbol);
			}
			foreach(Wire wire in logicalCircuit.Wires()) {
				this.Add(wire);
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				this.Add(symbol);
			}
			this.UpdateSolders();
			this.wireValidator.Update();
		}

		//--- Diagram Primitives ---

		protected abstract void Edit(Symbol symbol);
		public abstract void Edit(LogicalCircuit logicalCircuit);

		private static bool IsPinClose(Point p1, Point p2) {
			return Point.Subtract(p1, p2).LengthSquared <= Symbol.PinRadius * Symbol.PinRadius * 5;
		}

		private static Rect ClickArea(Point point) {
			return new Rect(
				point.X - EditorDiagram.ClickProximity, point.Y - EditorDiagram.ClickProximity,
				2 * EditorDiagram.ClickProximity, 2 * EditorDiagram.ClickProximity
			);
		}

		private Wire? FindWireNear(Point point) {
			Rect rect = EditorDiagram.ClickArea(point);
			foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
				if(Symbol.Intersected(Symbol.ScreenPoint(wire.Point1), Symbol.ScreenPoint(wire.Point2), rect)) {
					return wire;
				}
			}
			return null;
		}

		private Jam? JamAt(GridPoint point) {
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

		private Jam? JamNear(Point point) {
			GridPoint gridPoint = Symbol.GridPoint(point);
			return Editor.IsPinClose(point, Symbol.ScreenPoint(gridPoint)) ? this.JamAt(gridPoint) : null;
		}

		private void DeleteEmptyWires() {
			foreach(Wire wire in this.Project.LogicalCircuit.Wires().Where(wire => wire.Point1 == wire.Point2).ToList()) {
				this.Unselect(wire);
				wire.Delete();
			}
		}

		private Wire? CreateWire(GridPoint point1, GridPoint point2) {
			Wire? wire = null;
			if(point1 != point2) {
				this.CircuitProject.InTransaction(() => wire = this.CircuitProject.WireSet.Create(this.Project.LogicalCircuit, point1, point2));
			}
			return wire;
		}

		private bool Split(Wire wire, GridPoint point) {
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			Tracer.Assert(wire.LogicalCircuit == logicalCircuit);
			if(wire.Point1 != point && wire.Point2 != point) {
				Wire? wire2 = null;
				this.ClearSelection();
				this.CircuitProject.InTransaction(() => {
					wire2 = this.CircuitProject.WireSet.Create(logicalCircuit, point, wire.Point2);
					wire.Point2 = point;
				});
				this.Select(wire);
				this.Select(wire2!);
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
			if(symbol is CircuitSymbol circuitSymbol) {
				if(circuitSymbol.Circuit is CircuitButton) {
					return new ButtonMarker(circuitSymbol);
				} else {
					return new CircuitSymbolMarker(circuitSymbol);
				}
			}
			if(symbol is Wire wire) {
				return new WireMarker(wire);
			}
			if(symbol is TextNote textNote) {
				return new TextNoteMarker(textNote);
			}
			throw new InvalidOperationException();
		}

		private Marker? FindMarker(Symbol symbol) {
			if(this.selection.TryGetValue(symbol, out Marker? marker)) {
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
			Marker? marker = this.FindMarker(symbol);
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
			Marker? marker = this.FindMarker(symbol);
			if(marker != null) {
				this.selection.Remove(marker.Symbol);
				this.selectionLayer!.Children.Remove(marker.Glyph);
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
				if(symbol.Rotation != Rotation.Up) {
					item = Symbol.Transform(item, Symbol.RotationTransform(symbol.Rotation, symbol.X, symbol.Y, symbol.Circuit.SymbolWidth, symbol.Circuit.SymbolHeight));
				}
				if(area.Contains(item)) {
					this.Select(symbol);
				}
			}
			foreach(Wire wire in logicalCircuit.Wires()) {
				if(area.Contains(Symbol.ScreenPoint(wire.Point1)) && area.Contains(Symbol.ScreenPoint(wire.Point2))) {
					this.Select(wire);
				}
			}
			foreach(TextNote symbol in logicalCircuit.TextNotes()) {
				Rect item = new Rect(Symbol.ScreenPoint(symbol.Point),
					new Size(Symbol.ScreenPoint(symbol.Width), Symbol.ScreenPoint(symbol.Height))
				);
				if(symbol.Rotation != Rotation.Up) {
					item = Symbol.Transform(item, Symbol.RotationTransform(symbol.Rotation, symbol.X, symbol.Y, symbol.Width, symbol.Height));
				}
				if(area.Contains(item)) {
					this.Select(symbol);
				}
			}
		}

		private void SelectConductor(Wire wire) {
			Tracer.Assert(wire.LogicalCircuit == this.Project.LogicalCircuit);
			ConductorMap map = this.Project.LogicalCircuit.ConductorMap();
			if(map.TryGetValue(wire.Point1, out Conductor? conductor)) {
				this.Select(conductor!.Wires);
			}
		}

		private void UnselectConductor(Wire wire) {
			Tracer.Assert(wire.LogicalCircuit == this.Project.LogicalCircuit);
			ConductorMap map = this.Project.LogicalCircuit.ConductorMap();
			if(map.TryGetValue(wire.Point1, out Conductor? conductor)) {
				foreach(Wire w in conductor!.Wires) {
					this.Unselect(w);
				}
			}
		}

		//--- Moving markers ---

		private void StartMove(Marker marker, Point startPoint, string tip) {
			Tracer.Assert(this.movingMarker == null);
			Tracer.Assert(marker);
			Mouse.Capture(this.Diagram, CaptureMode.Element);
			this.movingMarker = marker;
			this.moveStart = startPoint;
			this.Mainframe.Status = tip;

			if(marker is WirePledge || marker is AreaMarker) {
				this.maxMove = new Point(0, 0);
			} else {
				Rect bound = marker.Bounds();
				if(bound.Width < 3 * Symbol.PinRadius && this.SelectionCount == 1) {
					this.maxMove = new Point(0, 0);
				} else {
					bound = this.Selection().Aggregate(new Rect(startPoint, startPoint), (rect, symbol) => Rect.Union(rect, symbol.Bounds()));
					this.maxMove = new Point(
						startPoint.X - bound.X,
						startPoint.Y - bound.Y
					);
				}
			}
		}

		private void StartMove(Marker marker, Point startPoint) {
			this.StartMove(marker, startPoint, Properties.Resources.TipOnStartMove);
		}

		private void MoveSelection(Point point) {
			Debug.Assert(this.moveVector != null);
			this.moveVector.X = point.X - this.moveStart.X;
			this.moveVector.Y = point.Y - this.moveStart.Y;
		}

		protected void CancelMove() {
			if(this.movingMarker != null) {
				Debug.Assert(this.selectionLayer != null && this.moveVector != null);
				Mouse.Capture(null);
				this.movingMarker.CancelMove(this.selectionLayer);
				this.movingMarker = null;
				this.moveVector.X = this.moveVector.Y = 0;
			}
		}

		private void StartWire(Point point) {
			this.ClearSelection();
			this.CancelMove();
			WirePledge wirePledge = new WirePledge(point);
			this.AddMarkerGlyph(wirePledge);
			this.StartMove(wirePledge, point, Properties.Resources.TipOnStartWire);
		}

		private void StartAreaSelection(Point point) {
			this.CancelMove();
			AreaMarker marker = new AreaMarker(point);
			this.AddMarkerGlyph(marker);
			this.StartMove(marker, point, Properties.Resources.TipOnAwaitingArea(this.Project.LogicalCircuit.Name, Symbol.GridPoint(point)));
		}

		private void FinishMove(Point position, bool withWires) {
			Debug.Assert(this.movingMarker != null);
			Marker marker = this.movingMarker;
			this.CancelMove();
			marker.Commit(this, new Point(Math.Max(this.maxMove.X, position.X), Math.Max(this.maxMove.Y, position.Y)), withWires);
		}

		private void CommitMove(Point point, bool withWires) {
			int dx = Symbol.GridPoint(point.X - this.moveStart.X);
			int dy = Symbol.GridPoint(point.Y - this.moveStart.Y);
			if(dx != 0 || dy != 0) {
				HashSet<GridPoint>? movedPoints = null;
				if(withWires) {
					movedPoints = new HashSet<GridPoint>();
					foreach(Marker marker in this.selection.Values) {
						if(marker.Symbol is CircuitSymbol symbol) {
							foreach(Jam jam in symbol.Jams()) {
								movedPoints.Add(jam.AbsolutePoint);
							}
						} else {
							if(marker.Symbol is Wire wire) {
								movedPoints.Add(wire.Point1);
								movedPoints.Add(wire.Point2);
							}
						}
					}
				}
				this.CircuitProject.InTransaction(() => {
					foreach(Marker marker in this.selection.Values) {
						marker.Symbol.Shift(dx, dy);
						marker.Symbol.PositionGlyph();
						marker.Refresh();
					}
					if(withWires) {
						Debug.Assert(movedPoints != null);
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

		private void CommitMove(TextNoteMarker marker) {
			TextNote textNote = marker.TextNote;
			Rect rect = marker.ResizedRect();

			int x = Symbol.GridPoint(rect.X);
			int y = Symbol.GridPoint(rect.Y);
			int w = Math.Max(Symbol.GridPoint(rect.Width), 1);
			int h = Math.Max(Symbol.GridPoint(rect.Height), 1);

			if(x != textNote.X || y != textNote.Y || w != textNote.Width || h != textNote.Height) {
				this.CircuitProject.InTransaction(() => {
					textNote.X = x;
					textNote.Y = y;
					textNote.Width = w;
					textNote.Height = h;
				});
			}
			marker.Refresh();
		}

		private void CommitMove(ButtonMarker marker, bool withWires) {
			CircuitSymbol symbol = (CircuitSymbol)marker.Symbol;
			CircuitButton button = (CircuitButton)symbol.Circuit;
			Tracer.Assert(symbol.LogicalCircuit == this.Project.LogicalCircuit);
			Tracer.Assert(this.SelectedSymbols.Count() == 1 && this.SelectedSymbols.First() == symbol);
			GridPoint originalPoint = symbol.Jams().First().AbsolutePoint;
			Rect rect = marker.ResizedRect();

			int x = Symbol.GridPoint(rect.X);
			int y = Symbol.GridPoint(rect.Y);
			int w = Math.Max(2, Math.Min(Symbol.GridPoint(rect.Width), CircuitButton.MaxWidth));
			int h = Math.Max(2, Math.Min(Symbol.GridPoint(rect.Height), CircuitButton.MaxHeight));

			if(x != symbol.X || y != symbol.Y || w != button.Width || h != button.Height) {
				this.CircuitProject.InTransaction(() => {
					symbol.X = x;
					symbol.Y = y;
					button.Width = w;
					button.Height = h;
					if(withWires) {
						button.ResetPins();
						symbol.ResetJams();
						symbol.PositionGlyph();
						GridPoint point = symbol.Jams().First().AbsolutePoint;
						foreach(Wire wire in symbol.LogicalCircuit.Wires()) {
							if(originalPoint == wire.Point1) {
								wire.X1 = point.X;
								wire.Y1 = point.Y;
								wire.PositionGlyph();
							}
							if(originalPoint == wire.Point2) {
								wire.X2 = point.X;
								wire.Y2 = point.Y;
								wire.PositionGlyph();
							}
						}
					}
				});
			}
			marker.Refresh();
		}

		public void RotateLeft(IRotatable symbol, bool withWires) {
			this.Rotate(symbol, withWires, EditorDiagram.RotateLeft);
		}

		public void RotateRight(IRotatable symbol, bool withWires) {
			this.Rotate(symbol, withWires, EditorDiagram.RotateRight);
		}

		private void Rotate(IRotatable symbol, bool withWires, Action<IRotatable> rotation) {
			Tracer.Assert(((Symbol)symbol).LogicalCircuit == this.Project.LogicalCircuit);
			this.ClearSelection();
			CircuitSymbol? circuitSymbol = symbol as CircuitSymbol;
			if(circuitSymbol == null) {
				withWires = false;
			}
			Dictionary<GridPoint, int>? oldPoints = null;
			if(withWires) {
				oldPoints = new Dictionary<GridPoint, int>();
				int i = 0;
				foreach(Jam jam in circuitSymbol!.Jams()) {
					oldPoints.Add(jam.AbsolutePoint, i++);
				}
			}
			this.CircuitProject.InTransaction(() => {
				rotation(symbol);
				if(withWires) {
					List<GridPoint> newPoints = new List<GridPoint>(circuitSymbol!.Jams().Select(j => j.AbsolutePoint));
					foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
						int index;
						if(oldPoints!.TryGetValue(wire.Point1, out index)) {
							wire.Point1 = newPoints[index];
							wire.PositionGlyph();
						}
						if(oldPoints.TryGetValue(wire.Point2, out index)) {
							wire.Point2 = newPoints[index];
							wire.PositionGlyph();
						}
					}
				}
				this.DeleteEmptyWires();
			});
			this.Select((Symbol)symbol);
		}

		private static void RotateLeft(IRotatable symbol) {
			switch(symbol.Rotation) {
			case Rotation.Up:
				symbol.Rotation = Rotation.Left;
				break;
			case Rotation.Right:
				symbol.Rotation = Rotation.Up;
				break;
			case Rotation.Down:
				symbol.Rotation = Rotation.Right;
				break;
			case Rotation.Left:
				symbol.Rotation = Rotation.Down;
				break;
			default:
				Tracer.Fail();
				break;
			}
		}

		private static void RotateRight(IRotatable symbol) {
			switch(symbol.Rotation) {
			case Rotation.Up:
				symbol.Rotation = Rotation.Right;
				break;
			case Rotation.Right:
				symbol.Rotation = Rotation.Down;
				break;
			case Rotation.Down:
				symbol.Rotation = Rotation.Left;
				break;
			case Rotation.Left:
				symbol.Rotation = Rotation.Up;
				break;
			default:
				Tracer.Fail();
				break;
			}
		}

		private bool CanAlign(Func<Rect, GridPoint> interval) {
			if(this.InEditMode && 1 < this.SelectionCount) {
				// Here the GridPoint is used as interval: X - start, Y - end
				List<GridPoint> intervals = new List<GridPoint>();
				foreach(Symbol symbol in this.SelectedSymbols.Where(s => s is not Wire)) {
					Rect rect = symbol.Bounds();
					intervals.Add(interval(rect));
				}
				if(1 < intervals.Count) {
					intervals.Sort((a, b) => {
						int delta = a.X - b.X;
						if(delta == 0) {
							delta = a.Y - b.Y;
						}
						return delta;
					});
					for(int i = 1; i < intervals.Count; i++) {
						if(intervals[i - 1].Y > intervals[i].X) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}

		public bool CanAlignHorizontally() => this.CanAlign(r => new GridPoint((int)r.X, (int)r.Right));
		public bool CanAlignVertically() => this.CanAlign(r => new GridPoint((int)r.Y, (int)r.Bottom));

		private void Align(Func<List<Symbol>, int> findBase, Func<int, Symbol, int> findDelta, Action<Symbol, int> shiftSymbol, Action<Wire, int, int> shiftWire) {
			List<Symbol> symbols = this.SelectedSymbols.Where(s => s is not Wire).ToList();
			if(1 < symbols.Count) {
				int baseOn = findBase(symbols);
				this.CircuitProject.InTransaction(() => {
					Dictionary<GridPoint, int> movedPoints = new();
					foreach(Symbol symbol in symbols) {
						int delta = 0;
						if(symbol is CircuitSymbol circuitSymbol) {
							delta = findDelta(baseOn, circuitSymbol);
							if(delta != 0) {
								foreach(Jam jam in circuitSymbol.Jams()) {
									movedPoints.Add(jam.AbsolutePoint, delta);
								}
							}
						} else if(symbol is TextNote noteSymbol) {
							delta = findDelta(baseOn, noteSymbol);
						}
						if(delta != 0) {
							shiftSymbol(symbol, delta);
							symbol.PositionGlyph();
							if(this.selection.TryGetValue(symbol, out Marker? marker)) {
								marker.Refresh();
							}
						}
					}
					foreach(Wire wire in this.Project.LogicalCircuit.Wires()) {
						int delta;
						bool moved = false;
						if(movedPoints.TryGetValue(wire.Point1, out delta)) {
							shiftWire(wire, 1, delta);
							wire.PositionGlyph();
							moved = true;
						}
						if(movedPoints.TryGetValue(wire.Point2, out delta)) {
							shiftWire(wire, 2, delta);
							wire.PositionGlyph();
							moved = true;
						}
						if(moved && this.selection.TryGetValue(wire, out Marker? marker)) {
							marker.Refresh();
						}
					}
				});
			}
		}

		private void AlignHorizontally(Func<List<Symbol>, int> findBase, Func<int, Symbol, int> findDelta) {
			if(this.CanAlignHorizontally()) {
				this.Align(
					findBase,
					findDelta,
					(Symbol symbol, int dy) => symbol.Shift(0, dy),
					(Wire wire, int point, int dy) => wire.ShiftPoint(point, 0, dy)
				);
			}
		}

		private void AlignVertically(Func<List<Symbol>, int> findBase, Func<int, Symbol, int> findDelta) {
			if(this.CanAlignVertically()) {
				this.Align(
					findBase,
					findDelta,
					(Symbol symbol, int dx) => symbol.Shift(dx, 0),
					(Wire wire, int point, int dx) => wire.ShiftPoint(point, dx, 0)
				);
			}
		}

		public void AlignTop() {
			this.AlignHorizontally(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Min(s => s.Bounds().Top)),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(symbol.Bounds().Y)
			);
		}

		public void AlignMiddle() {
			double middle(Rect rect) => rect.Top + Symbol.ScreenPoint(Symbol.GridPoint(rect.Height) / 2);
			this.AlignHorizontally(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Average(s => middle(s.Bounds()))),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(middle(symbol.Bounds()))
			);
		}

		public void AlignBottom() {
			this.AlignHorizontally(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Max(s => s.Bounds().Bottom)),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(symbol.Bounds().Bottom)
			);
		}

		public void AlignLeft() {
			this.AlignVertically(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Min(s => s.Bounds().Left)),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(symbol.Bounds().Left)
			);
		}

		public void AlignCenter() {
			double center(Rect rect) => rect.Left + Symbol.ScreenPoint(Symbol.GridPoint(rect.Width) / 2);
			this.AlignVertically(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Average(s => center(s.Bounds()))),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(center(symbol.Bounds()))
			);
		}

		public void AlignRight() {
			this.AlignVertically(
				(List<Symbol> symbols) => Symbol.GridPoint(symbols.Max(s => s.Bounds().Right)),
				(int baseOn, Symbol symbol) => baseOn - Symbol.GridPoint(symbol.Bounds().Right)
			);
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
				if(e.Data.GetData(EditorDiagram.CircuitDescriptorDataFormat, false) is IDescriptor descriptor) {
					descriptor.CreateSymbol(this, Symbol.GridPoint(e.GetPosition(this.Diagram)));
				}
			}
			e.Handled = true;
		}

		private WirePointMarker? FindPointMarkerNear(Point point) {
			if(0 < this.SelectionCount) {
				Rect clickArea = EditorDiagram.ClickArea(point);
				foreach(Symbol other in this.SelectedSymbols) {
					if(other is Wire otherWire) {
						WireMarker? wireMarker = this.FindMarker(otherWire) as WireMarker;
						Tracer.Assert(wireMarker);
						if(clickArea.Contains(Symbol.ScreenPoint(wireMarker!.Point1.WirePoint()))) {
							return wireMarker.Point1;
						} else if(clickArea.Contains(Symbol.ScreenPoint(wireMarker.Point2.WirePoint()))) {
							return wireMarker.Point2;
						}
					}
				}
			}
			return null;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void DiagramMouseDown(MouseButtonEventArgs e) {
			FrameworkElement? element = e.OriginalSource as FrameworkElement;
			if(element == null) {
				FrameworkContentElement? content = e.OriginalSource as FrameworkContentElement;
				Tracer.Assert(content != null);
				while(element == null && content != null) {
					element = content.Parent as FrameworkElement;
					content = content.Parent as FrameworkContentElement;
				}
			}
			Tracer.Assert(element);
			Marker? marker = null;
			Symbol? symbol = null;
			Jam? jam = null;
			if(element != this.Diagram) { // something on the diagram was clicked
				marker = element!.DataContext as Marker;
				if(marker == null && this.SelectionCount == 1 && Keyboard.Modifiers == ModifierKeys.Alt) {
					// Support a special case when user splitting wire and Alt-clicking its marker and missing just a bit pointing to other nearby wire
					// ignore this click and assume user meant to click to marker
					if(this.Selection().FirstOrDefault() is Wire wire) {
						if(Symbol.Intersected(Symbol.ScreenPoint(wire.Point1), Symbol.ScreenPoint(wire.Point2), EditorDiagram.ClickArea(e.GetPosition(this.Diagram)))) {
							marker = this.FindMarker(wire);
						}
					}
				}
				if(marker == null) {
					jam = element.DataContext as Jam;
					if(jam == null) {
						symbol = element.DataContext as Symbol;
						if(symbol == null) {
							FrameworkElement? root = element;
							while(root != null && !(root.DataContext is Symbol)) {
								root = (root.Parent ?? root.TemplatedParent) as FrameworkElement;
							}
							if(root != null) {
								symbol = root.DataContext as Symbol;
							}
						}
						if(symbol == null && element.DataContext == this.wireJunctionPoint) {
							// User clicked solder
							symbol = this.FindWireNear(e.GetPosition(this.Diagram));
						}
						Tracer.Assert(symbol != null);
						if(symbol is Wire && 0 < this.SelectionCount) {
							marker = this.FindPointMarkerNear(e.GetPosition(this.Diagram));
							if(marker != null) {
								symbol = null;
							}
						}
					} else {
						if(!(element is Ellipse)) { // Jam's notations - text on circuit symbol was clicked. Treat this as symbol click
							symbol = jam.CircuitSymbol;
						}
					}
				} else {
					// Check if wire marker was clicked near tip of the wire then treat this as click on PointMarker
					if(marker is WireMarker wireMarker) {
						Rect clickArea = EditorDiagram.ClickArea(e.GetPosition(this.Diagram));
						if(clickArea.Contains(Symbol.ScreenPoint(wireMarker.Point1.WirePoint()))) {
							marker = wireMarker.Point1;
						} else if(clickArea.Contains(Symbol.ScreenPoint(wireMarker.Point2.WirePoint()))) {
							marker = wireMarker.Point2;
						}
					}
				}
			} else { // click on the empty space of the diagram
				Point point = e.GetPosition(this.Diagram);
				Wire? wire = null;
				if(this.SelectionCount == 1 && Keyboard.Modifiers == ModifierKeys.Alt) {
					wire = this.Selection().FirstOrDefault() as Wire;
					if(wire != null && !Symbol.Intersected(Symbol.ScreenPoint(wire.Point1), Symbol.ScreenPoint(wire.Point2), EditorDiagram.ClickArea(point))) {
						wire = null;
					}
				}
				if(wire == null) {
					wire = this.FindWireNear(point);
				}
				if(wire != null) {
					marker = this.FindPointMarkerNear(point);
					if(marker == null) {
						marker = this.FindMarker(wire);
					}
					if(marker == null) {
						symbol = wire;
					} else if(this.SelectionCount == 1) {
						// Check if user clicked close to WirePointMarker and treat this as attempt to move one end of the wire.
						if(marker is WireMarker wireMarker) {
							Rect rect = EditorDiagram.ClickArea(point);
							if(rect.Contains(Symbol.ScreenPoint(wireMarker.Point1.WirePoint()))) {
								marker = wireMarker.Point1;
							} else if(rect.Contains(Symbol.ScreenPoint(wireMarker.Point2.WirePoint()))) {
								marker = wireMarker.Point2;
							}
						}
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
			} else if(this.InEditMode && Keyboard.Modifiers != ModifierKeys.Control) { // Nothing was clicked on the diagram
				if(e.ClickCount < 2) {
					if(Keyboard.Modifiers != ModifierKeys.Shift) {
						this.ClearSelection();
					}
					this.StartAreaSelection(e.GetPosition(this.Diagram));
				} else {
					this.ClearSelection();
					this.Edit(this.Project.LogicalCircuit);
				}
			} else if(Keyboard.Modifiers == ModifierKeys.Control && Mouse.Capture(this.Diagram, CaptureMode.Element)) {
				this.panStart = e.GetPosition(this.Mainframe);
				this.panning = true;
			}
		}

		public void DiagramMouseUp(MouseEventArgs e) {
			if(this.InEditMode) {
				if(this.movingMarker != null) {
					this.FinishMove(e.GetPosition(this.Diagram), (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
				}
			}
			if(this.panning) {
				Mouse.Capture(null);
				this.panning = false;
			}
		}

		public void DiagramMouseMove(MouseEventArgs e) {
			if(this.InEditMode && this.movingMarker != null) {
				Point point = e.GetPosition(this.Diagram);
				this.movingMarker.Move(this, new Point(Math.Max(this.maxMove.X, point.X), Math.Max(this.maxMove.Y, point.Y)));
			} else if(this.panning) {
				Point point = e.GetPosition(this.Mainframe);
				ScrollViewer scroll = this.Mainframe.DiagramScroll;
				scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + this.panStart.X - point.X);
				scroll.ScrollToVerticalOffset(scroll.VerticalOffset + this.panStart.Y - point.Y);
				this.panStart = point;
			}
		}

		public void DiagramMouseWheel(ScrollViewer diagramScroll, MouseWheelEventArgs e) {
			if(this.movingMarker == null) {
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					diagramScroll.ScrollToHorizontalOffset(diagramScroll.HorizontalOffset - e.Delta);
					e.Handled = true;
				} else if(Keyboard.Modifiers == ModifierKeys.Control) {
					double old = this.Project.Zoom;
					double zoom = Project.CheckZoom(old + Math.Sign(e.Delta) * 0.1);
					if(0.0001 < Math.Abs(zoom - old)) {
						this.CircuitProject.InTransaction(() => this.Project.Zoom = zoom);
					}
					e.Handled = true;
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private void SymbolMouseDown(Symbol symbol, MouseEventArgs e) {
			if(this.InEditMode) {
				if(symbol is Wire wire) {
					if(Keyboard.Modifiers == ModifierKeys.Control) {
						this.StartMove(this.SelectSymbol(wire), e.GetPosition(this.Diagram));
					} else if(Keyboard.Modifiers == ModifierKeys.Shift) {
						this.ClearSelection();
						this.SelectConductor(wire);
						this.StartMove(this.FindMarker(wire)!, e.GetPosition(this.Diagram));
					} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
						this.SelectConductor(wire);
						this.StartMove(this.FindMarker(wire)!, e.GetPosition(this.Diagram));
					} else {
						Point point = e.GetPosition(this.Diagram);
						if(Editor.IsPinClose(point, Symbol.ScreenPoint(wire.Point1)) || Editor.IsPinClose(point, Symbol.ScreenPoint(wire.Point2))) {
							this.StartWire(point);
							return;
						}
						this.ClearSelection();
						this.StartMove(this.SelectSymbol(wire), e.GetPosition(this.Diagram));
					}
					this.Mainframe.Status = Properties.Resources.TipOnWireSelect;
					return;
				}

				if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
					this.Select(symbol);
				} else {
					this.ClearSelection();
					this.StartMove(this.SelectSymbol(symbol), e.GetPosition(this.Diagram));
				}

				if(symbol is CircuitSymbol circuitSymbol) {
					this.ShowStatus(circuitSymbol);
				}
			} else {
				if(symbol is CircuitSymbol circuitSymbol) {
					this.ShowStatus(circuitSymbol);
					return;
				}
				if(symbol is Wire wire) {
					WireDisplayControl display = new WireDisplayControl(this.Diagram, e.GetPosition(this.Diagram), wire);
					display.Start();
					return;
				}
			}
		}

		private void MarkerMouseDown(Marker marker, MouseEventArgs e) {
			if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
				this.Unselect(marker.Symbol);
				return;
			}

			if(marker is WireMarker wireMarker) {
				Wire wire = wireMarker.Wire;
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					this.ClearSelection();
					this.SelectConductor(wire);
					this.StartMove(this.FindMarker(wire)!, e.GetPosition(this.Diagram));
					return;
				} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
					this.UnselectConductor(wire);
					return;
				} else if(Keyboard.Modifiers == ModifierKeys.Alt) {
					this.Split(wire, Symbol.GridPoint(e.GetPosition(this.Diagram)));
					return;
				}
			}

			if(marker is WirePointMarker wirePointMarker) {
				Wire wire = wirePointMarker.Parent.Wire;
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					this.ClearSelection();
					this.SelectConductor(wire);
					this.StartMove(this.FindMarker(wire)!, e.GetPosition(this.Diagram));
					return;
				} else if(Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control)) {
					this.UnselectConductor(wire);
					return;
				} else if(Keyboard.Modifiers == ModifierKeys.Alt && this.SelectionCount == 2) {
					GridPoint point = wirePointMarker.WirePoint();
					foreach(Symbol symbol in this.selection.Keys) {
						if(symbol is Wire wire2 && wire2 != wire && (wire2.Point1 == point || wire2.Point2 == point)) {
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
				this.Edit(marker.Symbol);
			}
		}

		private void JamMouseDown(MouseEventArgs e) {
			if(this.InEditMode) {
				this.StartWire(e.GetPosition(this.Diagram));
			}
		}
	}
}
