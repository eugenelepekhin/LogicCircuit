using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace LogicCircuit {
	partial class Editor {
		private sealed class Switcher {
			public Editor Editor { get; }
			private readonly List<LogicalCircuit> history = new List<LogicalCircuit>();
			private int tab;

			public Switcher(Editor editor) {
				this.Editor = editor;
				LogicalCircuit active = this.Editor.Project.LogicalCircuit;
				Tracer.Assert(active);
				foreach(LogicalCircuit logicalCircuit in this.Editor.CircuitProject.LogicalCircuitSet) {
					if(logicalCircuit != active) {
						this.history.Add(logicalCircuit);
					}
				}
				this.history.Add(active);
				this.Editor.Project.PropertyChanged += new PropertyChangedEventHandler(this.ProjectPropertyChanged);
				this.Editor.CircuitProject.LogicalCircuitSet.CollectionChanged += new NotifyCollectionChangedEventHandler(this.LogicalCircuitSetCollectionChanged);
			}

			public void OnControlDown() {
				this.tab = 0;
			}

			public void OnControlUp() {
				this.tab = 0;
				LogicalCircuit logicalCircuit = this.Editor.Project.LogicalCircuit;
				if(logicalCircuit != this.history[this.history.Count - 1]) {
					this.history.Remove(logicalCircuit);
					this.history.Add(logicalCircuit);
				}
			}

			public void OnTabDown(bool control, bool shift) {
				if(control && this.history.Count > 1) {
					int count = this.history.Count;
					int i = ++this.tab % count;
					if(!shift) {
						i = count - i - 1;
					}
					this.Editor.OpenLogicalCircuit(this.history[i]);
				} else {
					this.tab = 0;
				}
			}

			public LogicalCircuit? SuggestNext() {
				return (1 < this.history.Count) ? this.history[this.history.Count - 2] : null;
			}

			private void ProjectPropertyChanged(object? sender, PropertyChangedEventArgs e) {
				if(this.tab == 0 && e.PropertyName == "LogicalCircuit") {
					this.OnControlUp();
				}
			}

			private void LogicalCircuitSetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
				this.tab = 0;
				if(e.NewItems != null && 0 < e.NewItems.Count) {
					foreach(object item in e.NewItems) {
						if(item is LogicalCircuit logicalCircuit) {
							// In case of undo of circuit removal, the circuit will already be in the history after call to ProjectPropertyChanged. So, remove it first.
							this.history.Remove(logicalCircuit);
							this.history.Insert(0, logicalCircuit);
						}
					}
				}
				if(e.OldItems != null && 0 < e.OldItems.Count) {
					foreach(object item in e.OldItems) {
						if(item is LogicalCircuit logicalCircuit) {
							this.history.Remove(logicalCircuit);
						}
					}
				}
				Tracer.Assert(this.history.Count == this.Editor.CircuitProject.LogicalCircuitSet.Count(),
					$"History count={this.history.Count} does not match circuit set count={this.Editor.CircuitProject.LogicalCircuitSet.Count()}"
				);
			}
		}
	}
}
