using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLineParser;

namespace ResourceWrapper.Generator {
	internal class Program {
		// parameters for debug:
		// /p ..\..\..\..\TestProject\TestProject.csproj /pseudo TestProject.SubFolder.Resource /FlowDirection
		private static int Main(string[] args) {
			try {
				string projectPath = null;
				List<string> pseudo = new List<string>();
				bool optionalParameterDeclaration = false;
				bool flowDirection = false;
				bool printHelp = false;

				CommandLine commandLine = new CommandLine()
					.AddString("Project", "p", "<filePath>", "Path to project file (.csproj)", true, p => projectPath = p)
					.AddString("Pseudo", null, "<class name>", "Fully qualified name of resource wrapper class to pseudo localize", false, l => pseudo.Add(l))
					.AddFlag("Optional", "o", "Optional parameter declaration. Set this flag during conversion existing project for the time of conversion", false, o => optionalParameterDeclaration = o)
					.AddFlag("FlowDirection", null, "Turns on generation of FlowDirection property", false, f => flowDirection = f)
					.AddFlag("Help", "?", "Print help", false, h => printHelp = h)
				;
				string errors = commandLine.Parse(args, null);

				if(printHelp) {
					Program.Usage(commandLine.Help());
				} else if(errors != null) {
					Program.BadArgument(errors, commandLine.Help());
				} else {
					projectPath = Path.GetFullPath(projectPath);
					if(File.Exists(projectPath)) {
						ProjectParser parser = new ProjectParser();
						if(parser.Generate(projectPath, pseudo, !optionalParameterDeclaration, flowDirection)) {
							Message.Write("Resource wrappers generation complete without errors");
							Message.Flush();
							return 0;
						}
					} else {
						Message.Error("Project file not found: {0}", projectPath);
					}
				}
			} catch(Exception exception) {
				Message.Error("Error executing ResourceWrapper.Generator: {0}", exception.Message);
			}
			Message.Flush();
			return 1;
		}

		private static void BadArgument(string errors, string help) {
			Message.Error("{0}: {1}", Process.GetCurrentProcess().ProcessName, errors);
			Program.Usage(help);
		}

		private static void Usage(string help) {
			string name = Process.GetCurrentProcess().ProcessName;
			Message.Write("{0}: Generates strong typed wrappers for managed resources.", name);
			Message.Write("Usage: {0} Options", name);
			Message.Write("");
			Message.Write("Options:");
			Message.Write(help);
		}
	}
}
