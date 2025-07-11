using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xaml;

namespace ItemWrapper.Generator {
	public class ItemWrapperGenerator {
		public string? SchemaPath { get; set; }
		public string? TargetFolder { get; set; }
		public bool UseDispatcher { get; set; }
		public RealmType RealmType { get; set; }

		private Store? store;
		public Store Store => this.store!;
		private Table? table;
		public Table Table => this.table!;

		private CompilerErrorCollection? errorsField;

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
			Debug.Assert(this.SchemaPath != null, "SchemaPath must be set before calling Generate");
			this.store = (Store)XamlServices.Load(this.SchemaPath);
			this.Store.Validate();

			foreach(Table table in this.Store) {
				this.table = table;
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
				this.Save(text, this.table.Name);
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
			Debug.Assert(this.TargetFolder != null, "TargetFolder must be set before calling Save");
			string path = Path.Combine(this.TargetFolder, item + ".cs");
			if(!File.Exists(path) || File.ReadAllText(path, Encoding.UTF8) != text) {
				string directory = Path.GetDirectoryName(path)!;
				if(!Directory.Exists(directory)) {
					Directory.CreateDirectory(directory);
				}
				File.WriteAllText(path, text, Encoding.UTF8);
			}
		}
	}
}
