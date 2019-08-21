using System;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
 
        public event EventHandler CanExecuteChanged;
 
        public DelegateCommand(Action execute)
        {
            _execute = x => execute();
        }

        public DelegateCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;
 
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}