using System;
using System.Globalization;
using System.Windows.Data;

namespace LogicCircuit {
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Tracer.Assert(targetType == typeof(bool));
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return this.Convert(value, targetType, parameter, culture);
		}
	}
}
