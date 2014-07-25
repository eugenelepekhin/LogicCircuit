using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace LogicCircuit {
	public class FunctionSensor : CircuitFunction, IFunctionClock, IFunctionVisual {

		public override string ReportName { get { return Properties.Resources.NameSensor; } }

		public bool Invalid { get; set; }

		public CircuitSymbol CircuitSymbol { get; private set; }

		public Sensor Sensor { get { return (Sensor)this.CircuitSymbol.Circuit; } }

		public int BitWidth { get { return this.Sensor.BitWidth; } }

		private readonly SensorValue sensorValue;

		public int Value { get { return this.sensorValue.Value; } }

		public FunctionSensor(CircuitState circuitState, CircuitSymbol symbol, int[] result) : base(circuitState, null, result) {
			this.CircuitSymbol = symbol;
			Tracer.Assert(this.BitWidth == result.Length);
			switch(this.Sensor.SensorType) {
			case SensorType.Series:
			case SensorType.Loop:
				this.sensorValue = new SeriesValue(this.Sensor.Data, this.Sensor.SensorType == SensorType.Loop, this.Sensor.BitWidth);
				break;
			case SensorType.Random:
				this.sensorValue = new RandomValue(10, 20, this.Sensor.BitWidth);
				break;
			case SensorType.Manual:
				this.sensorValue = new RandomValue(10, 20, this.Sensor.BitWidth);
				break;
			default:
				Tracer.Fail();
				this.sensorValue = null;
				break;
			}
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
			// do nothing
		}

		public void TurnOff() {
			TextBlock textBlock = this.CircuitSymbol.ProbeView as TextBlock;
			if(textBlock != null) {
				textBlock.Text = Sensor.UnknownValue;
			}
		}

		public void Redraw() {
			TextBlock textBlock = this.CircuitSymbol.ProbeView as TextBlock;
			if(textBlock != null) {
				textBlock.Text = this.Value.ToString("X", CultureInfo.InvariantCulture);
			}
		}

		private abstract class SensorValue {
			public int BitWidth { get; private set; }
			public int Value { get; protected set; }

			protected SensorValue(int bitWidth) {
				Tracer.Assert(0 < bitWidth && bitWidth <= 32);
				this.BitWidth = bitWidth;
			}

			public abstract bool Flip();
		}

		private class RandomValue : SensorValue {
			private Random random;
			private int maxValue;
			private int minTick;
			private int maxTick;
			private int flip;
			private int tick;
			
			public RandomValue(int minTick, int maxTick, int bitWidth) : base(bitWidth) {
				Tracer.Assert(0 < minTick && minTick < int.MaxValue / 2);
				Tracer.Assert(0 < maxTick && maxTick < int.MaxValue / 2);
				Tracer.Assert(minTick <= maxTick);
				this.random = new Random();
				this.maxValue = (bitWidth < 32) ? 1 << bitWidth : int.MaxValue;
				this.minTick = minTick;
				this.maxTick = maxTick;
				this.Reset();
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
				this.list = Sensor.ParseSeries(data, bitWidth);
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
	}
}
