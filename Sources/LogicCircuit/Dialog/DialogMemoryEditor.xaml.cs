using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private SettingsStringCache openFileFolder;
		public SettingsGridLengthCache DataHeight { get; private set; }
		public SettingsGridLengthCache NoteHeight { get; private set; }
		
		public Memory Memory { get; private set; }

		private byte[] data;
		private bool initialized = false;

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
			this.note.Text = this.Memory.Note;

			this.FunctionMemory = new MemoryEditor(this.data, this.Memory.AddressBitWidth, this.DataBitWidth);

			this.initialized = true;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "addressBitWidth")]
		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataBitWidth")]
		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				int addressBitWidth = this.AddressBitWidth;
				int dataBitWidth = this.DataBitWidth;
				bool writeOn1 = this.Memory.Writable && ((EnumDescriptor<bool>)this.writeOn.SelectedItem).Value;
				MemoryOnStart memoryOnStart = ((EnumDescriptor<MemoryOnStart>)this.onStart.SelectedItem).Value;
				bool saveData = !this.Memory.Writable || memoryOnStart == MemoryOnStart.Data;
				if(!this.Memory.Writable) {
					memoryOnStart = MemoryOnStart.Random; // set to default value for ROM
				}
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

				if(this.Memory.AddressBitWidth != addressBitWidth || this.Memory.DataBitWidth != dataBitWidth || this.Memory.Note != text ||
					this.Memory.WriteOn1 != writeOn1 || this.Memory.OnStart != memoryOnStart || (saveData && !equal(this.Memory.MemoryValue(), this.data))
				) {
					this.Memory.CircuitProject.InTransaction(() => {
						this.Memory.AddressBitWidth = addressBitWidth;
						this.Memory.DataBitWidth = dataBitWidth;
						this.Memory.WriteOn1 = writeOn1;
						this.Memory.OnStart = memoryOnStart;
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

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "addressBitWidth")]
		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataBitWidth")]
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
					this.FunctionMemory = new MemoryEditor(this.data, addressBitWidth, dataBitWidth);
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

			private byte[] data;

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
	}
}
