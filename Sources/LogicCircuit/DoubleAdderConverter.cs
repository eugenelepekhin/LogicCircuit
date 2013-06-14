using System;
using System.Globalization;
using System.Windows.Data;

namespace LogicCircuit {
	public class DoubleAdderConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(targetType == typeof(double) && value is double && parameter != null) {
				double delta;
				if(!double.TryParse(parameter.ToString(), out delta)) {
					delta = 0;
				}
				return Math.Max(0, (double)value + delta);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new InvalidOperationException();
		}
	}
}
