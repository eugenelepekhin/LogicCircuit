using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	public class SaturationPicker : Canvas {
		public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(SaturationPicker));
		public double Saturation {
			get { return (double)this.GetValue(SaturationPicker.SaturationProperty); }
			set { this.SetValue(SaturationPicker.SaturationProperty, value); }
		}

		public static readonly DependencyProperty LightnessProperty = DependencyProperty.Register("Lightness", typeof(double), typeof(SaturationPicker));
		public double Lightness {
			get { return (double)this.GetValue(SaturationPicker.LightnessProperty); }
			set { this.SetValue(SaturationPicker.LightnessProperty, value); }
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);
			this.FromPoint(e.GetPosition(this));
			this.CaptureMouse();
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonUp(e);
			if(this.IsMouseCaptured) {
				this.ReleaseMouseCapture();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if(e.LeftButton == MouseButtonState.Pressed) {
				this.FromPoint(e.GetPosition(this));
			}
		}

		private void FromPoint(Point point) {
			this.Saturation = Math.Max(0, Math.Min(point.X / this.ActualWidth, 1));
			this.Lightness = Math.Max(0, Math.Min(1 - point.Y / this.ActualHeight, 1));
		}
	}
}
