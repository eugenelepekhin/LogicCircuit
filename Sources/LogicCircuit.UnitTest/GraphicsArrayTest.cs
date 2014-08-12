using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogicCircuit;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// Test of GraphicsArray
	/// </summary>
	[TestClass]
	public class GraphicsArrayTest {

		private void TestRange(Func<int, int> check, int min, int max) {
			Assert.IsTrue(min <= max);
			for(int i = min - 20; i < max + 30; i++) {
				int value = check(i);
				Assert.IsTrue(min <= value && value <= max);
			}
		}

		[TestMethod]
		public void GraphicsArrayCheckBitsPerPixelTest() {
			Assert.AreEqual(1, GraphicsArray.CheckBitsPerPixel(-20));
			Assert.AreEqual(1, GraphicsArray.CheckBitsPerPixel(-2));
			Assert.AreEqual(1, GraphicsArray.CheckBitsPerPixel(0));
			Assert.AreEqual(1, GraphicsArray.CheckBitsPerPixel(1));
			Assert.AreEqual(2, GraphicsArray.CheckBitsPerPixel(2));
			Assert.AreEqual(3, GraphicsArray.CheckBitsPerPixel(3));
			Assert.AreEqual(4, GraphicsArray.CheckBitsPerPixel(4));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(5));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(6));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(7));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(8));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(9));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(10));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(100));
			Assert.AreEqual(8, GraphicsArray.CheckBitsPerPixel(1000));
		}

		[TestMethod]
		public void GraphicsArrayCheckWidthTest() {
			this.TestRange(GraphicsArray.CheckWidth, 1, GraphicsArray.MaxWidth);
		}

		[TestMethod]
		public void GraphicsArrayCheckHeightTest() {
			this.TestRange(GraphicsArray.CheckHeight, 1, GraphicsArray.MaxHeight);
		}
	}
}
