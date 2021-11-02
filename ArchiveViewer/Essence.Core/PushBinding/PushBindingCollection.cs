// Decompiled with JetBrains decompiler
// Type: Essence.Core.PushBinding.PushBindingCollection
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace Essence.Core.PushBinding
{
  public class PushBindingCollection : FreezableCollection<Essence.Core.PushBinding.PushBinding>
  {
    public PushBindingCollection()
    {
    }

    public PushBindingCollection(DependencyObject targetObject)
    {
      this.TargetObject = targetObject;
      ((INotifyCollectionChanged) this).CollectionChanged += new NotifyCollectionChangedEventHandler(this.CollectionChanged);
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action != NotifyCollectionChangedAction.Add)
        return;
      foreach (Essence.Core.PushBinding.PushBinding newItem in (IEnumerable) e.NewItems)
        newItem.SetupTargetBinding(this.TargetObject);
    }

    public DependencyObject TargetObject { get; private set; }
  }
}
