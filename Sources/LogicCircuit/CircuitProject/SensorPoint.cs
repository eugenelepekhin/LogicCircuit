using System;

namespace LogicCircuit {
	public struct SensorPoint {
		public int Tick { get; private set; }
		public int Value { get; private set; }

		public SensorPoint(int tick, int value) : this() {
			this.Tick = tick;
			this.Value = value;
		}

		public static bool operator ==(SensorPoint point1, SensorPoint point2) {
			return point1.Tick == point2.Tick && point1.Value == point2.Value;
		}

		public static bool operator !=(SensorPoint point1, SensorPoint point2) {
			return point1.Tick != point2.Tick || point1.Value != point2.Value;
		}

		public override bool Equals(object obj) {
			if((obj == null) || !(obj is SensorPoint)) {
				return false;
			}
			SensorPoint point = (SensorPoint)obj;
			return this == point;
		}

		public override int GetHashCode() {
			return this.Tick ^ this.Value;
		}

		#if DEBUG
			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:X}:{1:X}", this.Tick, this.Value);
			}
		#endif
	}
}
