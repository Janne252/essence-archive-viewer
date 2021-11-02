// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.PropertyCommentChangedEventArgs
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  public class PropertyCommentChangedEventArgs : EventArgs
  {
    public PropertyCommentChangedEventArgs(string propertyName) => this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));

    public string PropertyName { get; }
  }
}
