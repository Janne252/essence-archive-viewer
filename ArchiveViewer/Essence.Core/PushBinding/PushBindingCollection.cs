using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace Essence.Core.PushBinding
{
  public class PushBindingCollection : FreezableCollection<PushBinding>
  {
    public PushBindingCollection()
    {
    }

    public PushBindingCollection(DependencyObject targetObject)
    {
      TargetObject = targetObject;
      ((INotifyCollectionChanged) this).CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChanged);
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action != NotifyCollectionChangedAction.Add)
        return;
      foreach (PushBinding newItem in (IEnumerable) e.NewItems)
        newItem.SetupTargetBinding(TargetObject);
    }

    public DependencyObject TargetObject { get; private set; }
  }
}
