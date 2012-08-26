using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogText.xaml
	/// </summary>
	public partial class DialogText : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }
		private SettingsStringCache openImageFolder = new SettingsStringCache(Settings.User, "DialogText.OpenImage.Folder", Mainframe.DefaultPictureFolder());

		private SettingsIntegerCache editToolBarBand = new SettingsIntegerCache(Settings.User, "DialogText.EditToolBarBand", 0, 10, 0);
		public SettingsIntegerCache EditToolBarBand { get { return this.editToolBarBand; } }

		private SettingsIntegerCache editToolBarBandIndex = new SettingsIntegerCache(Settings.User, "DialogText.EditToolBarBandIndex", 0, 10, 0);
		public SettingsIntegerCache EditToolBarBandIndex { get { return this.editToolBarBandIndex; } }

		private SettingsIntegerCache fontToolBarBand = new SettingsIntegerCache(Settings.User, "DialogText.FontToolBarBand", 0, 10, 1);
		public SettingsIntegerCache FontToolBarBand { get { return this.fontToolBarBand; } }

		private SettingsIntegerCache fontToolBarBandIndex = new SettingsIntegerCache(Settings.User, "DialogText.FontToolBarBandIndex", 0, 10, 0);
		public SettingsIntegerCache FontToolBarBandIndex { get { return this.fontToolBarBandIndex; } }

		private SettingsIntegerCache paraToolBarBand = new SettingsIntegerCache(Settings.User, "DialogText.ParaToolBarBand", 0, 10, 0);
		public SettingsIntegerCache ParaToolBarBand { get { return this.paraToolBarBand; } }

		private SettingsIntegerCache paraToolBarBandIndex = new SettingsIntegerCache(Settings.User, "DialogText.ParaToolBarBandIndex", 0, 10, 1);
		public SettingsIntegerCache ParaToolBarBandIndex { get { return this.paraToolBarBandIndex; } }

		private SettingsIntegerCache otherToolBarBand = new SettingsIntegerCache(Settings.User, "DialogText.OtherToolBarBand", 0, 10, 1);
		public SettingsIntegerCache OtherToolBarBand { get { return this.otherToolBarBand; } }

		private SettingsIntegerCache otherToolBarBandIndex = new SettingsIntegerCache(Settings.User, "DialogText.OtherToolBarBandIndex", 0, 10, 1);
		public SettingsIntegerCache OtherToolBarBandIndex { get { return this.otherToolBarBandIndex; } }

		public string Document { get; set; }

		public IEnumerable<FontFamily> FontFamilies { get { return Fonts.SystemFontFamilies.OrderBy(f => f.Source); } }
		public FontFamily CurrentFontFamily {
			get {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						return this.editor.Selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.get_CurrentFontFamily", exception);
				}
				return null;
			}
			set {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						this.editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, value);
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.set_CurrentFontFamily", exception);
				}
			}
		}

		public IEnumerable<double> FontSizes { get { return new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 }; } }
		public double CurrentFontSize {
			get {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						object size = this.editor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
						if(size is double) {
							return (double)size;
						}
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.get_CurrentFontSize", exception);
				}
				return 0;
			}
			set {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						this.editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, value);
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.set_CurrentFontSize", exception);
				}
			}
		}

		private Color highlightColor = Colors.Yellow;
		public Color HighlightColor {
			get { return highlightColor; }
			set {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						this.editor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(value));
						this.highlightColor = value;
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.set_HighlightColor", exception);
				}
			}
		}

		private Color foregroundColor = SystemColors.WindowTextColor;
		public Color ForegroundColor {
			get { return foregroundColor; }
			set {
				try {
					if(this.editor != null && this.editor.Selection != null) {
						this.editor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(value));
						this.foregroundColor = value;
					}
				} catch(Exception exception) {
					Tracer.Report("DialogText.set_ForegroundColor", exception);
				}
			}
		}

		public bool IsBoldFont { get { return this.IsSelected(FontWeights.Bold, TextElement.FontWeightProperty); } }
		public bool IsItalicFont { get { return this.IsSelected(FontStyles.Italic, TextElement.FontStyleProperty); } }
		public bool IsUnderlineFont { get { return this.IsSelected(TextDecorations.Underline, Inline.TextDecorationsProperty); } }

		public bool IsLeftAlignment { get { return this.IsSelected(TextAlignment.Left, Paragraph.TextAlignmentProperty); } }
		public bool IsCenterAlignment { get { return this.IsSelected(TextAlignment.Center, Paragraph.TextAlignmentProperty); } }
		public bool IsRightAlignment { get { return this.IsSelected(TextAlignment.Right, Paragraph.TextAlignmentProperty); } }
		public bool IsJustifyAlignment { get { return this.IsSelected(TextAlignment.Justify, Paragraph.TextAlignmentProperty); } }

		public bool IsBulletted { get { return this.IsSelected(TextMarkerStyle.Disc); } }
		public bool IsNumbered { get { return this.IsSelected(TextMarkerStyle.Decimal); } }

		public LambdaUICommand HyperlinkCommand { get; private set; }
		public LambdaUICommand InsertImageCommand { get; private set; }

		public DialogText(string document) {
			this.HyperlinkCommand = new LambdaUICommand(LogicCircuit.Resources.CommandHyperlink,
				o => {
					try {
						DialogHyperlink dialog = new DialogHyperlink(this.editor);
						dialog.Owner = this;
						dialog.ShowDialog();
					} catch(Exception exception) {
						App.Mainframe.ReportException(exception);
					}
				}
			);
			this.InsertImageCommand = new LambdaUICommand(LogicCircuit.Resources.CommandInsertImage, o => this.InsertImage());

			this.Document = document;
			this.DataContext = this;
			this.InitializeComponent();
			if(!string.IsNullOrEmpty(this.Document)) {
				FlowDocument flowDoc = TextNote.Load(this.Document);
				if(flowDoc != null) {
					this.editor.Document = flowDoc;
				}
			}
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private bool IsSelected(object value, DependencyProperty property) {
			return this.editor!= null && this.editor.Selection != null && value.Equals(this.editor.Selection.GetPropertyValue(property));
		}

		private bool IsSelected(TextMarkerStyle textMarkerStyle) {
			if(this.editor!= null && this.editor.Selection != null) {
				Paragraph start = this.editor.Selection.Start.Paragraph;
				Paragraph end = this.editor.Selection.End.Paragraph;
				if(start != null && end != null) {
					ListItem sli = start.Parent as ListItem;
					ListItem eli = end.Parent as ListItem;
					if(sli != null && eli != null && sli.List == eli.List && sli.List != null) {
						return sli.List.MarkerStyle == textMarkerStyle;
					}
				}

			}
			return false;
		}

		private void UpdateToolbar() {
			try {
				this.NotifyPropertyChanged("CurrentFontFamily");
				this.NotifyPropertyChanged("CurrentFontSize");

				this.NotifyPropertyChanged("IsBoldFont");
				this.NotifyPropertyChanged("IsItalicFont");
				this.NotifyPropertyChanged("IsUnderlineFont");

				this.NotifyPropertyChanged("IsLeftAlignment");
				this.NotifyPropertyChanged("IsCenterAlignment");
				this.NotifyPropertyChanged("IsRightAlignment");
				this.NotifyPropertyChanged("IsJustifyAlignment");

				this.NotifyPropertyChanged("IsBulletted");
				this.NotifyPropertyChanged("IsNumbered");
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				this.Document = TextNote.Save(this.editor.Document);
				this.DialogResult = true;
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void editorTextChanged(object sender, TextChangedEventArgs e) {
			this.UpdateToolbar();
		}

		private void editorSelectionChanged(object sender, RoutedEventArgs e) {
			this.UpdateToolbar();
		}

		private void FontFamilySelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				ComboBox combo = sender as ComboBox;
				if(combo != null) {
					FontFamily fontFamily = combo.SelectedItem as FontFamily;
					if(fontFamily != null) {
						this.editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void InsertImage() {
			try {
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.InitialDirectory = Mainframe.IsDirectoryPathValid(this.openImageFolder.Value) ? this.openImageFolder.Value : Mainframe.DefaultPictureFolder();
				dialog.Filter = LogicCircuit.Resources.ImageFilter("*.bmp;*.dib;*.gif;*.jpg;*.jpeg;*.jpe;*.png;*.tiff;*.tif");
				dialog.FilterIndex = 0;
				bool? result = dialog.ShowDialog(this);
				if(result.HasValue && result.Value) {
					string path = dialog.FileName;
					this.openImageFolder.Value = Path.GetDirectoryName(path);
					Uri uri;
					if(Uri.TryCreate(path, UriKind.Absolute, out uri)) {
						Image image = new Image();
						image.BeginInit();
						image.Source = new BitmapImage(uri);
						image.MaxWidth = image.Source.Width;
						image.EndInit();
						Span span = new Span(this.editor.Selection.End, this.editor.Selection.End);
						span.Inlines.Add(image);
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
