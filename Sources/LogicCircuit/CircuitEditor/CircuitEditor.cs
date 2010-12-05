using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LogicCircuit {
	public partial class CircuitEditor : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public Mainframe Mainframe { get; private set; }
		public string File { get; private set; }
		public CircuitProject CircuitProject { get; private set; }
		private int savedVersion;
		public CircuitDescriptorList CircuitDescriptorList { get; private set; }

		private readonly HashSet<GridPoint> solder = new HashSet<GridPoint>();

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

		public CircuitEditor(Mainframe mainframe, string file) {
			this.Mainframe = mainframe;
			this.File = file;
			if(this.File == null) {
				this.CircuitProject = CircuitProject.Create();
			} else {
				this.CircuitProject = CircuitProject.Load(this.File);
			}
			this.savedVersion = this.CircuitProject.Version;
			this.CircuitDescriptorList = new CircuitDescriptorList(this.CircuitProject);
		}

		public void Save(string file) {
			this.CircuitProject.Save(file);
			this.File = file;
			this.savedVersion = this.CircuitProject.Version;
			this.NotifyPropertyChanged("File");
		}

		private void NotifyPropertyChanged(string propertyName) {
			this.Mainframe.NotifyPropertyChanged(this.PropertyChanged, this, propertyName);
		}

		public double Zoom {
			get { return this.CircuitProject.ProjectSet.Project.Zoom; }
			set {
				if(this.Zoom != value) {
					try {
						this.CircuitProject.InTransaction(() => this.CircuitProject.ProjectSet.Project.Zoom = value);
						this.NotifyPropertyChanged("Zoom");
					} catch(Exception exception) {
						this.Mainframe.ReportException(exception);
					}
				}
			}
		}

		public int Frequency {
			get; set;
		}

		public bool IsMaximumSpeed {
			get; set;
		}

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void RedrawDiagram() {
			this.Diagram.Children.Clear();
			this.solder.Clear();
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				Line line = wire.WireGlyph;
				Point p = Plotter.ScreenPoint(wire.Point1);
				line.X1 = p.X;
				line.Y1 = p.Y;
				p = Plotter.ScreenPoint(wire.Point2);
				line.X2 = p.X;
				line.Y2 = p.Y;
				this.Diagram.Children.Add(line);
				this.DrawSolder(wire.Point1);
				this.DrawSolder(wire.Point2);
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Point point = Plotter.ScreenPoint(symbol.Point);
				Canvas.SetLeft(symbol.Glyph, point.X);
				Canvas.SetTop(symbol.Glyph, point.Y);
				this.Diagram.Children.Add(symbol.Glyph);
			}
		}

		private void DrawSolder(GridPoint point) {
			//if(!this.solder.Contains(point)) {
			//    Wire wire = this.CircuitProject.WireSet .ProjectManager.WireStore.NeedSolder(this.LogicalCircuit, point);
			//    if(wire != null) {
			//        this.solder.Add(point);
			//        Ellipse ellipse = new Ellipse();
			//        ellipse.Tag = wire;
			//        ellipse.Width = ellipse.Height = 2 * Plotter.PinRadius;
			//        Canvas.SetLeft(ellipse, point.X * Plotter.GridSize);
			//        Canvas.SetTop(ellipse, point.Y * Plotter.GridSize);
			//        ellipse.Fill = Plotter.JamDirectFill;
			//        this.Canvas.Children.Add(ellipse);
			//    }
			//}
		}
	}
}
