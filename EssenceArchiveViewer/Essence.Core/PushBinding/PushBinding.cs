using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Essence.Core.PushBinding
{
  public class PushBinding : FreezableBinding
  {
    public static DependencyProperty TargetPropertyMirrorProperty = DependencyProperty.Register(nameof (TargetPropertyMirror), typeof (object), typeof (PushBinding));
    public static DependencyProperty TargetPropertyListenerProperty = DependencyProperty.Register(nameof (TargetPropertyListener), typeof (object), typeof (PushBinding), new UIPropertyMetadata(null, new PropertyChangedCallback(OnTargetPropertyListenerChanged)));

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
      if (targetObject == null || DesignerProperties.GetIsInDesignMode(this))
        return;
      var binding = new Binding
      {
        Source = targetObject,
        Mode = BindingMode.OneWay,
        Path = TargetDependencyProperty == null ? new PropertyPath(TargetProperty, Array.Empty<object>()) : new PropertyPath(TargetDependencyProperty)
      };
      BindingOperations.SetBinding(this, TargetPropertyListenerProperty, binding);
      BindingOperations.SetBinding(this, TargetPropertyMirrorProperty, Binding);
      TargetPropertyValueChanged();
      switch (targetObject)
      {
        case FrameworkElement _:
          ((FrameworkElement) targetObject).Loaded += (_param1, _param2) => TargetPropertyValueChanged();
          break;
        case FrameworkContentElement _:
          ((FrameworkContentElement) targetObject).Loaded += (_param1, _param2) => TargetPropertyValueChanged();
          break;
      }
    }

    private void TargetPropertyValueChanged()
    {
      var obj = GetValue(TargetPropertyListenerProperty);
      SetValue(TargetPropertyMirrorProperty, obj);
    }

    protected override void CloneCore(Freezable sourceFreezable)
    {
      var pushBinding = sourceFreezable as PushBinding;
      TargetProperty = pushBinding.TargetProperty;
      TargetDependencyProperty = pushBinding.TargetDependencyProperty;
      base.CloneCore(sourceFreezable);
    }

    protected override Freezable CreateInstanceCore() => new PushBinding();
  }
}
