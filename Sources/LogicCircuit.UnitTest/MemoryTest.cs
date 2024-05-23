using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicCircuit.UnitTest {
	/// <summary>
	/// This is a test class for Memory and is intended
	/// to contain all MemoryTest Unit Tests
	///</summary>
	[TestClass()]
	public class MemoryTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		///A test for BytesPerCellFor
		///</summary>
		[TestMethod()]
		public void MemoryBytesPerCellForTest() {
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(1));
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(2));
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(4));
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(6));
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(7));
			Assert.AreEqual<int>(1, Memory.BytesPerCellFor(8));

			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 1));
			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 2));
			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 4));
			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 6));
			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 7));
			Assert.AreEqual<int>(2, Memory.BytesPerCellFor(8 + 8));

			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 1));
			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 2));
			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 4));
			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 6));
			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 7));
			Assert.AreEqual<int>(3, Memory.BytesPerCellFor(16 + 8));

			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 1));
			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 2));
			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 4));
			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 6));
			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 7));
			Assert.AreEqual<int>(4, Memory.BytesPerCellFor(24 + 8));
		}

		/// <summary>
		///A test for NumberCellsFor
		///</summary>
		[TestMethod()]
		public void MemoryNumberCellsForTest() {
			Assert.AreEqual<int>(2, Memory.NumberCellsFor(1));
			Assert.AreEqual<int>(4, Memory.NumberCellsFor(2));
			Assert.AreEqual<int>(8, Memory.NumberCellsFor(3));
			Assert.AreEqual<int>(16, Memory.NumberCellsFor(4));
			Assert.AreEqual<int>(32, Memory.NumberCellsFor(5));
			Assert.AreEqual<int>(64, Memory.NumberCellsFor(6));
			Assert.AreEqual<int>(128, Memory.NumberCellsFor(7));
			Assert.AreEqual<int>(256, Memory.NumberCellsFor(8));
			Assert.AreEqual<int>(512, Memory.NumberCellsFor(9));
			Assert.AreEqual<int>(1024, Memory.NumberCellsFor(10));
			Assert.AreEqual<int>(2048, Memory.NumberCellsFor(11));
			Assert.AreEqual<int>(4096, Memory.NumberCellsFor(12));
			Assert.AreEqual<int>(8192, Memory.NumberCellsFor(13));
			Assert.AreEqual<int>(16384, Memory.NumberCellsFor(14));
			Assert.AreEqual<int>(32768, Memory.NumberCellsFor(15));
			Assert.AreEqual<int>(65536, Memory.NumberCellsFor(16));
		}

		/// <summary>
		///A test for CellValue
		///</summary>
		[TestMethod()]
		public void MemoryCellValueTest() {
			byte[] data = new byte[512];
			for(int i = 0; i < data.Length; i++) {
				data[i] = (byte)i;
			}
			Assert.AreEqual<int>(0x020100, Memory.CellValue(data, 18, 0));
			Assert.AreEqual<int>(0x010403, Memory.CellValue(data, 18, 1));
			Assert.AreEqual<int>(0x0100FF, Memory.CellValue(data, 24, 85));
			Assert.AreEqual<int>(0x040302, Memory.CellValue(data, 24, 86));

			Assert.AreEqual<int>(0x01, Memory.CellValue(data, 8, 1));
			Assert.AreEqual<int>(0x02, Memory.CellValue(data, 8, 2));
			Assert.AreEqual<int>(0xFF, Memory.CellValue(data, 8, 255));
		}

		/// <summary>
		///A test for SetCellValue
		///</summary>
		[TestMethod()]
		public void MemorySetCellValueTest() {
			byte[] data = new byte[512];

			Memory.SetCellValue(data, 18, 0, 0xFFFFFF);
			Assert.AreEqual<byte>(0xFF, data[0]);
			Assert.AreEqual<byte>(0xFF, data[1]);
			Assert.AreEqual<byte>(0x03, data[2]);
			Assert.AreEqual<byte>(0x00, data[3]);

			Memory.SetCellValue(data, 15, 2, 0xFFFFFF);
			Assert.AreEqual<byte>(0xFF, data[4]);
			Assert.AreEqual<byte>(0x7F, data[5]);
			Assert.AreEqual<byte>(0x00, data[6]);
			Assert.AreEqual<byte>(0x00, data[7]);

			Memory.SetCellValue(data, 32, 2, -1);
			Assert.AreEqual<byte>(0xFF, data[8]);
			Assert.AreEqual<byte>(0xFF, data[9]);
			Assert.AreEqual<byte>(0xFF, data[10]);
			Assert.AreEqual<byte>(0xFF, data[11]);

			Memory.SetCellValue(data, 32, 3, 0x12345678);
			Assert.AreEqual<byte>(0x78, data[12]);
			Assert.AreEqual<byte>(0x56, data[13]);
			Assert.AreEqual<byte>(0x34, data[14]);
			Assert.AreEqual<byte>(0x12, data[15]);
		}

		[TestMethod()]
		public void LoadTextFileTest() {
			DialogMemoryEditor.TextNumberReader Reader(string text, DialogMemoryEditor.TextFileFormat format) => new DialogMemoryEditor.TextNumberReader(new StringReader(text), format);
			void Check(DialogMemoryEditor.TextNumberReader reader, params int[] toExpect) {
				foreach(int expected in toExpect) {
					long value = reader.Next();
					Assert.AreNotEqual(-1, value);
					int actual = (int)value;
					Assert.AreEqual(expected, actual);
				}
				Assert.AreEqual(-1, reader.Next());
			}

			var reader = Reader("", DialogMemoryEditor.TextFileFormat.Dec);
			long value = reader.Next();
			Assert.AreEqual(-1, value);
			value = reader.Next();
			Assert.AreEqual(-1, value);

			Check(Reader("1 2 3", DialogMemoryEditor.TextFileFormat.Dec), 1, 2, 3);
			Check(Reader(" 1,2; 3  ", DialogMemoryEditor.TextFileFormat.Dec), 1, 2, 3);
			Check(Reader(" \n\r123	456	789  \r\n", DialogMemoryEditor.TextFileFormat.Dec), 123, 456, 789);
			Check(Reader(" \n\r123	-456;	789  \r\n", DialogMemoryEditor.TextFileFormat.Dec), 123, -456, 789);
			Check(Reader(" \n\r123	,-  456	+ 789  \r\n", DialogMemoryEditor.TextFileFormat.Dec), 123, -456, 789);
			Check(Reader(" \n\r123	+456,;,;-789  \r\n", DialogMemoryEditor.TextFileFormat.Dec), 123, 456, - 789);

			Check(Reader(" \nabc	+456	;,f01 - eEF \r\n", DialogMemoryEditor.TextFileFormat.Hex), 0xabc, 0x456, 0xf01, -0xeef);
			Check(Reader(" \n101;;;,	1101	,1000100010001 \r\n", DialogMemoryEditor.TextFileFormat.Bin), 0b101, 0b1101, 0b1000100010001);
			Check(Reader(" \n+101	-1101	+1000100010001 \r\n", DialogMemoryEditor.TextFileFormat.Bin), 0b101, -0b1101, 0b1000100010001);

			Check(Reader(int.MaxValue.ToString(), DialogMemoryEditor.TextFileFormat.Dec), int.MaxValue);
			Check(Reader("7fFFFfff", DialogMemoryEditor.TextFileFormat.Hex), int.MaxValue);
			Check(Reader("1111111111111111111111111111111", DialogMemoryEditor.TextFileFormat.Bin), int.MaxValue);

			Assert.ThrowsException<CircuitException>(() => Check(Reader("2147483648", DialogMemoryEditor.TextFileFormat.Dec), 1, 2, 3));
			Assert.ThrowsException<CircuitException>(() => Check(Reader("80000000", DialogMemoryEditor.TextFileFormat.Hex), int.MaxValue));

			Assert.ThrowsException<CircuitException>(() => Check(Reader("1=0", DialogMemoryEditor.TextFileFormat.Dec), 1, int.MaxValue));
			Assert.ThrowsException<CircuitException>(() => Check(Reader("1=0", DialogMemoryEditor.TextFileFormat.Hex), 1, int.MaxValue));
			Assert.ThrowsException<CircuitException>(() => Check(Reader("1=0", DialogMemoryEditor.TextFileFormat.Bin), 1, int.MaxValue));
		}
	}
}
