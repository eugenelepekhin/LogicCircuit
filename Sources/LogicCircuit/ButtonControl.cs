using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	public class ButtonControl : Button {
		public bool Clickable { get; set; }

		public Action<CircuitSymbol, bool> ButtonPressed { get; set; }

		public ButtonControl() : base() {
			//this.Clickable = false;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseDown(e);
			}
		}
		protected override void OnMouseUp(MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseUp(e);
			}
		}
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseLeftButtonDown(e);
			}
		}
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseLeftButtonUp(e);
			}
		}
		protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e) {
			if(this.Clickable) {
				base.OnIsPressedChanged(e);

				Action<CircuitSymbol, bool> action = this.ButtonPressed;
				if(action != null) {
					action((CircuitSymbol)this.DataContext, this.IsPressed);
				}
			}
		}
	}
}
