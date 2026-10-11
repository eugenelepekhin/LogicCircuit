using System.Text;

namespace LogicCircuit {
	public abstract class T4Transformation {
		public T4ToStringHelper ToStringHelper { get; }

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected StringBuilder GenerationEnvironment { get; }

		protected T4Transformation() {
			this.ToStringHelper = new T4ToStringHelper();
			this.GenerationEnvironment = new StringBuilder();
		}

		public abstract string TransformText();

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public void Write(string textToAppend) {
			if(!string.IsNullOrEmpty(textToAppend)) {
				this.GenerationEnvironment.Append(textToAppend);
			}
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public void WriteLine(string textToAppend) {
			this.Write(textToAppend);
			this.GenerationEnvironment.AppendLine();
		}

		/// <summary>
		/// Writes new line
		/// </summary>
		public void WriteLine() {
			this.GenerationEnvironment.AppendLine();
		}

		public string Format(string format, params object[] args) {
			return string.Format(this.ToStringHelper.FormatProvider, format, args);
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public void Write(string format, params object[] args) {
			this.Write(this.Format(format, args));
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public void WriteLine(string format, params object[] args) {
			this.WriteLine(this.Format(format, args));
		}
	}
}
