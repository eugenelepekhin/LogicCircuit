using System;
using System.Windows.Input;

namespace LogicCircuit {
	public class LambdaUICommand : ICommand {

		public event EventHandler CanExecuteChanged;

		public string Text { get; private set; }

		private Predicate<object> canExecutePredicate;
		private Action<object> executeAction;

		public KeyGesture KeyGesture { get; private set; }
		public string InputGestureText {
			get {
				if(this.KeyGesture != null) {
					string text = this.KeyGesture.DisplayString;
					if(string.IsNullOrEmpty(text)) {
						text = this.KeyGesture.GetDisplayStringForCulture(Properties.Resources.Culture);
					}
					return text;
				}
				return null;
			}
		}

		public LambdaUICommand(string text, Predicate<object> canExecute, Action<object> execute, KeyGesture keyGesture) {
			if(string.IsNullOrEmpty(text)) {
				throw new ArgumentNullException("text");
			}
			if(execute == null) {
				throw new ArgumentNullException("execute");
			}
			this.Text = text;
			this.canExecutePredicate = canExecute;
			this.executeAction = execute;
			this.KeyGesture = keyGesture;
		}

		public LambdaUICommand(string text, Predicate<object> canExecute, Action<object> execute) : this(text, canExecute, execute, null) {
		}

		public LambdaUICommand(string text, Action<object> execute, KeyGesture keyGesture) : this(text, null, execute, keyGesture) {
		}

		public LambdaUICommand(string text, Action<object> execute) : this(text, null, execute, null) {
		}

		public bool CanExecute(object parameter) {
			if(this.canExecutePredicate != null) {
				try {
					return this.canExecutePredicate(parameter);
				} catch(Exception exception) {
					App.Mainframe.ReportException(exception);
					return false;
				}
			}
			return true;
		}

		public void Execute(object parameter) {
			try {
				if(this.CanExecute(parameter)) {
					this.executeAction(parameter);
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}

		public void NotifyCanExecuteChanged() {
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
