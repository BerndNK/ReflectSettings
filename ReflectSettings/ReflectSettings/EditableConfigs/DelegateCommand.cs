using System;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
 
        public event EventHandler CanExecuteChanged;
 
        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;
 
        public void Execute(object parameter)
        {
            _execute();
        }
    }
}