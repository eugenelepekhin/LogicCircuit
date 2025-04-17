﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Scripting.Utils;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogTruthTable.xaml
	/// </summary>
	public partial class DialogTruthTable : Window {
		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public static readonly DependencyProperty TruthTableProperty = DependencyProperty.Register(
			"TruthTable", typeof(ListCollectionView), typeof(DialogTruthTable)
		);
		public static readonly DependencyProperty TruncatedProperty = DependencyProperty.Register(
			"Truncated", typeof(bool), typeof(DialogTruthTable)
		);
		public static readonly DependencyProperty ShowProgressProperty = DependencyProperty.Register(
			"ShowProgress", typeof(bool), typeof(DialogTruthTable), new PropertyMetadata(false)
		);
		public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
			"Progress", typeof(double), typeof(DialogTruthTable)
		);

		public ListCollectionView TruthTable {
			get { return (ListCollectionView)this.GetValue(DialogTruthTable.TruthTableProperty); }
			set { this.SetValue(DialogTruthTable.TruthTableProperty, value); }
		}

		public bool Truncated {
			get { return (bool)this.GetValue(DialogTruthTable.TruncatedProperty); }
			set { this.SetValue(DialogTruthTable.TruncatedProperty, value); }
		}

		public bool InvertFilter { get; set; }
		public ObservableCollection<string> OldFilters { get; } = new ObservableCollection<string>();

		public bool ShowProgress {
			get { return (bool)this.GetValue(DialogTruthTable.ShowProgressProperty); }
			set { this.SetValue(DialogTruthTable.ShowProgressProperty, value); }
		}

		public double Progress {
			get { return (double)this.GetValue(DialogTruthTable.ProgressProperty); }
			set { this.SetValue(DialogTruthTable.ProgressProperty, value); }
		}

		private bool abort;

		private readonly CircuitTestSocket testSocket;
		private readonly TruthStateComparer sortComparer;

		public const int MaxRows = 1 << 12;
		public BigInteger TotalRows { get; private set; }

		private readonly LogicalCircuit logicalCircuit;
		public LambdaUICommand CommandDeleteOldFilter { get; }

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public DialogTruthTable(LogicalCircuit logicalCircuit) {
			this.logicalCircuit = logicalCircuit;
			this.InvertFilter = true;
			this.testSocket  = new CircuitTestSocket(this.logicalCircuit);
			int inputBits = this.testSocket.Inputs.Sum(p => p.Pin.BitWidth);
			if(0 < inputBits) {
				this.TotalRows = BigInteger.One << inputBits;
			} else {
				this.TotalRows = 0;
				this.TruthTable = new ListCollectionView(new List<TruthState>());
			}

			this.BuildTruthTable();

			this.CommandDeleteOldFilter = new LambdaUICommand("-", o => this.RemoveFilter((o as string)!)) {
				IconPath = "Icon/Delete.xaml"
			};

			this.DataContext = this;
			this.InitializeComponent();

			Dictionary<DataGridTextColumn, Func<TruthState, int>> dataAccessor = new Dictionary<DataGridTextColumn, Func<TruthState, int>>();
			void addColumn(Pin pin, string path, int i) {
				DataGridTextColumn column = new DataGridTextColumn();
				column.Header = pin.Name.Replace("_", "__", StringComparison.Ordinal);
				column.Binding = new Binding(path);
				if(pin.PinType == PinType.Input) {
					column.Binding.StringFormat = "{0:X}";
				}
				Style style = new Style(typeof(DataGridColumnHeader));
				style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, pin.ToolTip));
				column.HeaderStyle = style;
				this.dataGrid.Columns.Add(column);
				dataAccessor.Add(column, DialogTruthTable.InputFieldAccesor(i));
			}

			int index = 0;
			foreach(InputPinSocket socket in this.testSocket.Inputs) {
				addColumn(socket.Pin, "Input[" + index + "]", index);
				index++;
			}
			index = 0;
			foreach(OutputPinSocket socket in this.testSocket.Outputs) {
				addColumn(socket.Pin, "[" + index + "]", index);
				index++;
			}

			this.sortComparer = new TruthStateComparer(dataAccessor);
			this.dataGrid.Sorting += new DataGridSortingEventHandler(this.DataGridSorting);
			DataObject.AddPastingHandler(this.filter, this.OnFilterPaste);
			this.OldFilters.AddRange(this.logicalCircuit.Validators.Split('\t').Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()));
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			e.Cancel = this.ShowProgress;
			if(!this.ShowProgress) {
				string text = this.OldFilters.Aggregate("", (x, y) => string.IsNullOrEmpty(x) ? y : x + "\t" + y);
				if(text != this.logicalCircuit.Validators) {
					this.logicalCircuit.CircuitProject.InOmitTransaction(() => this.logicalCircuit.Validators = text);
				}
			}
		}

		private void OnFilterPaste(object sender, DataObjectPastingEventArgs e) {
			if(sender == this.filter && e.DataObject != null && e.DataObject.GetDataPresent(DataFormats.Text)) {
				string? original = e.DataObject.GetData(DataFormats.Text) as string;
				if(!string.IsNullOrWhiteSpace(original)) {
					string text = Regex.Replace(original.Trim(), @"\s+", " ");
					if(original != text) {
						e.DataObject = new DataObject(DataFormats.Text, text);
					}
				}
			}
		}

		private void SaveFilter() {
			if(this.filter != null) {
				string text = this.filter.Text.Trim();
				if(!string.IsNullOrEmpty(text)) {
					int index = this.OldFilters.IndexOf(text);
					if(0 < index) {
						this.OldFilters.RemoveAt(index);
					}
					if(0 != index) {
						this.OldFilters.Insert(0, text);
						this.filter.Text = text;
						while(LogicalCircuit.MaxTruthTableFilters < this.OldFilters.Count) {
							this.OldFilters.RemoveAt(this.OldFilters.Count - 1);
						}
					}
				}
			}
		}

		private void RemoveFilter(string text) {
			int index = this.OldFilters.IndexOf(text);
			if(0 <= index) {
				this.OldFilters.RemoveAt(index);
				if(this.filter != null && this.filter.Text == text) {
					this.filter.Text = "";
				}
			}
		}

		private static Func<TruthState, int> InputFieldAccesor(int index) {
			return s => s.Input[index];
		}

		private static Func<TruthState, int> OutputFieldAccesor(int index) {
			return s => s.Output[index];
		}

		private void DataGridSorting(object sender, DataGridSortingEventArgs e) {
			this.sortComparer.ToggleColumn((DataGridTextColumn)e.Column, (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
			this.TruthTable.CustomSort  = this.sortComparer.IsEmpty ? null : this.sortComparer;
			e.Handled = true;
		}

		private void BuildTruthTable() {
			if(!this.ShowProgress && 0 < this.TotalRows) {
				Predicate<TruthState>? include = null;
				if(DialogTruthTable.MaxRows < this.TotalRows) {
					bool success;
					include = this.Filter(out success);
					if(!success) {
						return;
					}
				}
				this.abort = false;
				this.Progress = 0;
				this.ShowProgress = true;
				ThreadPool.QueueUserWorkItem(o => {
					Exception? error = null;
					bool oscillation = false;
					try {
						bool truncated = false;
						IList<TruthState>? table = this.testSocket.BuildTruthTable(
							this.SetProgress, () => !this.abort, include, DialogTruthTable.MaxRows, out truncated
						);
						if(table != null) {
							this.Dispatcher.BeginInvoke(new Action(() => {
								this.TruthTable = new ListCollectionView((IList)table);
								this.Truncated = truncated;
							}));
						} else {
							oscillation = true;
						}
					} catch(Exception exception) {
						error = exception;
					} finally {
						this.SetShowProgress(false);
					}
					if(error != null) {
						App.Mainframe.ReportException(error);
					} else if(oscillation) {
						App.Mainframe.ErrorMessage(Properties.Resources.Oscillation);
					}
				});
			}
		}

		private void SetProgress(double progress) {
			this.Dispatcher.BeginInvoke(new Action(() => this.Progress = progress));
		}

		private void SetShowProgress(bool show) {
			this.Dispatcher.Invoke(new Action(() => this.ShowProgress = show));
		}

		private void ButtonApplyClick(object sender, RoutedEventArgs e) {
			try {
				this.SaveFilter();
				if(DialogTruthTable.MaxRows < this.TotalRows) {
					this.BuildTruthTable();
				} else {
					bool success;
					Predicate<TruthState>? include = this.Filter(out success);
					if(include != null) {
						this.TruthTable.Filter = o => {
							if(o is TruthState state) {
								return (state.Output == null) || include(state);
							}
							return false;
						};
					} else {
						this.TruthTable.Filter = null;
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonStopClick(object sender, RoutedEventArgs e) {
			this.abort = true;
			this.ShowProgress = false;
		}

		private Predicate<TruthState>? Filter(out bool success) {
			success = true;
			if(this.filter != null) {
				string text = this.filter.Text.Trim();
				if(!string.IsNullOrEmpty(text)) {
					ExpressionParser parser = new ExpressionParser(this.testSocket);
					Predicate<TruthState>? func = parser.Parse(text, this.InvertFilter);
					if(parser.ErrorCount == 0) {
						return func;
					} else {
						App.Mainframe.ErrorMessage(parser.ErrorText);
						success = false;
					}
				}
			}
			return null;
		}

		private class TruthStateComparer : IComparer<TruthState>, IComparer {
			private struct Sort {
				public Func<TruthState, int> Data;
				public int Direction;
				public DataGridTextColumn Column;
			}
			
			private readonly Dictionary<DataGridTextColumn, Func<TruthState, int>> dataAccessor;
			private readonly List<Sort> sort = new List<Sort>();

			public bool IsEmpty { get { return this.sort.Count == 0; } }

			public TruthStateComparer(Dictionary<DataGridTextColumn, Func<TruthState, int>> dataAccessor) {
				this.dataAccessor = dataAccessor;
			}

			public void ToggleColumn(DataGridTextColumn column, bool extend) {
				if(extend) {
					bool found = false;
					for(int i = 0; i < this.sort.Count; i++) {
						Sort s = this.sort[i];
						if(s.Column == column) {
							if(s.Direction == 1) {
								s.Direction = -1;
								this.sort[i] = s;
								column.SortDirection = ListSortDirection.Descending;
							} else {
								this.sort.RemoveAt(i);
								column.SortDirection = null;
							}
							found = true;
							break;
						}
					}
					if(!found) {
						this.sort.Add(new Sort() {
							Data = this.dataAccessor[column],
							Direction = 1,
							Column = column
						});
						column.SortDirection = ListSortDirection.Ascending;
					}
				} else {
					for(int i = 0; i < this.sort.Count; i++) {
						Sort s = this.sort[i];
						if(s.Column != column) {
							s.Column.SortDirection = null;
						}
					}
					if(column.SortDirection.HasValue) {
						if(column.SortDirection == ListSortDirection.Ascending) {
							column.SortDirection = ListSortDirection.Descending;
						} else {
							column.SortDirection = null;
						}
					} else {
						column.SortDirection = ListSortDirection.Ascending;
					}
					this.sort.Clear();
					if(column.SortDirection.HasValue) {
						this.sort.Add(new Sort() {
							Data = this.dataAccessor[column],
							Direction = (column.SortDirection == ListSortDirection.Ascending ? 1 : -1),
							Column = column
						});
					}
				}
			}

			public int Compare(TruthState x, TruthState y) {
				foreach(Sort s in this.sort) {
					int result = s.Data(x).CompareTo(s.Data(y));
					if(result != 0) {
						return result * s.Direction;
					}
				}
				return 0;
			}

			public int Compare(object? x, object? y) {
				Debug.Assert(x != null && y != null);
				if(x is TruthState && y is TruthState) {
					return this.Compare((TruthState)x, (TruthState)y);
				}
				return 0;
			}
		}
	}
}
