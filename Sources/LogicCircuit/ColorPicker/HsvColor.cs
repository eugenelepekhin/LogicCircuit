using System;
using System.Windows.Media;

namespace LogicCircuit {
	internal struct HsvColor {
		public double Hue;
		public double Saturation;
		public double Value;

		public HsvColor(int red, int green, int blue) {
			double r = (double)red / 255;
			double g = (double)green / 255;
			double b = (double)blue / 255;
			double min = Math.Min(r, Math.Min(g, b));
			this.Value = Math.Max(r, Math.Max(g, b));
			double delta = this.Value - min;
			if(this.Value == 0) {
				this.Saturation = 0;
			} else {
				this.Saturation = delta / this.Value;
			}
			if(this.Saturation == 0) {
				this.Hue = 0;
			} else {
				if(r == this.Value) {
					this.Hue = (g - b) / delta;
				} else if(g == this.Value) {
					this.Hue = 2 + (b - r) / delta;
				} else { //b == this.Value
					this.Hue = 4 + (r - g) / delta;
				}
			}
			this.Hue *= 60;
			if(this.Hue < 0) {
				this.Hue += 360;
			}
		}

		public Color ToRgb(int alpha) {
			double chroma = this.Value * this.Saturation;
			if(this.Hue == 360) {
				this.Hue = 0;
			}
			double a = (double)alpha / 255;
			double hue = this.Hue / 60;
			double x = chroma * (1 - Math.Abs(hue % 2 - 1));
			double m = this.Value - chroma;
			switch((int)hue) {
			case 0:  return HsvColor.Rgb(a, chroma, x, 0, m);
			case 1:  return HsvColor.Rgb(a, x, chroma, 0, m);
			case 2:  return HsvColor.Rgb(a, 0, chroma, x, m);
			case 3:  return HsvColor.Rgb(a, 0, x, chroma, m);
			case 4:  return HsvColor.Rgb(a, x, 0, chroma, m);
			default: return HsvColor.Rgb(a, chroma, 0, x, m);
			}
		}

		private static Color Rgb(double alpha, double red, double green, double blue, double m) {
			return Color.FromArgb(
				(byte)(alpha * 255 + 0.5),
				(byte)((red + m) * 255 + 0.5),
				(byte)((green + m) * 255 + 0.5),
				(byte)((blue + m) * 255 + 0.5)
			);
		}
	}
}
