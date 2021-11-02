using System;
using System.Windows.Input;

namespace Essence.Core.Commands
{
  public abstract class BaseCommand : ICommand
  {
    bool ICommand.CanExecute(object parameter) => CanExecute();

    void ICommand.Execute(object parameter) => Execute();

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
      EventHandler canExecuteChanged = CanExecuteChanged;
      if (canExecuteChanged == null)
        return;
      canExecuteChanged((object) this, EventArgs.Empty);
    }

    public abstract bool CanExecute();

    public abstract void Execute();
  }
}
