// Ignore Spelling: Verilog Verilator

using System.Diagnostics;
using System.Text.RegularExpressions;
using LogicCircuit.UnitTest;

namespace LogicCircuit.VerilogTest {
	internal class ExportRunner {
		public string CircuitProjectFile { get; }
		public string ExportRootFolder { get; }
		public string VerilatorRoot { get; }
		public string? CompilerInitPath { get; }

		public CircuitProject CircuitProject;

		public ExportRunner(string circuitProjectFile, string exportRootFolder, string verilatorRoot, string? compilerInitPath) {
			this.CircuitProjectFile = circuitProjectFile;
			this.ExportRootFolder = exportRootFolder;
			this.VerilatorRoot = verilatorRoot;
			this.CompilerInitPath = compilerInitPath;
			ProjectTester.InitResources();
			this.CircuitProject = ProjectTester.LoadCircuitProject(circuitProjectFile, null);
			ExportRunner.EnsureFolder(exportRootFolder);
		}

		public bool TestExport(string circuitName) {
			Program.LogMessage("");
			Program.LogMessage($"=== Starting test of export of circuit {circuitName} ===");
			Program.LogMessage($"1). Exporting circuit {circuitName} with all sub circuits");
			bool success = this.ExportCircuit(circuitName, out string targetFolder, out string testBenchModuleName, out string testBenchFileName);
			if(success) {
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

		private bool ExportCircuit(string circuitName, out string targetFolder, out string testBenchModuleName, out string testBenchFileName) {
			VerilogExport export = new VerilogExport(exportTests: true, commentPoints: true, fixNames: true, logMessage: Program.LogMessage, logError: Program.LogError, logWarning: Program.LogWarning);
			if(!export.IsValid(circuitName)) {
				throw new ArgumentException($"Invalid circuit name: {circuitName}");
			}
			LogicalCircuit logicalCircuit = ProjectTester.SwitchTo(this.CircuitProject, circuitName);
			if(!CircuitTestSocket.IsTestable(logicalCircuit)) {
				throw new InvalidOperationException($"Circuit is not testable: {circuitName}");
			}
			string folder = Path.Combine(this.ExportRootFolder, circuitName);
			ExportRunner.EnsureFolder(folder);
			bool success = export.ExportCircuit(logicalCircuit, folder, false, false, i => true, () => {});
			targetFolder = folder;
			testBenchModuleName = export.TestBenchModuleName(circuitName);
			testBenchFileName = export.TestBenchFileName(circuitName);
			return success;
		}

		private bool Verilate(string targetFolder, string testBenchModuleName, string testBenchFileName) {
			string verilatorExe = Path.Combine(this.VerilatorRoot, "bin", "verilator.exe");
			string arguments = $"--cc --timing --trace -Mdir \"{targetFolder}\" --top {testBenchModuleName} \"{testBenchFileName}\"";
			Program.LogMessage($"{verilatorExe} {arguments}");
			ProcessStartInfo startInfo = new ProcessStartInfo(verilatorExe, arguments) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			startInfo.EnvironmentVariables["VERILATOR_ROOT"] = this.VerilatorRoot;
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
				Session = new Dictionary<string, object>() {
					{ "ModuleName", testBenchModuleName }
				}
			};
			string text = mainGenerator.TransformText();
			File.WriteAllText(mainFileName, text);
			return true;
		}

		private bool Compile(string targetFolder, string circuitName, out string textExe) {
			ResponseFile response = new ResponseFile() {
				Session = new Dictionary<string, object>() {
					{ "VerilatorRoot", this.VerilatorRoot }
				}
			};
			string responseFile = "build.rsp";
			File.WriteAllText(Path.Combine(targetFolder, responseFile), response.TransformText());

			string exeFile = "test.exe";
			string compileArgs = $"@{responseFile} *.cpp /Fe{exeFile} /link /STACK:{16 * 1024 * 1024}";

			string arguments = string.IsNullOrEmpty(this.CompilerInitPath) ? compileArgs : $"/c \"\"{this.CompilerInitPath}\" && cl.exe {compileArgs}\"";
			string command = string.IsNullOrEmpty(this.CompilerInitPath) ? "cl.exe" : "cmd.exe";

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
	}
}
