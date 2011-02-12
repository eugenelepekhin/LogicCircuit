using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	public class ProjectLoader {
		public static CircuitProject Load(TestContext testContext, string project) {
			string path = Path.Combine(testContext.TestRunDirectory, "project.xml");
			File.WriteAllText(path, project, Encoding.UTF8);
			return CircuitProject.Create(path);
		}


	}
}
