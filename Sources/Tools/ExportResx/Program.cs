using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLineParser;

namespace ExportResx {
	internal class Program {
		private static void Main(string[] args) {
			// -c fa
			bool help = false;
			List<string> cultures = new List<string>();
			CommandLine commandLine = new CommandLine()
				.AddFlag("help", "?", "get help", false, h => help = h)
				.AddString("culture", "c", "<culture code>", "code of exported culture", true, c => cultures.Add(c))
			;
			string errors = commandLine.Parse(args, null);
			if(!string.IsNullOrEmpty(errors)) {
				Program.Log(errors);
			}
			if(!string.IsNullOrEmpty(errors) || help) {
				Program.Log("ExportResx parameters");
				Program.Log(commandLine.Help());
				return;
			}
			Program.Log("Exporting: {0}", cultures.Aggregate((text, code) => string.IsNullOrEmpty(text) ? code : text + ", " + code));
			string folder = Program.ResxFolder();
			string output = Path.Combine(folder, cultures.Aggregate((text, code) => string.IsNullOrEmpty(text) ? code : text + "_" + code) + ".xml");
			Exporter exporter = new Exporter(folder, "Resources", cultures);
			if(exporter.Export(output)) {
				Program.Log("Successfully exported to {0}", output);
			}
		}

		private static string ResxFolder() {
			string exe = Assembly.GetExecutingAssembly().Location;
			string path = exe;
			while(Path.GetFileName(path) != "Tools") {
				path = Path.GetDirectoryName(path);
			}
			path = Path.GetDirectoryName(path);
			return Path.Combine(path, "LogicCircuit", "Properties");
		}

		public static void Log(string text) {
			Console.WriteLine(text);
			Debug.WriteLine(text);
		}

		public static void Log(string format, params object[] args) {
			Program.Log(string.Format(format, args));
		}
	}
}
