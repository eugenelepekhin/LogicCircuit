using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogMemoryEditor.xaml
	/// </summary>
	public abstract partial class DialogMemoryEditor : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private SettingsStringCache openFileFolder;
		public SettingsGridLengthCache DataHeight { get; private set; }
		public SettingsGridLengthCache NoteHeight { get; private set; }
		
		public Memory Memory { get; private set; }

		public static DependencyProperty RowListProperty = DependencyProperty.Register("RowList", typeof(RowList), typeof(DialogMemoryEditor));
		public RowList Rows {
			get { return (RowList)this.GetValue(DialogMemoryEditor.RowListProperty); }
			private set { this.SetValue(DialogMemoryEditor.RowListProperty, value); }
		}

		private byte[] data;
		private int currentRow = 0;
		private int currentCol = 0;
		private bool initialized = false;

		private int AddressBitWidth { get { return (int)this.addressBitWidth.SelectedItem; } }
		private int DataBitWidth { get { return (int)this.dataBitWidth.SelectedItem; } }

		protected DialogMemoryEditor(Memory memory) {
			string typeName = this.GetType().Name;
			this.openFileFolder = new SettingsStringCache(Settings.User, typeName + ".OpenFile.Folder", Mainframe.DefaultProjectFolder());
			this.DataHeight = new SettingsGridLengthCache(Settings.User, typeName + ".Data.Height", memory.Writable ? "0.25*" : "0.75*");
			this.NoteHeight = new SettingsGridLengthCache(Settings.User, typeName + ".Note.Height", memory.Writable ? "0.75*" : "0.25*");

			this.Memory = memory;
			this.data = memory.MemoryValue();
			this.Rows = new RowList(this.data, this.Memory.AddressBitWidth, this.Memory.DataBitWidth);

			this.DataContext = this;
			this.InitializeComponent();

			this.addressBitWidth.ItemsSource = MemoryDescriptor.AddressBitWidthRange;
			this.dataBitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			this.writeOn.ItemsSource = new string[] { LogicCircuit.Resources.WriteOn0, LogicCircuit.Resources.WriteOn1 };
			this.onStart.ItemsSource = new string[] {
				LogicCircuit.Resources.MemoryOnStartRandom, LogicCircuit.Resources.MemoryOnStartZeros, LogicCircuit.Resources.MemoryOnStartOnes, LogicCircuit.Resources.MemoryOnStartData
			};

			this.addressBitWidth.SelectedItem = this.Memory.AddressBitWidth;
			this.dataBitWidth.SelectedItem = this.Memory.DataBitWidth;
			this.writeOn.SelectedIndex = this.Memory.WriteOn1 ? 1 : 0;
			this.onStart.SelectedIndex = (int)this.Memory.OnStart;
			this.note.Text = this.Memory.Note;
			this.Loaded += new RoutedEventHandler(this.DialogLoaded);
		}

		private void DialogLoaded(object sender, RoutedEventArgs e) {
			try {
				this.UpdateListView();
				this.initialized = true;
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void UpdateListView() {
			if(0 < this.data.Length) {
				this.SelectCell(this.currentRow, this.currentCol);
			}
			GridView view = (GridView)this.listView.View;
			foreach(GridViewColumn column in view.Columns) {
				column.Width = 0;
			}
			foreach(GridViewColumn column in view.Columns) {
				column.Width = Double.NaN;
			}
		}

		private void ApplyChanges() {
			this.Rows.ApplyChanges(this.data);
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				this.ApplyChanges();
				int addressBitWidth = this.AddressBitWidth;
				int dataBitWidth = this.DataBitWidth;
				byte[] originalData = this.Memory.MemoryValue();
				string text = this.note.Text.Trim();
				Func<byte[], byte[], bool> equal = (a, b) => {
					if(a.Length == b.Length) {
						for(int i = 0; i < a.Length; i++) {
							if(a[i] != b[i]) {
								return false;
							}
						}
						return true;
					}
					return false;
				};

				if(this.Memory.AddressBitWidth != addressBitWidth || this.Memory.DataBitWidth != dataBitWidth || this.Memory.Note != text || !equal(originalData, this.data)) {
					this.Memory.CircuitProject.InTransaction(() => {
						this.Memory.AddressBitWidth = addressBitWidth;
						this.Memory.DataBitWidth = dataBitWidth;
						this.Memory.SetMemoryValue(this.data);
						this.Memory.Note = text;
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonLoadClick(object sender, RoutedEventArgs e) {
			try {
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.InitialDirectory = Mainframe.IsDirectoryPathValid(this.openFileFolder.Value) ? this.openFileFolder.Value : Mainframe.DefaultProjectFolder();
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					this.openFileFolder.Value = Path.GetDirectoryName(dialog.FileName);
					int addressBitWidth = this.AddressBitWidth;
					int dataBitWidth = this.DataBitWidth;
					byte[] buffer = new byte[Memory.BytesPerCellFor(dataBitWidth)];
					int cellCount = Memory.NumberCellsFor(addressBitWidth);
					Tracer.Assert(cellCount * buffer.Length == this.data.Length);
					using(FileStream stream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						for(int i = 0; i < cellCount; i++) {
							int readed = stream.Read(buffer, 0, buffer.Length);
							if(readed <= 0) {
								Array.Clear(this.data, i * buffer.Length, this.data.Length - i * buffer.Length);
								break;
							}
							int value = Memory.CellValue(buffer, Math.Min(8 * readed, dataBitWidth), 0);
							Memory.SetCellValue(this.data, dataBitWidth, i, value);
						}
					}
					this.currentRow = 0;
					this.currentCol = 0;
					this.Rows = new RowList(this.data, addressBitWidth, dataBitWidth);
					this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.UpdateListView));
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonSaveClick(object sender, RoutedEventArgs e) {
			try {
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.InitialDirectory = Mainframe.IsDirectoryPathValid(this.openFileFolder.Value) ? this.openFileFolder.Value : Mainframe.DefaultProjectFolder();
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					this.ApplyChanges();
					string file = dialog.FileName;
					this.openFileFolder.Value = Path.GetDirectoryName(file);
					using(FileStream stream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Write)) {
						stream.Write(this.data, 0, this.data.Length);
						stream.Flush();
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void MemorySizeChanged(object sender, SelectionChangedEventArgs e) {
			try {
				if(this.initialized) {
					int addressBitWidth = this.AddressBitWidth;
					int dataBitWidth = this.DataBitWidth;
					if(this.Rows.AddressBitWidth != addressBitWidth || this.Rows.DataBitWidth != dataBitWidth) {
						this.ApplyChanges();
						this.data = Memory.Reallocate(this.data, this.Rows.AddressBitWidth, this.Rows.DataBitWidth, addressBitWidth, dataBitWidth);
						this.currentRow = 0;
						this.currentCol = 0;
						this.Rows = new RowList(this.data, addressBitWidth, dataBitWidth);
						this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.UpdateListView));
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void GridView_MouseDown(object sender, MouseButtonEventArgs e) {
			try {
				TextBlock text = sender as TextBlock;
				if(text != null) {
					this.listView.Focus();
					ContentPresenter cp = text.TemplatedParent as ContentPresenter;
					if(cp != null) {
						Row row = cp.Content as Row;
						if(row != null) {
							int column = Row.IndexOf(text.Name);
							if(row[column] != null) {
								this.UnselectCell(this.currentRow, this.currentCol);
								this.currentRow = row.RowIndex;
								this.currentCol = column;
								this.SelectCell(this.currentRow, this.currentCol);
							}
						}
					}
					e.Handled = true;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void listView_PreviewKeyDown(object sender, KeyEventArgs e) {
			try {
				this.listView.Focus();
				switch(e.Key) {
				case Key.Up:
					if(0 < this.currentRow) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentRow--;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.Down:
					if(this.currentRow < this.Rows.Count - 1) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentRow++;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.Left:
					if(0 < this.currentCol) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentCol--;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.Right:
					if(this.currentCol < 15 && this.Rows[this.currentRow][this.currentCol + 1] != null) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentCol++;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.Home:
					if(this.currentCol != 0) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentCol = 0;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.End:
					if(this.currentCol != 15) {
						int column = 15;
						while(this.currentCol < column && this.Rows[this.currentRow][column] == null) {
							column--;
						}
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentCol = column;
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.PageUp:
					if(0 < this.currentRow) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentRow = Math.Max(0, this.currentRow - 16);
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.PageDown:
					if(this.currentRow < this.Rows.Count - 1) {
						this.UnselectCell(this.currentRow, this.currentCol);
						this.currentRow = Math.Min(this.Rows.Count - 1, this.currentRow + 16);
						this.SelectCell(this.currentRow, this.currentCol);
					}
					break;
				case Key.Tab:
					if((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						FrameworkElement c = this.listView.PredictFocus(FocusNavigationDirection.Up) as FrameworkElement;
						if(c != null) {
							c.Focus();
						}
					} else {
						FrameworkElement c = this.listView.PredictFocus(FocusNavigationDirection.Down) as FrameworkElement;
						if(c != null) {
							c.Focus();
						}
					}
					break;
				default:
					return;
				}
				this.listView.ScrollIntoView(this.Rows[this.currentRow]);
				e.Handled = true;
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void listView_PreviewTextInput(object sender, TextCompositionEventArgs e) {
			try {
				Row row = this.Rows[this.currentRow];
				row.AddDigit(this.currentCol, e.Text);
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				this.listView.SelectedItem = null;
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private T Find<T>(FrameworkElement element) where T : FrameworkElement {
			for(int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
				FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
				if(child != null) {
					if(child is T) {
						return (T)child;
					} else {
						child = this.Find<T>(child);
						if(child != null) {
							return (T)child;
						}
					}
				}
			}
			return null;
		}

		private TextBlock Cell(int row, int col) {
			ListViewItem item = this.listView.ItemContainerGenerator.ContainerFromIndex(row) as ListViewItem;
			if(item != null) {
				GridViewRowPresenter r = this.Find<GridViewRowPresenter>(item);
				if(r != null) {
					Tracer.Assert(VisualTreeHelper.GetChildrenCount(r) == 17);
					return this.Find<TextBlock>((FrameworkElement)VisualTreeHelper.GetChild(r, col + 1));
				}
			}
			return null;
		}

		private void SelectCell(int row, int col) {
			TextBlock text = this.Cell(row, col);
			if(text != null) {
				text.Background = SystemColors.HighlightBrush;
				text.Foreground = SystemColors.HighlightTextBrush;
			}
		}

		private void UnselectCell(int row, int col) {
			TextBlock text = this.Cell(row, col);
			if(text != null) {
				text.Background = Brushes.Transparent;
				text.Foreground = SystemColors.WindowTextBrush;
			}
		}

		public class Row : INotifyPropertyChanged {
			private static readonly string[] Cell = new string[] {
				"C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF"
			};

			public event PropertyChangedEventHandler PropertyChanged;

			private byte[] data;
			private int rowIndex;
			private int bitWidth;
			private string[] text;
			private string format;
			private string rowIndexText;

			public Row(byte[] data, int rowIndex, int bitWidth, string rowIndexFormat) {
				Tracer.Assert(data != null);
				Tracer.Assert(0 < bitWidth && bitWidth <= BasePin.MaxBitWidth);
				Tracer.Assert(data.Length % Memory.BytesPerCellFor(bitWidth) == 0);
				Tracer.Assert(0 <= rowIndex && rowIndex * Memory.BytesPerCellFor(bitWidth) * 16 < data.Length);
				this.PropertyChanged = null;
				this.data = data;
				this.rowIndex = rowIndex;
				this.bitWidth = bitWidth;
				this.rowIndexText = string.Format(CultureInfo.InvariantCulture, rowIndexFormat, rowIndex);
				this.text = null;
				this.format = null;
				this.format = string.Format(CultureInfo.InvariantCulture, "{{0:X{0}}}", this.DigitsPerCell);
			}

			private int DigitsPerCell { get { return this.bitWidth / 4 + (((this.bitWidth % 4) == 0) ? 0 : 1); } }
			private int BytesPerCell { get { return Memory.BytesPerCellFor(this.bitWidth); } }

			public string this[int index] {
				get {
					Tracer.Assert(0 <= index && index < 16);
					if(this.text == null) {
						this.text = new string[16];
					}
					if(this.text[index] == null) {
						if((index + 1 + this.rowIndex * 16) * this.BytesPerCell <= this.data.Length) {
							this.text[index] = string.Format(CultureInfo.InvariantCulture, this.format,
								Memory.CellValue(this.data, this.bitWidth, this.rowIndex * 16 + index)
							);
						}
					}
					return this.text[index];
				}
			}

			public void ApplyChanges(byte[] newData) {
				if(this.text != null) {
					for(int i = 0; i < this.text.Length; i++) {
						if(this.text[i] != null) {
							Memory.SetCellValue(newData, this.bitWidth, this.rowIndex * 16 + i,
								int.Parse(this.text[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture)
							);
						}
					}
				}
			}

			public string RowNumber { get { return this.rowIndexText; } }
			public int RowIndex { get { return this.rowIndex; } }

			public string C0 { get { return this[0x0]; } }
			public string C1 { get { return this[0x1]; } }
			public string C2 { get { return this[0x2]; } }
			public string C3 { get { return this[0x3]; } }
			public string C4 { get { return this[0x4]; } }
			public string C5 { get { return this[0x5]; } }
			public string C6 { get { return this[0x6]; } }
			public string C7 { get { return this[0x7]; } }
			public string C8 { get { return this[0x8]; } }
			public string C9 { get { return this[0x9]; } }
			public string CA { get { return this[0xA]; } }
			public string CB { get { return this[0xB]; } }
			public string CC { get { return this[0xC]; } }
			public string CD { get { return this[0xD]; } }
			public string CE { get { return this[0xE]; } }
			public string CF { get { return this[0xF]; } }

			public void AddDigit(int index, string digits) {
				string text = this[index];
				foreach(char c in digits) {
					char d = char.ToUpper(c, CultureInfo.InvariantCulture);
					if(0 <= "0123456789ABCDEF".IndexOf(d)) {
						text = string.Concat(text.Substring(1), d);
					}
				}
				int v = int.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				if(this.bitWidth < 32) {
					v = v & ((1 << this.bitWidth) - 1);
				}
				this.text[index] = string.Format(CultureInfo.InvariantCulture, this.format, v);
				PropertyChangedEventHandler handler = this.PropertyChanged;
				if(handler != null) {
					handler(this, new PropertyChangedEventArgs(Row.Cell[index]));
				}
			}

			public static int IndexOf(string name) {
				return Array.IndexOf<string>(Row.Cell, name);
			}
		}

		public class RowList : IEnumerable<Row> {

			private Row[] list;
			public  int AddressBitWidth { get; private set; }
			public  int DataBitWidth { get; private set; }

			public RowList(byte[] data, int addressBitWidth, int dataBitWidth) {
				this.AddressBitWidth = addressBitWidth;
				this.DataBitWidth = dataBitWidth;
				int cells = Memory.NumberCellsFor(this.AddressBitWidth);
				this.list = new Row[cells / 16 + (((cells % 16) == 0) ? 0 : 1)];
				string rowIndexFormat = string.Format(CultureInfo.InvariantCulture, "{{0:X{0}}}",
					string.Format(CultureInfo.InvariantCulture, "{0:X}", this.list.Length - 1).Length
				);
				for(int i = 0; i < this.list.Length; i++) {
					this.list[i] = new Row(data, i, this.DataBitWidth, rowIndexFormat);
				}
			}

			public void ApplyChanges(byte[] data) {
				foreach(Row row in this.list) {
					if(row != null) {
						row.ApplyChanges(data);
					}
				}
			}

			public Row this[int index] {
				get { return this.list[index]; }
			}

			public int Count { get { return this.list.Length; } }

			public IEnumerator<Row> GetEnumerator() {
				return new RowEnumerator(this);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
			}

			private class RowEnumerator : IEnumerator<Row> {

				private RowList rowList;
				private int current = -1;

				public RowEnumerator(RowList list) {
					this.rowList = list;
				}

				public Row Current {
					get { return this.rowList.list[this.current]; }
				}

				public void Dispose() {
				}

				object System.Collections.IEnumerator.Current {
					get { return this.Current; }
				}

				public bool MoveNext() {
					this.current++;
					return this.current < this.rowList.list.Length;
				}

				public void Reset() {
					throw new NotImplementedException();
				}
			}
		}
	}
}
