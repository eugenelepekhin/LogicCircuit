using System;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for SettingsEnumCache and is intended
	/// to contain all SettingsEnumCache Unit Tests
	/// </summary>
	[TestClass()]
	public class SettingsEnumCacheTest {

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

		private enum Num1 {
			Zero,
			One,
			Two,
			Three,
		}

		/// <summary>
		/// Check if simple enums are working.
		/// </summary>
		[TestMethod()]
		public void SettingsEnumCacheSingleParseTest() {
			Assert.AreEqual(Num1.Zero, SettingsEnumCache<Num1>.Parse("Zero", Num1.Zero));
			Assert.AreEqual(Num1.One, SettingsEnumCache<Num1>.Parse("One", Num1.Zero));
			Assert.AreEqual(Num1.Two, SettingsEnumCache<Num1>.Parse("Two", Num1.Zero));
			Assert.AreEqual(Num1.Three, SettingsEnumCache<Num1>.Parse("Three", Num1.Zero));

			Assert.AreEqual(Num1.Zero, SettingsEnumCache<Num1>.Parse("ZERO", Num1.Zero));
			Assert.AreEqual(Num1.One, SettingsEnumCache<Num1>.Parse("one", Num1.Zero));
			Assert.AreEqual(Num1.Two, SettingsEnumCache<Num1>.Parse("tWO", Num1.Zero));
			Assert.AreEqual(Num1.Three, SettingsEnumCache<Num1>.Parse("tHReE", Num1.Zero));

			Assert.AreEqual((Num1)100, SettingsEnumCache<Num1>.Parse("hello", (Num1)100));
			Assert.AreEqual(Num1.Two, SettingsEnumCache<Num1>.Parse("Two", (Num1)100));
			Assert.AreEqual(Num1.One, SettingsEnumCache<Num1>.Parse("world", Num1.One));

			Assert.AreEqual(Num1.Two, SettingsEnumCache<Num1>.Parse("2", Num1.Three));
			Assert.AreEqual(Num1.Three, SettingsEnumCache<Num1>.Parse("100", Num1.Three));
			Assert.AreEqual(Num1.Three, SettingsEnumCache<Num1>.Parse("7", Num1.Three));
		}

		[Flags]
		private enum Num2 {
			Zero =  0,
			One  =  1,
			Two  =  2,
			Three = 4,
		}

		/// <summary>
		/// Check if Flags enums are working.
		/// </summary>
		[TestMethod()]
		public void SettingsEnumCacheFlagsParseTest() {
			Assert.AreEqual(Num2.Zero, SettingsEnumCache<Num2>.Parse("Zero", Num2.Zero));
			Assert.AreEqual(Num2.One, SettingsEnumCache<Num2>.Parse("One", Num2.Zero));
			Assert.AreEqual(Num2.Two, SettingsEnumCache<Num2>.Parse("Two", Num2.Zero));
			Assert.AreEqual(Num2.Three, SettingsEnumCache<Num2>.Parse("Three", Num2.Zero));

			Assert.AreEqual(Num2.One | Num2.Two, SettingsEnumCache<Num2>.Parse("one, two", Num2.Three));
			Assert.AreEqual(Num2.One | Num2.Two, SettingsEnumCache<Num2>.Parse("two, one", Num2.Three));
			Assert.AreEqual(Num2.One | Num2.Two, SettingsEnumCache<Num2>.Parse("3", Num2.Three));

			Assert.AreEqual(Num2.One | Num2.Two | Num2.Three, SettingsEnumCache<Num2>.Parse("7", Num2.Zero));
		}
	}
}
