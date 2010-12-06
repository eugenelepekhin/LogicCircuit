using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace LogicCircuit {
	public class ButtonControl : Button {
		public bool Clickable { get; set; }

		public ButtonControl() : base() {
			//this.Clickable = false;
		}

		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseDown(e);
			}
		}
		protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseUp(e);
			}
		}
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseLeftButtonDown(e);
			}
		}
		protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e) {
			if(this.Clickable) {
				base.OnMouseLeftButtonUp(e);
			}
		}
	}
}
