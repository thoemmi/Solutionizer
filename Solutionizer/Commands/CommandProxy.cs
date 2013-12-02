using System;
using System.Windows;
using System.Windows.Input;

namespace Solutionizer.Commands {
    public class CommandProxy : Freezable, ICommand {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof (ICommand),
            typeof (CommandProxy), new PropertyMetadata(OnCommandChanged));

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool CanExecute(object parameter) {
            if (Command != null) {
                return Command.CanExecute(parameter);
            }
            return false;
        }

        public void Execute(object parameter) {
            Command.Execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var commandReference = (CommandProxy)d;
            var oldCommand = e.OldValue as ICommand;
            var newCommand = e.NewValue as ICommand;

            if (oldCommand != null) {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }
            if (newCommand != null) {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }

        protected override Freezable CreateInstanceCore() {
            throw new NotImplementedException();
        }
    }
}