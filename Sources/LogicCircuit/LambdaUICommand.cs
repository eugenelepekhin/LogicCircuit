using System;
using System.Windows.Input;

namespace LogicCircuit {
	public class LambdaUICommand : ICommand {

		public event EventHandler CanExecuteChanged;

		public string Text { get; private set; }
		public Predicate<object> CanExecutePredicate { get; private set; }
		public Action<object> ExecuteAction { get; private set; }

		public LambdaUICommand(string text, Action<object> execute, Predicate<object> canExecute = null) {
			if(string.IsNullOrEmpty(text)) {
				throw new ArgumentNullException("text");
			}
			if(execute == null) {
				throw new ArgumentNullException("execute");
			}
			this.Text = text;
			this.CanExecutePredicate = canExecute;
			this.ExecuteAction = execute;
		}

		public bool CanExecute(object parameter) {
			if(this.CanExecutePredicate != null) {
				return this.CanExecutePredicate(parameter);
			}
			return true;
		}

		public void Execute(object parameter) {
			this.ExecuteAction(parameter);
		}

		public void NotifyCanExecuteChanged() {
			EventHandler handler = this.CanExecuteChanged;
			if(handler != null) {
				handler(this, EventArgs.Empty);
			}
		}
	}
}
