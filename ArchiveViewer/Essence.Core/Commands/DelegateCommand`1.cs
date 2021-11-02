// Decompiled with JetBrains decompiler
// Type: Essence.Core.Commands.DelegateCommand`1
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      this.m_canExecuteFunc = canExecuteFunc;
      this.m_executeAction = executeAction ?? throw new ArgumentNullException(nameof (executeAction));
    }

    public override bool CanExecute(T parameter)
    {
      Func<T, bool> canExecuteFunc = this.m_canExecuteFunc;
      return canExecuteFunc == null || canExecuteFunc(parameter);
    }

    public override void Execute(T parameter) => this.m_executeAction(parameter);
  }
}
