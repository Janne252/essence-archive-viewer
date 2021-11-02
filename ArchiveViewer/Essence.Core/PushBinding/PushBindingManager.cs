// Decompiled with JetBrains decompiler
// Type: Essence.Core.PushBinding.PushBindingManager
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Windows;

namespace Essence.Core.PushBinding
{
  public class PushBindingManager
  {
    public static DependencyProperty PushBindingsProperty = DependencyProperty.RegisterAttached("PushBindingsInternal", typeof (PushBindingCollection), typeof (PushBindingManager), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));
    public static DependencyProperty StylePushBindingsProperty = DependencyProperty.RegisterAttached("StylePushBindings", typeof (PushBindingCollection), typeof (PushBindingManager), (PropertyMetadata) new UIPropertyMetadata((object) null, new PropertyChangedCallback(PushBindingManager.StylePushBindingsChanged)));

    public static PushBindingCollection GetPushBindings(DependencyObject obj)
    {
      if (obj.GetValue(PushBindingManager.PushBindingsProperty) == null)
        obj.SetValue(PushBindingManager.PushBindingsProperty, (object) new PushBindingCollection(obj));
      return (PushBindingCollection) obj.GetValue(PushBindingManager.PushBindingsProperty);
    }

    public static void SetPushBindings(DependencyObject obj, PushBindingCollection value) => obj.SetValue(PushBindingManager.PushBindingsProperty, (object) value);

    public static PushBindingCollection GetStylePushBindings(
      DependencyObject obj)
    {
      return (PushBindingCollection) obj.GetValue(PushBindingManager.StylePushBindingsProperty);
    }

    public static void SetStylePushBindings(DependencyObject obj, PushBindingCollection value) => obj.SetValue(PushBindingManager.StylePushBindingsProperty, (object) value);

    public static void StylePushBindingsChanged(
      DependencyObject target,
      DependencyPropertyChangedEventArgs e)
    {
      if (target == null)
        return;
      PushBindingCollection newValue = e.NewValue as PushBindingCollection;
      PushBindingCollection pushBindings = PushBindingManager.GetPushBindings(target);
      foreach (Freezable freezable in (FreezableCollection<Essence.Core.PushBinding.PushBinding>) newValue)
      {
        Essence.Core.PushBinding.PushBinding pushBinding = freezable.Clone() as Essence.Core.PushBinding.PushBinding;
        pushBindings.Add(pushBinding);
      }
    }
  }
}
