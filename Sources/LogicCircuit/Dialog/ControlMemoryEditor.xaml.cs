using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for ControlMemoryEditor.xaml
	/// </summary>
	public partial class ControlMemoryEditor : UserControl {
		public static readonly DependencyProperty FunctionMemoryProperty = DependencyProperty.Register("FunctionMemory", typeof(IFunctionMemory), typeof(ControlMemoryEditor));
		public IFunctionMemory FunctionMemory {
			get { return (IFunctionMemory)this.GetValue(ControlMemoryEditor.FunctionMemoryProperty); }
			set { this.SetValue(ControlMemoryEditor.FunctionMemoryProperty, value); }
		}

		public static readonly DependencyProperty IsReadOnlyProperty = DataGrid.IsReadOnlyProperty.AddOwner(typeof(ControlMemoryEditor));
		public bool IsReadOnly {
			get { return (bool)this.GetValue(ControlMemoryEditor.IsReadOnlyProperty); }
			set { this.SetValue(ControlMemoryEditor.IsReadOnlyProperty, value); }
		}

		public static readonly DependencyProperty DataDigitsProperty = DependencyProperty.Register("DataDigits", typeof(int), typeof(ControlMemoryEditor));
		public int DataDigits {
			get { return (int)this.GetValue(ControlMemoryEditor.DataDigitsProperty); }
			set { this.SetValue(ControlMemoryEditor.DataDigitsProperty, value); }
		}

		public ControlMemoryEditor() {
			this.InitializeComponent();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			try {
				IFunctionMemory functionMemory;
				if(e.Property == ControlMemoryEditor.FunctionMemoryProperty && (functionMemory = this.FunctionMemory) != null) {
					this.dataGrid.Columns.Clear();
					int addressBits = functionMemory.AddressBitWidth;
					int count = (4 <= addressBits) ? 16 : 1 << addressBits;
					this.DataDigits = ControlMemoryEditor.HexDigits(functionMemory.DataBitWidth);
					Style styleViewer = (Style)this.FindResource("cellViewer");
					Style styleEditor = (Style)this.FindResource("cellEditor");
					for(int i = 0; i < count; i++) {
						DataGridTextColumn column = new DataGridTextColumn() {
							Header = string.Format(CultureInfo.InvariantCulture, "{0:X}", i),
							Binding = new Binding(MemoryEditorRow.ColumnName[i]) {
								UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
								ValidatesOnDataErrors = true
							},
							ElementStyle = styleViewer,
							EditingElementStyle = styleEditor
						};
						this.dataGrid.Columns.Add(column);
					}
					this.dataGrid.ItemsSource = this.RowList();
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private List<MemoryEditorRow> RowList() {
			IFunctionMemory functionMemory = this.FunctionMemory;
			int cells = Memory.NumberCellsFor(functionMemory.AddressBitWidth);
			int rows = cells / 16 + (((cells % 16) == 0) ? 0 : 1);
			string format = "{{0:X{0}}}";
			string rowIndexFormat = string.Format(CultureInfo.InvariantCulture, format,
				Math.Max(1, ControlMemoryEditor.HexDigits(functionMemory.AddressBitWidth) - 1)
			);
			string cellFormat = string.Format(CultureInfo.InvariantCulture, format, ControlMemoryEditor.HexDigits(functionMemory.DataBitWidth));

			List<MemoryEditorRow> list = new List<MemoryEditorRow>(rows);

			for(int i = 0; i < rows; i++) {
				list.Add(new MemoryEditorRow(functionMemory, i, rowIndexFormat, cellFormat));
			}

			return list;
		}

		private static int HexDigits(int bitWidth) {
			return bitWidth / 4 + (((bitWidth % 4) == 0) ? 0 : 1);
		}

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class MemoryEditorRow : INotifyPropertyChanged, IDataErrorInfo, IEditableObject {
			[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
			public static readonly string[] ColumnName = new string[] {
				"X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X8", "X9", "XA", "XB", "XC", "XD", "XE", "XF"
			};

			public event PropertyChangedEventHandler PropertyChanged;

			private IFunctionMemory memory;
			private int rowIndex;
			string rowIndexFormat;
			string cellFormat;

			private string[] text;
			private string[] error;

			public string RowIndex { get { return string.Format(CultureInfo.InvariantCulture, rowIndexFormat, rowIndex); } }

			public MemoryEditorRow(IFunctionMemory memory, int rowIndex, string rowIndexFormat, string cellFormat) {
				this.memory = memory;
				this.rowIndex = rowIndex;
				this.rowIndexFormat = rowIndexFormat;
				this.cellFormat = cellFormat;
			}

			private string this[int index] {
				get {
					Tracer.Assert(0 <= index && index < 16);
					if(this.text != null && this.text[index] != null) {
						return this.text[index];
					}
					return string.Format(CultureInfo.InvariantCulture, this.cellFormat,
						this.memory[this.rowIndex * 16 + index]
					);
				}
				set {
					if(this.text == null) {
						this.text = new string[16];
					}
					this.text[index] = value;
					int i;
					string parseError = this.Parse(value, out i);
					if(parseError == null) {
						this.memory[this.rowIndex * 16 + index] = i;
						if(this.error != null && this.error[index] != null) {
							this.error[index] = null;
							if(!this.error.Any(e => !string.IsNullOrWhiteSpace(e))) {
								this.error = null;
							}
						}
					} else {
						if(this.error == null) {
							this.error = new string[16];
						}
						this.error[index] = parseError;
					}
					this.NotifyPropertyChanged(MemoryEditorRow.ColumnName[index]);
				}
			}

			private string Parse(string number, out int value) {
				if(!int.TryParse(number, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value)) {
					return Properties.Resources.ErrorBadHexNumber;
				}

				int bitWidth = this.memory.DataBitWidth;
				Tracer.Assert(0 < bitWidth && bitWidth <= Pin.MaxBitWidth);

				if(bitWidth < 32 && (1 << bitWidth) <= value) {
					return Properties.Resources.ErrorBadHexInRange(0, (1 << bitWidth) - 1);
				}
				return null;
			}

			private void NotifyPropertyChanged(string propertyName) {
				PropertyChangedEventHandler handler = this.PropertyChanged;
				if (handler != null) {
					handler(this, new PropertyChangedEventArgs(propertyName));
				}
			}

			public string Error {
				get {
					if(this.error != null) {
						return this.error.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));
					}
					return null;
				}
			}

			public string this[string columnName] {
				get {
					if(string.IsNullOrWhiteSpace(columnName)) {
						return this.Error;
					}
					if(this.error != null) {
						int index = Array.IndexOf(MemoryEditorRow.ColumnName, columnName);
						Tracer.Assert(0 <= index && index < 16);
						return this.error[index];
					}
					return null;
				}
			}

			public string X0 { get { return this[0x0]; } set { this[0x0] = value; } }
			public string X1 { get { return this[0x1]; } set { this[0x1] = value; } }
			public string X2 { get { return this[0x2]; } set { this[0x2] = value; } }
			public string X3 { get { return this[0x3]; } set { this[0x3] = value; } }
			public string X4 { get { return this[0x4]; } set { this[0x4] = value; } }
			public string X5 { get { return this[0x5]; } set { this[0x5] = value; } }
			public string X6 { get { return this[0x6]; } set { this[0x6] = value; } }
			public string X7 { get { return this[0x7]; } set { this[0x7] = value; } }
			public string X8 { get { return this[0x8]; } set { this[0x8] = value; } }
			public string X9 { get { return this[0x9]; } set { this[0x9] = value; } }
			public string XA { get { return this[0xA]; } set { this[0xA] = value; } }
			public string XB { get { return this[0xB]; } set { this[0xB] = value; } }
			public string XC { get { return this[0xC]; } set { this[0xC] = value; } }
			public string XD { get { return this[0xD]; } set { this[0xD] = value; } }
			public string XE { get { return this[0xE]; } set { this[0xE] = value; } }
			public string XF { get { return this[0xF]; } set { this[0xF] = value; } }

			public void BeginEdit() {
			}

			public void CancelEdit() {
				this.text = null;
				foreach(string name in MemoryEditorRow.ColumnName) {
					this.NotifyPropertyChanged(name);
				}
				this.NotifyPropertyChanged("Error");
			}

			public void EndEdit() {
				this.CancelEdit();
			}
		}
	}
}
