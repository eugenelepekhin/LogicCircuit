using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Media;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogMessage.xaml
	/// </summary>
	public partial class DialogMessage : Window {

		public static MessageBoxResult Show(
			Window parent, string caption, string message, string details, MessageBoxImage image, MessageBoxButton button
		) {
			DialogMessage dialog = new DialogMessage();
			dialog.Caption = caption ?? string.Empty;
			dialog.Message = message ?? string.Empty;
			dialog.Details = details ?? string.Empty;
			dialog.image = image;
			dialog.MessageBoxButton = button;
			dialog.DataContext = dialog;
			dialog.InitializeComponent();
			dialog.Owner = parent;
			dialog.ShowDialog();
			return dialog.messageBoxResult;
		}

		public string Caption { get; private set; }
		public string Message { get; private set; }
		public string Details { get; private set; }
		private MessageBoxImage image;
		public MessageBoxButton MessageBoxButton { get; private set; }
		private MessageBoxResult messageBoxResult = MessageBoxResult.None;

		private DialogMessage() {
			this.Loaded += new RoutedEventHandler(this.DialogMessageLoaded);
		}

		private void DialogMessageLoaded(object sender, RoutedEventArgs e) {
			try {
				SystemSound sound;
				switch(this.image) {
				case MessageBoxImage.Information:
					sound = SystemSounds.Asterisk;
					break;
				case MessageBoxImage.Warning:
					sound = SystemSounds.Exclamation;
					break;
				case MessageBoxImage.Question:
					sound = SystemSounds.Question;
					break;
				case MessageBoxImage.Error:
				default:
					sound = SystemSounds.Hand;
					break;
				}
				sound.Play();

				switch(this.MessageBoxButton) {
				case MessageBoxButton.OK:
				default:
					this.OK.IsDefault = this.OK.IsCancel = true;
					this.Yes.Visibility = this.No.Visibility = this.Cancel.Visibility = Visibility.Collapsed;
					this.OK.Focus();
					break;
				case MessageBoxButton.OKCancel:
					this.OK.IsDefault = this.Cancel.IsCancel = true;
					this.Yes.Visibility = this.No.Visibility = Visibility.Collapsed;
					this.OK.Focus();
					break;
				case MessageBoxButton.YesNo:
					this.Yes.IsDefault = true;
					this.OK.Visibility = this.Cancel.Visibility = Visibility.Collapsed;
					this.Yes.Focus();
					break;
				case MessageBoxButton.YesNoCancel:
					this.Yes.IsDefault = this.Cancel.IsCancel = true;
					this.OK.Visibility = Visibility.Collapsed;
					this.Yes.Focus();
					break;
				}
			} catch {
			}
		}

		public BitmapSource BitmapSource {
			get {
				string icon;
				switch(this.image) {
				case MessageBoxImage.Information:
					icon = "pack://application:,,,/Properties/info.png";
					break;
				case MessageBoxImage.Warning:
					icon = "pack://application:,,,/Properties/warning.png";
					break;
				case MessageBoxImage.Question:
					icon = "pack://application:,,,/Properties/question.png";
					break;
				case MessageBoxImage.Error:
				default:
					icon = "pack://application:,,,/Properties/error.png";
					break;
				}
				PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(icon), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				return decoder.Frames[0];
			}
		}

		private void ButtonClick(object sender, RoutedEventArgs e) {
			Button button = (Button)sender;
			if(button == this.OK) {
				this.messageBoxResult = MessageBoxResult.OK;
			} else if(button == this.Cancel) {
				this.messageBoxResult = MessageBoxResult.Cancel;
			} else if(button == this.Yes) {
				this.messageBoxResult = MessageBoxResult.Yes;
			} else if(button == this.No) {
				this.messageBoxResult = MessageBoxResult.No;
			} else {
				this.messageBoxResult = MessageBoxResult.None;
			}
			this.Close();
		}
	}
}
