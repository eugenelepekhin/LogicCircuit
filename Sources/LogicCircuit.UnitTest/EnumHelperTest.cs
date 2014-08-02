using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for SettingsEnumCache and is intended
	/// to contain all SettingsEnumCache Unit Tests
	/// </summary>
	[TestClass()]
	public class EnumHelperTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

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
		public void EnumHelperSingleParseTest() {
			Assert.AreEqual(Num1.Zero,  EnumHelper.Parse("Zero",  Num1.Zero));
			Assert.AreEqual(Num1.One,   EnumHelper.Parse("One",   Num1.Zero));
			Assert.AreEqual(Num1.Two,   EnumHelper.Parse("Two",   Num1.Zero));
			Assert.AreEqual(Num1.Three, EnumHelper.Parse("Three", Num1.Zero));

			Assert.AreEqual(Num1.Zero,  EnumHelper.Parse("ZERO",  Num1.Zero));
			Assert.AreEqual(Num1.One,   EnumHelper.Parse("one",   Num1.Zero));
			Assert.AreEqual(Num1.Two,   EnumHelper.Parse("tWO",   Num1.Zero));
			Assert.AreEqual(Num1.Three, EnumHelper.Parse("tHReE", Num1.Zero));

			Assert.AreEqual((Num1)100,  EnumHelper.Parse("hello", (Num1)100));
			Assert.AreEqual(Num1.Two,   EnumHelper.Parse("Two",   (Num1)100));
			Assert.AreEqual(Num1.One,   EnumHelper.Parse("world", Num1.One));

			Assert.AreEqual(Num1.Two,   EnumHelper.Parse("2",   Num1.Three));
			Assert.AreEqual(Num1.Three, EnumHelper.Parse("100", Num1.Three));
			Assert.AreEqual(Num1.Three, EnumHelper.Parse("7",   Num1.Three));
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
		public void EnumHelperFlagsParseTest() {
			Assert.AreEqual(Num2.Zero,  EnumHelper.Parse("Zero",  Num2.Zero));
			Assert.AreEqual(Num2.One,   EnumHelper.Parse("One",   Num2.Zero));
			Assert.AreEqual(Num2.Two,   EnumHelper.Parse("Two",   Num2.Zero));
			Assert.AreEqual(Num2.Three, EnumHelper.Parse("Three", Num2.Zero));

			Assert.AreEqual(Num2.One | Num2.Two, EnumHelper.Parse("one, two", Num2.Three));
			Assert.AreEqual(Num2.One | Num2.Two, EnumHelper.Parse("two, one", Num2.Three));
			Assert.AreEqual(Num2.One | Num2.Two, EnumHelper.Parse("3", Num2.Three));

			Assert.AreEqual(Num2.One | Num2.Two | Num2.Three, EnumHelper.Parse("7", Num2.Zero));
		}
	}
}
