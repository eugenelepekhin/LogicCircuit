using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LogicCircuit {
	public class AutoWidthComboBox : ComboBox {
		public AutoWidthComboBox() {
			this.Loaded += (object sender, RoutedEventArgs e) => {
				Popup popup = this.GetTemplateChild("PART_Popup") as Popup;
				FrameworkElement button = this.GetTemplateChild("toggleButton") as FrameworkElement;
				if(popup != null) {
					UIElement child = popup.Child;
					child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					this.MinWidth = Math.Max(this.MinWidth, child.DesiredSize.Width + ((button != null) ? button.DesiredSize.Width : 20));
				}
			};
		}
	}
}
