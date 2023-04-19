using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal sealed class HyperlinkNavigationCommand : ICommand {

		#pragma warning disable CS0067 // The event 'HyperlinkNavigationCommand.CanExecuteChanged' is never used
			public event EventHandler? CanExecuteChanged;
		#pragma warning restore CS0067 // The event 'HyperlinkNavigationCommand.CanExecuteChanged' is never used

		public bool CanExecute(object? parameter) {
			return true;
		}

		public void Execute(object? parameter) {
			try {
				Uri? uri = parameter as Uri;
				if(uri != null && !uri.IsFile && !uri.IsUnc &&
					(StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttp) || StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttps))
				) {
					ProcessStartInfo psi = new ProcessStartInfo(uri.AbsoluteUri);
					psi.UseShellExecute = true;
					Process.Start(psi);
				}
			} catch(Exception exception) {
				App.Mainframe.ErrorMessage(Properties.Resources.ErrorUnsupportedUri, exception);
			}
		}
	}
}
