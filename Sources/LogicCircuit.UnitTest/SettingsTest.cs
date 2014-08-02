using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for Settings and is intended
	///to contain all Settings Unit Tests
	///</summary>
	[TestClass()]
	public class SettingsTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		private class TestSettings : Settings {
			public void SaveSettings(string file) {
				this.Save(file);
			}
			public void LoadSettings(string file) {
				this.Load(file);
			}
		}

		/// <summary>
		/// A test for Load/Save settings.
		/// </summary>
		[TestMethod()]
		public void SettingsLoadSaveTest() {
			string dir = Path.Combine(this.TestContext.TestRunDirectory, this.TestContext.TestName + DateTime.UtcNow.Ticks, "Settings Test Sub Directory");
			string file = Path.Combine(dir, "Settings Test File.xml");

			string key = "hello";
			string value = "world !";

			TestSettings s1 = new TestSettings();
			Assert.IsTrue(!Directory.Exists(dir));
			s1.LoadSettings(file);
			s1[key] = value;
			s1.SaveSettings(file);
			Assert.IsTrue(File.Exists(file));

			TestSettings s2 = new TestSettings();
			s2.LoadSettings(file);
			Assert.AreEqual(value, s2[key]);

			File.Delete(file);
		}
	}
}
