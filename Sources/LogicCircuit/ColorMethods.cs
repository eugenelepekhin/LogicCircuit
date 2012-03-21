using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace LogicCircuit {
	public static class ColorMethods {
		public static int CompareTo(this Color color1, Color color2) {
			return color1.ToInt32() - color2.ToInt32();
		}

		public static int ToInt32(this Color color) {
			return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
		}
	}
}
