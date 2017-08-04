using System;
using System.Windows;
using System.Windows.Input;

namespace InvAddIn.ViewModel
{
    public class SaveCommand : ICommand
    {
        private ISettingsViewModel vm;

        public SaveCommand(ISettingsViewModel _vm)
        {
            vm = _vm;
        }
            
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            double temp;
            return double.TryParse(vm.Gap, out temp);
        }

        public void Execute(object parameter)
        {
            vm.SaveToModel();
            ((Window)parameter).Close();
        }
    }
}
