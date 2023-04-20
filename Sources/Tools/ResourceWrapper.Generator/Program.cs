using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommandLineParser;

namespace ResourceWrapper.Generator {
	internal static class Program {
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		static int Main(string[] args) {
			int result = 0;
			try {
				string? fileName = null;
				bool pseudo = false;
				bool optionalParameters = false;
				bool flowDirection = false;
				bool verbose = false;
				bool help = false;
				CommandLine commandLine = new CommandLine()
					.AddString("ProjectFile", "p", "<project file name>", "Project file to generate resources for", true, p => {
						if(fileName != null && fileName != p) {
							Program.Error("Command line parameter ProjectFile been already provided.");
							result = 1;
						} else if(!File.Exists(p)) {
							Program.Error($"Project file {p} not found");
							result = 1;
						}
						fileName = p;
					})
					.AddFlag("Pseudo", null, "Generate pseudo localization", false, p => pseudo = p)
					.AddFlag("Optional", "o", "Optional parameter declaration. Set this flag during conversion existing project for the time of conversion", false, o => optionalParameters = o)
					.AddFlag("FlowDirection", null, "Turns on generation of FlowDirection property", false, f => flowDirection = f)
					.AddFlag("Verbose", "v", "Verbose output", false, v => verbose = v)
					.AddFlag("Help", "?", "Print help", false, h => help = h)
				;
				string? errors = commandLine.Parse(args, null);
				if(errors != null) {
					Program.Error(errors);
					result = 1;
				}
				if(result == 1 || help) {
					Program.Usage(commandLine.Help());
				} else {
					ProjectParser parser = new ProjectParser(fileName!, pseudo, optionalParameters, flowDirection, verbose);
					ProjectParser.InitMsBuild();
					if(!parser.Parse()) {
						result = 1;
					}
				}
				if(result == 0) {
					Program.Message($"Resource wrappers generation complete without errors for project: {fileName}");
				}
			} catch(Exception exception) {
				Program.Error(exception.ToString());
				result = 1;
			}
			return result;
		}

		public static void Error(string message) {
			Console.Error.WriteLine(message);
			Debug.WriteLine(message);
		}

		public static void Warning(string message) {
			Console.Error.WriteLine(message);
			Debug.WriteLine(message);
		}

		public static void Message(string message) {
			Console.Out.WriteLine(message);
			Debug.WriteLine(message);
		}

		private static void Usage(string options) {
			string name = Process.GetCurrentProcess().ProcessName;
			Program.Message($"{name}: Generates strong typed wrappers for managed resources.");
			Program.Message($"Usage: {name} Options");
			Program.Message("");
			Program.Message("Options:");
			Program.Message(options);
		}
	}
}
