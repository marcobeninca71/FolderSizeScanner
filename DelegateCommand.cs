using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dws.Utilities
{
	public class DelegateCommand : ICommand
	{
		private readonly Func<bool>? _canExecute;
		private readonly Action<object>? _execute;

		public event EventHandler? CanExecuteChanged;

		public DelegateCommand(Action<object>? execute)
			: this(execute, null)
		{
		}

		public DelegateCommand(Action<object>? execute,
					   Func<bool>? canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if(_canExecute == null)
			{
				return true;
			}

			return _canExecute();
		}

		public void Execute(object parameter)
		{
            if(_execute != null)
			    _execute(parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			if(CanExecuteChanged != null)
			{
				CanExecuteChanged(this, EventArgs.Empty);
			}
		}

	}

    public class DelegateCommandAsync<T> : ICommand
    {
        private readonly Func<T, Task>? _executeTask;
        private readonly Predicate<object>? _canExecute;
        private bool _locked;

        public DelegateCommandAsync(Func<T, Task>? executeTask) : this(executeTask, o => true)
        {
        }

        public DelegateCommandAsync(Func<T, Task>? executeTask, Predicate<object>? canExecute)
        {
            _executeTask = executeTask;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_locked && (_canExecute != null &&_canExecute.Invoke(parameter));
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (_executeTask != null)
                {
                    _locked = true;
                    CanExecuteChanged?.Invoke(this, new CommandExecuteChangedArgs("Command Looked for async execution"));
                    await _executeTask.Invoke((T)parameter);
                }
            }
            finally
            {
                _locked = false;

                CanExecuteChanged?.Invoke(this, new CommandExecuteChangedArgs("Command Unlooked for async execution terminated"));
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void ChangeCanExecute()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    internal class CommandExecuteChangedArgs : EventArgs
    {
        private string Message;

        public CommandExecuteChangedArgs(string m)
        {
            this.Message = m;
        }
    }
}
