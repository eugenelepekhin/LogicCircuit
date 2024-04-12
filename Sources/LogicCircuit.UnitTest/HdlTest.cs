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
			HdlContext hdl = new HdlContext(file, message => this.TestContext.WriteLine(message));
			hdl.Parse();
		}
	}
}
