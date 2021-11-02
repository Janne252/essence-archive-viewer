// Decompiled with JetBrains decompiler
// Type: Essence.Core.Commands.DelegateCommand
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      this.m_canExecuteFunc = canExecuteFunc;
      this.m_executeAction = executeAction ?? throw new ArgumentNullException(nameof (executeAction));
    }

    public override bool CanExecute()
    {
      Func<bool> canExecuteFunc = this.m_canExecuteFunc;
      return canExecuteFunc == null || canExecuteFunc();
    }

    public override void Execute() => this.m_executeAction();
  }
}
