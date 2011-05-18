using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace LogicCircuit {
	internal sealed class HyperlinkNavigationCommand : ICommand {

		#pragma warning disable 0067
			public event EventHandler CanExecuteChanged;
		#pragma warning restore 0067

		public bool CanExecute(object parameter) {
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Execute(object parameter) {
			if(parameter != null) {
				try {
					Uri uri = new Uri(parameter.ToString());
					if(!uri.IsFile && !uri.IsUnc &&
						(StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttp) || StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, Uri.UriSchemeHttps))
					) {
						Process.Start(uri.AbsoluteUri);
					}
				} catch(Exception exception) {
					App.Mainframe.ReportException(exception);
				}
			}
		}
	}
}
