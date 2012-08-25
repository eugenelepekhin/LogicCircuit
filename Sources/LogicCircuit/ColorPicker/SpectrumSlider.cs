using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogicCircuit {
	public class SpectrumSlider : Slider {
		public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(SpectrumSlider));
		public double Hue {
			get { return (double)this.GetValue(SpectrumSlider.HueProperty); }
			set { this.SetValue(SpectrumSlider.HueProperty, value); }
		}

		private bool changing = false;
		
		public SpectrumSlider() {
			this.SetBackground();
		}

		private void SetBackground() {
			LinearGradientBrush brush = new LinearGradientBrush();
			Color[] colors = SpectrumSlider.CreateSpectrum(36);
			brush.StartPoint = new Point(0.5, 0);
			brush.EndPoint = new Point(0.5, 1);
			for(int i = 0; i < colors.Length; i++) {
				brush.GradientStops.Add(new GradientStop(colors[i], (double)i / colors.Length));
			}
			this.Background = brush;
		}

		protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate) {
			base.OnTemplateChanged(oldTemplate, newTemplate);
			this.SetBackground();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			try {
				if(!this.changing) {
					this.changing = true;
					if(e.Property == SpectrumSlider.ValueProperty) {
						this.Hue = (double)e.NewValue;
					} else if(e.Property == SpectrumSlider.HueProperty) {
						this.Value = (double)e.NewValue;
					}
				}
			} catch(Exception exception) {
				Tracer.Report("SpectrumSlider.OnPropertyChanged", exception);
			} finally {
				this.changing = false;
			}
		}

		private static Color[] CreateSpectrum(int colorCount) {
			Color[] spectrum = new Color[colorCount];
			for(int i = 0; i < colorCount; i++) {
				double hue = (i * 360.0) / spectrum.Length;
				spectrum[i] = new HsvColor() { Hue = hue, Saturation = 1, Value = 1 }.ToRgb(255);
			}
			return spectrum;
		}
	}
}
