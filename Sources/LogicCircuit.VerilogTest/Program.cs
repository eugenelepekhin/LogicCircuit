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
			public List<string>? Circuit;
			public List<string>? Category;

			public bool Parse(string[] args) {
				bool empty(string? text) => string.IsNullOrEmpty(text);
				string? targetFolder = null;
				string? projectFile = null;
				string? verilator = null;
				string? vcvars = null;
				bool showHelp = false;
				List<string> arguments = new List<string>();
				List<string> circuit = new();
				List<string> category = new();

				CommandLine commandLine = new CommandLine()
					.AddString("root", "r", "Root folder", "Folder where sub folders for each export will reside", true, v => targetFolder = v.Trim())
					.AddString("project", "p", "Circuit Project", "Circuit project file", true, f => projectFile = f.Trim())
					.AddString("verilator", "v", "Verilator Root", "Verilator folder", true, v => verilator = v.Trim())
					.AddString("vcvars", null, "vcvars64.bat", "Path to vcvars64.bat", false, b => vcvars = b.Trim())

					.AddString("circuit", "c", "circuit", "Export single circuit to test", false, c => circuit.Add(c.Trim()))
					.AddString("category", "cat", "category", "Export all circuits in category to test", false, c => category.Add(c.Trim()))

					.AddFlag("help", "?", "Show this help", false, h => showHelp = h)
				;
				string? errors = commandLine.Parse(args, a => arguments.AddRange(a));
				if(showHelp || !empty(errors)) {
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
				if(!empty(vcvars)) {
					if(!File.Exists(vcvars)) {
						Program.LogError($"File \"{vcvars}\" not found.");
						Program.Usage(null, commandLine.Help());
						return false;
					}
				} else {
					string? vscmdVer = Environment.GetEnvironmentVariable("VSCMD_VER");
					if(empty(vscmdVer) || !Version.TryParse(vscmdVer, out Version? version) || version < new Version(18, 11)) {
						Program.LogError($"This program must be run from Developer Command Prompt for VS or parameter 'vcvars' must be provided.");
						Program.Usage(null, commandLine.Help());
						return false;
					}
				}
				if(circuit.Count == 0 && category.Count == 0) {
					Program.LogError($"Specify either /circuit <circuit name> of /category <category name> or both.");
					Program.Usage(null, commandLine.Help());
					return false;
				}

				this.TargetFolder = targetFolder;
				this.ProjectFile = projectFile;
				this.VerilatorPath = verilator;
				this.CompilerInitPath = empty(vcvars) ? null : vcvars;

				this.Circuit = circuit;
				this.Category = category;

				return true;
			}
		}

		[STAThread]
		static int Main(string[] args) {
			#if DEBUG && !true
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
				int count = 0;
				if(0 < arguments.Circuit!.Count) {
					foreach(string circuit in arguments.Circuit) {
						success = runner.TestExport(circuit);
						if(!success) break;
						count++;
					}
				}
				if(0 < arguments.Category!.Count) {
					StringComparer comparer = StringComparer.OrdinalIgnoreCase;
					foreach(string category in arguments.Category) {
						List<string> names = runner.CircuitProject.LogicalCircuitSet.Where(c => comparer.Equals(c.Category, category)).Select(c => c.Name).ToList();
						if(0 < names.Count) {
							foreach(string name in names) {
								success = runner.TestExport(name);
								if(!success)  break;
								count++;
							}
						} else {
							Program.LogError($"No circuit of '{arguments.Category}' category found in file \"{arguments.ProjectFile}\"");
							success = false;
						}
					}
				}
				Program.LogMessage($"Tested {count} circuits.");
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
