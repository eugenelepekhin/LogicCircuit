using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogAbout.xaml
	/// </summary>
	public partial class DialogAbout : Window {

		public string Version { get; set; }
		private WebClient webClient;

		public DialogAbout() {
			this.Version = this.GetType().Assembly.GetName().Version.ToString();
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void CheckVersionButtonClick(object sender, RoutedEventArgs e) {
			try {
				if(Interlocked.CompareExchange(ref this.webClient, new WebClient(), null) == null) {
					this.Cursor = Cursors.Wait;
					this.webClient.UseDefaultCredentials = true;
					this.webClient.DownloadStringCompleted += this.DownloadCompleted;
					this.webClient.DownloadStringAsync(new Uri("http://www.LogicCircuit.org/LatestVersion.txt"));
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
				this.Cursor = Cursors.Arrow;
				this.webClient = null;
			}
		}

		private void DownloadCompleted(object sender, DownloadStringCompletedEventArgs e) {
			try {
				this.Cursor = Cursors.Arrow;
				this.webClient = null;
				if(e.Error != null) {
					App.Mainframe.ReportException(e.Error);
				} else {
					int result = new Version(this.Version).CompareTo(new Version(e.Result.Trim()));
					if(result < 0) {
						this.outdatedVersion.Visibility = Visibility.Visible;
					} else if(result == 0) {
						this.latestVersion.Visibility = Visibility.Visible;
					} else {
						this.previewVersion.Visibility = Visibility.Visible;
					}
					this.checkVersion.Visibility = Visibility.Hidden;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
