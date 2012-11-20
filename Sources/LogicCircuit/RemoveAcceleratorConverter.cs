using System;
using System.Windows.Data;

namespace LogicCircuit {
	[ValueConversion(typeof(bool), typeof(bool))]
	public class RemoveAcceleratorConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if(value != null) {
				return value.ToString().Replace("_", string.Empty);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new InvalidOperationException();
		}
	}
}
