using System;
using System.IO;
using CommandLineParser;

namespace ItemWrapper.Generator {
	public class Program {
		// Usage: ItemWrapper.Generator /Schema:<file.xaml> /Target:<Destination folder> [/UseDispatcher:<true|false>] [/RealmType:<None|Universe|Multiverse>]
		// Debug: /Schema:E:\Projects\LogicCircuit\SnapVersion\Tests\Wrapper.Generator\Test.xaml /Target:E:\Projects\LogicCircuit\SnapVersion\Tests\Wrapper.Generator\Wrappers
		public static int Main(string[] args) {
			int returnCode = 0;
			try {
				bool printHelp = false;
				Generator generator = new Generator();
				generator.RealmType = RealmType.Universe;
				CommandLine commandLine = new CommandLine()
					.AddString("Schema", "s", "<filePath>", "Path to database schema definition file", true, value => {
						if(generator.SchemaPath != null) {
							throw new Usage(TextMessage.ArgumentRedefinition("Schema"));
						}
						generator.SchemaPath = value;
					})
					.AddString("Target", "t", "<dir>", "Path to destination folder", true, value => {
						if(generator.TargetFolder != null) {
							throw new Usage(TextMessage.ArgumentRedefinition("Target"));
						}
						generator.TargetFolder = value;
					})
					.AddFlag("UseDispatcher", "d", "Set this flag to send notifications via dispatcher", false, value => generator.UseDispatcher = value)
					.AddFlag("Multiverse", "m", "Set this flag to generate Multiverse realm", false, value => generator.RealmType = value ? RealmType.Multiverse : RealmType.Universe)
					.AddFlag("Help", "?", "Print help", false, value => printHelp = true)
				;
				string errors = commandLine.Parse(args, null);
				if(printHelp) {
					Console.Out.WriteLine(TextMessage.Usage);
					Console.Out.WriteLine(commandLine.Help());
				} else if(errors != null) {
					Console.Error.WriteLine(errors);
				} else {
					generator.Generate();
					Console.Out.WriteLine(TextMessage.ReportSuccess);
				}
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
