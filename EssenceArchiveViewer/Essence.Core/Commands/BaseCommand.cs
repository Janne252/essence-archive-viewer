using System;
using System.Windows.Input;

namespace Essence.Core.Commands
{
  public abstract class BaseCommand<T> : ICommand
  {
    bool ICommand.CanExecute(object parameter) => CanExecute(parameter is not T obj ? default : obj);

    void ICommand.Execute(object parameter) => Execute(parameter is not T obj ? default : obj);

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
      var canExecuteChanged = CanExecuteChanged;
      if (canExecuteChanged == null)
        return;
      canExecuteChanged(this, EventArgs.Empty);
    }

    public abstract bool CanExecute(T parameter);

    public abstract void Execute(T parameter);
  }
}
