using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	public class ButtonControl : Button {
		public Action<CircuitSymbol, bool> ButtonStateChanged { get; set; }

		public ButtonControl() : base() {
			//this.ButtonStateChanged = null;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			if(this.ButtonStateChanged != null) {
				base.OnMouseDown(e);
			}
		}
		protected override void OnMouseUp(MouseButtonEventArgs e) {
			if(this.ButtonStateChanged != null) {
				base.OnMouseUp(e);
			}
		}
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			if(this.ButtonStateChanged != null) {
				base.OnMouseLeftButtonDown(e);
			}
		}
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			if(this.ButtonStateChanged != null) {
				base.OnMouseLeftButtonUp(e);
			}
		}
		protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e) {
			Action<CircuitSymbol, bool> action = this.ButtonStateChanged;
			if(action != null) {
				base.OnIsPressedChanged(e);

				action((CircuitSymbol)this.DataContext, this.IsPressed);
			}
		}
	}
}
