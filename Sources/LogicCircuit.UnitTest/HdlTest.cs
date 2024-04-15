using LogicCircuit.UnitTest.HDL;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class HdlTest {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestParser() {
			string file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Merge3.hdl";
			file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\Inverter.hdl";
			//file = @"C:\Projects\LogicCircuit\LogicCircuit\Temp\hdl\MergeSplit1_2.hdl";

			string folder = Path.GetDirectoryName(file);
			string name = Path.GetFileNameWithoutExtension(file);

			HdlContext hdl = new HdlContext(folder, message => this.TestContext.WriteLine(message));
			HdlState state = hdl.Load(name);
			if(state != null) {
				string text = state.Chip.ToString();
				this.TestContext.WriteLine(text);

				state.Set(state.Chip.Pin("x"), 1);
				state.Chip.Evaluate(state);
				int q = state.Get(state.Chip.Pin("q"));
				Assert.AreEqual(0, q);

				state.Set(state.Chip.Pin("x"), 0);
				state.Chip.Evaluate(state);
				q = state.Get(state.Chip.Pin("q"));
				Assert.AreEqual(1, q);
			}
		}
	}
}
