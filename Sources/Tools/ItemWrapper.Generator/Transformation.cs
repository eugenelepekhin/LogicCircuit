using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ItemWrapper.Generator {
	public abstract class Transformation {

		public const string PersistentStoreNameSpace = "LogicCircuit.DataPersistent";

		public string StoreNameSpace { get { return Transformation.PersistentStoreNameSpace; } }

		public Generator Generator { get; set; }
		public CompilerErrorCollection Errors { get { return this.Generator.Errors; } }
		public Store Store { get { return this.Generator.Store; } }
		public Table Table { get { return this.Generator.Table; } }
		public bool UseDispatcher { get { return this.Generator.UseDispatcher; } }
		public RealmType RealmType { get { return this.Generator.RealmType; } }

		private StringBuilder generationEnvironmentField;

		public string MakeString(string text) {
			// TODO: escape text
			return "\"" + text + "\"";
		}

		public string Camelize(string text) {
			if(!string.IsNullOrEmpty(text)) {
				return string.Concat(char.ToLower(text[0], CultureInfo.InvariantCulture), text.Substring(1));
			}
			return text;
		}

		public abstract string TransformText();

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

		/// <summary>
		/// Raise an error
		/// </summary>
		public void Error(string message) {
			CompilerError error = new CompilerError();
			error.ErrorText = message;
			this.Errors.Add(error);
		}

		/// <summary>
		/// Raise a warning
		/// </summary>
		public void Warning(string message) {
			CompilerError error = new CompilerError();
			error.ErrorText = message;
			error.IsWarning = true;
			this.Errors.Add(error);
		}

		#region ToString Helpers
		/// <summary>
		/// Utility class to produce culture-oriented representation of an object as a string.
		/// </summary>
		public class ToStringInstanceHelper {
			private System.IFormatProvider formatProviderField = global::System.Globalization.CultureInfo.InvariantCulture;
			/// <summary>
			/// Gets or sets format provider to be used by ToStringWithCulture method.
			/// </summary>
			public System.IFormatProvider FormatProvider {
				get { return this.formatProviderField; }
				set {
					if((value != null)) {
						this.formatProviderField = value;
					}
				}
			}
			/// <summary>
			/// This is called from the compile/run appdomain to convert objects within an expression block to a string
			/// </summary>
			public string ToStringWithCulture(object objectToConvert) {
				if((objectToConvert == null)) {
					throw new global::System.ArgumentNullException("objectToConvert");
				}
				System.Type t = objectToConvert.GetType();
				System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] { typeof(System.IFormatProvider) });
				if((method == null)) {
					return objectToConvert.ToString();
				} else {
					return ((string)(method.Invoke(objectToConvert, new object[] { this.formatProviderField })));
				}
			}
		}
		private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
		public ToStringInstanceHelper ToStringHelper { get { return this.toStringHelperField; } }
		#endregion
	}
}
