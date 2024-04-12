using LogicCircuit.UnitTest.HDL;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class HdlTest {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestParser() {
			string file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Merge3.hdl";
			//file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Inverter.hdl";
			file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\MergeSplit1_2.hdl";

			string folder = Path.GetDirectoryName(file);
			string name = Path.GetFileNameWithoutExtension(file);

			HdlContext hdl = new HdlContext(folder, message => this.TestContext.WriteLine(message));
			HdlChip chip = hdl.Chip(name);
			string text = chip.ToString();
		}
	}
}
