using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LogicCircuit {
	[ValueConversion(typeof(double), typeof(Color))]
	public class HueToColorConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(value != null && value is double && targetType == typeof(Color)) {
				return new HsvColor() { Hue = (double)value, Saturation = 1, Value = 1 }.ToRgb(255);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new InvalidOperationException();
		}
	}
}
