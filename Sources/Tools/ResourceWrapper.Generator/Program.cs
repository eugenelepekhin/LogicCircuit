using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ResourceWrapper.Generator {
	internal class Program {

		//$(SolutionDir)Utility\ResourceWrapper.Generator.exe ProjectPath=$(ProjectPath) ProjectDir=$(ProjectDir)

		private const string ProjectPathName = "ProjectPath";
		private const string ProjectRootName = "ProjectDir";

		public static int Main(string[] args) {
			try {
				string projectPath;
				string projectRoot;
				if(Program.ParseArguments(args, out projectPath, out projectRoot)) {
					ProjectParser parser = new ProjectParser();
					if(parser.Generate(projectPath, projectRoot)) {
						Message.Write("Success");
						Message.Flush();
						return 0;
					}
				}
			} catch(Exception exception) {
				Message.Error("ReportException", exception.Message);
			}
			Message.Flush();
			return 1;
		}

		private static bool ParseArguments(string[] args, out string projectPath, out string projectRoot) {
			//parsing for the following parameter syntax:
			//ResourceWrapper.Generator.exe ProjectPath=$(ProjectPath) [ProjectDir=$(ProjectDir)]
			string path = projectPath = null;
			string root = projectRoot = null;
			if(args == null || !(1 <= args.Length && args.Length <= 2)) {
				return Program.BadArgument();
			}
			StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;
			for(int i = 0; i < args.Length; i++) {
				string arg = args[i].Trim();
				if(arg.StartsWith(Program.ProjectPathName, StringComparison.InvariantCultureIgnoreCase)) {
					if(path != null) {
						return Program.BadArgument();
					}
					path = Program.Value(arg);
					if(path == null) {
						return Program.BadArgument();
					}
				} else if(arg.StartsWith(Program.ProjectRootName, StringComparison.InvariantCultureIgnoreCase)) {
					if(root != null) {
						return Program.BadArgument();
					}
					root = Program.Value(arg);
					if(root == null) {
						return Program.BadArgument();
					}
				} else {
					return Program.BadArgument();
				}
			}
			if(path == null) {
				return Program.BadArgument();
			}
			if(!File.Exists(path)) {
				Message.Error("FileMissing", path);
				Message.Error("Usage");
				return false;
			}
			if(root == null) {
				root = Path.GetDirectoryName(path);
			}
			if(!Directory.Exists(root)) {
				Message.Error("DirMissing", path);
				Message.Error("Usage");
				return false;
			}
			projectPath = path;
			projectRoot = root;
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
			Message.Error("BadArgument");
			Message.Error("Usage");
			return false;
		}
	}
}
