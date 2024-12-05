using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LogicCircuit {
	public class FunctionSensor : CircuitFunction, IFunctionClock, IFunctionVisual {

		public override string ReportName { get { return Properties.Resources.NameSensor; } }

		public bool Invalid { get; set; }

		private readonly List<CircuitSymbol> circuitSymbol;

		private Sensor Sensor => (Sensor)this.circuitSymbol[0].Circuit;

		private int BitWidth => this.Sensor.BitWidth;

		private readonly SensorValue sensorValue;

		public int Value { get { return this.sensorValue.Value; } }

		private Brush? defaultBackground;
		private Brush? errorBackground;

		public FunctionSensor(CircuitState circuitState, IEnumerable<CircuitSymbol> symbols, int[] result) : base(circuitState, null, result) {
			this.circuitSymbol = symbols.ToList();
			Tracer.Assert(this.BitWidth == result.Length);
			switch(this.Sensor.SensorType) {
			case SensorType.Series:
			case SensorType.Loop:
				this.sensorValue = new SeriesValue(this.Sensor.Data, this.Sensor.SensorType == SensorType.Loop, this.Sensor.BitWidth);
				break;
			case SensorType.Random:
				this.sensorValue = new RandomValue(this.Sensor.Data, this.Sensor.BitWidth, this.CircuitState.Random);
				break;
			case SensorType.Manual:
				this.sensorValue = new ManualValue(this.Sensor.Data, this.Sensor.BitWidth);
				break;
			case SensorType.KeyCode:
			case SensorType.ASCII:
				this.sensorValue = new KeyboardValue(this);
				break;
			case SensorType.Sequence:
				this.sensorValue = new SequenceValue(this.Sensor.BitWidth);
				break;
			case SensorType.Clock:
				this.sensorValue = new ClockValue(this.Sensor.BitWidth);
				break;
			default:
				Tracer.Fail();
				break;
			}
			Debug.Assert(this.sensorValue != null);
		}

		public bool Flip() {
			return this.sensorValue.Flip();
		}

		public override bool Evaluate() {
			if(this.SetResult(this.Value)) {
				this.Invalid = true;
				return true;
			}
			return false;
		}

		public void TurnOn() {
			foreach(CircuitSymbol symbol in this.circuitSymbol.Where(s => s.HasCreatedGlyph)) {
				FrameworkElement? element = this.ProbeView(symbol);
				if(element is TextBox textBox) {
					textBox.IsEnabled = true;
					if(this.Sensor.SensorType == SensorType.KeyCode || this.Sensor.SensorType == SensorType.ASCII) {
						textBox.Text = Key.None.ToString();
					} else {
						textBox.Text = this.Value.ToString("X", CultureInfo.InvariantCulture);
					}
					this.HookupEvents(textBox);
					this.defaultBackground = textBox.Background;
				}
			}
			this.errorBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0x56, 0x16));
		}

		public void TurnOff() {
			foreach(CircuitSymbol symbol in this.circuitSymbol.Where(s => s.HasCreatedGlyph)) {
				FrameworkElement? element = this.ProbeView(symbol);
				if(element is TextBlock textBlock) {
					textBlock.Text = Sensor.UnknownValue;
				} else if(element is TextBox textBox) {
					textBox.IsEnabled = false;
					textBox.Text = Sensor.UnknownValue;
					textBox.Background = this.defaultBackground;
					this.UnhookEvents(textBox);
				}
			}
		}

		public void Redraw() {
			if(this.circuitSymbol[0].ProbeView is TextBlock textBlock) {
				textBlock.Text = this.Value.ToString("X", CultureInfo.InvariantCulture);
			}
		}

		private FrameworkElement? ProbeView(CircuitSymbol symbol) {
			if(symbol == this.circuitSymbol[0]) {
				return this.circuitSymbol[0].ProbeView!;
			} else {
				DisplayCanvas canvas = (DisplayCanvas)symbol.Glyph;
				return canvas.DisplayOf(this.circuitSymbol);
			}
		}

		private void HookupEvents(TextBox textBox) {
			switch(this.Sensor.SensorType) {
			case SensorType.Manual:
				textBox.PreviewKeyUp += textBox_PreviewKeyUp;
				textBox.PreviewLostKeyboardFocus += textBox_PreviewLostKeyboardFocus;
				break;
			case SensorType.KeyCode:
				textBox.PreviewKeyDown += this.textBox_PreviewKeyDown;
				textBox.KeyUp += this.textBox_KeyUp;
				break;
			case SensorType.ASCII:
				textBox.KeyUp += this.textBox_KeyUp;
				textBox.TextChanged += this.textBox_TextChanged;
				break;
			}
		}

		private void UnhookEvents(TextBox textBox) {
			switch(this.Sensor.SensorType) {
			case SensorType.Manual:
				textBox.PreviewKeyUp -= textBox_PreviewKeyUp;
				textBox.PreviewLostKeyboardFocus -= textBox_PreviewLostKeyboardFocus;
				break;
			case SensorType.KeyCode:
				textBox.PreviewKeyDown -= this.textBox_PreviewKeyDown;
				textBox.KeyUp -= this.textBox_KeyUp;
				break;
			case SensorType.ASCII:
				textBox.KeyUp -= this.textBox_KeyUp;
				textBox.TextChanged -= this.textBox_TextChanged;
				break;
			}
		}

		private void textBox_PreviewKeyUp(object sender, KeyEventArgs e) {
			if(sender is TextBox textBox && e.Key == Key.Enter) {
				this.SetManualValue(textBox);
			}
		}

		private void textBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			if(sender is TextBox textBox) {
				this.SetManualValue(textBox);
			}
		}

		private void textBox_PreviewKeyDown(object sender, KeyEventArgs e) {
			if(sender is TextBox textBox && this.sensorValue is KeyboardValue keyboardValue) {
				keyboardValue.SetKey(textBox, e.Key, e.KeyboardDevice.Modifiers);
				e.Handled = true;
			}
		}

		private void textBox_KeyUp(object sender, KeyEventArgs e) {
			if(sender is TextBox textBox && this.sensorValue is KeyboardValue keyboardValue) {
				keyboardValue.SetKey(textBox, Key.None, ModifierKeys.None);
				e.Handled = true;
			}
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			if(sender is TextBox textBox && this.sensorValue is KeyboardValue keyboardValue) {
				keyboardValue.SetText(textBox);
			}
		}

		private void SetManualValue(TextBox textBox) {
			if(int.TryParse((textBox.Text ?? string.Empty).Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value)) {
				this.sensorValue.Value = value;
				textBox.Background = this.defaultBackground;
			} else {
				textBox.Background = this.errorBackground;
			}
		}

		private abstract class SensorValue {
			public int BitWidth { get; private set; }

			private int sensorValue;
			public int Value {
				get => this.sensorValue;
				set => this.sensorValue = Constant.Normalize(value, this.BitWidth);
			}

			protected SensorValue(int bitWidth) {
				Tracer.Assert(0 < bitWidth && bitWidth <= 32);
				this.BitWidth = bitWidth;
			}

			public abstract bool Flip();
		}

		private class RandomValue : SensorValue {
			private readonly Random random;
			private readonly int maxValue;
			private readonly int minTick;
			private readonly int maxTick;
			private int flip;
			private int tick;
			
			public RandomValue(string data, int bitWidth, Random random) : base(bitWidth) {
				int minTick;
				int maxTick;
				if(Sensor.TryParsePoint(data, 32, out SensorPoint point)) {
					minTick = point.Tick;
					maxTick = point.Value;
				} else {
					minTick = Sensor.DefaultRandomMinInterval;
					maxTick = Sensor.DefaultRandomMaxInterval;
				}
				Tracer.Assert(0 < minTick && minTick <= maxTick);
				this.random = random;
				this.maxValue = (bitWidth < 32) ? 1 << bitWidth : int.MaxValue;
				this.minTick = minTick;
				this.maxTick = maxTick;
				this.Reset();
				this.Value = this.random.Next(this.maxValue);
			}

			public override bool Flip() {
				if(this.flip == this.tick++) {
					this.Reset();
					this.Value = this.random.Next(this.maxValue);
					return true;
				}
				return false;
			}

			private void Reset() {
				this.flip = this.random.Next(this.minTick, this.maxTick);
				this.tick = 0;
			}
		}

		private class SeriesValue : SensorValue {
			private readonly IList<SensorPoint> list;
			private readonly bool isLoop;
			private int index;
			private int tick;
			private bool stop;

			public SeriesValue(string data, bool isLoop, int bitWidth) : base(bitWidth) {
				IList<SensorPoint>? l;
				if(!Sensor.TryParseSeries(data, bitWidth, out l)) {
					l = new List<SensorPoint>();
				}
				Debug.Assert(l != null);
				this.list = l;
				this.stop = (this.list.Count == 0);
				this.isLoop = isLoop;
				this.Reset();
			}

			public override bool Flip() {
				if(!this.stop && this.list[this.index].Tick == this.tick++) {
					this.Value = this.list[this.index].Value;
					if(this.list.Count <= ++this.index) {
						if(this.isLoop) {
							this.Reset();
						} else {
							this.stop = true;
						}
					}
					return true;
				}
				return false;
			}

			private void Reset() {
				this.index = 0;
				this.tick = 0;
			}
		}

		private class ManualValue : SensorValue {
			private int lastValue;

			public ManualValue(string data, int bitWidth) : base(bitWidth) {
				int value;
				if(string.IsNullOrEmpty(data) || !int.TryParse(data, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
					value = 0;
				}
				this.Value = value;
				this.lastValue = value + 1;
			}

			public override bool Flip() {
				if(this.lastValue != this.Value) {
					this.lastValue = this.Value;
					return true;
				}
				return false;
			}
		}

		private class KeyboardValue : SensorValue {
			private readonly FunctionSensor functionSensor;
			private string? lastText;
			private bool ignoreText;

			public KeyboardValue(FunctionSensor functionSensor) : base(functionSensor.BitWidth) {
				this.functionSensor = functionSensor;
			}

			public void SetKey(TextBox textBox, Key key, ModifierKeys modifierKeys) {
				int value = (int)key;
				try {
					this.ignoreText = true;
					textBox.Text = key.ToString();
				} finally {
					this.ignoreText = false;
				}
				if(this.Value != value) {
					this.Value = value;
					this.functionSensor.CircuitState.MarkUpdated(this.functionSensor);
				}
			}

			public void SetText(TextBox textBox) {
				if(!this.ignoreText) {
					string text = textBox.Text;
					if(!string.IsNullOrEmpty(this.lastText)) {
						Match match = Regex.Match(text, Regex.Escape(this.lastText));
						if(match.Success && match.Index != 0 && match.Length != text.Length) {
							text = text.Remove(match.Index, match.Length);
						}
					}
					if(1 < text.Length) {
						text = text.Substring(0, 1);
					}
					this.lastText = text;
					textBox.Text = text;
					int value = text[0];
					if(this.Value != value) {
						this.Value = value;
						this.functionSensor.CircuitState.MarkUpdated(this.functionSensor);
					}
				}
			}

			public override bool Flip() {
				return false;
			}
		}

		private class SequenceValue : SensorValue {
			private readonly int maxValue;

			public SequenceValue(int bitWidth) : base(bitWidth) {
				this.maxValue = (bitWidth < 32) ? 1 << bitWidth : int.MaxValue;
			}

			public override bool Flip() {
				this.Value = Math.Min(this.Value + 1, this.maxValue);
				return true;
			}
		}

		private class ClockValue : SensorValue {
			public ClockValue(int bitWidth) : base(bitWidth) {}

			public override bool Flip() {
				DateTime now = DateTime.Now;
				int value = Constant.Normalize(
					(ToDecimalBinary(now.Hour) << 24) +
					(ToDecimalBinary(now.Minute) << 16) +
					(ToDecimalBinary(now.Second) << 8) +
					ToDecimalBinary(now.Millisecond / 10),
					this.BitWidth
				);
				if(this.Value != value) {
					this.Value = value;
					return true;
				}
				return false;
			}

			private static int ToDecimalBinary(int value) {
				Debug.Assert(0 <= value && value < 100);
				int dec = (((value / 10) % 10) << 4) + (value % 10);
				return dec;
			}
		}
	}
}
