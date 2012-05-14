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
				test3.Verify(State.On1, 1, 1, 1, 1, 1, 1);

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
				test3.Verify(State.On0, 1, 1, 1, 1, 1, 1);

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

		private void TestFunction(Func<CircuitState, int[], int, CircuitFunction> create, Func<CircuitState, int[], int, bool> isValid) {
			for(int count = 2; count <= 12; count++) {
				CircuitState state = new CircuitState(3);
				int result = state.ReserveState();
				int[] param = new int[count];
				for(int i = 0; i < param.Length; i++) {
					param[i] = state.ReserveState();
				}
				CircuitFunction func = create(state, param, result);
				bool canContinue = true;
				while(canContinue) {
					func.Evaluate();
					Assert.IsTrue(isValid(state, param, result));
					for(int i = param.Length - 1; canContinue = (0 <= i); i--) {
						switch(state[param[i]]) {
						case State.Off:
							state[param[i]] = State.On0;
							break;
						case State.On0:
							state[param[i]] = State.On1;
							break;
						case State.On1:
							state[param[i]] = State.Off;
							break;
						}
						if(state[param[i]] != State.Off) {
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// A test of And function
		/// </summary>
		[TestMethod()]
		public void FunctionAndFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionAnd(state, param, result),
				(state, param, result) => {
					bool value = true;
					foreach(int p in param) {
						value &= (state[p] != State.On0);
					}
					return value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}

		/// <summary>
		/// A test of AndNot function
		/// </summary>
		[TestMethod()]
		public void FunctionAndNotFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionAndNot(state, param, result),
				(state, param, result) => {
					bool value = true;
					foreach(int p in param) {
						value &= (state[p] != State.On0);
					}
					return !value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}

		/// <summary>
		/// A test of Or function
		/// </summary>
		[TestMethod()]
		public void FunctionOrFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionOr(state, param, result),
				(state, param, result) => {
					bool value = false;
					foreach(int p in param) {
						value |= (state[p] == State.On1);
					}
					return value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}

		/// <summary>
		/// A test of OrNot function
		/// </summary>
		[TestMethod()]
		public void FunctionOrNotFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionOrNot(state, param, result),
				(state, param, result) => {
					bool value = false;
					foreach(int p in param) {
						value |= (state[p] == State.On1);
					}
					return !value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}

		/// <summary>
		/// A test of Xor function
		/// </summary>
		[TestMethod()]
		public void FunctionXorFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionXor(state, param, result),
				(state, param, result) => {
					bool value = false;
					foreach(int p in param) {
						value ^= (state[p] == State.On1);
					}
					return value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}

		/// <summary>
		/// A test of XorNot function
		/// </summary>
		[TestMethod()]
		public void FunctionXorNotFullTest() {
			this.TestFunction(
				(state, param, result) => new FunctionXorNot(state, param, result),
				(state, param, result) => {
					bool value = false;
					foreach(int p in param) {
						value ^= (state[p] == State.On1);
					}
					return !value ? state[result] == State.On1 : state[result] == State.On0;
				}
			);
		}
	}
}
