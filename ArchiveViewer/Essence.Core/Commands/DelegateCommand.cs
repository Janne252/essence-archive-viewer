using System;

namespace Essence.Core.Commands
{
  public class DelegateCommand<T> : BaseCommand<T>
  {
    private readonly Action<T> m_executeAction;
    private readonly Func<T, bool> m_canExecuteFunc;

    public DelegateCommand(Action<T> executeAction)
      : this(executeAction, (Func<T, bool>) null)
    {
    }

    public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteFunc)
    {
      m_canExecuteFunc = canExecuteFunc;
      m_executeAction = executeAction ?? throw new ArgumentNullException(nameof (executeAction));
    }

    public override bool CanExecute(T parameter)
    {
      Func<T, bool> canExecuteFunc = m_canExecuteFunc;
      return canExecuteFunc == null || canExecuteFunc(parameter);
    }

    public override void Execute(T parameter) => m_executeAction(parameter);
  }
}
