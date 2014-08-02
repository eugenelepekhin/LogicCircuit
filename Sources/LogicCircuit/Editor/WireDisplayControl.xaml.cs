using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for WireDisplayControl.xaml
	/// </summary>
	public partial class WireDisplayControl : UserControl {
		private readonly Editor editor;
		private DispatcherTimer timer;

		private CircuitState circuitState;
		private int[] parameter;
		private State[] state;
		private bool initiated;
		
		public WireDisplayControl(Canvas canvas, Point point, Wire wire) {
			this.InitializeComponent();
			Panel.SetZIndex(this, int.MaxValue);
			Canvas.SetLeft(this, point.X - 3);
			Canvas.SetTop(this, point.Y - 3);
			canvas.Children.Add(this);
			this.UpdateLayout();

			if(this.CaptureMouse() && Mouse.LeftButton == MouseButtonState.Pressed) {
				this.editor = App.Mainframe.Editor;
				CircuitMap map = this.editor.CircuitRunner.VisibleMap;
				Tracer.Assert(wire.LogicalCircuit == map.Circuit);
				this.parameter = map.StateIndexes(wire).ToArray();
				if(0 < this.parameter.Length) {
					this.circuitState = this.editor.CircuitRunner.CircuitState;
					this.state = new State[this.parameter.Length];
					this.bitWidth.Text = Properties.Resources.WireDisplayBitWidth(this.parameter.Length);

					this.timer = new DispatcherTimer(DispatcherPriority.Normal, App.Mainframe.Dispatcher);
					this.timer.Tick += TimerTick;
					this.timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / (this.editor.IsMaximumSpeed ? 25 : Math.Min(25, this.editor.Frequency * 4)));
				} else {
					this.bitWidth.Text = Properties.Resources.WireDisplayDisconnected;
					this.display.Visibility = Visibility.Collapsed;
				}
			} else {
				this.Cancel();
			}
		}

		public void Start() {
			DispatcherTimer t = this.timer;
			if(t != null) {
				this.TimerTick(null, null);
				t.Start();
			}
		}

		private void Cancel() {
			this.ReleaseMouseCapture();
			Panel parent = this.Parent as Panel;
			if(parent != null) {
				parent.Children.Remove(this);
			}
			DispatcherTimer t = this.timer;
			this.timer = null;
			if(t != null) {
				t.Stop();
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e) {
			base.OnMouseUp(e);
			this.Cancel();
		}

		protected override void OnLostMouseCapture(MouseEventArgs e) {
			base.OnLostMouseCapture(e);
			this.Cancel();
		}
		
		private void TimerTick(object sender, EventArgs e) {
			if(!this.editor.InEditMode) {
				if(this.WasChanged()) {
					this.display.Text = Properties.Resources.WireDisplayValue(CircuitFunction.ToText(this.state, true));
				}
			} else {
				this.Cancel();
			}
		}

		private bool WasChanged() {
			bool chaged = this.GetState();
			if(this.initiated) {
				return chaged;
			}
			this.initiated = true;
			return true;
		}

		// This is very similar to probe. But for now lets not merge code.

		private bool GetState() {
			bool changed = false;
			for(int i = 0; i < this.parameter.Length; i++) {
				State s = this.circuitState[this.parameter[i]];
				if(this.state[i] != s) {
					this.state[i] = s;
					changed = true;
				}
			}
			return changed;
		}
	}
}
