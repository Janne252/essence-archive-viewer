using System;
using System.Windows.Input;

namespace Essence.Core.Commands
{
  public abstract class BaseCommand<T> : ICommand
  {
    bool ICommand.CanExecute(object parameter) => CanExecute(!(parameter is T obj) ? default (T) : obj);

    void ICommand.Execute(object parameter) => Execute(!(parameter is T obj) ? default (T) : obj);

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
      EventHandler canExecuteChanged = CanExecuteChanged;
      if (canExecuteChanged == null)
        return;
      canExecuteChanged((object) this, EventArgs.Empty);
    }

    public abstract bool CanExecute(T parameter);

    public abstract void Execute(T parameter);
  }
}
