// Ignore Spelling: Camelize

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;

namespace ItemWrapper.Generator {
	public abstract class Transformation {

		public const string PersistentStoreNameSpace = "LogicCircuit.DataPersistent";

		public string StoreNameSpace { get; } = Transformation.PersistentStoreNameSpace;

		public ToStringHelper ToStringHelper { get; } = new ToStringHelper();

		private ItemWrapperGenerator? generator;
		public ItemWrapperGenerator Generator {
			get => this.generator!;
			set => this.generator = value;
		}
		public CompilerErrorCollection Errors => this.Generator.Errors;
		public Store Store => this.Generator.Store;
		public Table Table => this.Generator.Table;
		public bool UseDispatcher => this.Generator.UseDispatcher;
		public RealmType RealmType => this.Generator.RealmType;

		private StringBuilder? generationEnvironmentField;

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

		public static string MakeString(string text) {
			StringBuilder stringBuilder = new StringBuilder();
			using StringWriter writer = new StringWriter(stringBuilder);
			using CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
			provider.GenerateCodeFromExpression(new CodePrimitiveExpression(text), writer, new CodeGeneratorOptions());
			return stringBuilder.ToString();
		}

		public static string Camelize(string text) {
			if(!string.IsNullOrEmpty(text)) {
				return string.Concat(char.ToLower(text[0], CultureInfo.InvariantCulture), text.Substring(1));
			}
			return text;
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
	}
}
