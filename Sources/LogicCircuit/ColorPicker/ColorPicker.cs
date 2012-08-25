using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LogicCircuit {
	public class ColorPicker : Control {
		public const int ColorsInRow = 10;

		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColorPicker));
		public bool IsOpen {
			get { return (bool)this.GetValue(ColorPicker.IsOpenProperty); }
			set { this.SetValue(ColorPicker.IsOpenProperty, value); }
		}

		public static readonly DependencyProperty ColorPickerModeProperty = DependencyProperty.Register("ColorPickerMode", typeof(ColorPickerMode), typeof(ColorPicker));
		public ColorPickerMode ColorPickerMode {
			get { return (ColorPickerMode)this.GetValue(ColorPicker.ColorPickerModeProperty); }
			set { this.SetValue(ColorPicker.ColorPickerModeProperty, value); }
		}

		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker));
		public Color SelectedColor {
			get { return (Color)this.GetValue(ColorPicker.SelectedColorProperty); }
			set { this.SetValue(ColorPicker.SelectedColorProperty, value); }
		}

		public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(ColorPicker));
		public object SelectedValue {
			get { return this.GetValue(ColorPicker.SelectedValueProperty); }
			set { this.SetValue(ColorPicker.SelectedValueProperty, value); }
		}

		public static readonly DependencyProperty CustomColorProperty = DependencyProperty.Register("CustomColor", typeof(Color), typeof(ColorPicker));
		public Color CustomColor {
			get { return (Color)this.GetValue(ColorPicker.CustomColorProperty); }
			set { this.SetValue(ColorPicker.CustomColorProperty, value); }
		}

		public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(ColorPicker));
		public double Hue {
			get { return (double)this.GetValue(ColorPicker.HueProperty); }
			set { this.SetValue(ColorPicker.HueProperty, value); }
		}

		public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(ColorPicker));
		public double Saturation {
			get { return (double)this.GetValue(ColorPicker.SaturationProperty); }
			set { this.SetValue(ColorPicker.SaturationProperty, value); }
		}

		public static readonly DependencyProperty LightnessProperty = DependencyProperty.Register("Lightness", typeof(double), typeof(ColorPicker));
		public double Lightness {
			get { return (double)this.GetValue(ColorPicker.LightnessProperty); }
			set { this.SetValue(ColorPicker.LightnessProperty, value); }
		}

		public static readonly DependencyProperty RProperty = DependencyProperty.Register("R", typeof(int), typeof(ColorPicker));
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "R")]
		public int R {
			get { return (int)this.GetValue(ColorPicker.RProperty); }
			set { this.SetValue(ColorPicker.RProperty, value); }
		}

		public static readonly DependencyProperty GProperty = DependencyProperty.Register("G", typeof(int), typeof(ColorPicker));
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "G")]
		public int G {
			get { return (int)this.GetValue(ColorPicker.GProperty); }
			set { this.SetValue(ColorPicker.GProperty, value); }
		}

		public static readonly DependencyProperty BProperty = DependencyProperty.Register("B", typeof(int), typeof(ColorPicker));
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
		public int B {
			get { return (int)this.GetValue(ColorPicker.BProperty); }
			set { this.SetValue(ColorPicker.BProperty, value); }
		}

		public static readonly DependencyProperty AProperty = DependencyProperty.Register("A", typeof(int), typeof(ColorPicker));
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
		public int A {
			get { return (int)this.GetValue(ColorPicker.AProperty); }
			set { this.SetValue(ColorPicker.AProperty, value); }
		}

		public static readonly DependencyProperty RedSliderLeftColorProperty = DependencyProperty.Register("RedSliderLeftColor", typeof(Color), typeof(ColorPicker));
		public Color RedSliderLeftColor {
			get { return (Color)this.GetValue(ColorPicker.RedSliderLeftColorProperty); }
			set { this.SetValue(ColorPicker.RedSliderLeftColorProperty, value); }
		}

		public static readonly DependencyProperty RedSliderRightColorProperty = DependencyProperty.Register("RedSliderRightColor", typeof(Color), typeof(ColorPicker));
		public Color RedSliderRightColor {
			get { return (Color)this.GetValue(ColorPicker.RedSliderRightColorProperty); }
			set { this.SetValue(ColorPicker.RedSliderRightColorProperty, value); }
		}

		public static readonly DependencyProperty GreenSliderLeftColorProperty = DependencyProperty.Register("GreenSliderLeftColor", typeof(Color), typeof(ColorPicker));
		public Color GreenSliderLeftColor {
			get { return (Color)this.GetValue(ColorPicker.GreenSliderLeftColorProperty); }
			set { this.SetValue(ColorPicker.GreenSliderLeftColorProperty, value); }
		}

		public static readonly DependencyProperty GreenSliderRightColorProperty = DependencyProperty.Register("GreenSliderRightColor", typeof(Color), typeof(ColorPicker));
		public Color GreenSliderRightColor {
			get { return (Color)this.GetValue(ColorPicker.GreenSliderRightColorProperty); }
			set { this.SetValue(ColorPicker.GreenSliderRightColorProperty, value); }
		}

		public static readonly DependencyProperty BlueSliderLeftColorProperty = DependencyProperty.Register("BlueSliderLeftColor", typeof(Color), typeof(ColorPicker));
		public Color BlueSliderLeftColor {
			get { return (Color)this.GetValue(ColorPicker.BlueSliderLeftColorProperty); }
			set { this.SetValue(ColorPicker.BlueSliderLeftColorProperty, value); }
		}

		public static readonly DependencyProperty BlueSliderRightColorProperty = DependencyProperty.Register("BlueSliderRightColor", typeof(Color), typeof(ColorPicker));
		public Color BlueSliderRightColor {
			get { return (Color)this.GetValue(ColorPicker.BlueSliderRightColorProperty); }
			set { this.SetValue(ColorPicker.BlueSliderRightColorProperty, value); }
		}

		public static readonly DependencyProperty AlphaSliderRightColorProperty = DependencyProperty.Register("AlphaSliderRightColor", typeof(Color), typeof(ColorPicker));
		public Color AlphaSliderRightColor {
			get { return (Color)this.GetValue(ColorPicker.AlphaSliderRightColorProperty); }
			set { this.SetValue(ColorPicker.AlphaSliderRightColorProperty, value); }
		}

		public ObservableCollection<Color> RecentColors { get; private set; }
		public IList<Color> StandardColors { get; private set; }
		public IList<Color> AvailableColors { get; private set; }

		public ICommand SelectColorCommand { get; private set; }

		private bool editing;

		public ColorPicker() : base() {
			this.RecentColors = new ObservableCollection<Color>();
			
			this.StandardColors = new List<Color>(ColorPicker.ColorsInRow);
			this.StandardColors.Add(Colors.Transparent);
			this.StandardColors.Add(Colors.White);
			this.StandardColors.Add(Colors.Gray);
			this.StandardColors.Add(Colors.Black);
			this.StandardColors.Add(Colors.Red);
			this.StandardColors.Add(Colors.Green);
			this.StandardColors.Add(Colors.Blue);
			this.StandardColors.Add(Colors.Yellow);
			this.StandardColors.Add(Colors.Orange);
			this.StandardColors.Add(Colors.Purple);
			System.Diagnostics.Debug.Assert(this.StandardColors.Count == ColorPicker.ColorsInRow);

			List<Color> list = new List<Color>(
				typeof(Colors)
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Select(p => (Color)p.GetValue(null, null))
				.Where(c => !this.StandardColors.Contains(c))
			);
			list.Sort(ColorComparer.comparer);
			this.AvailableColors = new List<Color>(list.Distinct(ColorComparer.comparer));

			this.SelectColorCommand = new LambdaUICommand(LogicCircuit.Resources.ColorPickerCaptionSelectCustom,
				o => {
					this.IsOpen = false;
					this.SelectedColor = this.CustomColor;
					this.AddRecent(this.CustomColor);
				}
			);

			this.SelectedColor = Colors.White;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if(e.Property == ColorPicker.ColorPickerModeProperty && this.RecentColors.Count == 0) {
				switch(this.ColorPickerMode) {
				case ColorPickerMode.Background:
					this.AddRecent(Colors.Yellow);
					this.SelectedColor = Colors.Yellow;
					break;
				case ColorPickerMode.Foreground:
					this.AddRecent(Colors.Black);
					this.SelectedColor = Colors.Black;
					break;
				}
			} else if(e.Property == ColorPicker.SelectedColorProperty) {
				this.SetColor((Color)e.NewValue);
			} else if(!this.editing) {
				try {
					this.editing = true;
					if(e.Property == ColorPicker.SelectedValueProperty && e.NewValue != null && this.IsOpen) {
						this.IsOpen = false;
						Color color = (Color)e.NewValue;
						this.SelectedColor = color;
						this.AddRecent(color);
						this.Dispatcher.BeginInvoke(new Action(() => this.SelectedValue = null));
					} else if(e.Property == ColorPicker.CustomColorProperty) {
						this.SetColor((Color)e.NewValue);
					} else if(e.Property == ColorPicker.HueProperty) {
						this.SetColor((double)e.NewValue, this.Saturation, this.Lightness);
					} else if(e.Property == ColorPicker.SaturationProperty) {
						this.SetColor(this.Hue, (double)e.NewValue, this.Lightness);
					} else if(e.Property == ColorPicker.LightnessProperty) {
						this.SetColor(this.Hue, this.Saturation, (double)e.NewValue);
					} else if(e.Property == ColorPicker.RProperty) {
						this.SetColor(this.A, (int)e.NewValue, this.G, this.B);
					} else if(e.Property == ColorPicker.GProperty) {
						this.SetColor(this.A, this.R, (int)e.NewValue, this.B);
					} else if(e.Property == ColorPicker.BProperty) {
						this.SetColor(this.A, this.R, this.G, (int)e.NewValue);
					} else if(e.Property == ColorPicker.AProperty) {
						this.SetColor((int)e.NewValue, this.R, this.G, this.B);
					}
				} finally {
					this.editing = false;
				}
			}
		}

		private void SetColor(int a, int r, int g, int b) {
			this.SetColor(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b));
		}

		private void SetColor(Color color) {
			this.CustomColor = color;
			this.A = color.A;
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;

			this.SetSlidersBackground(color);

			HsvColor hsv = new HsvColor(color.R, color.G, color.B);
			this.Hue = hsv.Hue;
			this.Saturation = hsv.Saturation;
			this.Lightness = hsv.Value;
		}

		private void SetColor(double hue, double saturation, double value) {
			Color c = new HsvColor() { Hue = hue, Saturation = saturation, Value = value }.ToRgb(this.A);
			this.CustomColor = c;
			this.R = c.R;
			this.G = c.G;
			this.B = c.B;

			this.SetSlidersBackground(c);

			this.Hue = hue;
			this.Saturation = saturation;
			this.Lightness = value;
		}

		private void SetSlidersBackground(Color color) {
			this.RedSliderLeftColor = Color.FromRgb(0, color.G, color.B);
			this.RedSliderRightColor = Color.FromRgb(255, color.G, color.B);

			this.GreenSliderLeftColor = Color.FromRgb(color.R, 0, color.B);
			this.GreenSliderRightColor = Color.FromRgb(color.R, 255, color.B);

			this.BlueSliderLeftColor = Color.FromRgb(color.R, color.G, 0);
			this.BlueSliderRightColor = Color.FromRgb(color.R, color.G, 255);

			this.AlphaSliderRightColor = Color.FromRgb(color.R, color.G, color.B);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Key == Key.Escape) {
				this.IsOpen = false;
			}
		}

		private void AddRecent(Color item) {
			this.RecentColors.Remove(item);
			while(ColorPicker.ColorsInRow <= this.RecentColors.Count) {
				this.RecentColors.RemoveAt(this.RecentColors.Count - 1);
			}
			this.RecentColors.Insert(0, item);
		}

		private class ColorComparer : IComparer<Color>, IEqualityComparer<Color> {
			public static readonly ColorComparer comparer = new ColorComparer();

			public int Compare(Color x, Color y) {
				HsvColor xx = new HsvColor(x.R, x.G, x.B);
				HsvColor yy = new HsvColor(y.R, y.G, y.B);
				return Math.Sign(
					(xx.Hue * 1000000 + 10000 * xx.Value + 1 * xx.Saturation) -
					(yy.Hue * 1000000 + 10000 * yy.Value + 1 * yy.Saturation)
				);
			}

			public bool Equals(Color x, Color y) {
				return x.Equals(y);
			}

			public int GetHashCode(Color obj) {
				return obj.GetHashCode();
			}
		}
	}
}
