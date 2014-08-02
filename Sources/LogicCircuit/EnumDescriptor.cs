using System;

namespace LogicCircuit {
	/// <summary>
	/// Allow to show to user elements of enum as friendly text. Can be used in any ComboBox that allow selection of enum values.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EnumDescriptor<T> where T:struct {
		public T Value { get; private set; }
		public string Text { get; private set; }

		public EnumDescriptor(T value, string text) {
			Tracer.Assert(!(value is Enum) || EnumHelper.IsValid(value));
			this.Value = value;
			this.Text = text;
		}

		public override string ToString() {
			return this.Text;
		}
	}
}
