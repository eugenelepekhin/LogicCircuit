using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace LogicCircuit {
	public class ColorThumb : Thumb {
		public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(ColorThumb));
		public double Saturation {
			get { return (double)this.GetValue(ColorThumb.SaturationProperty); }
			set { this.SetValue(ColorThumb.SaturationProperty, value); }
		}

		public static readonly DependencyProperty LightnessProperty = DependencyProperty.Register("Lightness", typeof(double), typeof(ColorThumb));
		public double Lightness {
			get { return (double)this.GetValue(ColorThumb.LightnessProperty); }
			set { this.SetValue(ColorThumb.LightnessProperty, value); }
		}

		public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(ColorThumb));
		public double X {
			get { return (double)this.GetValue(ColorThumb.XProperty); }
			set { this.SetValue(ColorThumb.XProperty, value); }
		}

		public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ColorThumb));
		public double Y {
			get { return (double)this.GetValue(ColorThumb.YProperty); }
			set { this.SetValue(ColorThumb.YProperty, value); }
		}

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);
			e.Handled = false;
		}

		protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e) {
			base.OnMouseLeftButtonUp(e);
			e.Handled = false;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if(e.Property == ColorThumb.SaturationProperty || e.Property == ColorThumb.ActualWidthProperty) {
				Size size = this.ParentSize();
				this.X = size.Width * this.Saturation;
			} else if(e.Property == ColorThumb.LightnessProperty || e.Property == ColorThumb.ActualHeightProperty) {
				Size size = this.ParentSize();
				this.Y = size.Height * (1 - this.Lightness);
			}
		}

		private Size ParentSize() {
			FrameworkElement panel = this.Parent as FrameworkElement;
			if(panel != null) {
				return new Size(panel.ActualWidth, panel.ActualHeight);
			}
			return Size.Empty;
		}
	}
}
