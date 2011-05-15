using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

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

		private CircuitRunner circuitRunner;
		public CircuitRunner CircuitRunner {
			get { return this.circuitRunner; }
			private set {
				if(this.circuitRunner != value) {
					this.circuitRunner = value;
					this.NotifyPropertyChanged("CircuitRunner");
				}
			}
		}

		public bool Power {
			get { return this.CircuitRunner != null; }
			set {
				try {
					if(this.Power != value) {
						if(value) {
							this.CancelMove();
							this.ClearSelection();
							this.CircuitRunner = new CircuitRunner(this);
							this.CircuitRunner.Start();
						} else {
							this.CircuitRunner.Stop();
							this.CircuitRunner = null;
						}
					}
				} catch(Exception exception) {
					this.Mainframe.ReportException(exception);
				}
				this.NotifyPropertyChanged("Power");
			}
		}

		public override bool InEditMode { get { return !this.Power; } }

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
			this.NotifyPropertyChanged("Caption");
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

		public void OpenLogicalCircuit(CircuitMap map) {
			Tracer.Assert(this.Power);
			this.OpenLogicalCircuit(map.Circuit);
			this.CircuitRunner.VisibleMap = map;
			map.Redraw();
			this.Mainframe.Status = map.Path();
		}

		protected override void UpdateGlyph(LogicalCircuit logicalCircuit) {
			this.CircuitDescriptorList.UpdateGlyph(logicalCircuit);
		}

		public void FullRefresh() {
			foreach(CircuitSymbol symbol in this.CircuitProject.CircuitSymbolSet) {
				symbol.Reset();
			}
			foreach(Wire wire in this.CircuitProject.WireSet) {
				wire.Reset();
			}
			this.Refresh();
			this.CircuitDescriptorList.Refresh();
		}

		//--- Edit Operation

		public void Import(string file) {
			this.CancelMove();
			this.ClearSelection();
			DialogImport dialog = new DialogImport(file, this.CircuitProject);
			bool? result = this.Mainframe.ShowDialog(dialog);
			if(result.HasValue && result.Value) {
				LogicalCircuit target = this.Project.LogicalCircuit;
				this.CircuitProject.InTransaction(() => {
					foreach(LogicalCircuit circuit in dialog.ImportList) {
						circuit.CopyTo(target);
					}
				});
			}
		}

		public RenderTargetBitmap ExportImage() {
			Rect rect = new Rect();
			bool isEmpty = true;
			LogicalCircuit logicalCircuit = this.Project.LogicalCircuit;
			foreach(Wire wire in logicalCircuit.Wires()) {
				Rect wireRect = new Rect(Symbol.ScreenPoint(wire.Point1), Symbol.ScreenPoint(wire.Point2));
				if(isEmpty) {
					rect = wireRect;
					isEmpty = false;
				} else {
					rect.Union(wireRect);
				}
			}
			foreach(CircuitSymbol symbol in logicalCircuit.CircuitSymbols()) {
				Rect symbolRect = new Rect(Symbol.ScreenPoint(symbol.Point), new Size(symbol.Glyph.Width, symbol.Glyph.Height));
				if(isEmpty) {
					rect = symbolRect;
					isEmpty = false;
				} else {
					rect.Union(symbolRect);
				}
			}
			if(!isEmpty) {
				Canvas diagram = this.Diagram;
				Brush oldBackground = diagram.Background;
				Transform oldRenderTransform = diagram.RenderTransform;
				Transform oldLayoutTransform = diagram.LayoutTransform;
				double horizontalOffset = 0;
				double verticalOffset = 0;
				ScrollViewer scrollViewer = diagram.Parent as ScrollViewer;
				try {
					if(scrollViewer != null) {
						horizontalOffset = scrollViewer.HorizontalOffset;
						verticalOffset = scrollViewer.VerticalOffset;
						scrollViewer.ScrollToHorizontalOffset(0);
						scrollViewer.ScrollToVerticalOffset(0);
						scrollViewer.UpdateLayout();
					}
					diagram.Background = Brushes.White;
					rect.Inflate(Symbol.GridSize, Symbol.GridSize);
					rect.Intersect(new Rect(0, 0, Symbol.LogicalCircuitWidth, Symbol.LogicalCircuitHeight));
					diagram.RenderTransform = new TranslateTransform(-rect.X, -rect.Y);
					diagram.UpdateLayout();
					RenderTargetBitmap bitmap = new RenderTargetBitmap(
						(int)Math.Round(rect.Width), (int)Math.Round(rect.Height), 96, 96, PixelFormats.Pbgra32
					);
					bitmap.Render(diagram);
					return bitmap;
				} finally {
					diagram.Background = oldBackground;
					diagram.RenderTransform = oldRenderTransform;
					diagram.LayoutTransform = oldLayoutTransform;
					diagram.UpdateLayout();
					if(scrollViewer != null) {
						scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
						scrollViewer.ScrollToVerticalOffset(verticalOffset);
						scrollViewer.UpdateLayout();
					}
				}
			}
			return null;
		}

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public void Copy() {
			this.CancelMove();
			if(0 < this.SelectionCount) {
				XmlDocument xml = this.CircuitProject.Copy(this.SelectedSymbols);
				StringBuilder text = new StringBuilder();
				using(StringWriter stringWriter = new StringWriter(text, CultureInfo.InvariantCulture)) {
					using(XmlTextWriter writer = new XmlTextWriter(stringWriter)) {
						writer.Formatting = Formatting.None;
						xml.WriteTo(writer);
					}
				}
				Clipboard.SetDataObject(text.ToString(), false);
			}
		}

		public static bool CanPaste() {
			return CircuitProject.CanPaste(Clipboard.GetText());
		}

		public void Paste() {
			this.CancelMove();
			this.ClearSelection();
			string text = Clipboard.GetText();
			if(CircuitProject.CanPaste(text)) {
				XmlDocument xml = new XmlDocument();
				xml.LoadXml(text);
				IEnumerable<Symbol> result = null;
				this.CircuitProject.InTransaction(() => {
					result = this.CircuitProject.Paste(xml);
				});
				Tracer.Assert(result.All(symbol => symbol.LogicalCircuit == this.Project.LogicalCircuit));
				this.Select(result);
			}
		}

		public void Delete() {
			this.CancelMove();
			if(0 < this.SelectionCount) {
				IEnumerable<Symbol> selection = this.Selection();
				this.ClearSelection();
				this.CircuitProject.InTransaction(() => {
					foreach(Symbol symbol in selection) {
						symbol.DeleteSymbol();
					}
				});
			}
		}

		public void Cut() {
			this.CancelMove();
			if(0 < this.SelectionCount) {
				this.Copy();
				this.Delete();
			}
		}

		public void Edit(Project project) {
			Tracer.Assert(project == this.Project);
			this.Mainframe.ShowDialog(new DialogProject(project));
		}

		public override void Edit(LogicalCircuit logicalCircuit) {
			this.Mainframe.ShowDialog(new DialogCircuit(logicalCircuit));
		}

		private void Edit(CircuitButton button) {
			this.Mainframe.ShowDialog(new DialogButton(button));
		}

		private void Edit(Constant constant) {
			this.Mainframe.ShowDialog(new DialogConstant(constant));
		}

		private void Edit(Memory memory) {
			this.Mainframe.ShowDialog(memory.Writable ? (Window)new DialogRAM(memory) : (Window)new DialogROM(memory));
		}

		private void Edit(Pin pin) {
			this.Mainframe.ShowDialog(new DialogPin(pin));
		}

		private void Edit(TextNote textNote) {
			DialogText dialog = new DialogText(textNote.Note);
			bool? result = this.Mainframe.ShowDialog(dialog);
			if(result.HasValue && result.Value) {
				string text = dialog.Document;
				if(TextNote.IsValidText(text)) {
					this.CircuitProject.InTransaction(() => { textNote.Note = dialog.Document; });
					textNote.UpdateGlyph();
				} else {
					this.CircuitProject.InTransaction(() => textNote.Delete());
				}
			}
		}

		protected override void Edit(Symbol symbol) {
			CircuitSymbol circuitSymbol = symbol as CircuitSymbol;
			if(circuitSymbol != null) {
				if(this.InEditMode) {
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
				} else if(this.CircuitRunner != null && this.CircuitRunner.VisibleMap != null) {
					CircuitMap map = this.CircuitRunner.VisibleMap.Child(circuitSymbol);
					if(map != null) {
						this.OpenLogicalCircuit(map);
						return;
					}
					Gate gate = circuitSymbol.Circuit as Gate;
					if(gate != null && gate.GateType == GateType.Probe) {
						FunctionProbe functionProbe = this.CircuitRunner.VisibleMap.FunctionProbe(circuitSymbol);
						if(functionProbe != null) {
							this.Mainframe.ShowDialog(new DialogProbeHistory(functionProbe));
						}
						return;
					}
					Memory memory = circuitSymbol.Circuit as Memory;
					if(memory != null) {
						FunctionMemory functionMemory = this.CircuitRunner.VisibleMap.FunctionMemory(circuitSymbol);
						if(functionMemory != null) {
							this.Mainframe.ShowDialog(new DialogMemory(functionMemory));
						}
						return;
					}
					Constant constant = circuitSymbol.Circuit as Constant;
					if(constant != null) {
						if(this.CircuitRunner.Root.First() == this.CircuitRunner.VisibleMap) {
							FunctionConstant functionConstant = (FunctionConstant)this.CircuitRunner.VisibleMap.Input(circuitSymbol);
							if(functionConstant != null) {
								this.CircuitProject.InOmitTransaction(() => functionConstant.Value++);
							}
						} else {
							this.Mainframe.Status = Resources.MessageNotRootConstant(this.CircuitRunner.Root.First().Circuit.Name);
						}
					}
				}
			} else {
				TextNote textNote = symbol as TextNote;
				if(textNote != null) {
					this.Edit(textNote);
				}
			}
		}

		protected override void ButtonIsPressedChanged(CircuitSymbol symbol, bool isPressed) {
			if(this.Power && this.CircuitRunner.VisibleMap != null) {
				FunctionButton function = (FunctionButton)this.CircuitRunner.VisibleMap.Input(symbol);
				if(function != null) {
					if(isPressed) {
						function.SymbolPress();
					} else {
						function.SymbolRelease();
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
				foreach(TextNote symbol in logicalCircuit.TextNotes()) {
					this.Select(symbol);
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
				foreach(Symbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
					this.Select(symbol);
				}
				foreach(Symbol symbol in this.Project.LogicalCircuit.TextNotes()) {
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
				foreach(Symbol symbol in this.Project.LogicalCircuit.CircuitSymbols()) {
					this.Unselect(symbol);
				}
				foreach(Symbol symbol in this.Project.LogicalCircuit.TextNotes()) {
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
				IDescriptor descriptor = sender.DataContext as IDescriptor;
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
