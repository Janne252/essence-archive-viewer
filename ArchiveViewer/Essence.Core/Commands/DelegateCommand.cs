using System;

namespace Essence.Core.Commands
{
  public class DelegateCommand : BaseCommand
  {
    private readonly Action m_executeAction;
    private readonly Func<bool> m_canExecuteFunc;

    public DelegateCommand(Action executeAction)
      : this(executeAction, (Func<bool>) null)
    {
    }

    public DelegateCommand(Action executeAction, Func<bool> canExecuteFunc)
    {
      m_canExecuteFunc = canExecuteFunc;
      m_executeAction = executeAction ?? throw new ArgumentNullException(nameof (executeAction));
    }

    public override bool CanExecute()
    {
      Func<bool> canExecuteFunc = m_canExecuteFunc;
      return canExecuteFunc == null || canExecuteFunc();
    }

    public override void Execute() => m_executeAction();
  }
}
