using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LogicCircuit {
	public class AutoWidthComboBox : ComboBox {
		public AutoWidthComboBox() {
			this.Loaded += (object sender, RoutedEventArgs e) => {
				if(this.GetTemplateChild("PART_Popup") is Popup popup) {
					UIElement child = popup.Child;
					child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					this.MinWidth = Math.Max(this.MinWidth, child.DesiredSize.Width + ((this.GetTemplateChild("toggleButton") is FrameworkElement button) ? button.DesiredSize.Width : 20));
				}
			};
		}
	}
}
