using System;
using System.Collections.Generic;
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
		
		public WireDisplayControl(Canvas canvas, Point point, Wire wire) {
			this.InitializeComponent();
			Panel.SetZIndex(this, int.MaxValue);
			Canvas.SetLeft(this, point.X - 3);
			Canvas.SetTop(this, point.Y - 3);
			canvas.Children.Add(this);
			this.UpdateLayout();

			if(this.CaptureMouse() && Mouse.LeftButton == MouseButtonState.Pressed) {
				this.editor = App.Mainframe.Editor;
				this.timer = new DispatcherTimer(DispatcherPriority.Normal, App.Mainframe.Dispatcher);
				this.timer.Tick += TimerTick;
				this.timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / Math.Min(3, this.editor.Frequency));
				this.GetState(wire);
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

		private void GetState(Wire wire) {
			CircuitMap map = this.editor.CircuitRunner.VisibleMap;
			Tracer.Assert(wire.LogicalCircuit == map.Circuit);

		}
		
		private void TimerTick(object sender, EventArgs e) {
			if(!this.editor.InEditMode) {
				this.display.Text = string.Format("time={0}", DateTime.Now.ToLongTimeString());
			} else {
				this.Cancel();
			}
		}
	}
}
