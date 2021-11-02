// Decompiled with JetBrains decompiler
// Type: Essence.Core.Commands.BaseCommand
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Windows.Input;

namespace Essence.Core.Commands
{
  public abstract class BaseCommand : ICommand
  {
    bool ICommand.CanExecute(object parameter) => this.CanExecute();

    void ICommand.Execute(object parameter) => this.Execute();

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
      EventHandler canExecuteChanged = this.CanExecuteChanged;
      if (canExecuteChanged == null)
        return;
      canExecuteChanged((object) this, EventArgs.Empty);
    }

    public abstract bool CanExecute();

    public abstract void Execute();
  }
}
