using System.Windows;

namespace Essence.Core.PushBinding
{
  public class PushBindingManager
  {
    public static DependencyProperty PushBindingsProperty = DependencyProperty.RegisterAttached("PushBindingsInternal", typeof (PushBindingCollection), typeof (PushBindingManager), new UIPropertyMetadata(null));
    public static DependencyProperty StylePushBindingsProperty = DependencyProperty.RegisterAttached("StylePushBindings", typeof (PushBindingCollection), typeof (PushBindingManager), new UIPropertyMetadata(null, new PropertyChangedCallback(StylePushBindingsChanged)));

    public static PushBindingCollection GetPushBindings(DependencyObject obj)
    {
      if (obj.GetValue(PushBindingsProperty) == null)
        obj.SetValue(PushBindingsProperty, new PushBindingCollection(obj));
      return (PushBindingCollection) obj.GetValue(PushBindingsProperty);
    }

    public static void SetPushBindings(DependencyObject obj, PushBindingCollection value) => obj.SetValue(PushBindingsProperty, value);

    public static PushBindingCollection GetStylePushBindings(
      DependencyObject obj)
    {
      return (PushBindingCollection) obj.GetValue(StylePushBindingsProperty);
    }

    public static void SetStylePushBindings(DependencyObject obj, PushBindingCollection value) => obj.SetValue(StylePushBindingsProperty, value);

    public static void StylePushBindingsChanged(
      DependencyObject target,
      DependencyPropertyChangedEventArgs e)
    {
      if (target == null)
        return;
      var newValue = e.NewValue as PushBindingCollection;
      var pushBindings = GetPushBindings(target);
      foreach (Freezable freezable in newValue)
      {
        var pushBinding = freezable.Clone() as PushBinding;
        pushBindings.Add(pushBinding);
      }
    }
  }
}
