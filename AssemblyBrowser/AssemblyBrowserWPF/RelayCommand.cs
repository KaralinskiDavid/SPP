using System;
using System.Windows.Input;

namespace AssemblyBrowserWPF
{
    class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
