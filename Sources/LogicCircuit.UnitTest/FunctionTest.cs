using System;
using System.Collections.Generic;
using System.Diagnostics;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for all functions testing.
	///</summary>
	[TestClass()]
	public class FunctionTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// A test of Clock function
		/// </summary>
		[TestMethod()]
		public void FunctionClockTest() {
			FunctionSocket test = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "ClockTest"));
			test.Execute(() => {
				// Clock always starting from 1
				test.Verify(State.On1);
				test.Verify(State.On0);
				test.Verify(State.On1);
				test.Verify(State.On0);
				test.Verify(State.On1);
				test.Verify(State.On0);
			});
		}

		/// <summary>
		/// A test of Tri-state function
		/// </summary>
		[TestMethod()]
		public void FunctionTriStateTest() {
			FunctionSocket test1 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "TriStateTest"));
			test1.Execute(() => {
				test1.Verify(State.On0, 0, 1);
				test1.Verify(State.On1, 1, 1);

				test1.Verify(State.Off, 0, 0);
				test1.Verify(State.Off, 1, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "TriStateMultyTest"));
			test3.Execute(() => {
				test3.Verify(State.On0, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On1, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.Off, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of Not function
		/// </summary>
		[TestMethod()]
		public void FunctionNotTest() {
			FunctionSocket test = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "NotTest"));
			test.Execute(() => {
				test.Verify(State.On1, 0, 1);
				test.Verify(State.On0, 1, 1);

				test.Verify(State.On0, 0, 0);
				test.Verify(State.On0, 1, 0);
			});
		}

		/// <summary>
		/// A test of And function
		/// </summary>
		[TestMethod()]
		public void FunctionAndTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "And2Test"));
			test2.Execute(() => {
				test2.Verify(State.On0, 0, 1, 0, 1);
				test2.Verify(State.On0, 1, 1, 0, 1);
				test2.Verify(State.On0, 0, 1, 1, 1);
				test2.Verify(State.On1, 1, 1, 1, 1);

				test2.Verify(State.On1, 0, 0, 1, 1);
				test2.Verify(State.On1, 1, 1, 0, 0);
				test2.Verify(State.On1, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "And3Test"));
			test3.Execute(() => {
				test3.Verify(State.On0, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On1, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On1, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of NAnd function
		/// </summary>
		[TestMethod()]
		public void FunctionNAndTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "And2NotTest"));
			test2.Execute(() => {
				test2.Verify(State.On1, 0, 1, 0, 1);
				test2.Verify(State.On1, 1, 1, 0, 1);
				test2.Verify(State.On1, 0, 1, 1, 1);
				test2.Verify(State.On0, 1, 1, 1, 1);

				test2.Verify(State.On0, 0, 0, 1, 1);
				test2.Verify(State.On0, 1, 1, 0, 0);
				test2.Verify(State.On0, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "And3NotTest"));
			test3.Execute(() => {
				test3.Verify(State.On1, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On0, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On0, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of Or function
		/// </summary>
		[TestMethod()]
		public void FunctionOrTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Or2Test"));
			test2.Execute(() => {
				test2.Verify(State.On0, 0, 1, 0, 1);
				test2.Verify(State.On1, 1, 1, 0, 1);
				test2.Verify(State.On1, 0, 1, 1, 1);
				test2.Verify(State.On1, 1, 1, 1, 1);

				test2.Verify(State.On1, 0, 0, 1, 1);
				test2.Verify(State.On1, 1, 1, 0, 0);
				test2.Verify(State.On0, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Or3Test"));
			test3.Execute(() => {
				test3.Verify(State.On0, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On1, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On0, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of NOr function
		/// </summary>
		[TestMethod()]
		public void FunctionNOrTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Or2NotTest"));
			test2.Execute(() => {
				test2.Verify(State.On1, 0, 1, 0, 1);
				test2.Verify(State.On0, 1, 1, 0, 1);
				test2.Verify(State.On0, 0, 1, 1, 1);
				test2.Verify(State.On0, 1, 1, 1, 1);

				test2.Verify(State.On0, 0, 0, 1, 1);
				test2.Verify(State.On0, 1, 1, 0, 0);
				test2.Verify(State.On1, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Or3NotTest"));
			test3.Execute(() => {
				test3.Verify(State.On1, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On0, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On1, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of Xor function
		/// </summary>
		[TestMethod()]
		public void FunctionXorTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Xor2Test"));
			test2.Execute(() => {
				test2.Verify(State.On0, 0, 1, 0, 1);
				test2.Verify(State.On1, 1, 1, 0, 1);
				test2.Verify(State.On1, 0, 1, 1, 1);
				test2.Verify(State.On0, 1, 1, 1, 1);

				test2.Verify(State.On1, 0, 0, 1, 1);
				test2.Verify(State.On1, 1, 1, 0, 0);
				test2.Verify(State.On0, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Xor3Test"));
			test3.Execute(() => {
				test3.Verify(State.On0, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On0, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On0, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of XNor function
		/// </summary>
		[TestMethod()]
		public void FunctionXNorTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Xor2NotTest"));
			test2.Execute(() => {
				test2.Verify(State.On1, 0, 1, 0, 1);
				test2.Verify(State.On0, 1, 1, 0, 1);
				test2.Verify(State.On0, 0, 1, 1, 1);
				test2.Verify(State.On1, 1, 1, 1, 1);

				test2.Verify(State.On0, 0, 0, 1, 1);
				test2.Verify(State.On0, 1, 1, 0, 0);
				test2.Verify(State.On1, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Xor3NotTest"));
			test3.Execute(() => {
				test3.Verify(State.On1, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On1, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On1, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of Even function
		/// </summary>
		[TestMethod()]
		public void FunctionEvenTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Even2Test"));
			test2.Execute(() => {
				test2.Verify(State.On1, 0, 1, 0, 1);
				test2.Verify(State.On0, 1, 1, 0, 1);
				test2.Verify(State.On0, 0, 1, 1, 1);
				test2.Verify(State.On1, 1, 1, 1, 1);

				test2.Verify(State.On0, 0, 0, 1, 1);
				test2.Verify(State.On0, 1, 1, 0, 0);
				test2.Verify(State.On1, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Even3Test"));
			test3.Execute(() => {
				test3.Verify(State.On1, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On1, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On1, 0, 0, 0, 0, 0, 0);
			});
		}

		/// <summary>
		/// A test of Odd function
		/// </summary>
		[TestMethod()]
		public void FunctionOddTest() {
			FunctionSocket test2 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Odd2Test"));
			test2.Execute(() => {
				test2.Verify(State.On0, 0, 1, 0, 1);
				test2.Verify(State.On1, 1, 1, 0, 1);
				test2.Verify(State.On1, 0, 1, 1, 1);
				test2.Verify(State.On0, 1, 1, 1, 1);

				test2.Verify(State.On1, 0, 0, 1, 1);
				test2.Verify(State.On1, 1, 1, 0, 0);
				test2.Verify(State.On0, 0, 0, 0, 0);
			});

			FunctionSocket test3 = new FunctionSocket(new ProjectTester(this.TestContext, Properties.Resources.FunctionTest, "Odd3Test"));
			test3.Execute(() => {
				test3.Verify(State.On0, 0, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 1, 1, 0, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 1, 1, 0, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 1);
				test3.Verify(State.On1, 0, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 1, 1, 1);
				test3.Verify(State.On0, 0, 1, 1, 1, 1, 1);
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

				test3.Verify(State.On0, 0, 0, 1, 1, 1, 1);
				test3.Verify(State.On0, 1, 1, 0, 0, 1, 1);
				test3.Verify(State.On0, 1, 1, 1, 1, 0, 0);
				test3.Verify(State.On0, 0, 0, 0, 0, 0, 0);
			});
		}

		private class FunctionSocket {
			private ProjectTester Tester { get; set; }

			public FunctionSocket(ProjectTester tester) {
				this.Tester = tester;
				Assert.IsTrue(this.Tester.Input.Length <= 32);
			}

			private int this[int index] {
				get { return this.Tester.Input[index].Value; }
				set { this.Tester.Input[index].Value = value; }
			}

			private State Result {
				get { return this.Tester.Output[0][0]; }
			}

			public void Execute(Action test) {
				this.Tester.CircuitProject.InOmitTransaction(test);
			}

			public void Verify(State expected, params int[] args) {
				Assert.IsTrue(args != null && args.Length == this.Tester.Input.Length);
				for(int i = 0; i < args.Length; i++) {
					Assert.AreEqual(args[i], args[i] & 1);
					this[i] = args[i];
				}
				Assert.IsTrue(this.Tester.CircuitState.Evaluate(true));
				Assert.AreEqual(expected, this.Result);
			}
		}
	}
}
