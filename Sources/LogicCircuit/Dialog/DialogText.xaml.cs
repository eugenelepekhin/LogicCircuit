using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
		private SettingsStringCache openImageFolder = new SettingsStringCache(Settings.User,
			"DialogText.OpenImage.Folder",
			Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
		);
		
		public string Document { get; set; }

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

		private void InsertImage() {
			try {
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.InitialDirectory = this.openImageFolder.Value;
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
