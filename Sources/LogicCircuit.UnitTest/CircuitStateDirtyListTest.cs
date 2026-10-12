using System.Reflection;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class CircuitStateDirtyListTest {
		public TestContext TestContext { get; set; }

		private class DirtyList {
			private readonly object instance;
			private readonly MethodInfo addMethod;
			private readonly MethodInfo getMethod;

			public DirtyList(int seed) {
				Type type = typeof(CircuitState).GetNestedType("DirtyList", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.IsNotNull(type);
				object obj = Activator.CreateInstance(type, seed);
				Assert.IsNotNull(obj);
				this.instance = obj;
				
				MethodInfo add = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, [typeof(CircuitFunction)]);
				Assert.IsNotNull(add);
				this.addMethod = add;

				MethodInfo get = type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
				Assert.IsNotNull(get);
				this.getMethod = get;
			}

			public void Add(CircuitFunction function) {
				this.addMethod.Invoke(this.instance, new object[] { function });
			}

			public CircuitFunction Get() {
				return (CircuitFunction)this.getMethod.Invoke(this.instance, null);
			}
		}

		private class TestFunction : CircuitFunction {
			public override string ReportName => throw new NotImplementedException();

			public int Order { get; set; }

			public TestFunction(CircuitState state) : base(state, 0, 0) {
			}

			public override bool Evaluate() {
				throw new NotImplementedException();
			}
		}

		[TestMethod]
		public void DirtyListAddGetTest() {
			int seed = 12345;
			DirtyList dirtyList = new DirtyList(seed);

			CircuitState state = new CircuitState(3);
			TestFunction f1 = new TestFunction(state);
			TestFunction f2 = new TestFunction(state);

			CircuitFunction retrieved0 = dirtyList.Get();
			Assert.IsNull(retrieved0);

			dirtyList.Add(f1);
			dirtyList.Add(f2);

			CircuitFunction retrieved1 = dirtyList.Get();
			CircuitFunction retrieved2 = dirtyList.Get();
			CircuitFunction retrieved3 = dirtyList.Get();

			Assert.IsTrue(f1 == retrieved1 || f2 == retrieved1);
			Assert.IsTrue(f1 == retrieved2 || f2 == retrieved2);
			Assert.IsNull(retrieved3);
		}

		[TestMethod]
		public void DirtyListOrderTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			//seed = -142808611;
			this.TestContext.WriteLine($"Seed: {seed}");
			DirtyList dirtyList = new DirtyList(seed);
			CircuitState state = new CircuitState(3);

			int count = 100;
			for(int i = 0; i < count; i++) {
				TestFunction f = new TestFunction(state) { Order = i };
				dirtyList.Add(f);
			}

			List<TestFunction> retrievedFunctions = new List<TestFunction>();
			for(int i = 0; i < count; i++) {
				CircuitFunction retrieved = dirtyList.Get();
				Assert.IsNotNull(retrieved);
				retrievedFunctions.Add((TestFunction)retrieved);
			}

			Assert.IsNull(dirtyList.Get());
			Assert.IsNull(dirtyList.Get());
			Assert.IsNull(dirtyList.Get());

			int inversions = 0;
			for(int i = 0; i < count; i++) {
				for(int j = i + 1; j < count; j++) {
					if(retrievedFunctions[i].Order > retrievedFunctions[j].Order) {
						inversions++;
					}
				}
			}

			int maxInversions = count * (count - 1) / 2;
			this.TestContext.WriteLine($"Number of inversions: {inversions} which is {(double)inversions / maxInversions:P} of randomness");
			Assert.IsTrue(0 < inversions && inversions < maxInversions, "This can fail due to randomization. In such case, please run the test again.");

			retrievedFunctions.Sort((f1, f2) => f1.Order.CompareTo(f2.Order));
			for(int i = 0; i < count; i++) {
				Assert.AreEqual(i, retrievedFunctions[i].Order);
			}
		}

		[TestMethod]
		public void WaveTest() {
			int seed = (int)DateTime.UtcNow.Ticks;
			//seed = -142808611;
			this.TestContext.WriteLine($"Seed: {seed}");
			DirtyList dirtyList = new DirtyList(seed);
			CircuitState state = new CircuitState(3);

			List<TestFunction> retrievedFunctions = new List<TestFunction>();

			int wave0 = 11;
			int[] waves = { 5, 3, 9, 0 }; // keep 0 at the end to go through all functions.
			int order = 0;
			int count = wave0;

			for(int i = 0; i < count; i++) {
				TestFunction f = new TestFunction(state) { Order = order++ };
				dirtyList.Add(f);
			}

			for(int w = 0; w < waves.Length; w++) {
				int i = 0;
				for( ; i < count / 2; i++) {
					CircuitFunction retrieved = dirtyList.Get();
					Assert.IsNotNull(retrieved);
					retrievedFunctions.Add((TestFunction)retrieved);
				}
				for(int j = 0; j < waves[w]; j++) {
					TestFunction f = new TestFunction(state) { Order = order++ };
					dirtyList.Add(f);
				}
				for(; i < count; i++) {
					CircuitFunction retrieved = dirtyList.Get();
					Assert.IsNotNull(retrieved);
					retrievedFunctions.Add((TestFunction)retrieved);
				}
				count = waves[w];
			}

			Assert.IsNull(dirtyList.Get());

			Assert.AreEqual(wave0 + waves.Sum(), retrievedFunctions.Count);

			int max = wave0;
			int waveIndex = 0;
			for(int i = 0; i < retrievedFunctions.Count; i++) {
				if(max <= i) {
					max += waves[waveIndex++];
				}
				Assert.IsLessThanOrEqualTo(max, retrievedFunctions[i].Order);
			}
		}
	}
}
