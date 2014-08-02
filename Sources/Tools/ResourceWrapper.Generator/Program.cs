using System;
using System.Collections.Generic;
using System.IO;

namespace ResourceWrapper.Generator {
	internal class Program {
		// parameters for debug:
		// ProjectPath=C:\Eugene\Projects\ResourceWrapper.Generator\Main\Source\ResourceWrapper.Generator\TestProject\TestProject.csproj Pseudo=TestProject.SubFolder.Resource
		private static int Main(string[] args) {
			try {
				string projectPath;
				IEnumerable<string> pseudo;
				if(Program.ParseArguments(args, out projectPath, out pseudo)) {
					ProjectParser parser = new ProjectParser();
					if(parser.Generate(projectPath, pseudo)) {
						Message.Write("Resource wrappers generation complete without errors");
						Message.Flush();
						return 0;
					}
				}
			} catch(Exception exception) {
				Message.Error("Error executing ResourceWrapper.Generator: {0}", exception.Message);
			}
			Message.Flush();
			return 1;
		}

		private static bool ParseArguments(string[] args, out string projectPath, out IEnumerable<string> pseudo) {
			//parsing for the following parameter syntax:
			//ResourceWrapper.Generator.exe ProjectPath=$(ProjectPath) [Pseudo=ResourceName]*
			string path = projectPath = null;
			List<string> pseudoList = new List<string>();
			pseudo = pseudoList;
			if(args == null || args.Length < 1) {
				return Program.BadArgument();
			}
			for(int i = 0; i < args.Length; i++) {
				string arg = args[i].Trim();
				if(arg.StartsWith("ProjectPath", StringComparison.InvariantCultureIgnoreCase)) {
					if(path != null) {
						return Program.BadArgument();
					}
					path = Program.Value(arg);
					if(path == null) {
						return Program.BadArgument();
					}
				} else if(arg.StartsWith("Pseudo", StringComparison.InvariantCultureIgnoreCase)) {
					string res = Program.Value(arg);
					if(res == null) {
						return Program.BadArgument();
					}
					pseudoList.Add(res);
				} else {
					return Program.BadArgument();
				}
			}
			if(path == null) {
				return Program.BadArgument();
			}
			if(!File.Exists(path)) {
				Message.Error("File \"{0}\" does not exist", path);
				Program.Usage();
				return false;
			}
			projectPath = path;
			return true;
		}

		private static string Value(string arg) {
			int index = arg.IndexOf('=');
			if(index > 0) { //must be > 0: a=b
				return arg.Substring(index + 1).Trim();
			}
			return null;
		}

		private static bool BadArgument() {
			Message.Error("Bad arguments provided");
			Program.Usage();
			return false;
		}

		private static void Usage() {
			Message.Error("ResourceWrapper.Generator.exe ProjectPath=$(ProjectPath) [Pseudo=ResourceName]*");
		}
	}
}
