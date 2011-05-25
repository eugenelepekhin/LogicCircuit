using System;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace LogicCircuit.UnitTest {
	/// <summary>
	///This is a test class for CircuitStateTest and is intended
	///to contain all CircuitStateTest Unit Tests
	///</summary>
	[TestClass()]
	public class CircuitTest {

		#region Additional test attributes
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		/// <summary>
		/// A test of Digital Clock circuit
		/// </summary>
		[TestMethod()]
		public void CircuitDigitalClockTest() {
			ClockSocket clock = new ClockSocket(new ProjectTester(this.TestContext, Properties.Resources.Digital_Clock, "Unit Test"));
			clock.Tester.CircuitProject.InTransaction(() => {
				clock.Start();
				clock.TestTick(60 * 60 * 24 * 2);
			});
		}

		private class ClockSocket {
			public ProjectTester Tester { get; private set; }
			private InputSocket clock;
			private InputSocket mPlus;
			private InputSocket hPlus;
			private InputSocket s0;
			private InputSocket clr;
			private OutputSocket h;
			private OutputSocket m;
			private OutputSocket s;

			public ClockSocket(ProjectTester tester) {
				this.Tester = tester;
				Assert.AreEqual(5, this.Tester.Input.Length);
				Assert.AreEqual(3, this.Tester.Output.Length);

				this.clock = new InputSocket(tester.Input[0]);
				this.mPlus = new InputSocket(tester.Input[1]);
				this.hPlus = new InputSocket(tester.Input[2]);
				this.s0 = new InputSocket(tester.Input[2]);
				this.clr = new InputSocket(tester.Input[3]);

				this.h = new OutputSocket(tester.Output[0]);
				this.m = new OutputSocket(tester.Output[1]);
				this.s = new OutputSocket(tester.Output[2]);
			}

			public int Clock {
				get { return this.clock.Value; }
				set { this.clock.Value = value; }
			}
			public int HPlus {
				get { return this.hPlus.Value; }
				set { this.hPlus.Value = value; }
			}
			public int MPlus {
				get { return this.mPlus.Value; }
				set { this.mPlus.Value = value; }
			}
			public int S0 {
				get { return this.s0.Value; }
				set { this.s0.Value = value; }
			}

			private static int Range(int value, int max) {
				Assert.IsTrue(0 <= value && value < max);
				return value;
			}

			public int H { get { return ClockSocket.Range(this.h.FromBinaryDecimal(), 24); } }
			public int M { get { return ClockSocket.Range(this.m.FromBinaryDecimal(), 60); } }
			public int S { get { return ClockSocket.Range(this.s.FromBinaryDecimal(), 60); } }

			public TimeSpan Time() {
				return new TimeSpan(this.H, this.M, this.S);
			}

			public void Evaluate() {
				Assert.IsTrue(this.Tester.CircuitState.Evaluate(true), "evaluation falied");
			}

			public void Start() {
				this.Clock = 0;
				this.Evaluate();

				Assert.AreEqual(0, this.H);
				Assert.AreEqual(0, this.M);
				Assert.AreEqual(0, this.S);
			}

			public TimeSpan Tick() {
				int h = this.H;
				int m = this.M;
				int s = this.S;
			
				this.Clock = 1;
				this.Evaluate();

				Assert.AreEqual(h, this.H);
				Assert.AreEqual(m, this.M);
				Assert.AreEqual(s, this.S);
			
				this.Clock = 0;
				this.Evaluate();

				return this.Time();
			}

			public void TestTick(int count) {
				TimeSpan time = this.Time();
				for(int i = 0; i < count; i++) {
					TimeSpan total = new TimeSpan(0, 0, i + 1) + time;
					TimeSpan expected = new TimeSpan(total.Hours, total.Minutes, total.Seconds);
					Assert.AreEqual(expected, this.Tick(), "wrong time");
				}
			}
		}
	}
}
