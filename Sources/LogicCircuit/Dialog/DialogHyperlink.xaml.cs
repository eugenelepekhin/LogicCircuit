using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogHyperlink.xaml
	/// </summary>
	public partial class DialogHyperlink : Window, IDataErrorInfo {
		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public static readonly DependencyProperty IsValidHyperlinkProperty = DependencyProperty.Register(
			"IsValidHyperlink",
			typeof(bool),
			typeof(DialogHyperlink)
		);
		public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
			"Error",
			typeof(string),
			typeof(DialogHyperlink)
		);

		public bool IsValidHyperlink {
			get { return (bool)this.GetValue(DialogHyperlink.IsValidHyperlinkProperty); }
			set { this.SetValue(DialogHyperlink.IsValidHyperlinkProperty, value); }
		}

		public string Error {
			get { return (string)this.GetValue(DialogHyperlink.ErrorProperty); }
			set { this.SetValue(DialogHyperlink.ErrorProperty, value); }
		}

		private readonly RichTextBox textBox;
		private string text;
		private string url;
		private Dictionary<string, string> errorInfo = new Dictionary<string, string>(3);

		public string HyperlinkText {
			get { return this.text; }
			set {
				if(this.text != value) {
					this.text = value;
					this.ValidateHyperlink();
				}
			}
		}
		public string HyperlinkUrl {
			get { return this.url; }
			set {
				if(this.url != value) {
					this.url = value;
					this.ValidateHyperlink();
				}
			}
		}

		public string this[string columnName] {
			get {
				string error;
				if(this.errorInfo.TryGetValue(columnName, out error)) {
					return error;
				}
				return null;
			}
		}

		public DialogHyperlink(RichTextBox textBox) {
			this.textBox = textBox;
			Hyperlink link = DialogHyperlink.SelectedHyperlink(this.textBox);
			if(link != null) {
				TextRange range = new TextRange(link.ContentStart, link.ContentEnd);
				this.HyperlinkText = range.Text;
				this.HyperlinkUrl = link.NavigateUri.ToString();
				textBox.Selection.Select(link.ElementStart, link.ElementEnd);
			} else {
				this.HyperlinkText = this.textBox.Selection.Text;
				string text = this.HyperlinkText.Trim();
				if(DialogHyperlink.IsUrl(text)) {
					this.HyperlinkUrl = text;
				} else {
					this.HyperlinkUrl = string.Empty;
				}
			}
			this.DataContext = this;
			this.InitializeComponent();
		}

		private static Hyperlink SelectedHyperlink(RichTextBox textBox) {
			if(textBox.Selection.IsEmpty) {
				return DialogHyperlink.SelectedHyperlink(textBox.Selection.Start);
			} else {
				Hyperlink h1 = DialogHyperlink.SelectedHyperlink(textBox.Selection.Start);
				Hyperlink h2 = DialogHyperlink.SelectedHyperlink(textBox.Selection.End);
				if(h1 == h2 && h1 != null) {
					return h1;
				}
				if(h1 != null) {
					return h1;
				}
				if(h2 != null) {
					return h2;
				}
				h1 = DialogHyperlink.FindHyperlink(textBox.Selection.Start, LogicalDirection.Forward);
				h2 = DialogHyperlink.FindHyperlink(textBox.Selection.End, LogicalDirection.Backward);
				if(h1 == h2) {
					return h1;
				}
			}
			return null;
		}

		private static Hyperlink SelectedHyperlink(TextPointer textPointer) {
			DependencyObject o = textPointer.Parent;
			while(o != null && !(o is Hyperlink)) {
				TextElement text = o as TextElement;
				if(text != null) {
					o = text.Parent;
				} else {
					o = null;
				}
			}
			return o as Hyperlink;
		}

		private static Hyperlink FindHyperlink(TextPointer textPointer, LogicalDirection logicalDirection) {
			TextPointer tp = textPointer.GetNextInsertionPosition(logicalDirection);
			if(tp != null) {
				return DialogHyperlink.SelectedHyperlink(tp);
			}
			return null;
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				if(0 < this.HyperlinkText.Length && DialogHyperlink.IsValidUrl(this.HyperlinkUrl)) {
					this.textBox.Selection.Text = this.HyperlinkText;
					Hyperlink h = new Hyperlink(this.textBox.Selection.Start, this.textBox.Selection.End);
					UriBuilder builder = new UriBuilder(this.HyperlinkUrl);
					h.NavigateUri = new Uri(builder.Uri.AbsoluteUri);
					this.Close();
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private void ValidateHyperlink() {
			bool validText = 0 < this.HyperlinkText.Trim().Length;
			this.errorInfo.Clear();
			if(!validText) {
				this.errorInfo["HyperlinkText"] = Properties.Resources.ErrorHyperlinkText;
			}
			bool validUrl = DialogHyperlink.IsValidUrl(this.HyperlinkUrl);
			if(!validUrl) {
				this.errorInfo["HyperlinkUrl"] = Properties.Resources.ErrorHyperlinkUrl;
			}
			this.IsValidHyperlink = validText && validUrl;
			if(0 < this.errorInfo.Count) {
				StringBuilder text = new StringBuilder();
				foreach(string error in this.errorInfo.Values) {
					text.AppendLine(error);
				}
				this.Error = text.ToString();
			} else {
				this.Error = string.Empty;
			}
		}

		private static bool IsValidUrl(string url) {
			try {
				UriBuilder builder = new UriBuilder(url);
				return StringComparer.OrdinalIgnoreCase.Equals(builder.Scheme, Uri.UriSchemeHttp);
			} catch {}
			return false;
		}

		private static bool IsUrl(string url) {
			try {
				Uri uri;
				if(Uri.TryCreate(url, UriKind.Absolute, out uri)) {
					return StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttp);
				}
			} catch {}
			return false;
		}
	}
}
