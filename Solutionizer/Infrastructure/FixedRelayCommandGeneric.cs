using System;
using System.Windows.Input;

namespace Solutionizer.Infrastructure {
    /// <summary>
    /// A generic command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'. This class allows you to accept command parameters in the
    /// Execute and CanExecute callback methods.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <remarks>
    /// HACK: this call is necessary because RelayCommand.CanExecuteChanged does not rely on 
    /// CommandProcessor in V4beta1. See http://mvvmlight.codeplex.com/workitem/7546.
    /// </remarks>
    public class FixedRelayCommand<T> : ICommand {
        private readonly Action<T> _execute;

        private readonly Func<T,bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class that 
        /// can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public FixedRelayCommand(Action<T> execute)
            : this(execute, null) {
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public FixedRelayCommand(Action<T> execute, Func<T,bool> canExecute) {
            if (execute == null) {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

#if SILVERLIGHT
    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
        public event EventHandler CanExecuteChanged;
#else
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add {
                if (_canExecute != null) {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove {
                if (_canExecute != null) {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }
#endif

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        public void RaiseCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data 
        /// to be passed, this object can be set to a null reference</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute((T) parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked. 
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data 
        /// to be passed, this object can be set to a null reference</param>
        public void Execute(object parameter) {
            _execute((T) parameter);
        }
    }
}