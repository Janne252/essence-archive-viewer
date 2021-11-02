// Decompiled with JetBrains decompiler
// Type: Essence.Core.DesignTimeResourceDictionary
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      get => DesignTimeResourceDictionary.IsInDesignMode ? base.Source : (Uri) null;
      set
      {
        if (!DesignTimeResourceDictionary.IsInDesignMode)
          return;
        base.Source = value;
      }
    }
  }
}
