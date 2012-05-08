using System;
using System.Collections.Generic;
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
			"TruthTable", typeof(IEnumerable<TruthState>), typeof(DialogTruthTable)
		);

		public IEnumerable<TruthState> TruthTable {
			get { return (IEnumerable<TruthState>)this.GetValue(DialogTruthTable.TruthTableProperty); }
			set { this.SetValue(DialogTruthTable.TruthTableProperty, value); }
		}

		public CircuitTestSocket TestSocket { get; private set; }
		private Task task;

		public GridView Columns {
			get {
				GridView gridView = new GridView();
				int index = 0;
				foreach(InputPinSocket socket in this.TestSocket.Inputs) {
					GridViewColumn column = new GridViewColumn();
					column.Header = socket.Pin.Name;
					column.DisplayMemberBinding = new Binding("Input[" + index + "]");
					gridView.Columns.Add(column);
					index++;
				}
				index = 0;
				foreach(OutputPinSocket socket in this.TestSocket.Outputs) {
					GridViewColumn column = new GridViewColumn();
					column.Header = socket.Pin.Name;
					column.DisplayMemberBinding = new Binding("Output[" + index + "]");
					gridView.Columns.Add(column);
					index++;
				}
				return gridView;
			}
		}

		public DialogTruthTable(LogicalCircuit logicalCircuit) {
			this.TestSocket  = new CircuitTestSocket(logicalCircuit);
			this.task = Task.Factory.StartNew(this.BuildTruthTable);
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void BuildTruthTable() {
			IEnumerable<TruthState> table = null;
			this.TestSocket.LogicalCircuit.CircuitProject.InTransaction(() => table = this.TestSocket.BuildTruthTable());
			this.Dispatcher.BeginInvoke(new Action(() => this.TruthTable = table), DispatcherPriority.Normal);
		}
	}
}
