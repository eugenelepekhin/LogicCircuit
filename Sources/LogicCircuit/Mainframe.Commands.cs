using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace LogicCircuit {
	partial class Mainframe {
		private bool IsEditorInEditMode() {
			return this.Editor != null && this.Editor.InEditMode;
		}

		public LambdaUICommand CommandNew { get; private set; }
		public LambdaUICommand CommandOpen { get; private set; }
		public LambdaUICommand CommandOpenRecent { get; private set; }
		public LambdaUICommand CommandSave { get; private set; }
		public LambdaUICommand CommandSaveAs { get; private set; }
		public LambdaUICommand CommandImport { get; private set; }
		public LambdaUICommand CommandExportImage { get; private set; }
		public LambdaUICommand CommandClose { get; private set; }
		public LambdaUICommand CommandHelp { get; private set; }
		public LambdaUICommand CommandAbout { get; private set; }

		private void InitCommands() {
			this.CommandNew = new LambdaUICommand(Properties.Resources.CommandFileNew, o => this.New(), new KeyGesture(Key.N, ModifierKeys.Control));
			this.CommandOpen = new LambdaUICommand(Properties.Resources.CommandFileOpen, o => this.Open(), new KeyGesture(Key.O, ModifierKeys.Control));
			this.CommandOpenRecent = new LambdaUICommand(Properties.Resources.CommandFileOpenRecent, file => this.OpenRecent(file as string));
			this.CommandSave = new LambdaUICommand(Properties.Resources.CommandFileSave, o => this.Save(), new KeyGesture(Key.S, ModifierKeys.Control));
			this.CommandSaveAs = new LambdaUICommand(Properties.Resources.CommandFileSaveAs, o => this.SaveAs());
			this.CommandImport = new LambdaUICommand(Properties.Resources.CommandFileFileImport, o => this.Editor != null && this.Editor.InEditMode, o => this.Import());
			this.CommandExportImage = new LambdaUICommand(Properties.Resources.CommandFileExportImage,
				o => this.Editor != null && !this.LogicalCircuit().IsEmpty(),
				o => this.ShowDialog(new DialogExportImage(this.Editor))
			);
			this.CommandClose = new LambdaUICommand(Properties.Resources.CommandFileClose, o => this.Close(), new KeyGesture(Key.F4, ModifierKeys.Alt));
			this.CommandHelp = new LambdaUICommand(Properties.Resources.CommandHelpView, o => Process.Start(Properties.Resources.HelpContent), new KeyGesture(Key.F1));
			this.CommandAbout = new LambdaUICommand(Properties.Resources.CommandHelpAbout, o => this.ShowDialog(new DialogAbout()));
		}

		private void EditUndoCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.CanUndo()) {
					this.Editor.Undo();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUndoCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditUndoCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null && this.Editor.CanUndo());
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUndoCommandCanExecut", exception);
				this.ReportException(exception);
			}
		}

		private void EditRedoCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.CanRedo()) {
					this.Editor.Redo();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRedoCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditRedoCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				if(this.Editor != null) {
					e.CanExecute = this.Editor.CanRedo();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRedoCommandCanExecut", exception);
				this.ReportException(exception);
			}
		}

		private void EditCutCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Cut();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditCutCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditCutCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditCutCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditCopyCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Copy();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditCopyCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditCopyCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditCopyCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditPasteCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Paste();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditPasteCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditPasteCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && Editor.CanPaste());
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditPasteCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditDeleteCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Delete();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditDeleteCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditDeleteCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditDeleteCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.SelectAll();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllWiresCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.SelectAllWires();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllWiresCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllWiresCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllWiresCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectFreeWiresCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					int count = this.Editor.SelectFreeWires();
					if(0 < count) {
						this.Status = Properties.Resources.MessageFreeWireCount(count);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectFreeWiresCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectFreeWiresCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectFreeWiresCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectFloatingSymbolsCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					int count = this.Editor.SelectFloatingSymbols();
					if(0 < count) {
						this.Status = Properties.Resources.MessageFloatingSymbolCount(count);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectFloatingSymbolsCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectFloatingSymbolsCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectFloatingSymbolsCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllButWiresCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.SelectAllButWires();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllButWiresCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllButWiresCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllButWiresCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditUnselectAllWiresCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount) {
					this.Editor.UnselectAllWires();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUnselectAllWiresCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditUnselectAllWiresCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUnselectAllWiresCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditUnselectAllButWiresCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount) {
					this.Editor.UnselectAllButWires();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUnselectAllButWiresCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditUnselectAllButWiresCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 0 < this.Editor.SelectionCount);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditUnselectAllButWiresCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllProbesCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.SelectAllProbes(false);
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllProbesCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllProbesCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllProbesCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllProbesWithWireCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.SelectAllProbes(true);
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllProbesWithWireCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditSelectAllProbesWithWireCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditSelectAllProbesWithWireCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditRotateLeftCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode() && this.Editor.SelectionCount == 1) {
					IRotatable symbol = this.Editor.SelectedSymbols.FirstOrDefault() as IRotatable;
					if(symbol != null) {
						this.Editor.RotateLeft(symbol, (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRotateLeftCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditRotateLeftCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && this.Editor.SelectionCount == 1 && this.Editor.SelectedSymbols.FirstOrDefault() is IRotatable);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRotateLeftCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void EditRotateRightCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode() && this.Editor.SelectionCount == 1) {
					IRotatable symbol = this.Editor.SelectedSymbols.FirstOrDefault() as IRotatable;
					if(symbol != null) {
						this.Editor.RotateRight(symbol, (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRotateRightCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void EditRotateRightCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && this.Editor.SelectionCount == 1 && this.Editor.SelectedSymbols.FirstOrDefault() is IRotatable);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.EditRotateRightCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitProjectCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Edit(this.Editor.CircuitProject.ProjectSet.Project);
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitProjectCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitProjectCommandCanExecuted(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitProjectCommandCanExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitCurrentCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Edit(this.LogicalCircuit());
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitCurrentCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitCurrentCommandCanExecuted(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitCurrentCommandCanExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitNewCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					CircuitProject circuitProject = this.Editor.CircuitProject;
					circuitProject.InTransaction(() => this.Editor.OpenLogicalCircuit(circuitProject.LogicalCircuitSet.Create()));
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitCurrentCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitNewCommandCanExecuted(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitNewCommandCanExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitDeleteCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode() && 1 < this.Editor.CircuitProject.LogicalCircuitSet.Count()) {
					this.Editor.DeleteLogicalCircuit();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitDeleteCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitDeleteCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.IsEditorInEditMode() && 1 < this.Editor.CircuitProject.LogicalCircuitSet.Count());
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitDeleteCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitUsageCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.LogicalCircuitUsage(this.LogicalCircuit());
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitUsageCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitUsageCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitUsageCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitFindCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.IsEditorInEditMode()) {
					this.Editor.Find();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitFindCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitFindCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = this.IsEditorInEditMode();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitFindCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitPowerCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null) {
					this.Editor.Power = !this.Editor.Power;
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitPowerCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitPowerCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitPowerCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitTruthTableExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null) {
					if(CircuitTestSocket.IsTestable(this.Editor.Project.LogicalCircuit)) {
						this.ShowDialog(new DialogTruthTable(this.Editor.Project.LogicalCircuit));
					} else {
						this.InformationMessage(Properties.Resources.MessageInputOutputPinsMissing);
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitTruthTableExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void CircuitTruthTableCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.CircuitTruthTableCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void ToolsReportCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null) {
					LogicalCircuit root = this.Editor.Project.LogicalCircuit;
					if(this.Editor.CircuitRunner != null) {
						CircuitMap map = this.Editor.CircuitRunner.VisibleMap;
						if(map != null && !this.Editor.InEditMode) {
							map = map.Root;
							root = map.Circuit;
						}
					}
					this.ShowDialog(new DialogReport(root));
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.ToolsReportCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void ToolsOscilloscopeCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.Power && this.Editor.CircuitRunner.HasProbes) {
					this.Editor.CircuitRunner.ShowOscilloscope();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.ToolsOscilloscopeCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void ToolsOscilloscopeCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null &&
					this.Editor.Power &&
					this.Editor.CircuitRunner.HasProbes &&
					this.Editor.CircuitRunner.DialogOscilloscope == null
				);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.ToolsOscilloscopeCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void ToolsOptionsCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.InEditMode) {
					bool? result = this.ShowDialog(new DialogOptions(this));
					if(result.HasValue && result.Value) {
						this.NotifyPropertyChanged("Editor");
						this.Editor.FullRefresh();
					}
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.ToolsOptionsCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void ToolsOptionsCommandCanExecuted(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null && this.Editor.InEditMode);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.ToolsOscilloscopeCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}
	}
}
