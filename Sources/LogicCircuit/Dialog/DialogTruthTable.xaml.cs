using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

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

		public ListCollectionView TruthTable {
			get { return (ListCollectionView)this.GetValue(DialogTruthTable.TruthTableProperty); }
			set { this.SetValue(DialogTruthTable.TruthTableProperty, value); }
		}

		private readonly CircuitTestSocket testSocket;
		private readonly TruthStateComparer sortComparer;
		private Task task;

		public DialogTruthTable(LogicalCircuit logicalCircuit) {
			this.testSocket  = new CircuitTestSocket(logicalCircuit);
			this.task = Task.Factory.StartNew(this.BuildTruthTable);
			this.DataContext = this;
			this.InitializeComponent();

			Dictionary<DataGridTextColumn, Func<TruthState, TriNumber>> dataAccessor = new Dictionary<DataGridTextColumn, Func<TruthState, TriNumber>>();
			int index = 0;
			foreach(InputPinSocket socket in this.testSocket.Inputs) {
				DataGridTextColumn column = new DataGridTextColumn();
				column.Header = socket.Pin.Name;
				column.Binding = new Binding("Input[" + index + "]");
				this.dataGrid.Columns.Add(column);
				dataAccessor.Add(column, DialogTruthTable.InputFieldAccesor(index));
				index++;
			}
			index = 0;
			foreach(OutputPinSocket socket in this.testSocket.Outputs) {
				DataGridTextColumn column = new DataGridTextColumn();
				column.Header = socket.Pin.Name;
				column.Binding = new Binding("Output[" + index + "]");
				this.dataGrid.Columns.Add(column);
				dataAccessor.Add(column, DialogTruthTable.OutputFieldAccesor(index));
				index++;
			}

			this.sortComparer = new TruthStateComparer(dataAccessor);
			this.dataGrid.Sorting += new DataGridSortingEventHandler(this.DataGridSorting);
		}

		private static Func<TruthState, TriNumber> InputFieldAccesor(int index) {
			return s => s.Input[index];
		}

		private static Func<TruthState, TriNumber> OutputFieldAccesor(int index) {
			return s => s.Output[index];
		}

		private void BuildTruthTable() {
			IList<TruthState> table = null;
			this.testSocket.LogicalCircuit.CircuitProject.InTransaction(() => table = this.testSocket.BuildTruthTable());
			this.Dispatcher.BeginInvoke(new Action(() => this.TruthTable = new ListCollectionView((IList)table)), DispatcherPriority.Normal);
		}

		private  void DataGridSorting(object sender, DataGridSortingEventArgs e) {
			this.sortComparer.ToggleColumn(this, (DataGridTextColumn)e.Column);
			this.TruthTable.CustomSort  = this.sortComparer;
			e.Handled = true;
		}

		private class TruthStateComparer : IComparer<TruthState>, IComparer {
			private struct Sort {
				public Func<TruthState, TriNumber> Data;
				public int Direction;
				public DataGridTextColumn Column;
			}
			
			private readonly Dictionary<DataGridTextColumn, Func<TruthState, TriNumber>> dataAccessor;
			private List<Sort> sort = new List<Sort>();


			public TruthStateComparer(Dictionary<DataGridTextColumn, Func<TruthState, TriNumber>> dataAccessor) {
				this.dataAccessor = dataAccessor;
			}

			public void ToggleColumn(DialogTruthTable dialog, DataGridTextColumn column) {
				bool found = false;
				for(int i = 0; i < this.sort.Count; i++) {
					Sort s = this.sort[i];
					if(s.Column == column) {
						if(s.Direction == 1) {
							s.Direction = -1;
							this.sort[i] = s;
						} else {
							this.sort.RemoveAt(i);
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
				}
			}

			public int Compare(TruthState x, TruthState y) {
				foreach(Sort s in this.sort) {
					int result = s.Data(x).CompareTo(s.Data(y));
					if(result != 0) {
						return result;
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
