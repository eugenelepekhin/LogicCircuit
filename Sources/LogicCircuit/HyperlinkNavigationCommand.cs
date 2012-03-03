using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	internal sealed class HyperlinkNavigationCommand : ICommand {

		#pragma warning disable 0067
			public event EventHandler CanExecuteChanged;
		#pragma warning restore 0067

		public bool CanExecute(object parameter) {
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Execute(object parameter) {
			try {
				Uri uri = parameter as Uri;
				if(uri != null && !uri.IsFile && !uri.IsUnc &&
					(StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttp) || StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttps))
				) {
					Process.Start(uri.AbsoluteUri);
				}
			} catch(Exception exception) {
				App.Mainframe.ErrorMessage(LogicCircuit.Resources.ErrorUnsupportedUri, exception);
			}
		}
	}
}
