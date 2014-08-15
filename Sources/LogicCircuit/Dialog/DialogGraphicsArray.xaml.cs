using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogGraphicsArray.xaml
	/// </summary>
	public partial class DialogGraphicsArray : Window, IDataErrorInfo {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private GraphicsArray graphicsArray;

		public string GraphicsArrayWidth { get; set; }
		public string GraphicsArrayHeight { get; set; }

		private int parsedWidth, parsedHeight;

		public string Error { get { return null; } }

		public string this[string columnName] {
			get {
				string error = null;
				switch(columnName) {
				case "GraphicsArrayWidth":
					error = DialogGraphicsArray.ParseIntRange(this.GraphicsArrayWidth, 1, GraphicsArray.MaxWidth, out this.parsedWidth);
					break;
				case "GraphicsArrayHeight":
					error = DialogGraphicsArray.ParseIntRange(this.GraphicsArrayHeight, 1, GraphicsArray.MaxHeight, out this.parsedHeight);
					break;
				}
				if(this.buttonOk != null) {
					this.buttonOk.IsEnabled = (0 < this.parsedWidth && 0 < this.parsedHeight);
				}
				return error;
			}
		}

		private static string ParseIntRange(string text, int min, int max, out int result) {
			Tracer.Assert(0 <= min && min <= max);
			int i;
			if(	string.IsNullOrWhiteSpace(text) ||
				!int.TryParse(text.Trim(), NumberStyles.None, Properties.Resources.Culture, out i) ||
				i < min || max < i
			) {
				result = -1;
				return Properties.Resources.ErrorBadIntegerInRange(min, max);
			}
			result = i;
			return null;
		}

		public DialogGraphicsArray(GraphicsArray graphicsArray) {
			this.graphicsArray = graphicsArray;
			this.parsedWidth = this.graphicsArray.Width;
			this.parsedHeight = this.graphicsArray.Height;
			this.GraphicsArrayWidth = this.parsedWidth.ToString(Properties.Resources.Culture);
			this.GraphicsArrayHeight = this.parsedHeight.ToString(Properties.Resources.Culture);

			this.DataContext = this;
			this.InitializeComponent();

			this.dataBitWidth.ItemsSource = GraphicsArrayDescriptor.DataBitWidthRange;
			this.bitsPerPixel.ItemsSource = GraphicsArrayDescriptor.BitsPerPixelRange;

			IEnumerable<EnumDescriptor<bool>> writeOnList = MemoryDescriptor.WriteOnList;
			this.writeOn.ItemsSource = writeOnList;

			EnumDescriptor<MemoryOnStart>[] onStartList = new EnumDescriptor<MemoryOnStart>[] {
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Random, Properties.Resources.MemoryOnStartRandom),
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Zeros, Properties.Resources.MemoryOnStartZeros),
				new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Ones, Properties.Resources.MemoryOnStartOnes),
				//new EnumDescriptor<MemoryOnStart>(MemoryOnStart.Data, Properties.Resources.MemoryOnStartData)
			};
			this.onStart.ItemsSource = onStartList;

			this.dataBitWidth.SelectedItem = this.graphicsArray.DataBitWidth;
			this.bitsPerPixel.SelectedItem = this.graphicsArray.BitsPerPixel;
			this.writeOn.SelectedItem = writeOnList.First(d => d.Value == this.graphicsArray.WriteOn1);
			this.onStart.SelectedItem = onStartList.First(d => d.Value == this.graphicsArray.OnStart);
			this.note.Text = this.graphicsArray.Note;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				if(0 < this.parsedWidth && 0 < this.parsedHeight) {
					bool writeOn = ((EnumDescriptor<bool>)this.writeOn.SelectedItem).Value;
					MemoryOnStart onStart =((EnumDescriptor<MemoryOnStart>)this.onStart.SelectedItem).Value;
					int dataBitWidth = (int)this.dataBitWidth.SelectedItem;
					int bitsPerPixel = (int)this.bitsPerPixel.SelectedItem;
					string note = this.note.Text.Trim();

					if(	this.graphicsArray.WriteOn1 != writeOn ||
						this.graphicsArray.OnStart != onStart ||
						this.graphicsArray.DataBitWidth != dataBitWidth ||
						this.graphicsArray.BitsPerPixel != bitsPerPixel ||
						this.graphicsArray.Width != this.parsedWidth ||
						this.graphicsArray.Height != this.parsedHeight ||
						this.graphicsArray.Note != note
					) {
						this.graphicsArray.CircuitProject.InTransaction(() => {
							this.graphicsArray.WriteOn1 = writeOn;
							this.graphicsArray.OnStart = onStart;
							this.graphicsArray.DataBitWidth = dataBitWidth;
							this.graphicsArray.BitsPerPixel = bitsPerPixel;
							this.graphicsArray.Width = this.parsedWidth;
							this.graphicsArray.Height = this.parsedHeight;
							this.graphicsArray.Note = note;
						});
					}
				}
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
