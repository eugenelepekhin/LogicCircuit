using System.Reflection;
using System.Windows;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class TestHelper {
		private static object syncRoot = new object();
		public static Assembly LogicCircuitAssembly { get; private set; }
		public static App App { get; private set; }

		public static Random Random { get; private set; }

		[AssemblyInitialize]
		public static void InitTests(TestContext context) {
			int seed = Environment.TickCount & 0x7FFFFFFF; // Ensure a positive seed value
			context.WriteLine($"seed={seed}");
			TestHelper.Random = new Random(seed);
			if(TestHelper.App == null) {
				lock(TestHelper.syncRoot) {
					if(TestHelper.App == null) {
						Assembly assembly = typeof(CircuitMap).Assembly;
						TestHelper.LogicCircuitAssembly = assembly;
						var _resourceAssemblyField = typeof(Application).GetField("_resourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
						if (_resourceAssemblyField != null) {
							_resourceAssemblyField.SetValue(null, assembly);
						}

						var resourceAssemblyProperty = typeof(Application).GetProperty("ResourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
						if (resourceAssemblyProperty != null) {
							resourceAssemblyProperty.SetValue(null, assembly);
						}

						App app = new App();
						app.InitializeComponent();
						TestHelper.App = app;
					}
				}
			}
		}
	}
}
