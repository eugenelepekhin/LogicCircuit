using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogicCircuit {
	public class CommandMenuItem : MenuItem {
		private static void RemoveInputBinding(Window window, KeyGesture keyGesture) {
			foreach(InputBinding old in window.InputBindings) {
				if(old.Gesture is KeyGesture gesture && gesture.Key == keyGesture.Key && gesture.Modifiers == keyGesture.Modifiers) {
					window.InputBindings.Remove(old);
					break;
				}
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			try {
				if(e.Property == MenuItem.CommandProperty) {
					Window window = Window.GetWindow(this);
					if(window != null) {
						if(this.Command is LambdaUICommand command && command.KeyGesture != null) {
							CommandMenuItem.RemoveInputBinding(window, command.KeyGesture);
							window.InputBindings.Add(new InputBinding(command, command.KeyGesture));
						} else {
							command = e.OldValue as LambdaUICommand;
							if(command != null && command.KeyGesture != null) {
								CommandMenuItem.RemoveInputBinding(window, command.KeyGesture);
							}
						}
					}
				} else if(e.Property == MenuItem.IsVisibleProperty && this.IsVisible) {
					if(this.Command is LambdaUICommand command) {
						command.NotifyCanExecuteChanged();
					}
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
