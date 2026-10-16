// Ignore Spelling: Verilog Verilator

using System.Diagnostics;
using CommandLineParser;

namespace LogicCircuit.VerilogTest {
	internal class Program {
		private struct Arguments {
			public string TargetFolder;
			public string ProjectFile;
			public string VerilatorPath;
			public string? CompilerInitPath;
			public string? Circuit;
			public string? Category;

			public bool Parse(string[] args) {
				string? targetFolder = null;
				string? projectFile = null;
				string? verilator = null;
				string? vcvars = null;
				bool showHelp = false;
				List<string> arguments = new List<string>();
				string? circuit = null;
				string? category = null;

				CommandLine commandLine = new CommandLine()
					.AddString("root", "r", "Root folder", "Folder where sub folders for each export will reside", true, v => targetFolder = v.Trim())
					.AddString("project", "p", "Circuit Project", "Circuit project file", true, f => projectFile = f.Trim())
					.AddString("verilator", "v", "Verilator Root", "Verilator folder", true, v => verilator = v.Trim())
					.AddString("vcvars", null, "vcvars64.bat", "Path to vcvars64.bat", false, b => vcvars = b.Trim())

					.AddString("circuit", "c", "circuit", "Export single circuit to test", false, c => circuit = c.Trim())
					.AddString("category", "cat", "category", "Export all circuits in category to test", false, c => category = c.Trim())

					.AddFlag("help", "?", "Show this help", false, h => showHelp = h)
				;
				string? errors = commandLine.Parse(args, a => arguments.AddRange(a));
				if(showHelp || !string.IsNullOrEmpty(errors)) {
					Program.Usage(errors, commandLine.Help());
					return false;
				}

				if(!Directory.Exists(targetFolder)) {
					Program.LogError($"Root folder \"{targetFolder}\" does not exists.");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				if(!File.Exists(projectFile)) {
					Program.LogError($"File \"{projectFile}\" not found.");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				if(!Directory.Exists(verilator)) {
					Program.LogError($"Verilator folder \"{verilator}\" not found.");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				if(!File.Exists(Path.Combine(verilator, "bin", "verilator.exe"))) {
					Program.LogError($"Verilator executable not found in folder \"{projectFile}\".");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				if(!string.IsNullOrEmpty(vcvars)) {
					if(!File.Exists(vcvars)) {
						Program.LogError($"File \"{vcvars}\" not found.");
						Program.Usage(null, commandLine.Help());
						return false;
					}
				} else {
					string? vscmdVer = Environment.GetEnvironmentVariable("VSCMD_VER");
					if(string.IsNullOrEmpty(vscmdVer) || !Version.TryParse(vscmdVer, out Version? version) || version < new Version(18, 11)) {
						Program.LogError($"This program must be run from Developer Command Prompt for VS or parameter 'vcvars' must be provided.");
						Program.Usage(null, commandLine.Help());
						return false;
					}
				}
				if(circuit != null && category != null || circuit == null && category == null) {
					Program.LogError($"Specify either /circuit <circuit name> of /category <category name>, but not both.");
					Program.Usage(null, commandLine.Help());
					return false;
				}

				this.TargetFolder = targetFolder;
				this.ProjectFile = projectFile;
				this.VerilatorPath = verilator;
				this.CompilerInitPath = string.IsNullOrEmpty(vcvars) ? null : vcvars;

				this.Circuit = circuit;
				this.Category = category;

				return true;
			}
		}

		[STAThread]
		static int Main(string[] args) {
			#if DEBUG && true
				if(args == null || args.Length == 0) {
					string targetFolder = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl";
					string project = @"C:\Projects\CircuitProjects\hdlTest.CircuitProject";
					string verilatorPath = @"C:\Tools\verilator-5.052";
					string compilerInitPath = @"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat";

					args = [
						"/r",		targetFolder,
						"/p",		project,
						"/v",		verilatorPath,
						"/vcvars",	compilerInitPath,
						"/circuit",	"BigAnd5",
						//"/category", "Switches"
					];
				}
			#endif

			Arguments arguments = new Arguments();
			if(arguments.Parse(args)) {
				ExportRunner runner = new ExportRunner(arguments.ProjectFile!, arguments.TargetFolder!, arguments.VerilatorPath!, arguments.CompilerInitPath);
				bool success = false;
				if(arguments.Circuit != null) {
					success = runner.TestExport(arguments.Circuit);
				} else {
					Debug.Assert(!string.IsNullOrEmpty(arguments.Category));
					StringComparer comparer = StringComparer.OrdinalIgnoreCase;
					List<string> names = runner.CircuitProject.LogicalCircuitSet.Where(c => comparer.Equals(c.Category, arguments.Category)).Select(c => c.Name).ToList();
					if(0 < names.Count) {
						foreach(string name in names) {
							success = runner.TestExport(name);
							if(!success) {
								break;
							}
						}
					} else {
						Program.LogError($"No circuit of '{arguments.Category}' category found in file \"{arguments.ProjectFile}\"");
						success = false;
					}
				}
				return success ? 0 : 1;
			}
			return 2;
		}

		private static void Usage(string? errors, string argHelp) {
			if(!string.IsNullOrEmpty(errors)) {
				Console.Error.WriteLine(errors);
			}
			Console.Out.WriteLine($"Verilog generation tester. Uses verilator to build, compile and run circuit exports.");
			Console.Out.WriteLine($"{typeof(Program).Assembly.GetName().Name} <options>");
			Console.Out.WriteLine(argHelp);
		}

		public static void LogMessage(string message) {
			Console.Out.WriteLine(message);
			Debug.WriteLine(message);
		}

		public static void LogError(string message) {
			Console.Error.WriteLine($"Error: {message}");
			Debug.WriteLine($"Error: {message}");
		}

		public static void LogWarning(string message) {
			Console.Out.WriteLine($"Warning: {message}");
			Debug.WriteLine($"Warning: {message}");
		}
	}
}
