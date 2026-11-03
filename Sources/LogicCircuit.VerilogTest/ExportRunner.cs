// Ignore Spelling: Verilog Verilator

using System.Diagnostics;
using System.Text.RegularExpressions;
using LogicCircuit.UnitTest;

namespace LogicCircuit.VerilogTest {
	internal class ExportRunner {
		public const string TextNoteSource = "@TextNote";
		public const string TextNoteTestBenchHeader = "// Verilog Test Bench";
		private readonly string exportRootFolder;
		private readonly string verilatorRoot;
		private readonly string? compilerInitPath;
		private readonly string? testBenchFile;
		private readonly List<string>? verilatorWarnings;

		public CircuitProject CircuitProject;

		public ExportRunner(string circuitProjectFile, string exportRootFolder, string verilatorRoot, List<string>? verilatorWarnings, string? compilerInitPath, string? testBenchFile) {
			this.exportRootFolder = exportRootFolder;
			this.verilatorRoot = verilatorRoot;
			this.verilatorWarnings = verilatorWarnings;
			this.compilerInitPath = compilerInitPath;
			this.testBenchFile = testBenchFile;
			ProjectTester.InitResources();
			this.CircuitProject = ProjectTester.LoadCircuitProject(circuitProjectFile, null);
			ExportRunner.EnsureFolder(exportRootFolder);
		}

		public bool ContainsTextNoteTestBench(LogicalCircuit logicalCircuit) => logicalCircuit.TextNotes().Any(textNote => ExportRunner.GetTestBench(textNote) != null);

		public bool TestExport(LogicalCircuit logicalCircuit) {
			string circuitName = logicalCircuit.Name;
			Program.LogMessage("");
			Program.LogMessage($"=== Starting test of export of circuit {circuitName} ===");
			Program.LogMessage($"1). Exporting circuit {circuitName} with all sub circuits");
			bool success = this.ExportCircuit(logicalCircuit, out string? targetFolder, out string? testBenchModuleName, out string? testBenchFileName);
			if(success) {
				Debug.Assert(targetFolder != null);
				Debug.Assert(testBenchModuleName != null);
				Debug.Assert(testBenchFileName != null);
				Program.LogMessage($"2). Veriating test bench {testBenchFileName}");
				success = this.Verilate(targetFolder, testBenchModuleName, Path.Combine(targetFolder, testBenchFileName));
				if(success) {
					Program.LogMessage($"3). Compiling C++ code for test bench {testBenchFileName}");
					success = this.Compile(targetFolder, circuitName, out string textExe);
					if(success) {
						Program.LogMessage("");
						Program.LogMessage($">>> 4). Running {circuitName} <<<<");
						success = this.RunCircuit(textExe);
					}
				}
			}
			string result = success ? "passed" : "failed";
			Program.LogMessage($"Test of export of circuit {circuitName} have {result}");
			return success;
		}

		private bool ExportCircuit(LogicalCircuit logicalCircuit, out string? targetFolder, out string? testBenchModuleName, out string? testBenchFileName) {
			targetFolder = null;
			testBenchModuleName = null;
			testBenchFileName = null;
			string circuitName = logicalCircuit.Name;
			
			bool exportGeneratedTests = this.testBenchFile is null;

			VerilogExport export = new VerilogExport(exportTests: exportGeneratedTests, commentPoints: true, fixNames: true, logMessage: Program.LogMessage, logError: Program.LogError, logWarning: Program.LogWarning);
			if(!export.IsValid(circuitName)) {
				Program.LogError($"Invalid circuit name: {circuitName}");
				return false;
			}
			if(!CircuitTestSocket.IsTestable(logicalCircuit)) {
				Program.LogError($"Circuit is not testable: {circuitName}");
				return false;
			}
			string folder = Path.Combine(this.exportRootFolder, circuitName);
			ExportRunner.EnsureFolder(folder);
			bool success = export.ExportCircuit(logicalCircuit, folder, false, false, i => true, () => {});
			if(success) {
				targetFolder = folder;
				testBenchModuleName = export.TestBenchModuleName(circuitName);
				testBenchFileName = export.TestBenchFileName(circuitName);
				if(!exportGeneratedTests) {
					Debug.Assert(!string.IsNullOrEmpty(this.testBenchFile));
					string? testBenchPath = this.testBenchFile;
					if(this.testBenchFile == ExportRunner.TextNoteSource) {
						testBenchPath = Path.Combine(folder, testBenchFileName);
						string? testBench = logicalCircuit.TextNotes().Select(ExportRunner.GetTestBench).FirstOrDefault(t => t != null);
						Debug.Assert(testBench != null);
						File.WriteAllText(testBenchPath, testBench);
					} else {
						testBenchFileName = Path.GetFileName(this.testBenchFile);
						File.Copy(this.testBenchFile, Path.Combine(folder, testBenchFileName), true);
					}

					string testCode = File.ReadAllText(testBenchPath);
					Match match = Regex.Match(testCode, @"module\s+(?<name>[a-zA-Z_][a-zA-Z_0-9]*)\s*;", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
					if(match.Success) {
						string name = match.Groups["name"].Value;
						testBenchModuleName = name;
					} else {
						Program.LogError($"Test bench module not found in file {this.testBenchFile}");
						return false;
					}
				}
			}
			return success;
		}

		private bool Verilate(string targetFolder, string testBenchModuleName, string testBenchFileName) {
			string verilatorExe = Path.Combine(this.verilatorRoot, "bin", "verilator.exe");
			string warnings = this.verilatorWarnings != null ? string.Join(null, this.verilatorWarnings.Select(w => $" -Wno-{w}")) : string.Empty;
			string arguments = $"--cc --timing --trace -Mdir \"{targetFolder}\"{warnings} --top {testBenchModuleName} \"{testBenchFileName}\"";
			Program.LogMessage($"{verilatorExe} {arguments}");
			ProcessStartInfo startInfo = new ProcessStartInfo(verilatorExe, arguments) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			startInfo.EnvironmentVariables["VERILATOR_ROOT"] = this.verilatorRoot;
			Process process = new Process {
				StartInfo = startInfo
			};
			int errorCount = 0;
			process.ErrorDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					Program.LogError(e.Data);
					errorCount++;
				}
			};
			process.OutputDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					Program.LogMessage(e.Data);
				}
			};
			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit();
			if(process.ExitCode == 0 && errorCount == 0) {
				Program.LogMessage($"Verilator succeeded for circuit {testBenchModuleName}");
				return this.GenerateMain(targetFolder, testBenchModuleName);
			} else {
				Program.LogError($"Verilator failed for circuit {testBenchModuleName} with exit code {process.ExitCode} and {errorCount} errors");
				return false;
			}
		}

		private bool GenerateMain(string targetFolder, string testBenchModuleName) {
			string mainFileName = Path.Combine(targetFolder, "main.cpp");
			VerilatorMain mainGenerator = new VerilatorMain() {
				Session = new Dictionary<string, string>() {
					{ "ModuleName", testBenchModuleName }
				}
			};
			string text = mainGenerator.TransformText();
			File.WriteAllText(mainFileName, text);
			return true;
		}

		private bool Compile(string targetFolder, string circuitName, out string textExe) {
			ResponseFile response = new ResponseFile() {
				Session = new Dictionary<string, string>() {
					{ "VerilatorRoot", this.verilatorRoot }
				}
			};
			string responseFile = "build.rsp";
			File.WriteAllText(Path.Combine(targetFolder, responseFile), response.TransformText());

			string exeFile = "test.exe";
			string compileArgs = $"@{responseFile} *.cpp /Fe{exeFile} /link /STACK:{16 * 1024 * 1024}";

			string arguments = string.IsNullOrEmpty(this.compilerInitPath) ? compileArgs : $"/c \"\"{this.compilerInitPath}\" && cl.exe {compileArgs}\"";
			string command = string.IsNullOrEmpty(this.compilerInitPath) ? "cl.exe" : "cmd.exe";

			Program.LogMessage($"cl.exe {compileArgs}");

			ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments) {
				WorkingDirectory = targetFolder,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process process = new Process {
				StartInfo = startInfo
			};
			process.ErrorDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					Program.LogMessage(e.Data);
				}
			};
			process.OutputDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					Program.LogMessage(e.Data);
				}
			};
			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit();

			if(process.ExitCode == 0) {
				Program.LogMessage($"Compilation succeeded for circuit {circuitName}");
				textExe = Path.Combine(targetFolder, exeFile);
				return true;
			} else {
				Program.LogError($"Compilation failed for circuit {circuitName} with exit code {process.ExitCode}");
				textExe = string.Empty;
				return false;
			}
		}

		private bool RunCircuit(string exeFile) {
			Regex errorText = new Regex("%Error:", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
			ProcessStartInfo startInfo = new ProcessStartInfo(exeFile) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			Process process = new Process {
				StartInfo = startInfo
			};
			int errorCount = 0;
			process.ErrorDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					Program.LogError(e.Data);
					errorCount++;
				}
			};
			process.OutputDataReceived += (sender, e) => {
				if(!string.IsNullOrEmpty(e.Data)) {
					string message = e.Data;
					if(errorText.IsMatch(message)) {
						Program.LogError(message);
						errorCount++;
					} else {
						Program.LogMessage(e.Data);
					}
				}
			};
			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit();
			if(process.ExitCode == 0 && errorCount == 0) {
				Program.LogMessage($"Execution succeeded for {exeFile}");
				return true;
			} else {
				Program.LogError($"Execution failed for {exeFile} with exit code {process.ExitCode} and {errorCount} errors");
				return false;
			}
		}

		private static void EnsureFolder(string folder) {
			if(!Directory.Exists(folder)) {
				Directory.CreateDirectory(folder);
			}
		}

		private static string? GetTestBench(TextNote textNote) {
			string text = textNote.GetText();
			if(Regex.IsMatch(text, ExportRunner.TextNoteTestBenchHeader, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline)) {
				return text;
			}
			return null;
		}
	}
}
