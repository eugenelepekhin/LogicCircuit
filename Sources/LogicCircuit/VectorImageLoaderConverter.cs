using System;
using System.Globalization;
using System.Windows.Data;

namespace LogicCircuit {
	class VectorImageLoaderConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			string path = parameter as string;
			if(path != null) {
				return Symbol.Skin(path);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new InvalidOperationException();
		}
	}
}
