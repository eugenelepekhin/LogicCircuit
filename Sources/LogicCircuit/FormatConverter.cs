using System;
using System.Globalization;
using System.Windows.Data;

namespace LogicCircuit {
	public class FormatConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Tracer.Assert(parameter != null && (parameter is string));
			Tracer.Assert(targetType == typeof(string));
			return string.Format(App.CurrentCulture, parameter.ToString(), value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
