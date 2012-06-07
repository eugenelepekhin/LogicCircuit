using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using timers = System.Timers;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogTruthTable.xaml
	/// </summary>
	public partial class DialogTruthTable : Window {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public static readonly DependencyProperty TruthTableProperty = DependencyProperty.Register(
			"TruthTable", typeof(ListCollectionView), typeof(DialogTruthTable)
		);
		public static readonly DependencyProperty TruncatedProperty = DependencyProperty.Register(
			"Truncated", typeof(bool), typeof(DialogTruthTable)
		);
		public static readonly DependencyProperty InvertFilterProperty = DependencyProperty.Register(
			"InvertFilter", typeof(bool), typeof(DialogTruthTable), new PropertyMetadata(true)
		);
		public static readonly DependencyProperty ShowProgressProperty = DependencyProperty.Register(
			"ShowProgress", typeof(bool), typeof(DialogTruthTable), new PropertyMetadata(false)
		);
		public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
			"Progress", typeof(int), typeof(DialogTruthTable)
		);

		public ListCollectionView TruthTable {
			get { return (ListCollectionView)this.GetValue(DialogTruthTable.TruthTableProperty); }
			set { this.SetValue(DialogTruthTable.TruthTableProperty, value); }
		}

		public bool Truncated {
			get { return (bool)this.GetValue(DialogTruthTable.TruncatedProperty); }
			set { this.SetValue(DialogTruthTable.TruncatedProperty, value); }
		}

		public bool InvertFilter {
			get { return (bool)this.GetValue(DialogTruthTable.InvertFilterProperty); }
			set { this.SetValue(DialogTruthTable.InvertFilterProperty, value); }
		}

		public bool ShowProgress {
			get { return (bool)this.GetValue(DialogTruthTable.ShowProgressProperty); }
			set { this.SetValue(DialogTruthTable.ShowProgressProperty, value); }
		}

		public int Progress {
			get { return (int)this.GetValue(DialogTruthTable.ProgressProperty); }
			set { this.SetValue(DialogTruthTable.ProgressProperty, value); }
		}

		private int building = 0;
		private bool abort;

		private readonly CircuitTestSocket testSocket;
		private readonly TruthStateComparer sortComparer;

		private const int MaxRows = 1 << 12;
		public BigInteger TotalRows { get; private set; }

		public DialogTruthTable(LogicalCircuit logicalCircuit) {
			this.testSocket  = new CircuitTestSocket(logicalCircuit);
			int inputBits = this.testSocket.Inputs.Sum(p => p.Pin.BitWidth);
			if(0 < inputBits) {
				this.TotalRows = BigInteger.One << inputBits;
			} else {
				this.TotalRows = 0;
			}

			this.BuildTruthTable();

			this.DataContext = this;
			this.InitializeComponent();

			Dictionary<DataGridTextColumn, Func<TruthState, int>> dataAccessor = new Dictionary<DataGridTextColumn, Func<TruthState, int>>();
			int index = 0;
			foreach(InputPinSocket socket in this.testSocket.Inputs) {
				DataGridTextColumn column = new DataGridTextColumn();
				column.Header = socket.Pin.Name;
				column.Binding = new Binding("Input[" + index + "]");
				column.Binding.StringFormat = "{0:X}";
				this.dataGrid.Columns.Add(column);
				dataAccessor.Add(column, DialogTruthTable.InputFieldAccesor(index));
				index++;
			}
			index = 0;
			foreach(OutputPinSocket socket in this.testSocket.Outputs) {
				DataGridTextColumn column = new DataGridTextColumn();
				column.Header = socket.Pin.Name;
				column.Binding = new Binding("Output[" + index + "]");
				column.Binding.StringFormat = "{0:X}";
				this.dataGrid.Columns.Add(column);
				dataAccessor.Add(column, DialogTruthTable.OutputFieldAccesor(index));
				index++;
			}

			this.sortComparer = new TruthStateComparer(dataAccessor);
			this.dataGrid.Sorting += new DataGridSortingEventHandler(this.DataGridSorting);
		}

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			if(this.building != 0) {
				e.Cancel = true;
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
			if(0 < this.TotalRows) {
				if(Interlocked.CompareExchange(ref this.building, 1, 0) == 0) {
					this.SetShowProgress(true);
					Predicate<TruthState> include = null;
					if(DialogTruthTable.MaxRows < this.TotalRows) {
						include = this.Filter();
					}
					this.Progress = 0;
					this.abort = false;
					ThreadPool.QueueUserWorkItem(o => {
						try {
							IList<TruthState> table = null;
							bool truncated = false;
							this.testSocket.LogicalCircuit.CircuitProject.InTransaction(
								() => table = this.testSocket.BuildTruthTable(
									this.SetProgress, () => !this.abort, include, DialogTruthTable.MaxRows, out truncated
								)
							);
							this.Dispatcher.BeginInvoke(new Action(() => {
								this.TruthTable = new ListCollectionView((IList)table);
								this.Truncated = truncated;
							}));
						} catch(Exception exception) {
							App.Mainframe.ReportException(exception);
						} finally {
							this.SetShowProgress(false);
							this.building = 0;
						}
					});
				}
			} else {
				this.TruthTable = new ListCollectionView(new List<TruthState>());
			}
		}

		private void SetProgress(int progress) {
			this.Dispatcher.BeginInvoke(new Action(() => this.Progress = progress));
		}

		private void SetShowProgress(bool show) {
			this.Dispatcher.Invoke(new Action(() => this.ShowProgress = show));
		}

		private void ApplyFilter() {
			try {
				if(DialogTruthTable.MaxRows < this.TotalRows) {
					this.BuildTruthTable();
				} else {
					Predicate<TruthState> include = this.Filter();
					if(include != null) {
						this.TruthTable.Filter = o => o is TruthState ? include((TruthState)o) : false;
					} else {
						this.TruthTable.Filter = null;
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonApplyClick(object sender, RoutedEventArgs e) {
			this.ApplyFilter();
		}

		private void ButtonStopClick(object sender, RoutedEventArgs e) {
			this.abort = true;
		}

		private Predicate<TruthState> Filter() {
			if(this.filter != null) {
				string text = this.filter.Text.Trim();
				bool inverted = this.InvertFilter;
				if(!string.IsNullOrEmpty(text)) {
					ExpressionParser parser = new ExpressionParser(this.testSocket);
					Func<TruthState, int> func = parser.Parse(text);
					if(parser.Error == null) {
						if(inverted) {
							return s => func(s) == 0;
						} else {
							return s => func(s) != 0;
						}
					} else {
						App.Mainframe.ErrorMessage(parser.Error);
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
			private List<Sort> sort = new List<Sort>();

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

			public int Compare(object x, object y) {
				if(x is TruthState && y is TruthState) {
					return this.Compare((TruthState)x, (TruthState)y);
				}
				return 0;
			}
		}
	}
}
