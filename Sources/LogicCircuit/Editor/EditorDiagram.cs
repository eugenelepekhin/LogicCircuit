using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace LogicCircuit {
	public abstract class EditorDiagram {

		private struct Connect {
			public int Count;
			public Ellipse Solder;
		}

		public Mainframe Mainframe { get; private set; }
		private Dispatcher Dispatcher { get { return this.Mainframe.Dispatcher; } }
		// TODO: make it private
		protected Canvas Diagram { get { return this.Mainframe.Diagram; } }

		public CircuitProject CircuitProject { get; private set; }
		public Project Project { get { return this.CircuitProject.ProjectSet.Project; } }

		private LogicalCircuit currentLogicalCircuit;
		private readonly Dictionary<GridPoint, Connect> wirePoint = new Dictionary<GridPoint, Connect>();

		protected EditorDiagram(Mainframe mainframe, CircuitProject circuitProject) {
			this.Mainframe = mainframe;
			this.CircuitProject = circuitProject;
			this.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
		}

		private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
			this.OnProjectPropertyChanged(e.PropertyName);
		}

		protected virtual void OnProjectPropertyChanged(string propertyName) {
			if(propertyName == "LogicalCircuit") {
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
			}
		}

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
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

		// TODO: make it private
		protected void Add(Wire wire) {
			wire.PositionGlyph();
			this.Diagram.Children.Add(wire.WireGlyph);
			this.AddWirePoint(wire.Point1);
			this.AddWirePoint(wire.Point2);
		}

		private void RedrawDiagram() {
			Canvas diagram = this.Diagram;
			diagram.Children.Clear();
			this.wirePoint.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				this.Add(wire);
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Point point = Symbol.ScreenPoint(symbol.Point);
				Canvas.SetLeft(symbol.Glyph, point.X);
				Canvas.SetTop(symbol.Glyph, point.Y);
				diagram.Children.Add(symbol.Glyph);
			}
		}
	}
}
