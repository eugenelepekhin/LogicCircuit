// Ignore Spelling: Verilog Verilator

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CommandLineParser;

namespace LogicCircuit.VerilogTest {
	internal class Program {
		private struct Arguments {
			public string TargetFolder;
			public string ProjectFile;
			public string VerilatorPath;
			public List<string>? VerilatorWarnings;
			public string? CompilerInitPath;
			public List<Regex>? Circuit;
			public List<Regex>? Category;
			public string? TestBench;

			private class UniqueList {
				private List<string> list = new();
				private HashSet<string> set = new(StringComparer.OrdinalIgnoreCase);
				public int Count => this.list.Count;
				public void Add(string value) {
					if(this.set.Add(value)) {
						this.list.Add(value);
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
				UniqueList warnings = new();
				string? vcvars = null;
				UniqueList circuit = new();
				UniqueList category = new();
				string? test = null;
				bool showHelp = false;

				CommandLine commandLine = new CommandLine()
					.AddString("root", "r", "Root folder", "Folder where sub folders for each export will reside", true, v => targetFolder = v.Trim())
					.AddString("project", "p", "Circuit Project", "Circuit project file", true, f => projectFile = f.Trim())
					.AddString("verilator", "v", "Verilator Root", "Verilator folder", true, v => verilator = v.Trim())
					.AddString("noWarnings", "nw", "Warnings to turn off", "Semicolon separated list of Verilator warnings to turn off", false, w => warnings.Add(split(w.Trim())))
					.AddString("vcvars", null, "vcvars64.bat", "Path to vcvars64.bat", false, b => vcvars = b.Trim())

					.AddString("circuit", "c", "circuit", "Semicolon separated list of individual circuit names patterns", false, c => circuit.Add(split(c.Trim())))
					.AddString("category", "cat", "category", "Semicolon separated list of patterns for names of categories", false, c => category.Add(split(c.Trim())))
					.AddString("test", null, "Manually written test bench",
						$"File with manually written test bench. Only one circuit must be specified for this option. Or {ExportRunner.TextNoteSource} for test bench coming from TextNote.",
						false,
						t => {
							if(test is not null) {
								Program.LogError($"Test bench \"{t}\" specified multiple times.");
							}
							test = t;
						}
					)

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
					Program.LogError($"Verilator executable not found in folder \"{verilator}\".");
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
				if(0 < warnings.Count) {
					this.VerilatorWarnings = warnings.ToList();
				}
				this.CompilerInitPath = empty(vcvars) ? null : vcvars;

				RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;
				this.Circuit = circuit.ToList().Select(p => new Regex(p, options)).ToList();
				this.Category = category.ToList().Select(p => new Regex(p, options)).ToList();

				if(!empty(test) && test != ExportRunner.TextNoteSource && (0 < this.Category.Count || 1 != this.Circuit.Count)) {
					Program.LogError($"When test bench file is provided, only one circuit name must be provided in Circuit parameter.");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				if(!empty(test) && test != ExportRunner.TextNoteSource && !File.Exists(test)) {
					Program.LogError($"Test bench file \"{test}\" not found.");
					Program.Usage(null, commandLine.Help());
					return false;
				}
				this.TestBench = empty(test) ? null : test;

				return true;
			}
		}

		private static readonly StringBuilder errors = new();
		private static int errorCount = 0;
		private static int warningCount = 0;

		[STAThread]
		static int Main(string[] args) {
			#if DEBUG && true
				if(args == null || args.Length == 0) {
					args = [
						"/r",		@"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl",
						"/p",		@"C:\Projects\LogicCircuit\LogicCircuit\master\Sources\LogicCircuit.UnitTest\Properties\VerilogTests.CircuitProject",
						"/v",		@"C:\Tools\verilator-5.052",
						"/vcvars",	@"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat",
						"/circuit",	"Wire Plug 2",
						//"/category", "Switches",
						//"/test",	@"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Wire_Plug_2_TestBench.sv",
						"/test",	@"@TextNote",
					];
				}
			#endif

			Arguments arguments = new Arguments();
			if(arguments.Parse(args)) {
				ExportRunner runner = new ExportRunner(arguments.ProjectFile!, arguments.TargetFolder!, arguments.VerilatorPath!, arguments.VerilatorWarnings, arguments.CompilerInitPath, arguments.TestBench);
				bool success = false;
				int count = 0;
				HashSet<LogicalCircuit> list = new();
				if(arguments.TestBench == null || arguments.TestBench == ExportRunner.TextNoteSource) {
					foreach(LogicalCircuit logicalCircuit in runner.CircuitProject.LogicalCircuitSet) {
						string name = logicalCircuit.Name;
						string category = logicalCircuit.Category;
						if(arguments.Circuit!.Any(r => r.IsMatch(name)) || arguments.Category!.Any(r => r.IsMatch(category))) {
							if(arguments.TestBench == ExportRunner.TextNoteSource && !runner.ContainsTextNoteTestBench(logicalCircuit)) {
								Program.LogError($"Circuit \"{name}\" does not have a TextNote with test bench.");
							} else {
								list.Add(logicalCircuit);
							}
						}
					}
				} else {
					Regex regexName = arguments.Circuit![0];
					foreach(LogicalCircuit logicalCircuit in runner.CircuitProject.LogicalCircuitSet) {
						string name = logicalCircuit.Name;
						Match match = regexName.Match(name);
						if(match.Success && match.Index == 0 && match.Length == name.Length) {
							list.Add(logicalCircuit);
							break;
						}
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
				Program.LogMessage($"Found {Program.errorCount} errors and {Program.warningCount} warnings.");
				//if((0 < Program.errorCount || 0 < Program.warningCount) && 1 < list.Count) {
				//	Program.LogMessage(Program.errors.ToString());
				//}
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
			string error = $"Error: {message}";
			Console.Error.WriteLine(error);
			Debug.WriteLine(error);
			Program.errors.AppendLine(error);
			Program.errorCount++;
		}

		public static void LogWarning(string message) {
			string warning = $"Warning: {message}";
			Console.Out.WriteLine(warning);
			Debug.WriteLine(warning);
			Program.errors.AppendLine(warning);
			Program.warningCount++;
		}
	}
}
