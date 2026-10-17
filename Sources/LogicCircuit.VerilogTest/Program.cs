// Ignore Spelling: Verilog Verilator

using System.Diagnostics;
using System.Text.RegularExpressions;
using CommandLineParser;

namespace LogicCircuit.VerilogTest {
	internal class Program {
		private struct Arguments {
			public string TargetFolder;
			public string ProjectFile;
			public string VerilatorPath;
			public string? CompilerInitPath;
			public List<Regex>? Circuit;
			public List<Regex>? Category;

			private class UniqueList {
				private List<string> list = new();
				private HashSet<string> set = new(StringComparer.OrdinalIgnoreCase);
				public int Count => this.list.Count;
				public void Add(string value) {
					if(set.Add(value)) {
						list.Add(value);
					}
				}
				public void Add(IEnumerable<string> values) {
					foreach(string s in values) {
						this.Add(s);
					}
				}
				public List<string> ToList() => this.list;
			}

			public bool Parse(string[] args) {
				bool empty(string? text) => string.IsNullOrEmpty(text);
				IEnumerable<string> split(string text) =>text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				string? targetFolder = null;
				string? projectFile = null;
				string? verilator = null;
				string? vcvars = null;
				bool showHelp = false;
				UniqueList circuit = new();
				UniqueList category = new();

				CommandLine commandLine = new CommandLine()
					.AddString("root", "r", "Root folder", "Folder where sub folders for each export will reside", true, v => targetFolder = v.Trim())
					.AddString("project", "p", "Circuit Project", "Circuit project file", true, f => projectFile = f.Trim())
					.AddString("verilator", "v", "Verilator Root", "Verilator folder", true, v => verilator = v.Trim())
					.AddString("vcvars", null, "vcvars64.bat", "Path to vcvars64.bat", false, b => vcvars = b.Trim())

					.AddString("circuit", "c", "circuit", "Semicolon separated list of individual circuit names patterns", false, c => circuit.Add(split(c.Trim())))
					.AddString("category", "cat", "category", "Semicolon separated list of patterns for names of categories", false, c => category.Add(split(c.Trim())))

					.AddFlag("help", "?", "Show this help", false, h => showHelp = h)
				;
				string? errors = commandLine.Parse(args, null);
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

				RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;
				this.Circuit = circuit.ToList().Select(p => new Regex(p, options)).ToList();
				this.Category = category.ToList().Select(p => new Regex(p, options)).ToList();

				return true;
			}
		}

		[STAThread]
		static int Main(string[] args) {
			#if DEBUG && true
				if(args == null || args.Length == 0) {
					args = [
						"/r",		@"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl",
						"/p",		@"C:\Projects\LogicCircuit\LogicCircuit\master\Sources\LogicCircuit.UnitTest\Properties\VerilogTests.CircuitProject",
						"/v",		@"C:\Tools\verilator-5.052",
						"/vcvars",	@"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat",
						"/circuit",	"Wire Plug 1;Wire Plug 2",
						//"/category", "Switches"
					];
				}
			#endif

			Arguments arguments = new Arguments();
			if(arguments.Parse(args)) {
				ExportRunner runner = new ExportRunner(arguments.ProjectFile!, arguments.TargetFolder!, arguments.VerilatorPath!, arguments.CompilerInitPath);
				bool success = false;
				int count = 0;
				HashSet<LogicalCircuit> list = new();
				foreach(LogicalCircuit logicalCircuit in runner.CircuitProject.LogicalCircuitSet) {
					string name = logicalCircuit.Name;
					string category = logicalCircuit.Category;
					if(arguments.Circuit!.Any(r => r.IsMatch(name)) ||arguments.Category!.Any(r => r.IsMatch(category))) {
						list.Add(logicalCircuit);
					}
				}
				Program.LogMessage($"Found {list.Count} circuits to test");
				foreach(LogicalCircuit logicalCircuit in list) {
					success = runner.TestExport(logicalCircuit);
					if(!success) break;
					count++;
				}
				Program.LogMessage("");
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
