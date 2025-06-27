using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CyberSecurityAwarenessChatbot.ViewModels
{
    // A generic implementation of ICommand, commonly used in MVVM.
    // It allows binding UI elements (like Buttons) to methods in the ViewModel.
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Event that fires when the CanExecute status changes.
        // CommandManager.RequerySuggested is a WPF mechanism that automatically
        // raises this event when various conditions change (e.g., keyboard focus).
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Constructor for commands that always execute (no CanExecute condition).
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        // Constructor for commands with a CanExecute condition.
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute)); // Ensure execute action is not null.
            _canExecute = canExecute; // Can be null, meaning the command always executes.
        }

        // Determines if the command can execute in its current state.
        public bool CanExecute(object parameter)
        {
            // If _canExecute is null, the command can always execute.
            // Otherwise, call the _canExecute predicate.
            return _canExecute == null || _canExecute(parameter);
        }

        // Executes the command logic.
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
