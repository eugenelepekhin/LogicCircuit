using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
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
		public Version Version { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
				this.Cursor = Cursors.Wait;
				VersionChecker.Check((version, error) => App.Dispatch(() => this.DownloadCompleted(version, error)));
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
				this.Cursor = Cursors.Arrow;
			}
		}

		private void DownloadCompleted(Version? version, Exception? error) {
			try {
				this.Cursor = Cursors.Arrow;
				if(error != null) {
					App.Mainframe.ReportException(error);
				} else {
					int result = version != null ? this.Version.CompareTo(version) : 0;
					if(result < 0) {
						this.outdatedVersion.Visibility = Visibility.Visible;
					} else if(result == 0) {
						this.latestVersion.Visibility = Visibility.Visible;
					} else {
						this.previewVersion.Visibility = Visibility.Visible;
					}
					this.checkVersionButton.Visibility = Visibility.Hidden;
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		private static Version CurrentVersion() {
			return Assembly.GetExecutingAssembly().GetName().Version!;
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
					VersionChecker.Check((version, error) => {
						if(error == null && version != null && DialogAbout.CurrentVersion() < version) {
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

		/// <summary>
		/// Check if https://www.LogicCircuit.org/TranslationRequests.txt contains name of current culture.
		/// If this is the case then show user message dialog asking to help translating this program.
		/// </summary>
		/// <param name="dispatcher"></param>
		/// <param name="recheck"></param>
		//[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "LogicCircuit.Mainframe.InformationMessage(System.String)")]
		//[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NavigateUri")]
		public static void CheckTranslationRequests(Dispatcher dispatcher) {
			try {
				string cultureName = App.CurrentCulture.Name;
				if(!cultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase)) {
					SettingsStringCache checkedVersion = DialogAbout.TranslationRequestVersion();
					if(!Version.TryParse(checkedVersion.Value, out Version? version) || version < DialogAbout.CurrentVersion()) {
						string? text = null;
						using(HttpClient client = new HttpClient()) {
							text = client.GetStringAsync(new Uri("https://www.LogicCircuit.org/TranslationRequests.txt")).Result;
						}
						if(!string.IsNullOrWhiteSpace(text) && text.Contains(cultureName, StringComparison.OrdinalIgnoreCase)) {
							dispatcher.BeginInvoke(
								new Action(() => App.Mainframe.InformationMessage(
									"If you can help translating this program to any language you are fluent in please contact me at:\n" +
									"<Hyperlink NavigateUri=\"https://www.logiccircuit.org/contact.html\">https://www.logiccircuit.org/contact.html</Hyperlink>"
								)),
								DispatcherPriority.ApplicationIdle
							);
						}
						checkedVersion.Value = DialogAbout.CurrentVersion().ToString();
					}
				}
			} catch(Exception exception) {
				Tracer.Report("DialogAbout.CheckTranslationRequests", exception);
				// ignore all exception here
			}
		}

		public static void ResetTranslationRequestVersion() {
			SettingsStringCache checkedVersion = DialogAbout.TranslationRequestVersion();
			checkedVersion.Value = null!;
		}

		private static SettingsStringCache TranslationRequestVersion() {
			return new SettingsStringCache(Settings.User, "DialogAbout.TranslationRequestVersion", string.Empty);
		}

		private static void ShowNewVersionAvailable(Version version) {
			DialogAbout dialog = new DialogAbout();
			dialog.DownloadCompleted(version, null);
			App.Mainframe.ShowDialog(dialog);
		}

		private static class VersionChecker {
			private static SettingsDateTimeCache? lastCheckedCache;
			private static SettingsBoolCache? checkVersionPeriodicallyCache;

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
					return VersionChecker.lastCheckedCache!;
				}
			}

			public static bool CheckVersionPeriodically {
				get {
					if(VersionChecker.checkVersionPeriodicallyCache == null) {
						SettingsBoolCache cache = new SettingsBoolCache(Settings.User, "DialogAbout.VersionChecker.CheckPeriodically", true);
						Interlocked.CompareExchange(ref VersionChecker.checkVersionPeriodicallyCache, cache, null);
						Tracer.Assert(VersionChecker.checkVersionPeriodicallyCache != null);
					}
					return VersionChecker.checkVersionPeriodicallyCache!.Value;
				}
				set {
					if(VersionChecker.CheckVersionPeriodically != value) {
						Tracer.Assert(VersionChecker.checkVersionPeriodicallyCache != null);
						VersionChecker.checkVersionPeriodicallyCache!.Value = value;
					}
				}
			}

			public static bool NeedToCheck() {
				return VersionChecker.CheckVersionPeriodically && VersionChecker.LastCheckedCache.Value.AddMonths(1) < DateTime.UtcNow;
			}

			public static void Check(Action<Version?, Exception?> completeAction) {
				async void run() {
					Version? version = null;
					Exception? exception = null;
					try {
						using HttpClient client = new HttpClient();
						string text = await client.GetStringAsync(new Uri("https://www.LogicCircuit.org/LatestVersion.txt")).ConfigureAwait(false);
						if(!string.IsNullOrWhiteSpace(text)) {
							if(Version.TryParse(text, out Version? v)) {
								version = v;
							}
						}
					} catch(Exception ex) {
						exception = ex;
					}
					completeAction(version, exception);
				}
				run();
			}
		}
	}
}
