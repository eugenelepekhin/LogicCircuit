using System;
using System.Collections.Generic;
using System.Linq;
using LogicCircuit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	[TestClass]
	public class SensorTest {
		public TestContext TestContext { get; set; }

		private bool AreEqual(IList<SensorPoint> list1, IList<SensorPoint> list2) {
			return list1.Zip(list2, (p1, p2) => p1 == p2).All(r => r);
		}

		[TestMethod]
		public void SensorParseSeriesTest() {
			IList<SensorPoint> expected = new List<SensorPoint>() {
				new SensorPoint(0x2, 0x5),
				new SensorPoint(0x4, 0xA),
				new SensorPoint(0x5, 0xE),
				new SensorPoint(0x7, 0x10),
			};
			IList<SensorPoint> actual;
			Assert.IsTrue(Sensor.TryParseSeries("2:5 4:A 5:E 7:10", 32, out actual));
			Assert.IsTrue(this.AreEqual(expected, actual));
		}

		[TestMethod]
		public void SensorParseSeriesEmptyTest() {
			IList<SensorPoint> actual;
			Assert.IsTrue(Sensor.TryParseSeries("", 32, out actual));
			Assert.IsTrue(actual != null && actual.Count == 0);
		}

		[TestMethod]
		public void SensorSaveSeriesTest() {
			IList<SensorPoint> expected = new List<SensorPoint>() {
				new SensorPoint(0x2, 0x5),
				new SensorPoint(0x4, 0xE),
				new SensorPoint(0x5, 0xA),
				new SensorPoint(0x7, 0x10),
				new SensorPoint(0x10, 0x4),
			};
			string text = Sensor.SaveSeries(expected);

			IList<SensorPoint> actual;
			Assert.IsTrue(Sensor.TryParseSeries(text, 32, out actual));
			Assert.IsTrue(this.AreEqual(expected, actual));
		}

		[TestMethod]
		public void SensorSaveSeriesEmptyTest() {
			IList<SensorPoint> expected = new List<SensorPoint>();
			string text = Sensor.SaveSeries(expected);

			Assert.IsTrue(text != null && text.Length == 0);
		}
	}
}
