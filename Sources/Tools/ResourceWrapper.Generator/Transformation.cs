using System;
using System.Globalization;
using System.Text;

namespace ResourceWrapper.Generator {
	public abstract class Transformation {
		private ToStringHelper toStringHelper = new ToStringHelper();
		public ToStringHelper ToStringHelper { get { return this.toStringHelper; } }

		private StringBuilder generationEnvironmentField;

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected StringBuilder GenerationEnvironment {
			get {
				if(this.generationEnvironmentField == null) {
					this.generationEnvironmentField = new StringBuilder();
				}
				return this.generationEnvironmentField;
			}
			set {
				this.generationEnvironmentField = value;
			}
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
		/// Write formatted text directly into the generated output
		/// </summary>
		public void Write(string format, params object[] args) {
			this.Write(string.Format(CultureInfo.InvariantCulture, format, args));
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public void WriteLine(string format, params object[] args) {
			this.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
		}
	}
}
