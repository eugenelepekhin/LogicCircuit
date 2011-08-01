using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace LogicCircuit {
	partial class Mainframe {
		private bool IsEditorInEditMode() {
			return this.Editor != null && this.Editor.InEditMode;
		}

		private void FileNewCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				this.New();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileNewCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileOpenCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				this.Open();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileOpenCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileSaveCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				this.Save();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileSaveCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileSaveAsCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				this.SaveAs();
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileSaveAsCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileImportCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && this.Editor.InEditMode) {
					this.Import();
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileImportCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileImportCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null && this.Editor.InEditMode);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileImportCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void FileExportImageCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				if(this.Editor != null && !this.LogicalCircuit().IsEmpty()) {
					this.ShowDialog(new DialogExportImage(this.Editor));
				}
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileExportImageCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void FileExportImageCommandCanExecute(object target, CanExecuteRoutedEventArgs e) {
			try {
				e.CanExecute = (this.Editor != null && !this.LogicalCircuit().IsEmpty());
			} catch(Exception exception) {
				Tracer.Report("Mainframe.FileExportImageCommandCanExecute", exception);
				this.ReportException(exception);
			}
		}

		private void FileCloseCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			this.Close();
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
						this.Status = LogicCircuit.Resources.MessageFreeWireCount(count);
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
						this.Status = LogicCircuit.Resources.MessageFloatingSymbolCount(count);
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
				if(this.Editor != null && this.Editor.Power && this.Editor.CircuitRunner.CircuitState.HasProbes) {
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
					this.Editor.CircuitRunner.CircuitState.HasProbes &&
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

		private void HelpContentCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				Process.Start(LogicCircuit.Resources.HelpContent);
			} catch(Exception exception) {
				Tracer.Report("Mainframe.HelpContentCommandExecuted", exception);
				this.ReportException(exception);
			}
		}

		private void HelpAboutCommandExecuted(object target, ExecutedRoutedEventArgs e) {
			try {
				this.ShowDialog(new DialogAbout());
			} catch(Exception exception) {
				Tracer.Report("Mainframe.HelpAboutCommandExecuted", exception);
				this.ReportException(exception);
			}
		}
	}
}
