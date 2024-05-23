using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogMemoryEditor.xaml
	/// </summary>
	public abstract partial class DialogMemoryEditor : Window {
		public enum TextFileFormat {
			Dec,
			Hex,
			Bin,
		}

		private SettingsWindowLocationCache? windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private readonly SettingsStringCache openFileFolder;
		private readonly SettingsStringCache openTextFileFolder;
		private readonly SettingsEnumCache<TextFileFormat> textFileFormat;
		public SettingsGridLengthCache DataHeight { get; private set; }
		public SettingsGridLengthCache NoteHeight { get; private set; }

		public IEnumerable<EnumDescriptor<TextFileFormat>> TextFileFormats { get; } = [
			new EnumDescriptor<TextFileFormat>(TextFileFormat.Dec,	Properties.Resources.TextFileFormatDec),
			new EnumDescriptor<TextFileFormat>(TextFileFormat.Hex,	Properties.Resources.TextFileFormatHex),
			new	EnumDescriptor<TextFileFormat>(TextFileFormat.Bin,	Properties.Resources.TextFileFormatBin),
		];

		public EnumDescriptor<TextFileFormat> CurrentTextFileFormat { get; set; }
		
		public Memory Memory { get; private set; }

		private byte[] data;
		private readonly bool initialized;

		private int AddressBitWidth { get { return (int)this.addressBitWidth.SelectedItem; } }
		private int DataBitWidth { get { return (int)this.dataBitWidth.SelectedItem; } }
		private int currentAddressBitWidth;
		private int currentDataBitWidth;

		public static readonly DependencyProperty FunctionMemoryProperty = DependencyProperty.Register("FunctionMemory", typeof(IFunctionMemory), typeof(DialogMemoryEditor));
		public IFunctionMemory FunctionMemory {
			get { return (IFunctionMemory)this.GetValue(DialogMemoryEditor.FunctionMemoryProperty); }
			set { this.SetValue(DialogMemoryEditor.FunctionMemoryProperty, value); }
		}

		protected DialogMemoryEditor(Memory memory) {
			string typeName = this.GetType().Name;
			this.openFileFolder = new SettingsStringCache(Settings.User, typeName + ".OpenFile.Folder", Mainframe.DefaultProjectFolder());
			this.openTextFileFolder = new SettingsStringCache(Settings.User, typeName + ".OpenTextFile.Folder", Mainframe.DefaultProjectFolder());
			this.textFileFormat = new SettingsEnumCache<TextFileFormat>(Settings.User, typeName + "." + nameof(TextFileFormat), TextFileFormat.Dec);
			this.CurrentTextFileFormat = this.TextFileFormats.First(d => d.Value == this.textFileFormat.Value);

			this.DataHeight = new SettingsGridLengthCache(Settings.User, typeName + ".Data.Height", memory.Writable ? "0.25*" : "0.75*");
			this.NoteHeight = new SettingsGridLengthCache(Settings.User, typeName + ".Note.Height", memory.Writable ? "0.75*" : "0.25*");

			this.Memory = memory;
			this.data = memory.MemoryValue();

			this.DataContext = this;
			this.InitializeComponent();

			this.addressBitWidth.ItemsSource = MemoryDescriptor.AddressBitWidthRange;
			this.dataBitWidth.ItemsSource = PinDescriptor.BitWidthRange;
			IEnumerable<EnumDescriptor<bool>> writeOnList = MemoryDescriptor.WriteOnList;
			this.writeOn.ItemsSource = writeOnList;
			EnumDescriptor<MemoryOnStart>[] onStartList = new EnumDescriptor<MemoryOnStart>[] {
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Random, Properties.Resources.MemoryOnStartRandom),
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Zeros, Properties.Resources.MemoryOnStartZeros),
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Ones, Properties.Resources.MemoryOnStartOnes),
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Data, Properties.Resources.MemoryOnStartData)
			};
			this.onStart.ItemsSource = onStartList;

			this.addressBitWidth.SelectedItem = this.currentAddressBitWidth = this.Memory.AddressBitWidth;
			this.dataBitWidth.SelectedItem = this.currentDataBitWidth = this.Memory.DataBitWidth;
			this.writeOn.SelectedItem = writeOnList.First(d => d.Value == this.Memory.WriteOn1);
			this.onStart.SelectedItem = onStartList.First(d => d.Value == (this.Memory.Writable ? this.Memory.OnStart : MemoryOnStart.Data));
			this.checkBoxDualPort.IsChecked = this.Memory.DualPort;
			this.note.Text = this.Memory.Note;

			this.FunctionMemory = new MemoryEditor(this.data, this.Memory.AddressBitWidth, this.DataBitWidth);

			this.initialized = true;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				int addressBitWidth = this.AddressBitWidth;
				int dataBitWidth = this.DataBitWidth;
				bool writeOn1 = this.Memory.Writable && ((EnumDescriptor<bool>)this.writeOn.SelectedItem).Value;
				MemoryOnStart memoryOnStart = ((EnumDescriptor<MemoryOnStart>)this.onStart.SelectedItem).Value;
				bool dualPort = this.checkBoxDualPort.IsChecked!.Value;
				bool saveData = !this.Memory.Writable || memoryOnStart == MemoryOnStart.Data;
				if(!this.Memory.Writable) {
					memoryOnStart = MemoryOnStart.Random; // set to default value for ROM
				}
				string text = this.note.Text.Trim();

				bool equal(byte[] a, byte[] b) {
					if(a.Length == b.Length) {
						for(int i = 0; i < a.Length; i++) {
							if(a[i] != b[i]) {
								return false;
							}
						}
						return true;
					}
					return false;
				}

				if(this.Memory.AddressBitWidth != addressBitWidth || this.Memory.DataBitWidth != dataBitWidth || this.Memory.Note != text ||
					this.Memory.WriteOn1 != writeOn1 || this.Memory.OnStart != memoryOnStart || this.Memory.DualPort != dualPort ||
					(saveData && !equal(this.Memory.MemoryValue(), this.data))
				) {
					this.Memory.CircuitProject.InTransaction(() => {
						this.Memory.AddressBitWidth = addressBitWidth;
						this.Memory.DataBitWidth = dataBitWidth;
						this.Memory.WriteOn1 = writeOn1;
						this.Memory.OnStart = memoryOnStart;
						this.Memory.DualPort = dualPort;
						this.Memory.SetMemoryValue(saveData ? this.data : null);
						this.Memory.Note = text;
						if(this.Memory.Writable) {
							MemorySet.UpdateWritePinName(this.Memory);
						}
					});
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonLoadClick(object sender, RoutedEventArgs e) {
			try {
				OpenFileDialog dialog = new OpenFileDialog {
					InitialDirectory = Mainframe.IsDirectoryPathValid(this.openFileFolder.Value) ? this.openFileFolder.Value : Mainframe.DefaultProjectFolder()
				};
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					this.openFileFolder.Value = Path.GetDirectoryName(dialog.FileName)!;
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
					this.FunctionMemory = new MemoryEditor(this.data, addressBitWidth, dataBitWidth);
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonLoadTextClick(object sender, RoutedEventArgs e) {
			try {
				OpenFileDialog dialog = new OpenFileDialog {
					InitialDirectory = Mainframe.IsDirectoryPathValid(this.openTextFileFolder.Value) ? this.openTextFileFolder.Value : Mainframe.DefaultProjectFolder()
				};
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					this.openTextFileFolder.Value = Path.GetDirectoryName(dialog.FileName)!;
					int addressBitWidth = this.AddressBitWidth;
					int dataBitWidth = this.DataBitWidth;
					int cellCount = Memory.NumberCellsFor(addressBitWidth);
					using(FileStream stream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						using StreamReader streamReader = new StreamReader(stream);
						TextNumberReader reader = new TextNumberReader(streamReader, this.CurrentTextFileFormat.Value);
						for(int i = 0; i < cellCount; i++) {
							long value = reader.Next();
							if(value == -1L) break;
							Memory.SetCellValue(this.data, dataBitWidth, i, (int)value);
						}
					}
					this.FunctionMemory = new MemoryEditor(this.data, addressBitWidth, dataBitWidth);
				}
				this.textFileFormat.Value = CurrentTextFileFormat.Value;
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonSaveClick(object sender, RoutedEventArgs e) {
			try {
				SaveFileDialog dialog = new SaveFileDialog {
					InitialDirectory = Mainframe.IsDirectoryPathValid(this.openFileFolder.Value) ? this.openFileFolder.Value : Mainframe.DefaultProjectFolder()
				};
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					string file = dialog.FileName;
					this.openFileFolder.Value = Path.GetDirectoryName(file)!;
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
					this.FunctionMemory = new MemoryEditor(this.Memory.MemoryValue(), this.Memory.AddressBitWidth, this.DataBitWidth);

					int newAddressBitWidth = this.AddressBitWidth;
					int newDataBitWidth = this.DataBitWidth;
					if(this.currentAddressBitWidth != newAddressBitWidth || this.currentDataBitWidth != newDataBitWidth) {
						this.data = Memory.Reallocate(this.data, this.currentAddressBitWidth, this.currentDataBitWidth, newAddressBitWidth, newDataBitWidth);
						this.currentAddressBitWidth = newAddressBitWidth;
						this.currentDataBitWidth = newDataBitWidth;
						this.FunctionMemory = new MemoryEditor(this.data, newAddressBitWidth, newDataBitWidth);
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private class MemoryEditor : IFunctionMemory {
			public int AddressBitWidth { get; private set; }
			public int DataBitWidth { get; private set; }

			private readonly byte[] data;

			public int this[int index] {
				get { return Memory.CellValue(this.data, this.DataBitWidth, index); }
				set { Memory.SetCellValue(this.data, this.DataBitWidth, index, value); }
			}

			public MemoryEditor(byte[] data, int addressBitWidth, int dataBitWidth) {
				this.AddressBitWidth = addressBitWidth;
				this.DataBitWidth = dataBitWidth;
				this.data = data;
			}
		}

		internal class TextNumberReader {
			private readonly TextReader reader;
			private readonly Func<long> next;

			private int line;
			private int pos;
			private int back;

			public TextNumberReader(TextReader reader, TextFileFormat textFileFormat) {
				this.reader = reader;
				this.next = textFileFormat switch {
					TextFileFormat.Dec => this.NextDec,
					TextFileFormat.Hex => this.NextHex,
					TextFileFormat.Bin => this.NextBin,
					_ => throw new InvalidOperationException()
				};
				this.line = 1;
				this.back = -1;
			}

			public long Next() => this.next();

			private string ErrorTooBig() => Properties.Resources.ErrorTextFileBigNumber(this.line, this.pos);
			private string ErrorChar() => Properties.Resources.ErrorTextFileChar(this.line, this.pos);

			private int Read() {
				int c = this.back;
				if(c != -1) {
					this.back = -1;
				} else {
					c = this.reader.Read();
					if(c == '\n') {
						this.line++;
						this.pos = 0;
						return c;
					} else if(c != '\r') {
						this.pos++;
					}
				}
				return c;
			}

			private int Skip(bool skipSeparators) {
				int c = this.Read();
				while(char.IsWhiteSpace((char)c) || skipSeparators && ",;".Contains((char)c, StringComparison.Ordinal)) {
					c = this.Read();
				}
				return c;
			}

			private long NextDec() {
				int c = this.Skip(true);
				int sign = 1;
				if(c == '-') {
					sign = -1;
					c = this.Skip(false);
				} else if(c == '+') {
					c = this.Skip(false);
				}
				int value = 0;
				bool anyDigits = false;
				while('0' <= c && c <= '9') {
					anyDigits = true;
					long temp = value * 10L + c - '0';
					if(int.MaxValue < temp) {
						throw new CircuitException(Cause.UserError, this.ErrorTooBig());
					}
					value = (int)temp;
					c = this.Read();
				}
				this.back = c;
				if(anyDigits) {
					return ((long)value * sign) & ((1L << 32) - 1);
				}
				if(c == -1) {
					return -1L;
				}
				throw new CircuitException(Cause.CorruptedFile, this.ErrorChar());
			}

			private long NextHex() {
				int c = this.Skip(true);
				int sign = 1;
				if(c == '-') {
					sign = -1;
					c = this.Skip(false);
				} else if(c == '+') {
					c = this.Skip(false);
				}
				int value = 0;
				bool anyDigits = false;
				while('0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F') {
					anyDigits = true;
					int delta = 0;
					if('0' <= c && c <= '9') {
						delta = c - '0';
					} else if('a' <= c && c <= 'f') {
						delta = c - 'a' + 10;
					} else {
						Debug.Assert('A' <= c && c <= 'F');
						delta = c - 'A' + 10;
					}
					long temp = value * 16L + delta;
					if(int.MaxValue < temp) {
						throw new CircuitException(Cause.UserError, this.ErrorTooBig());
					}
					value = (int)temp;
					c = this.Read();
				}
				this.back = c;
				if(anyDigits) {
					return ((long)value * sign) & ((1L << 32) - 1);
				}
				if(c == -1) {
					return -1L;
				}
				throw new CircuitException(Cause.CorruptedFile, this.ErrorChar());
			}

			private long NextBin() {
				int c = this.Skip(true);
				int sign = 1;
				if(c == '-') {
					sign = -1;
					c = this.Skip(false);
				} else if(c == '+') {
					c = this.Skip(false);
				}
				int value = 0;
				bool anyDigits = false;
				while('0' <= c && c <= '1') {
					anyDigits = true;
					long temp = value * 2L + c - '0';
					if(int.MaxValue < temp) {
						throw new CircuitException(Cause.UserError, this.ErrorTooBig());
					}
					value = (int)temp;
					c = this.Read();
				}
				this.back = c;
				if(anyDigits) {
					return ((long)value * sign) & ((1L << 32) - 1);
				}
				if(c == -1) {
					return -1L;
				}
				throw new CircuitException(Cause.CorruptedFile, this.ErrorChar());
			}
		}
	}
}
