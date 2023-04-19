using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommandLineParser;

namespace ResourceWrapper.Generator {
	internal class Program {
		// parameters for debug:
		// /v /f "C:\Projects\ResourceWrapper.Generator\master\Sources\TestProject" /n TestProject /a ErrorResource.resx;InternalResource.resx;NoCodeResource.resx;PublicResource.resx;SubFolder\NameSpacedResource.resx;SubFolder\Resource.resx;SubFolder\Resource.ru.resx;SubFolder\Resource.uz-UZ-Cyrl.resx /r SubFolder\Resource.resx /g ResXFileCodeGenerator /cs Resource.Designer.cs /rn ""
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		private static int Main(string[] args) {
			int result = 1;
			try {
				Parser parser = new Parser();
				bool printHelp = false;
				
				CommandLine commandLine = new CommandLine()
					.AddString("ProjectFolder", "f", "<project folder>", "The root folder of the project", true, f => parser.ProjectFolder = f)
					.AddString("NameSpace", "n", "<project namespace>", "The default name space of the project", true, n => parser.NameSpace = n)
					.AddString("Generator", "g", "<generator>", "Custom tool", true, g => parser.Generator = g)
					.AddString("Resource", "r", "<.resx file>", "The .resx file", true, r => parser.ResxFile = r)
					.AddString("Output", "cs", "<output>", "Output file with generated code", false, cs => parser.Output = cs)
					.AddString("ResSpace", "rn", "<custom namespace>", "Custom Tool Namespace", false, rn => parser.WrapperNamespace = rn)
					.AddString("AllItems", "a", "<all items>", "All .resx items in the project", true, a => parser.AllItems = a)

					.AddFlag("Pseudo", null, "Generate pseudo localization", false, p => parser.Pseudo = p)
					.AddFlag("Optional", "o", "Optional parameter declaration. Set this flag during conversion existing project for the time of conversion", false, o => parser.OptionalParameterDeclaration = o)
					.AddFlag("FlowDirection", null, "Turns on generation of FlowDirection property", false, f => parser.FlowDirection = f)
					.AddFlag("Verbose", "v", "Verbose output", false, v => Parser.Verbose = v)
					.AddFlag("Help", "?", "Print help", false, h => printHelp = h)
				;
				string? errors = commandLine.Parse(args, null);

				if(printHelp) {
					Program.Usage(commandLine.Help());
				} else if(errors != null) {
					Program.BadArgument(errors, commandLine.Help());
				} else if(parser.Parse()) {
					Message.Write("Resource wrappers generation complete without errors");
					result = 0;
				}
			} catch(Exception exception) {
				Message.Error("Error executing ResourceWrapper.Generator: {0}", exception.Message);
			}
			Message.Flush();
			return result;
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
