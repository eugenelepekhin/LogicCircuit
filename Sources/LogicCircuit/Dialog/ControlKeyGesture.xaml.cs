using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for ControlKeyGesture.xaml
	/// </summary>
	public partial class ControlKeyGesture : UserControl {
		private static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(Key), typeof(ControlKeyGesture));

		public Key Key {
			get => (Key)this.GetValue(ControlKeyGesture.KeyProperty);
			set => this.SetValue(ControlKeyGesture.KeyProperty, value);
		}

		private static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register(nameof(ModifierKeys), typeof(ModifierKeys), typeof(ControlKeyGesture));
		public ModifierKeys ModifierKeys {
			get => (ModifierKeys)this.GetValue(ControlKeyGesture.ModifierKeysProperty);
			set => this.SetValue(ControlKeyGesture.ModifierKeysProperty, value);
		}

		public ControlKeyGesture() {
			this.InitializeComponent();
			this.Reset();
		}

		protected override void OnGotFocus(RoutedEventArgs e) {
			base.OnGotFocus(e);
			this.textBox.Focus();
		}

		public void Refresh() {
			Key key = this.Key;
			ModifierKeys modifier = this.ModifierKeys;
			StringBuilder text = new StringBuilder();
			if(modifier != ModifierKeys.None) {
				text.Append(modifier.ToString());
				text.Append('+');
			}
			text.Append(key.ToString());
			this.textBox.Text = text.ToString();
		}

		private void Reset() {
			this.Key = Key.None;
			this.ModifierKeys = ModifierKeys.None;
			this.textBox.Text = string.Empty;
		}

		private void textBoxPreviewKeyDown(object sender, KeyEventArgs e) {
			Key key = e.Key;
			ModifierKeys modifier = e.KeyboardDevice.Modifiers;

			// When Alt is pressed, SystemKey is used instead
			if(key == Key.System) {
				key = e.SystemKey;
			}

			if(modifier == ModifierKeys.None && key == Key.Escape) {
				bool empty = this.Key == Key.None && this.ModifierKeys == ModifierKeys.None;
				this.Reset();
				if(!empty) {
					e.Handled = true;
				}
				return;
			}

			switch(key) {
			case Key.LeftCtrl:
			case Key.RightCtrl:
			case Key.LeftAlt:
			case Key.RightAlt:
			case Key.LeftShift:
			case Key.RightShift:
			case Key.LWin:
			case Key.RWin:
			case Key.Clear:
			case Key.OemClear:
			case Key.Apps:
			case Key.Tab:
			case Key.Left:
			case Key.Right:
			case Key.Up:
			case Key.Down:
			case Key.PageUp:
			case Key.PageDown:
			case Key.Home:
			case Key.End:
			case Key.Insert:
			case Key.Delete:
			case Key.CapsLock:
				return;
			}

			e.Handled = true;
			this.Key = key;
			this.ModifierKeys = modifier;
			this.Refresh();
		}
	}
}
