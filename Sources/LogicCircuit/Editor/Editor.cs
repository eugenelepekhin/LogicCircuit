using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

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

		//--- Drawing on Diagram --

		public void Refresh() {
			if(this.Dispatcher.Thread != Thread.CurrentThread) {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.RedrawDiagram));
			} else {
				this.RedrawDiagram();
			}
		}

		private void RedrawDiagram() {
			this.Diagram.Children.Clear();
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
				this.Diagram.Children.Add(line);
				this.AddWirePoint(wire.Point1);
				this.AddWirePoint(wire.Point2);
			}
			foreach(KeyValuePair<GridPoint, int> solder in this.wirePoint) {
				if(2 < solder.Value) {
					Ellipse ellipse = new Ellipse();
					Panel.SetZIndex(ellipse, 0);
					ellipse.Width = ellipse.Height = 2 * Symbol.PinRadius;
					Canvas.SetLeft(ellipse, Symbol.ScreenPoint(solder.Key.X));
					Canvas.SetTop(ellipse, Symbol.ScreenPoint(solder.Key.Y));
					ellipse.Fill = Symbol.JamDirectFill;
					this.Diagram.Children.Add(ellipse);
				}
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Point point = Symbol.ScreenPoint(symbol.Point);
				Canvas.SetLeft(symbol.Glyph, point.X);
				Canvas.SetTop(symbol.Glyph, point.Y);
				this.Diagram.Children.Add(symbol.Glyph);
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

		public int SelectionCount { get { return 0;/*this.selection.Count;*/ } }

		public void ClearSelection() {
			//foreach(Marker marker in this.selection.Values) {
			//    this.SymbolList.Remove(marker);
			//}
			//this.selection.Clear();
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
