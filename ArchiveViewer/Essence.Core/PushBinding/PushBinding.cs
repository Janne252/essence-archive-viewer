using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Essence.Core.PushBinding
{
  public class PushBinding : FreezableBinding
  {
    public static DependencyProperty TargetPropertyMirrorProperty = DependencyProperty.Register(nameof (TargetPropertyMirror), typeof (object), typeof (PushBinding));
    public static DependencyProperty TargetPropertyListenerProperty = DependencyProperty.Register(nameof (TargetPropertyListener), typeof (object), typeof (PushBinding), (PropertyMetadata) new UIPropertyMetadata((object) null, new PropertyChangedCallback(OnTargetPropertyListenerChanged)));

    private static void OnTargetPropertyListenerChanged(
      object sender,
      DependencyPropertyChangedEventArgs e)
    {
      (sender as PushBinding).TargetPropertyValueChanged();
    }

    public PushBinding() => Mode = BindingMode.OneWayToSource;

    public object TargetPropertyMirror
    {
      get => GetValue(TargetPropertyMirrorProperty);
      set => SetValue(TargetPropertyMirrorProperty, value);
    }

    public object TargetPropertyListener
    {
      get => GetValue(TargetPropertyListenerProperty);
      set => SetValue(TargetPropertyListenerProperty, value);
    }

    [DefaultValue(null)]
    public string TargetProperty { get; set; }

    [DefaultValue(null)]
    public DependencyProperty TargetDependencyProperty { get; set; }

    public void SetupTargetBinding(DependencyObject targetObject)
    {
      if (targetObject == null || DesignerProperties.GetIsInDesignMode((DependencyObject) this))
        return;
      Binding binding = new Binding()
      {
        Source = (object) targetObject,
        Mode = BindingMode.OneWay
      };
      binding.Path = TargetDependencyProperty == null ? new PropertyPath(TargetProperty, Array.Empty<object>()) : new PropertyPath((object) TargetDependencyProperty);
      BindingOperations.SetBinding((DependencyObject) this, TargetPropertyListenerProperty, (BindingBase) binding);
      BindingOperations.SetBinding((DependencyObject) this, TargetPropertyMirrorProperty, (BindingBase) Binding);
      TargetPropertyValueChanged();
      switch (targetObject)
      {
        case FrameworkElement _:
          ((FrameworkElement) targetObject).Loaded += (RoutedEventHandler) ((_param1, _param2) => TargetPropertyValueChanged());
          break;
        case FrameworkContentElement _:
          ((FrameworkContentElement) targetObject).Loaded += (RoutedEventHandler) ((_param1, _param2) => TargetPropertyValueChanged());
          break;
      }
    }

    private void TargetPropertyValueChanged()
    {
      object obj = GetValue(TargetPropertyListenerProperty);
      SetValue(TargetPropertyMirrorProperty, obj);
    }

    protected override void CloneCore(Freezable sourceFreezable)
    {
      PushBinding pushBinding = sourceFreezable as PushBinding;
      TargetProperty = pushBinding.TargetProperty;
      TargetDependencyProperty = pushBinding.TargetDependencyProperty;
      base.CloneCore(sourceFreezable);
    }

    protected override Freezable CreateInstanceCore() => (Freezable) new PushBinding();
  }
}
