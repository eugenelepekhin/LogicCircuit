using LogicCircuit.DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class ValueListTest {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void ConstructorTest() {
			ValueList<int> list = new ValueList<int>();
			Assert.AreEqual<int>(0, list.Count);
		}

		/// <summary>
		/// A test of ValueList.Add
		/// </summary>
		[TestMethod]
		public void AddTest() {
			int count = 1000 * (1 << 10);
			ValueList<int> list = new ValueList<int>();
			int seed = (int)DateTime.UtcNow.Ticks;
			this.TestContext.WriteLine("seed={0}", seed);
			Random rand = new Random(seed);
			for(int i = 0; i < count; i++) {
				Assert.AreEqual<int>(i, list.Count);
				int n = rand.Next();
				list.Add(ref n);
				ValueList<int>.Address a = list.ItemAddress(i);
				Assert.AreEqual<int>(n, a.Page[a.Index]);
			}
			Assert.AreEqual<int>(count, list.Count);
			rand = new Random(seed); // reset random to the initial state
			for(int i = 0; i < list.Count; i++) {
				int n = rand.Next();
				ValueList<int>.Address a = list.ItemAddress(i);
				Assert.AreEqual<int>(n, a.Page[a.Index]);
			}
		}

		/// <summary>
		/// A test of ValueList.Shrink
		/// </summary>
		[TestMethod]
		public void ShrinkTest() {
			int count = 5 * (1 << 10);
			ValueList<int> list = new ValueList<int>();

			Assert.AreEqual<int>(0, list.Count);
			list.Shrink(0);
			Assert.AreEqual<int>(0, list.Count);
			list.Add(ref count);
			Assert.AreEqual<int>(1, list.Count);

			Assert.Throws<ArgumentOutOfRangeException>(() => list.Shrink(2));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.Shrink(100));

			list.Shrink(1);
			Assert.AreEqual<int>(1, list.Count);

			list.Shrink(0);
			Assert.AreEqual<int>(0, list.Count);

			for(int i = 0; i < count; i++) {
				for(int j = list.Count; j < count; j++) {
					int n = j + 1;
					list.Add(ref n);
				}
				list.Shrink(i);
				Assert.AreEqual<int>(i, list.Count);
				for(int j = 0; j < i; j++) {
					ValueList<int>.Address a = list.ItemAddress(j);
					Assert.AreEqual<int>(j + 1, a.Page[a.Index]);
				}
			}
		}

		/// <summary>
		/// A test of ValueList.ItemAddress
		/// </summary>
		[TestMethod]
		public void AddressTest() {
			ValueList<int> list = new ValueList<int>();
			Assert.AreEqual<int>(0, list.Count);

			ValueList<int>.Address a = default(ValueList<int>.Address);
			Assert.Throws<ArgumentOutOfRangeException>(() => a = list.ItemAddress(0));
			Assert.IsNull(a.Page);

			int n = 194;
			list.Add(ref n);
			Assert.AreEqual<int>(1, list.Count);

			Assert.Throws<ArgumentOutOfRangeException>(() => a = list.ItemAddress(-1));
			Assert.IsNull(a.Page);

			Assert.Throws<ArgumentOutOfRangeException>(() => a = list.ItemAddress(-10));
			Assert.IsNull(a.Page);

			Assert.Throws<ArgumentOutOfRangeException>(() => a = list.ItemAddress(1));
			Assert.IsNull(a.Page);

			Assert.Throws<ArgumentOutOfRangeException>(() => a = list.ItemAddress(156));
			Assert.IsNull(a.Page);

			a = list.ItemAddress(0);
			Assert.AreEqual<int>(n, a.Page[a.Index]);
		}
	}
}
