using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LogicCircuit {

	/// <summary>
	/// Interaction logic for DialogOscilloscope.xaml
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public partial class DialogOscilloscope : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public CircuitRunner CircuitRunner { get; private set; }
		private Oscilloscope oscilloscope;
		private List<Oscillogram> oscillograms = new List<Oscillogram>();
		private Timer timer;
		public double GraphWidth { get; private set; }
		public double GraphHeight { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public Rect GraphGrid { get { return new Rect(0, 0, Oscillogram.DX, Oscillogram.DY); } }
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public Point GridEndPoint { get { return new Point(0, Oscillogram.DY); } }

		public DialogOscilloscope(CircuitRunner circuitRunner) {
			this.CircuitRunner = circuitRunner;
			this.CircuitRunner.DialogOscilloscope = this;
			this.oscilloscope = new Oscilloscope(this.CircuitRunner);
			this.GraphWidth = CircuitRunner.HistorySize * Oscillogram.DX;
			this.GraphHeight = 3 * Oscillogram.DY;
			this.DataContext = this;
			foreach(string probe in this.oscilloscope.Probes) {
				this.oscillograms.Add(new Oscillogram(probe, this.oscilloscope[probe]));
			}
			this.InitializeComponent();
			this.timer = new Timer(new TimerCallback(this.TimerTick), null, TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.0 / 20.0));
		}

		public IEnumerable<Oscillogram> Oscillograms { get { return this.oscillograms; } }

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			this.timer.Dispose();
			this.timer = null;
			this.CircuitRunner.DialogOscilloscope = null;
			base.OnClosing(e);
		}

		private void TimerTick(object dummy) {
			if(this.timer != null && this.CircuitRunner.Oscilloscoping && this.CircuitRunner.IsRunning) {
				this.CircuitRunner.Oscilloscope = this.oscilloscope;
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.Refresh));
			}
		}

		private void Refresh() {
			if(this.CircuitRunner.Oscilloscoping && this.CircuitRunner.IsRunning) {
				foreach(Oscillogram o in this.oscillograms) {
					o.Refresh();
				}
			}
		}

		private void OscillogramListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			//Just unselect all selected as blue selection interferes with oscillogram and makes it look ugly
			try {
				ListView listView = sender as ListView;
				if(listView != null) {
					foreach(object item in e.AddedItems) {
						ListViewItem container = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
						if(container != null) {
							container.IsSelected = false;
						}
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class Oscillogram {

			public const double DX = 8;
			public const double DY = 20;

			private State[] state;
			public string Name { get; private set; }
			public Polyline Line { get; private set; }

			public Oscillogram(string name, State[] state) {
				this.state = state;
				this.Name = name;
				this.Line = new Polyline();
				this.Refresh();
			}

			public void Refresh() {
				this.Line.Points.Clear();
				State s = this.state[0];
				this.Line.Points.Add(new Point(0, (2 - (int)s) * DY));
				for(int i = 1; i < this.state.Length; i++) {
					if(s != this.state[i]) {
						this.Line.Points.Add(new Point(i * DX, (2 - (int)s) * DY));
						s = this.state[i];
					}
					this.Line.Points.Add(new Point(i * DX, (2 - (int)s) * DY));
				}
			}
		}
	}
}
