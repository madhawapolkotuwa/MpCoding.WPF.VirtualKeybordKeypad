using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace VirtualKeys.Commands
{
    public class CommandBase : ICommand
    {
        private static readonly Predicate<object> _defaultCanExecute = a => true;
        private Predicate<object> _canExecute;
        private Action<object> _execute;

        private static Predicate<object> DefaultCanExecute => _defaultCanExecute;

        public CommandBase(Action<object> execute)
        {
            _canExecute = DefaultCanExecute;
            _execute = execute;
        }

        public CommandBase(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public virtual event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void SetExecuteMethods(Action<object> execute, Predicate<object> canExecute = null)
        {
            _canExecute = canExecute ?? _defaultCanExecute;
            _execute = execute;
        }
    }
}
