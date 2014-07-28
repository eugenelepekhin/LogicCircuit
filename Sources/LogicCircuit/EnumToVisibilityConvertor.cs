using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogicCircuit {
	public class EnumToVisibilityConvertor : IValueConverter {
		public Visibility HiddenVisibility { get; set; }

		public EnumToVisibilityConvertor() {
			this.HiddenVisibility = Visibility.Collapsed;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Tracer.Assert(targetType == typeof(Visibility));
			if(value.Equals(parameter)) {
				return Visibility.Visible;
			}
			return this.HiddenVisibility;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
