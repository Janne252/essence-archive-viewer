using System;
using System.ComponentModel;
using System.Windows;

namespace Essence.Core
{
  public class DesignTimeResourceDictionary : ResourceDictionary
  {
    private static bool IsInDesignMode => (bool) DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof (DependencyObject)).Metadata.DefaultValue;

    public new Uri Source
    {
      get => IsInDesignMode ? base.Source : null;
      set
      {
        if (!IsInDesignMode)
          return;
        base.Source = value;
      }
    }
  }
}
