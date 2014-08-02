using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Xaml;

namespace ItemWrapper.Generator {
	public class Generator {
		public string SchemaPath { get; set; }
		public string TargetFolder { get; set; }
		public bool UseDispatcher { get; set; }
		public RealmType RealmType { get; set; }

		public Store Store { get; private set; }
		public Table Table { get; private set; }

        private CompilerErrorCollection errorsField;

		/// <summary>
		/// The error collection for the generation process
		/// </summary>
		public CompilerErrorCollection Errors {
			get {
				if(this.errorsField == null) {
					this.errorsField = new CompilerErrorCollection();
				}
				return this.errorsField;
			}
		}

		public int Generate() {
			this.Store = XamlServices.Load(this.SchemaPath) as Store;
			this.Store.Validate();

			foreach(Table table in this.Store) {
				this.Table = table;
				GeneratorItem generatorItem = new GeneratorItem() {
					Generator = this
				};
				string text = generatorItem.TransformText();
				if(0 < this.Errors.Count) {
					foreach(CompilerError error in this.Errors) {
						Console.Error.WriteLine(error.ErrorText);
					}
					return 1;
				}
				this.Save(text, table.Name);
			}

			string storeText;
			if(this.RealmType != RealmType.None) {
				GeneratorRealm generatorRealm = new GeneratorRealm() {
					Generator = this
				};
				storeText = generatorRealm.TransformText();
			} else {
				GeneratorStore generatorStore = new GeneratorStore() {
					Generator = this
				};
				storeText = generatorStore.TransformText();
			}
			if(0 < this.Errors.Count) {
				foreach(CompilerError error in this.Errors) {
					Console.Error.WriteLine(error.ErrorText);
				}
				return 1;
			}
			this.Save(storeText, this.Store.Name);
			return 0;
		}

		private void Save(string text, string item) {
			string path = Path.Combine(this.TargetFolder, item + ".cs");
			if(!File.Exists(path) || File.ReadAllText(path, Encoding.UTF8) != text) {
				File.WriteAllText(path, text, Encoding.UTF8);
			}
		}
	}
}
