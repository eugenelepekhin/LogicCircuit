using System;
using System.IO;

namespace ItemWrapper.Generator {
	public class Program {
		// Usage: ItemWrapper.Generator /Schema:<file.xaml> /Target:<Destination folder> [/UseDispatcher:<true|false>] [/RealmType:<None|Universe|Multiverse>]
		// Debug: /Schema:E:\Projects\LogicCircuit\SnapVersion\Tests\Wrapper.Generator\Test.xaml /Target:E:\Projects\LogicCircuit\SnapVersion\Tests\Wrapper.Generator\Wrappers
		public static int Main(string[] args) {
			int returnCode = 0;
			try {
				string schemaPrefix = "/Schema:";
				string targetPrefix = "/Target:";
				string useDispatcherPrefix = "/UseDispatcher:";
				string realmTypePrefix = "/RealmType:";
				Generator generator = new Generator();
				foreach(string arg in args) {
					if(arg.StartsWith(schemaPrefix, StringComparison.OrdinalIgnoreCase)) {
						if(generator.SchemaPath != null) {
							throw new Usage(TextMessage.ArgumentRedefinition(schemaPrefix));
						}
						generator.SchemaPath = arg.Substring(schemaPrefix.Length);
					} else if(arg.StartsWith(targetPrefix, StringComparison.OrdinalIgnoreCase)) {
						if(generator.TargetFolder != null) {
							throw new Usage(TextMessage.ArgumentRedefinition(targetPrefix));
						}
						generator.TargetFolder = arg.Substring(targetPrefix.Length);
					} else if(arg.StartsWith(useDispatcherPrefix, StringComparison.OrdinalIgnoreCase)) {
						bool use;
						if(!bool.TryParse(arg.Substring(useDispatcherPrefix.Length), out use)) {
							throw new Usage("UseDispatcher should provide true or false value");
						}
						generator.UseDispatcher = use;
					} else if(arg.StartsWith(realmTypePrefix, StringComparison.OrdinalIgnoreCase)) {
						RealmType realmType;
						if(!Enum.TryParse<RealmType>(arg.Substring(realmTypePrefix.Length), out realmType)) {
							throw new Usage("RealmType should provide a valid value");
						}
						generator.RealmType = realmType;
					} else {
						throw new Usage(TextMessage.UnknownArgument(arg));
					}
				}
				if(generator.SchemaPath == null) {
					throw new Usage(TextMessage.ArgumentMissing(schemaPrefix));
				}
				if(generator.TargetFolder == null) {
					throw new Usage(TextMessage.ArgumentMissing(targetPrefix));
				}
				if(!File.Exists(generator.SchemaPath)) {
					throw new Usage(TextMessage.SchemaFileMissing(generator.SchemaPath));
				}
				if(!Directory.Exists(generator.TargetFolder)) {
					throw new Usage(TextMessage.TargetFolderMissing(generator.TargetFolder));
				}
				generator.Generate();
				Console.Out.WriteLine(TextMessage.ReportSuccess);
			} catch(Error error) {
				returnCode = 1;
				Console.Error.WriteLine(error.Message);
				if(error is Usage) {
					Console.Error.WriteLine(TextMessage.Usage);
				}
			} catch(Exception exception) {
				returnCode = 1;
				Console.Error.WriteLine(exception.ToString());
			}
			return returnCode;
		}
	}
}
