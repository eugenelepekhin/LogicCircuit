using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class ColorSlider : Slider {
		public static readonly DependencyProperty LeftColorProperty = DependencyProperty.Register("LeftColor", typeof(Color), typeof(ColorSlider));
		public Color LeftColor {
			get { return (Color)this.GetValue(ColorSlider.LeftColorProperty); }
			set { this.SetValue(ColorSlider.LeftColorProperty, value); }
		}

		public static readonly DependencyProperty RightColorProperty = DependencyProperty.Register("RightColor", typeof(Color), typeof(ColorSlider));
		public Color RightColor {
			get { return (Color)this.GetValue(ColorSlider.RightColorProperty); }
			set { this.SetValue(ColorSlider.RightColorProperty, value); }
		}
	}
}
