// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace STAExtensions.UnitTests {
	using System;
	using System.Threading;
	using Microsoft.TestFx.STAExtensions;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class STAThreadManagerTests {
		[TestMethod]
		public void STAThreadManagerMustCreateASTAThreadToExecuteFunction() {
			var staThreadManager = new STAThreadManager<int>();
			var random = new Random();
			var expectedRetValue = random.Next(1, 100);
			var apartmentState = ApartmentState.Unknown;
			var managedThreadIdOfFunc = 0;

			var actualRetValue = staThreadManager.Execute(
				() => {
					apartmentState = Thread.CurrentThread.GetApartmentState();
					managedThreadIdOfFunc = Thread.CurrentThread.ManagedThreadId;
					return expectedRetValue;
				});

			Assert.AreEqual(expectedRetValue, actualRetValue, "STAThreadManager must not mess with func return values");
			Assert.AreEqual(ApartmentState.STA, apartmentState, "STAThreadManager must use an STA thread to execute function");
			Assert.AreNotEqual(managedThreadIdOfFunc, Thread.CurrentThread.ManagedThreadId, "STAThreadManager must create a new thread");

			staThreadManager.Dispose();
		}

		[TestMethod]
		public void ExecuteMustReuseSameThreadForMultipleCalls() {
			var staThreadManager = new STAThreadManager<int>();
			var random = new Random();
			var apartmentState = ApartmentState.Unknown;
			var managedThreadIds = new int[5];

			for(int i = 0; i < 5; i++) {
				var expectedRetValue = random.Next(1, 100);
				var actualRetValue = staThreadManager.Execute(
					() => {
						managedThreadIds[i] = Thread.CurrentThread.ManagedThreadId;
						apartmentState = Thread.CurrentThread.GetApartmentState();
						return expectedRetValue;
					});

				Assert.AreEqual(expectedRetValue, actualRetValue, "STAThreadManager.Execute must not mess with func return values");
				Assert.AreEqual(ApartmentState.STA, apartmentState, "STAThreadManager.Execute must use an STA thread to execute function");

				if(i > 0) {
					Assert.AreEqual(managedThreadIds[i - 1], managedThreadIds[i], "Multiple execute calls must execute on same thread");
				}
			}

			staThreadManager.Dispose();
		}

		[TestMethod]
		public void ExecuteMustThrowExceptionIfFunctionThrowsException() {
			var staThreadManager = new STAThreadManager<int>();
			var expectedExMessage = "HelloWorld";

			Exception exThrown = null;
			try {
				var actualRetValue = staThreadManager.Execute(
					() => {
						throw new TestDummyException(expectedExMessage);
					});
			} catch(Exception ex) {
				exThrown = ex;
			}

			Assert.IsNotNull(exThrown, "Execute must not swallow function exceptions.");
			Assert.IsInstanceOfType(exThrown, typeof(TestDummyException), "Execute must not wrap or change exception types.");
			Assert.AreEqual(expectedExMessage, exThrown.Message, "Execute must not change exception message.");

			staThreadManager.Dispose();
		}

		[TestMethod]
		public void ExecuteMustReuseSameThreadEvenAfterAFunctionThatThrowsException() {
			var staThreadManager = new STAThreadManager<int>();
			var expectedExMessage = "HelloWorld";
			var managedThreadIdOfExceptionFunc = 0;
			var managedThreadIdOfLaterFunc = 0;

			Exception exThrown = null;
			try {
				staThreadManager.Execute(
					() => {
						managedThreadIdOfExceptionFunc = Thread.CurrentThread.ManagedThreadId;
						throw new TestDummyException(expectedExMessage);
					});
			} catch(Exception ex) {
				exThrown = ex;
			}

			Assert.IsNotNull(exThrown, "Execute must not swallow function exceptions.");
			Assert.IsInstanceOfType(exThrown, typeof(TestDummyException), "Execute must not wrap or change exception types.");
			Assert.AreEqual(expectedExMessage, exThrown.Message, "Execute must not change exception message.");

			var random = new Random();
			var expectedRetValue = random.Next(1, 100);

			var actualRetValue = staThreadManager.Execute(
				() => {
					managedThreadIdOfLaterFunc = Thread.CurrentThread.ManagedThreadId;
					return expectedRetValue;
				});

			Assert.AreEqual(managedThreadIdOfExceptionFunc, managedThreadIdOfLaterFunc,
				"STAThreadManager must not use same thread even after exception in a function.");

			staThreadManager.Dispose();
		}


		private class TestDummyException : Exception {
			public TestDummyException(string message) : base(message) {

			}
		}
	}
}
