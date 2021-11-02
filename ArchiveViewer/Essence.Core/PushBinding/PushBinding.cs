// Decompiled with JetBrains decompiler
// Type: Essence.Core.PushBinding.PushBinding
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Essence.Core.PushBinding
{
  public class PushBinding : FreezableBinding
  {
    public static DependencyProperty TargetPropertyMirrorProperty = DependencyProperty.Register(nameof (TargetPropertyMirror), typeof (object), typeof (Essence.Core.PushBinding.PushBinding));
    public static DependencyProperty TargetPropertyListenerProperty = DependencyProperty.Register(nameof (TargetPropertyListener), typeof (object), typeof (Essence.Core.PushBinding.PushBinding), (PropertyMetadata) new UIPropertyMetadata((object) null, new PropertyChangedCallback(Essence.Core.PushBinding.PushBinding.OnTargetPropertyListenerChanged)));

    private static void OnTargetPropertyListenerChanged(
      object sender,
      DependencyPropertyChangedEventArgs e)
    {
      (sender as Essence.Core.PushBinding.PushBinding).TargetPropertyValueChanged();
    }

    public PushBinding() => this.Mode = BindingMode.OneWayToSource;

    public object TargetPropertyMirror
    {
      get => this.GetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyMirrorProperty);
      set => this.SetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyMirrorProperty, value);
    }

    public object TargetPropertyListener
    {
      get => this.GetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyListenerProperty);
      set => this.SetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyListenerProperty, value);
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
      binding.Path = this.TargetDependencyProperty == null ? new PropertyPath(this.TargetProperty, Array.Empty<object>()) : new PropertyPath((object) this.TargetDependencyProperty);
      BindingOperations.SetBinding((DependencyObject) this, Essence.Core.PushBinding.PushBinding.TargetPropertyListenerProperty, (BindingBase) binding);
      BindingOperations.SetBinding((DependencyObject) this, Essence.Core.PushBinding.PushBinding.TargetPropertyMirrorProperty, (BindingBase) this.Binding);
      this.TargetPropertyValueChanged();
      switch (targetObject)
      {
        case FrameworkElement _:
          ((FrameworkElement) targetObject).Loaded += (RoutedEventHandler) ((_param1, _param2) => this.TargetPropertyValueChanged());
          break;
        case FrameworkContentElement _:
          ((FrameworkContentElement) targetObject).Loaded += (RoutedEventHandler) ((_param1, _param2) => this.TargetPropertyValueChanged());
          break;
      }
    }

    private void TargetPropertyValueChanged()
    {
      object obj = this.GetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyListenerProperty);
      this.SetValue(Essence.Core.PushBinding.PushBinding.TargetPropertyMirrorProperty, obj);
    }

    protected override void CloneCore(Freezable sourceFreezable)
    {
      Essence.Core.PushBinding.PushBinding pushBinding = sourceFreezable as Essence.Core.PushBinding.PushBinding;
      this.TargetProperty = pushBinding.TargetProperty;
      this.TargetDependencyProperty = pushBinding.TargetDependencyProperty;
      base.CloneCore(sourceFreezable);
    }

    protected override Freezable CreateInstanceCore() => (Freezable) new Essence.Core.PushBinding.PushBinding();
  }
}
