using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogMemory.xaml
	/// </summary>
	public partial class DialogMemory : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private FunctionMemory functionMemory;
		private string format;
		private string rowNumberFormat;
		public RowList Rows { get; private set; }

		public DialogMemory(FunctionMemory functionMemory) {
			this.functionMemory = functionMemory;
			this.format = string.Format(CultureInfo.InvariantCulture, "{{0:X{0}}}",
				this.functionMemory.DataBitWidth / 4 + (((this.functionMemory.DataBitWidth % 4) == 0) ? 0 : 1)
			);
			this.rowNumberFormat = string.Format(CultureInfo.InvariantCulture, "{{0:X{0}}}",
				string.Format(CultureInfo.InvariantCulture, "{0:X}", (this.TotalCells - 1) / 16).Length
			);
			this.Rows = new RowList(this);
			this.DataContext = this;
			this.InitializeComponent();
		}

		public int AddressBitWidth { get { return this.functionMemory.AddressBitWidth; } }
		public int DataBitWidth { get { return this.functionMemory.DataBitWidth; } }

		private int TotalCells { get { return Memory.NumberCellsFor(this.functionMemory.AddressBitWidth); } }

		public class Row {

			private DialogMemory dialogMemory;
			private int rowIndex;
			private string[] text;
			private string rowNumber;

			public Row(DialogMemory dialogMemory, int rowIndex) {
				this.dialogMemory = dialogMemory;
				this.rowIndex = rowIndex;
			}

			public string RowNumber {
				get {
					if(this.rowNumber == null) {
						this.rowNumber = string.Format(CultureInfo.InvariantCulture, this.dialogMemory.rowNumberFormat, this.rowIndex);
					}
					return this.rowNumber;
				}
			}

			public string this[int index] {
				get {
					Tracer.Assert(0 <= index && index < 16);
					if(this.text == null) {
						this.text = new string[16];
					}
					if(this.text[index] == null) {
						int address = index + this.rowIndex * 16;
						if(address < this.dialogMemory.TotalCells) {
							this.text[index] = string.Format(CultureInfo.InvariantCulture, this.dialogMemory.format,
								this.dialogMemory.functionMemory[address]
							);
						}
					}
					return this.text[index];
				}
			}
		}

		public class RowList : IEnumerable {

			private Row[] list;

			public RowList(DialogMemory dialogMemory) {
				this.list = new Row[dialogMemory.TotalCells / 16 + (((dialogMemory.TotalCells % 16) == 0) ? 0 : 1)];
				for(int i = 0; i < this.list.Length; i++) {
					this.list[i] = new Row(dialogMemory, i);
				}
			}

			public IEnumerator GetEnumerator() {
				return this.list.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
			}
		}
	}
}
