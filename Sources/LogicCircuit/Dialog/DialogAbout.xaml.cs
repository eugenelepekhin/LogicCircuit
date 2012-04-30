using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogAbout.xaml
	/// </summary>
	public partial class DialogAbout : Window {
		private VersionChecker versionChecker = new VersionChecker();
		public Version Version { get; set; }

		public bool CheckVersionPeriodically {
			get { return VersionChecker.CheckVersionPeriodically; }
			set { VersionChecker.CheckVersionPeriodically = value; }
		}

		public DialogAbout() {
			this.Version = DialogAbout.CurrentVersion();
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void CheckVersionButtonClick(object sender, RoutedEventArgs e) {
			try {
				if(this.versionChecker.Check((version, error) => this.DownloadCompleted(version, error))) {
					this.Cursor = Cursors.Wait;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
				this.Cursor = Cursors.Arrow;
			}
		}

		private void DownloadCompleted(Version version, Exception error) {
			try {
				this.Cursor = Cursors.Arrow;
				if(error != null) {
					App.Mainframe.ReportException(error);
				} else {
					int result = this.Version.CompareTo(version);
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

		private static Version CurrentVersion() {
			return Assembly.GetExecutingAssembly().GetName().Version;
		}

		private static DateTime ReleaseDate(Version version) {
			int year = version.Minor;
			int month = version.Build;
			int day = version.Revision;
			if(year < 2000) {
				year += 2000;
			}
			return new DateTime(year, month, day);
		}

		public static void CheckVersion(Dispatcher dispatcher) {
			try {
				if(VersionChecker.NeedToCheck()) {
					VersionChecker checker = new VersionChecker();
					checker.Check((version, error) => {
						if(error == null && DialogAbout.CurrentVersion() < version) {
							dispatcher.BeginInvoke(
								new Action(() => DialogAbout.ShowNewVersionAvailable(version)),
								DispatcherPriority.ApplicationIdle
							);
						}
					});
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private static void ShowNewVersionAvailable(Version version) {
			DialogAbout dialog = new DialogAbout();
			dialog.DownloadCompleted(version, null);
			App.Mainframe.ShowDialog(dialog);
		}

		private class VersionChecker {
			private static SettingsDateTimeCache lastCheckedCache;
			private static SettingsBoolCache checkVersionPeriodicallyCache;
			private WebClient webClient;
			private Action<Version, Exception> onComplete;

			private static SettingsDateTimeCache LastCheckedCache {
				get {
					if(VersionChecker.lastCheckedCache == null) {
						SettingsDateTimeCache cache = new SettingsDateTimeCache(
							Settings.User, "DialogAbout.VersionChecker.Checked",
							DialogAbout.ReleaseDate(DialogAbout.CurrentVersion()).AddMonths(1)
						);
						Interlocked.CompareExchange(ref VersionChecker.lastCheckedCache, cache, null);
						Tracer.Assert(VersionChecker.lastCheckedCache != null);
					}
					return VersionChecker.lastCheckedCache;
				}
			}

			public static bool CheckVersionPeriodically {
				get {
					if(VersionChecker.checkVersionPeriodicallyCache == null) {
						SettingsBoolCache cache = new SettingsBoolCache(Settings.User, "DialogAbout.VersionChecker.CheckPeriodically", true);
						Interlocked.CompareExchange(ref VersionChecker.checkVersionPeriodicallyCache, cache, null);
						Tracer.Assert(VersionChecker.checkVersionPeriodicallyCache != null);
					}
					return VersionChecker.checkVersionPeriodicallyCache.Value;
				}
				set {
					if(VersionChecker.CheckVersionPeriodically != value) {
						Tracer.Assert(VersionChecker.checkVersionPeriodicallyCache != null);
						VersionChecker.checkVersionPeriodicallyCache.Value = value;
					}
				}
			}

			public static bool NeedToCheck() {
				return VersionChecker.CheckVersionPeriodically && VersionChecker.LastCheckedCache.Value.AddMonths(1) < DateTime.UtcNow;
			}

			public bool Check(Action<Version, Exception> onComplete) {
				if(onComplete == null) {
					throw new ArgumentNullException("onComplete");
				}
				if(Interlocked.CompareExchange(ref this.webClient, new WebClient(), null) == null) {
					this.onComplete = onComplete;
					this.webClient.UseDefaultCredentials = true;
					this.webClient.DownloadStringCompleted += this.DownloadCompleted;
					this.webClient.DownloadStringAsync(new Uri("http://www.LogicCircuit.org/LatestVersion.txt"));
					return true;
				}
				return false;
			}

			private void DownloadCompleted(object sender, DownloadStringCompletedEventArgs e) {
				this.webClient = null;
				this.onComplete(
					(e.Error == null) ? new Version(e.Result.Trim()) : null,
					e.Error
				);
				if(e.Error == null && VersionChecker.LastCheckedCache.Value < DateTime.UtcNow) {
					VersionChecker.LastCheckedCache.Value = DateTime.UtcNow;
				}
			}
		}
	}
}
